using UnityEngine;
using System.Net.Sockets;
using System;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public NeuralWeaponController[] neuralControllers; // Array of 5
    public PlayerHealth playerHealth;

    public float totalSessionTime = 25f;
    public float sliceDuration = 5f;

    private float sessionTimer = 0f;
    private float sliceTimer = 0f;
    private int currentNNIndex = 0;
    private bool sessionEnded = false;

    public static int currentRound = 1;
    public static int maxRounds = 5;


    void Start()
    {
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

        Debug.Log($"[BattleManager] sessionTimer = {sessionTimer:F2} / {totalSessionTime}");

        if (playerHealth.CurrentHP <= 0)
        {
            EndSession();
            SceneManager.LoadScene("GameOverScene");
            return;
        }

        if (sessionTimer >= totalSessionTime)
        {
            if (currentRound >= maxRounds)
            {
                EndSession();
                SceneManager.LoadScene("SuccessScene"); // Final success
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

    public void SendShutdownSignalToPython()
    {
        try
        {
            // --- Shutdown nn_server (inference) ---
            using (TcpClient client = new TcpClient("127.0.0.1", 65432))
            {
                NetworkStream stream = client.GetStream();
                byte[] shutdownPacket = BitConverter.GetBytes(-1);
                stream.Write(shutdownPacket, 0, shutdownPacket.Length);
            }
            Debug.Log("Sent shutdown signal to nn_server (inference).");

            // --- Shutdown ga_trainer (metrics) ---
            using (TcpClient client = new TcpClient("127.0.0.1", 65433))
            {
                NetworkStream stream = client.GetStream();
                byte[] shutdownPacket = BitConverter.GetBytes(-1);
                stream.Write(shutdownPacket, 0, shutdownPacket.Length);
            }
            Debug.Log("Sent shutdown signal to ga_trainer (metrics).");
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to send shutdown signal: " + e.Message);
        }
    }

}
