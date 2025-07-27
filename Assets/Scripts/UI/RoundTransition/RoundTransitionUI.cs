using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Net.Sockets;

public class RoundTransitionUI : MonoBehaviour
{
    void Start()
    {
        // Automatically find buttons in the scene
        Button nextRoundButton = GameObject.Find("NextRoundButton")?.GetComponent<Button>();
        Button exitButton = GameObject.Find("ExitButton")?.GetComponent<Button>();

        if (nextRoundButton != null)
            nextRoundButton.onClick.AddListener(OnNextRoundButton);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitButton);
    }

    public void OnNextRoundButton()
    {
        SceneManager.LoadScene("BattleScene"); // Reload the battle scene
    }

    public void OnExitButton()
    {
        SendShutdownSignalToPython(); // 🔴 Shutdown Python servers
        SceneManager.LoadScene("StartScene"); // Go back to start menu
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
