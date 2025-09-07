import numpy as np
import random
import socket
import struct

import sys
import os
sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))
from LLM.generate_enemy import send_seq_and_save_json_verbatim


HOST = "127.0.0.1"
MOVEMENT_PORT = 65434
TOP_K = 3
TOTAL_ROUNDS = 2  # change as needed
persistent_base = r"C:\Users\jiahui li\AppData\LocalLow\DefaultCompany\Summer Project draft"

DIRS = ["s","l","r","u","d","ul","ur","dl","dr"]
IDX  = {d:i for i,d in enumerate(DIRS)}

def recv_exact(conn, n):
    buf = b""
    while len(buf) < n:
        chunk = conn.recv(n - len(buf))
        if not chunk:
            raise ConnectionError("Socket closed while receiving data")
        buf += chunk
    return buf

def recv_int32(conn):
    return struct.unpack("i", recv_exact(conn, 4))[0]

def recv_tokens(conn):
    count = recv_int32(conn)
    tokens = []
    for _ in range(count):
        ln = recv_int32(conn)
        data = recv_exact(conn, ln) if ln > 0 else b""
        tokens.append(data.decode("utf-8"))
    return tokens


def build_transition(sequences, dirs=DIRS, laplace=1e-6):
    """
    sequences: list[list[str]]  # multiple rounds of tokens
    Returns row-stochastic transition matrix P[i,j] = P(next=j | cur=i)
    """
    n = len(dirs)
    counts = np.full((n,n), laplace, dtype=float)  # Laplace smoothing

    for seq in sequences:
        seq = [t for t in seq if t in dirs]
        for a, b in zip(seq, seq[1:]):
            counts[IDX[a], IDX[b]] += 1.0

    row_sums = counts.sum(axis=1, keepdims=True)
    # If a row never appears, make it uniform to avoid NaNs
    zero_rows = (row_sums == 0).flatten()
    if zero_rows.any():
        counts[zero_rows, :] = 1.0
        row_sums = counts.sum(axis=1, keepdims=True)

    P = counts / row_sums
    return P


def perturb_transition_random(P, kappa=300.0, min_l1=0.08, max_l1=0.20, max_tries=200, seed=None):
    rng = np.random.default_rng(seed)
    n = P.shape[0]
    P2_last = None

    for _ in range(max_tries):
        P2 = np.zeros_like(P)
        for i in range(n):
            alpha = np.maximum(kappa * P[i], 1e-12)  # keep > 0
            P2[i] = rng.dirichlet(alpha)
        l1 = float(np.abs(P2 - P).sum())
        P2_last = P2
        if min_l1 <= l1 <= max_l1:
            return P2

    # Fallback: return the last random sample even if out of bounds
    return P2_last if P2_last is not None else P.copy()


def sample_sequence(P, length, start=None, seed=None):
    rng = random.Random(seed)
    n = len(DIRS)

    if start is None:
        # Use average of rows as starting weights
        start_weights = P.mean(axis=0).tolist()
        cur = rng.choices(range(n), weights=start_weights, k=1)[0]
    else:
        cur = IDX[start]

    out = [DIRS[cur]]
    for _ in range(length - 1):
        weights = P[cur].tolist()
        cur = rng.choices(range(n), weights=weights, k=1)[0]
        out.append(DIRS[cur])
    return out


if __name__ == "__main__":
    print(f"[movement_server] Listening on {HOST}:{MOVEMENT_PORT} for movement tokens...")

    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.bind((HOST, MOVEMENT_PORT))
    sock.listen(5)

    all_rounds = []
    round_counter = 1

    try:
        while True:  # keep running until shutdown
            conn, addr = sock.accept()
            print(f"\n[movement_server] Round {round_counter} connection from {addr}")

            try:
                # Peek first int32 for shutdown signal
                first4 = conn.recv(4, socket.MSG_PEEK)
                if len(first4) < 4:
                    raise ConnectionError("Socket closed before reading first int")
                peek_val = struct.unpack("i", first4)[0]

                if peek_val == -1:
                    print("[movement_server] 🔴 Shutdown signal received. Closing server.")
                    break  # exit main loop

                # Normal token receive
                tokens = recv_tokens(conn)
                all_rounds.append(tokens)  # store separately
                print(f"[movement_server] Round {round_counter}: received {len(tokens)} tokens.")

                if round_counter >= TOTAL_ROUNDS:
                    # 1) Learn transition from all collected rounds
                    P = build_transition(all_rounds, laplace=1e-6)

                    # 2) Random-but-bounded mutation
                    P2 = perturb_transition_random(P, kappa=300.0, min_l1=0.10, max_l1=0.18, seed=42)

                    # 3) Make 3 short sequences (diverse starts + seeds)
                    seqA = sample_sequence(P2, length=60, start="s",  seed=101)
                    enemy_pattern = send_seq_and_save_json_verbatim(seqA, persistent_base, filename="enemy_Type1.json")
                    print(enemy_pattern)
                    # reset for next set
                    all_rounds = []
                    round_counter = 0

            except Exception as e:
                print(f"[movement_server] Error in round {round_counter}: {e}")
            finally:
                conn.close()
                print(f"[movement_server] Round {round_counter} connection closed.")
                round_counter += 1

    finally:
        sock.close()
        print("[movement_server] Server shut down.")

