using UnityEngine;
using System.Net.Sockets;
using System;

public class NeuralWeaponController : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;

    private int[] currentModes = new int[3];
    private float[] metrics = new float[4]; // [time, dmgDealt, dmgTaken, kills]

    [Header("References")]
    public PlayerHealth playerHealth;
    public EnemySpawner enemySpawner;
    public WeaponHolder weaponHolder;
    public BattleInputBuilder inputBuilder;

    [Header("Model Settings")]
    public int nnIndex = 0;  // This controller will use NN #0 by default


    private bool isActive = false;

    private int lastHP = 0;
    private int lastKills = 0;

    public void Initialize()
    {
        client = new TcpClient("127.0.0.1", 65432);
        stream = client.GetStream();
    }

    public void ResetMetrics()
    {
        for (int i = 0; i < metrics.Length; i++)
            metrics[i] = 0f;

        lastHP = playerHealth.CurrentHP;
        lastKills = enemySpawner.TotalEnemiesKilled;
    }

    public void SetActive(bool active)
    {
        isActive = active;

        if (active)
        {
            lastHP = playerHealth.CurrentHP;
            lastKills = enemySpawner.TotalEnemiesKilled;
        }
    }

    void Update()
    {
        if (!isActive || client == null || !client.Connected) return;

        float[] input = BuildInputVector();
        byte[] inputBytes = FloatArrayToBytes(input);
        byte[] packet = new byte[4 + inputBytes.Length];

        // Add nnIndex as the first 4 bytes
        BitConverter.GetBytes(nnIndex).CopyTo(packet, 0);
        // Copy the input vector bytes after the index
        inputBytes.CopyTo(packet, 4);

        stream.Write(packet, 0, packet.Length);

        // Receive mode response
        byte[] response = new byte[3 * 4];
        int totalRead = 0;
        while (totalRead < 12)
        {
            int read = stream.Read(response, totalRead, 12 - totalRead);
            if (read <= 0) break;
            totalRead += read;
        }

        if (totalRead == 12)
        {
            for (int i = 0; i < 3; i++)
                currentModes[i] = BitConverter.ToInt32(response, i * 4);
                
            Debug.Log("CurrentModes: [" + string.Join(", ", currentModes) + "]");
            ApplyModes(currentModes);
        }

        AccumulateMetrics();
        Debug.Log($"[Unity] NN {nnIndex} metrics: [{string.Join(", ", metrics)}]");
    }


    void ApplyModes(int[] modes)
    {
        for (int i = 0; i < modes.Length && i < weaponHolder.equippedWeapons.Length; i++)
        {
            weaponHolder.equippedWeapons[i].SetMode(modes[i]);
        }
    }

    void AccumulateMetrics()
    {
        metrics[0] += Time.deltaTime;

        foreach (var enemy in enemySpawner.activeEnemies)
            metrics[1] += enemy.ConsumePlayerDamageThisFrame();
        metrics[1] += enemySpawner.ConsumePendingDamage();

        int currentHP = playerHealth.CurrentHP;
        if (currentHP < lastHP)
        {
            metrics[2] += lastHP - currentHP;
        }
        lastHP = currentHP;

        int currentKills = enemySpawner.TotalEnemiesKilled;
        if (currentKills > lastKills)
        {
            metrics[3] += currentKills - lastKills;
        }
        lastKills = currentKills;
    }

    public void CloseConnection()
    {
        try
        {
            if (client != null && client.Connected)
            {
                byte[] shutdownPacket = new byte[4 + 42 * 4]; // same as regular input size
                stream.Write(shutdownPacket, 0, shutdownPacket.Length);
                Debug.Log($"[Unity] Sent shutdown signal for NN {nnIndex}");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Error sending shutdown signal: " + e.Message);
        }
        finally
        {
            stream?.Close();
            client?.Close();
        }
    }

    public float[] GetMetrics()
    {
        return metrics;
    }

    private float[] BuildInputVector()
    {
        return inputBuilder.BuildInputVector();
    }

    private byte[] FloatArrayToBytes(float[] arr)
    {
        byte[] result = new byte[arr.Length * 4];
        for (int i = 0; i < arr.Length; i++)
            BitConverter.GetBytes(arr[i]).CopyTo(result, i * 4);
        return result;
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }
}
