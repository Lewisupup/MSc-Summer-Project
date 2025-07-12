import torch
import torch.nn as nn
import struct
import socket
import random
import numpy as np

# --- Neural network definition (must match nn_server.py) ---
class SynergyNN(nn.Module):
    def __init__(self, input_size, hidden_size=128, output_size=9):
        super().__init__()
        self.net = nn.Sequential(
            nn.Linear(input_size, hidden_size),
            nn.ReLU(),
            nn.Linear(hidden_size, output_size)
        )

    def forward(self, x):
        return self.net(x)

# --- Config ---
INPUT_SIZE = 42
NN_COUNT = 5
METRIC_PORT = 65433
GA_SYNC_PORT = 65431

# --- Helpers ---
def recv_exact(sock, num_bytes):
    data = b''
    while len(data) < num_bytes:
        packet = sock.recv(num_bytes - len(data))
        if not packet:
            raise ConnectionError("Connection closed unexpectedly.")
        data += packet
    return data

def serialize_model(model):
    return torch.cat([p.data.view(-1) for p in model.parameters()])

# --- Step 1: Listen for metrics from Unity ---
def listen_for_metrics():
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.bind(('localhost', METRIC_PORT))
    sock.listen(1)
    print("[GA] Waiting for Unity metrics on port 65433...")
    conn, _ = sock.accept()
    print("[GA] Metrics received from Unity.")

    data = recv_exact(conn, 5 * 4 * 4)  # 5 models × 4 floats
    # Example: unpack 20 floats from the byte data
    floats = struct.unpack('20f', data)
    print(f"[debug] Parsed metric floats ({len(floats)} values): {floats}")
    conn.close()
    sock.close()
    return np.array(struct.unpack('20f', data)).reshape(5, 4)

# --- Step 2: Fetch current models from nn_server ---
def fetch_models_from_nn_server():
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.connect(('localhost', GA_SYNC_PORT))
    sock.sendall(struct.pack('i', 0))  # Request model weights

    temp_model = SynergyNN(INPUT_SIZE)
    param_len = len(serialize_model(temp_model))
    all_weights = []
    for _ in range(NN_COUNT):
        flat = recv_exact(sock, 4 * param_len)
        weights = torch.tensor(struct.unpack(f'{param_len}f', flat))
        all_weights.append(weights)

    sock.close()
    print("[GA] Received models from nn_server.")
    return all_weights, param_len

# --- Step 3: GA evolution and upload ---
def run_genetic_algorithm(metrics, weights, param_len):
    scores = np.mean(metrics, axis=1)
    sorted_indices = np.argsort(scores)[::-1]  # Highest to lowest
    top3_indices = sorted_indices[:3]
    print(f"[GA] Top 3 models: {top3_indices} with scores: {scores[top3_indices]}")

    next_gen = []

    # --- Elitism: Keep best model unchanged ---
    next_gen.append(weights[top3_indices[0]].clone())

    # --- Generate remaining models using crossover + mutation ---
    while len(next_gen) < NN_COUNT:
        p1, p2 = random.sample(top3_indices.tolist(), 2)
        w1 = weights[p1]
        w2 = weights[p2]

        # Crossover: blend weights
        alpha = random.uniform(0.3, 0.7)
        child = alpha * w1 + (1 - alpha) * w2

        # Mutation
        mutation_strength = 0.1
        child += torch.randn_like(child) * mutation_strength

        next_gen.append(child)

    # --- Send updated weights to nn_server ---
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.connect(('localhost', GA_SYNC_PORT))
    sock.sendall(struct.pack('i', 1))  # Signal update mode
    print("[GA] Sending updated weights to nn_server...")

    for w in next_gen:
        sock.sendall(struct.pack(f'{param_len}f', *w.tolist()))

    sock.close()
    print("[GA] Update complete.")

# --- Main Entry ---
if __name__ == "__main__":
    metrics = listen_for_metrics()
    weights, param_len = fetch_models_from_nn_server()
    run_genetic_algorithm(metrics, weights, param_len)
