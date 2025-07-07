using System;
using System.Collections;
using System.Net.Sockets;
using UnityEngine;

public class WeaponModeController : MonoBehaviour
{
    public BattleInputBuilder inputBuilder;
    public WeaponHolder weaponHolder;

    [Header("Connection Settings")]
    public string host = "127.0.0.1";
    public int port = 65432;

    [Header("Model Settings")]
    public int inputSize = 42; // Must match BattleInputBuilder output length

    private TcpClient client;
    private NetworkStream stream;
    private bool running = true;

    void Start()
    {
        try
        {
            client = new TcpClient(host, port);
            stream = client.GetStream();
            Debug.Log("[Unity] Connected to Python NN server.");
        }
        catch (Exception e)
        {
            Debug.LogError("[Unity] Failed to connect to Python server: " + e.Message);
        }

        StartCoroutine(SendAndReceiveLoop());
    }

    IEnumerator SendAndReceiveLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f); // Send every 0.1s

        while (running)
        {
            if (client == null || !client.Connected)
            {
                yield return wait;
                continue;
            }

            float[] input = inputBuilder.BuildInputVector();
            Debug.Log("[Unity] Input vector: " + string.Join(", ", input));
            
            if (input.Length != inputSize)
            {
                Debug.LogError($"Input size mismatch! Expected {inputSize}, got {input.Length}");
                yield return wait;
                continue;
            }

            // Send input vector as bytes
            byte[] bytesToSend = new byte[inputSize * 4];
            Buffer.BlockCopy(input, 0, bytesToSend, 0, bytesToSend.Length);
            stream.Write(bytesToSend, 0, bytesToSend.Length);

            // Receive 3 ints = 12 bytes
            byte[] recvBuffer = new byte[12];
            int totalRead = 0;
            while (totalRead < 12)
            {
                int read = stream.Read(recvBuffer, totalRead, 12 - totalRead);
                if (read <= 0) break;
                totalRead += read;
            }

            if (totalRead == 12)
            {
                int[] modes = new int[3];
                Buffer.BlockCopy(recvBuffer, 0, modes, 0, 12);
                Debug.Log("[Unity] Received modes: " + string.Join(", ", modes));

                for (int i = 0; i < modes.Length && i < weaponHolder.equippedWeapons.Length; i++)
                {
                    weaponHolder.equippedWeapons[i].SetMode(modes[i]);
                }
            }

            yield return wait;
        }
    }

    void OnApplicationQuit()
    {
        running = false;
        stream?.Close();
        client?.Close();
    }
}
