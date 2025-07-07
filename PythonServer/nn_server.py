import socket
import struct
import torch
import torch.nn as nn

# --- Define your neural network ---
class SynergyNN(nn.Module):
    def __init__(self, input_size, hidden_size=128, output_size=9):  # 3 weapons × 3 modes
        super().__init__()
        self.net = nn.Sequential(
            nn.Linear(input_size, hidden_size),
            nn.ReLU(),
            nn.Linear(hidden_size, output_size)
        )

    def forward(self, x):
        return self.net(x)

# --- Setup ---
INPUT_SIZE = 42  # Must match Unity input vector length
PORT = 65432

model = SynergyNN(INPUT_SIZE)
model.eval()  # Inference mode

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.bind(('localhost', PORT))
sock.listen(1)

print(f"[Python] NN Server listening on port {PORT}...")

conn, addr = sock.accept()
print(f"[Python] Connected from {addr}")

while True:
    data = conn.recv(INPUT_SIZE * 4)
    if not data:
        break

    floats = struct.unpack(f'{INPUT_SIZE}f', data)
    input_tensor = torch.tensor(floats, dtype=torch.float32).unsqueeze(0)  # [1, INPUT_SIZE]

    with torch.no_grad():
        logits = model(input_tensor)[0]  # [9]
        modes = torch.argmax(logits.view(3, 3), dim=1)  # [3] weapon modes

    packed = struct.pack('iii', *modes.tolist())
    conn.sendall(packed)

print("Connection closed.")
