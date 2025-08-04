using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System.IO;
using TMPro;

public class WeaponUpgradeManager : MonoBehaviour
{
    public TMP_InputField modeInputField;
    public TMP_InputField descriptionInputField;
    public TMP_Text statusText;
    public RadialBurst radialBurstWeapon;

    private string pythonExePath = "python";
    private string scriptPath = @"C:\Users\jiahui li\Summer Project draft\LLM\generate_weapon.py";

    public void OnUpgradeWeaponClicked()
    {
        string mode = modeInputField.text.Trim();
        string description = descriptionInputField.text.Trim();

        if (string.IsNullOrEmpty(mode) || string.IsNullOrEmpty(description))
        {
            statusText.text = "❌ Please enter both mode and description.";
            return;
        }

        // Prepare Python command
        string args = $"\"{scriptPath}\" --mode {mode} --desc \"{description}\"";
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = pythonExePath,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Process process = new Process { StartInfo = psi };

        try
        {
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                statusText.text = "✅ Weapon upgraded! Loading updated config...";
                UnityEngine.Debug.Log(output);

                // 🔁 Load updated config and switch scene
                if (radialBurstWeapon != null)
                {
                    radialBurstWeapon.LoadConfig();
                    UnityEngine.Debug.Log("✅ Radial Burst config loaded successfully!");
                }
                else
                {
                    UnityEngine.Debug.LogWarning("⚠️ RadialBurst weapon not assigned.");
                }

                SceneManager.LoadScene("BattleScene");
            }
            else
            {
                statusText.text = "❌ Python script failed.";
                UnityEngine.Debug.LogError(error);
            }
        }
        catch (System.Exception ex)
        {
            statusText.text = "❌ Failed to run Python script.";
            UnityEngine.Debug.LogError(ex.Message);
        }
    }

    void Start()
    {
        Button upgradeButton = GameObject.Find("UpgradeWeaponButton")?.GetComponent<Button>();
        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(OnUpgradeWeaponClicked);
        }
        else
        {
            UnityEngine.Debug.LogWarning("⚠️ UpgradeWeaponButton not found in scene.");
        }
    }

}
