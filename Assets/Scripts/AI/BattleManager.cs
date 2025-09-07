using UnityEngine;
using System.Net.Sockets;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;               
using System.Text; 

public class BattleManager : MonoBehaviour
{
    public NeuralWeaponController[] neuralControllers; // Array of 5
    public SimpleMovementRecorder movementRecorder;
    public PlayerHealth playerHealth;

    public float totalSessionTime = 50f;
    public float sliceDuration = 5f;

    private float sessionTimer = 0f;
    private float sliceTimer = 0f;
    private int currentNNIndex = 0;
    private bool sessionEnded = false;

    public static int currentRound = 1;
    public static int maxRounds = 2;

    public TMPro.TextMeshProUGUI waveText;  
    public TMPro.TextMeshProUGUI timerText;



    void Start()
    {
        movementRecorder.BeginRound(); // start logging player movement

        foreach (var controller in neuralControllers)
        {
            controller.Initialize();
            controller.ResetMetrics();
            controller.SetActive(false);
        }

        currentNNIndex = 0;
        neuralControllers[currentNNIndex].SetActive(true);
    }

    void Update()
    {
        if (sessionEnded) return;

        sessionTimer += Time.deltaTime;
        sliceTimer += Time.deltaTime;

        // Update Counter UI
        if (waveText != null)
            waveText.text = $"Wave {currentRound}";
        float remaining = Mathf.Max(0f, totalSessionTime - sessionTimer);
        if (timerText != null)
            timerText.text = $"{(int)remaining}s";

        Debug.Log($"[BattleManager] sessionTimer = {sessionTimer:F2} / {totalSessionTime}");

        if (playerHealth.CurrentHP <= 0)
        {
            EndSession();
            SceneManager.LoadScene("GameOverScene");
            return;
        }

        if (sessionTimer >= totalSessionTime)
        {
            if (currentRound % maxRounds == 0)
            {
                EndSessionOnRoundComplete(); // infinite mode
                SceneManager.LoadScene("SuccessScene"); // Upgrade scene
                currentRound++;
            }
            else
            {
                EndSessionOnRoundComplete();
                currentRound++;
                SceneManager.LoadScene("RoundTransitionScene"); // Transition with a button to reload BattleScene
            }
            return;
        }


        // Rotate NN every 5 seconds
        if (sliceTimer >= sliceDuration)
        {
            neuralControllers[currentNNIndex].SetActive(false);
            currentNNIndex = (currentNNIndex + 1) % neuralControllers.Length;
            neuralControllers[currentNNIndex].SetActive(true);
            sliceTimer = 0f;
        }
    }

    void EndSession()
    {
        sessionEnded = true;

        List<string> movementTokens = movementRecorder.EndRound();
        SendMovementDataToPython(movementTokens);

        foreach (var controller in neuralControllers)
        {
            controller.SetActive(false);
            controller.CloseConnection(); // 🔧 CLOSE INFERENCE CONNECTION HERE
        }
        SendShutdownSignalToPython();
        Debug.Log("Shutdown signal sent. No GA update.");
    }

    void EndSessionOnRoundComplete()
    {
        sessionEnded = true;
        
        List<string> movementTokens = movementRecorder.EndRound();
        SendMovementDataToPython(movementTokens);


        foreach (var controller in neuralControllers)
        {
            controller.SetActive(false);
            controller.CloseConnection(); // 🔧 CLOSE INFERENCE CONNECTION HERE
        }
        
        float[] allMetrics = new float[5 * 4]; // 5 NNs × 4 metrics
        for (int i = 0; i < 5; i++)
        {
            float[] m = neuralControllers[i].GetMetrics();
            for (int j = 0; j < 4; j++)
                allMetrics[i * 4 + j] = m[j];
        }

        float[] processed = ProcessAndFlattenMetrics();
        SendMetricsToPython(processed);
        Debug.Log("Processed metrics: [" + string.Join(", ", processed) + "]");
        Debug.Log("Round complete. Metrics sent to Python for GA update.");
    }

    float[] ProcessAndFlattenMetrics()
    {
        int numControllers = neuralControllers.Length;
        int numMetrics = 4;

        float[,] raw = new float[numControllers, numMetrics];

        // Collect raw metrics
        for (int i = 0; i < numControllers; i++)
        {
            float[] m = neuralControllers[i].GetMetrics();
            for (int j = 0; j < numMetrics; j++)
                raw[i, j] = m[j];
        }

        // Normalize each column
        float[] maxVals = new float[numMetrics];
        for (int j = 0; j < numMetrics; j++)
        {
            maxVals[j] = 1e-6f; // avoid div by zero
            for (int i = 0; i < numControllers; i++)
                if (raw[i, j] > maxVals[j])
                    maxVals[j] = raw[i, j];
        }

        // Optional: Apply weighting to prioritize certain metrics
        float[] weights = new float[] { 1.0f, 1.0f, 1.0f, 2.0f };  // kill count gets higher weight

        // Normalize and apply weights
        float[] flattened = new float[numControllers * numMetrics];
        for (int i = 0; i < numControllers; i++)
        {
            for (int j = 0; j < numMetrics; j++)
            {
                float normalized = raw[i, j] / maxVals[j];
                flattened[i * numMetrics + j] = normalized * weights[j];
            }
        }

        return flattened;
    }


    void SendMetricsToPython(float[] metrics)
    {
        try
        {
            byte[] data = new byte[metrics.Length * 4];
            for (int i = 0; i < metrics.Length; i++)
                BitConverter.GetBytes(metrics[i]).CopyTo(data, i * 4);

            using TcpClient client = new TcpClient("127.0.0.1", 65433); // Port for receiving metrics
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            Debug.Log("Metrics successfully sent to Python.");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to send metrics: " + e.Message);
        }
    }

    void SendMovementDataToPython(List<string> tokens)
    {
        try
        {
            using TcpClient client = new TcpClient("127.0.0.1", 65434);
            NetworkStream stream = client.GetStream();
            var enc = Encoding.UTF8;

            // 1) write count (int32)
            stream.Write(BitConverter.GetBytes(tokens.Count), 0, 4);

            // 2) for each token: [len:int32][bytes]
            foreach (var t in tokens)
            {
                byte[] b = enc.GetBytes(t ?? "");
                stream.Write(BitConverter.GetBytes(b.Length), 0, 4);
                stream.Write(b, 0, b.Length);
            }

            // compact log
            int total = tokens.Count;
            Debug.Log($"[BattleManager] Movement data ({total} total): {string.Join(" ", tokens)}");
            Debug.Log("Movement tokens successfully sent to Python (port 65434).");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to send movement tokens: " + e.Message);
        }
    }


    public void SendShutdownSignalToPython()
    {
        try
        {
            byte[] shutdownPacket = BitConverter.GetBytes(-1);

            // --- Shutdown nn_server (inference) ---
            using (TcpClient client = new TcpClient("127.0.0.1", 65432))
            {
                NetworkStream stream = client.GetStream();
                stream.Write(shutdownPacket, 0, shutdownPacket.Length);
            }
            Debug.Log("Sent shutdown signal to nn_server (inference).");

            // --- Shutdown ga_trainer (metrics) ---
            using (TcpClient client = new TcpClient("127.0.0.1", 65433))
            {
                NetworkStream stream = client.GetStream();
                stream.Write(shutdownPacket, 0, shutdownPacket.Length);
            }
            Debug.Log("Sent shutdown signal to ga_trainer (metrics).");

            // --- Shutdown movement_server ---
            using (TcpClient client = new TcpClient("127.0.0.1", 65434))
            {
                NetworkStream stream = client.GetStream();
                stream.Write(shutdownPacket, 0, shutdownPacket.Length);
            }
            Debug.Log("Sent shutdown signal to movement_server.");
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to send shutdown signal: " + e.Message);
        }
    }
}
