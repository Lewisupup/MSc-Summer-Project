using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SuccessUI : MonoBehaviour
{
    void Start()
    {
        Button backButton = GameObject.Find("BackToMenuButton")?.GetComponent<Button>();

        if (backButton != null)
            backButton.onClick.AddListener(OnBackToMenuButton);
    }

    public void OnBackToMenuButton()
    {
        SceneManager.LoadScene("StartScene");
    }
}
