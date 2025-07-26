using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class StartMenuUI : MonoBehaviour
{
    public Button startButton;
    public Button quitButton;
    public Button helpButton;
    public TextMeshProUGUI helpText;

    void Start()
    {
        // Button listeners
        startButton.onClick.AddListener(OnStartClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
        helpButton.onClick.AddListener(OnHelpClicked);

        // Hide help text at start
        helpText.gameObject.SetActive(false);
    }

    void OnStartClicked()
    {
        // Replace with your weapon selection scene name
        SceneManager.LoadScene("WeaponSelectionScene");
    }

    void OnQuitClicked()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // For editor testing
        #endif
    }

    void OnHelpClicked()
    {
        helpText.gameObject.SetActive(!helpText.gameObject.activeSelf);
    }
}
