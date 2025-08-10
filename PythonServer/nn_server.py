import socket
import struct
import threading
import torch
import torch.nn as nn

# --- Neural network definition ---
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
INPUT_SIZE = 47
NN_COUNT = 5
INFER_PORT = 65432
GA_PORT = 65431
INPUT_PACKET_SIZE = 4 + INPUT_SIZE * 4  # 4 bytes index + input floats
shutdown_event = threading.Event()  # 🔧 Added for graceful shutdown

# --- Create and initialize models ---
models = [SynergyNN(INPUT_SIZE) for _ in range(NN_COUNT)]
for m in models:
    m.eval()

# --- Serialization utilities ---
def serialize_model(model):
    return torch.cat([param.data.view(-1) for param in model.parameters()])

def deserialize_model(model, flat_weights):
    offset = 0
    for param in model.parameters():
        numel = param.data.numel()
        param.data.copy_(flat_weights[offset:offset + numel].view_as(param.data))
        offset += numel

# --- MULTI-CLIENT HANDLER 🔧 NEW ---
def handle_client(conn, addr, initial_data=None):
    print(f"[nn_server] Connected to {addr}")
    try:
        while True:
            data = initial_data if initial_data else conn.recv(INPUT_PACKET_SIZE)
            initial_data = None  # Only use once

            if not data or len(data) < INPUT_PACKET_SIZE:
                break

            # Shutdown packet check (all-zero shutdown from Unity)
            if all(b == 0 for b in data):
                print(f"[nn_server] Round end signal received from {addr}")
                break

            nn_index = struct.unpack('i', data[:4])[0]
            if nn_index < 0 or nn_index >= NN_COUNT:
                print(f"[nn_server] Invalid index {nn_index}")
                continue

            input_floats = struct.unpack(f'{INPUT_SIZE}f', data[4:])
            print(f"[debug] From {addr} → NN index: {nn_index}")
            print(f"[debug] Input floats: {input_floats}")
            input_tensor = torch.tensor(input_floats, dtype=torch.float32).unsqueeze(0)

            with torch.no_grad():
                logits = models[nn_index](input_tensor)[0]
                modes = torch.argmax(logits.view(3, 3), dim=1)
            print(f"[debug] Weapon modes output: {modes.tolist()}")
            conn.sendall(struct.pack('iii', *modes.tolist()))

    except Exception as e:
        print(f"[nn_server] Client error: {e}")
    finally:
        conn.close()
        print(f"[nn_server] Connection to {addr} closed.")


# --- Updated inference loop: MULTI-CLIENT SUPPORT 🔧 NEW ---
def inference_loop():
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.bind(('localhost', INFER_PORT))
    sock.listen(5)
    sock.settimeout(1.0)
    print(f"[nn_server] Inference server listening on port {INFER_PORT}")

    while not shutdown_event.is_set():
        try:
            conn, addr = sock.accept()

            # Read the first 4 bytes
            first4 = conn.recv(4)
            if len(first4) < 4:
                conn.close()
                continue

            # Shutdown signal check
            if struct.unpack('i', first4)[0] == -1:
                print("[nn_server] 🔴 Shutdown signal received from BattleManager.")
                conn.close()
                shutdown_event.set()
                break

            # Receive the remaining input
            rest = b''
            while len(rest) < INPUT_PACKET_SIZE - 4:
                packet = conn.recv(INPUT_PACKET_SIZE - 4 - len(rest))
                if not packet:
                    break
                rest += packet
            full_data = first4 + rest

            # Handle client using thread
            threading.Thread(target=handle_client, args=(conn, addr, full_data), daemon=True).start()

        except socket.timeout:
            continue

    sock.close()
    print("[nn_server] Inference loop terminated.")

# --- GA weight sync loop on port 65431 ---
def ga_sync_loop():
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.bind(('localhost', GA_PORT))
    sock.listen(2)
    sock.settimeout(1.0)  # So it can check shutdown_event periodically
    print(f"[nn_server] GA sync server listening on port {GA_PORT}")

    try:
        while not shutdown_event.is_set():
            try:
                conn, addr = sock.accept()
                print(f"[nn_server] GA sync connection from {addr}")

                try:
                    while True:
                        cmd = conn.recv(4)
                        if not cmd or len(cmd) < 4:
                            break
                        request_type = struct.unpack('i', cmd)[0]

                        if request_type == 0:
                            print("[nn_server] GA requested model weights...")
                            for model in models:
                                flat = serialize_model(model)
                                print(f"[debug] Sending flat weights for model sent:")
                                print(flat.tolist()[:30])  # Print only the first 10 values
                                conn.sendall(struct.pack(f'{len(flat)}f', *flat.tolist()))
                            print("[nn_server] Sent model weights to GA.")

                        elif request_type == 1:
                            print("[nn_server] Receiving new weights from GA...")
                            param_len = len(serialize_model(models[0]))
                            for i in range(NN_COUNT):
                                data = b''
                                while len(data) < 4 * param_len:
                                    packet = conn.recv(4 * param_len - len(data))
                                    if not packet:
                                        raise ConnectionError("Incomplete model data received.")
                                    data += packet
                                weights = torch.tensor(struct.unpack(f'{param_len}f', data))
                                print(f"[debug] Received weights for model {i}:")
                                print(weights.tolist()[:30])  # Optional: truncate if large
                                deserialize_model(models[i], weights)
                            print("[nn_server] Models updated from GA.")

                except Exception as e:
                    print(f"[nn_server] GA sync error: {e}")
                finally:
                    conn.close()
                    print(f"[nn_server] GA sync connection from {addr} closed.")

            except socket.timeout:
                continue

    finally:
        sock.close()
        print("[nn_server] GA sync loop terminated.")


# --- Start nn_server ---
if __name__ == "__main__":
    print("[nn_server] Server running. Awaiting shutdown...")

    # Run inference and GA sync loops concurrently
    threading.Thread(target=inference_loop, daemon=True).start()
    threading.Thread(target=ga_sync_loop, daemon=True).start()

    # Keep the main thread alive until shutdown_event is set
    while not shutdown_event.is_set():
        threading.Event().wait(0.5)

    print("[nn_server] Shutdown complete.")
