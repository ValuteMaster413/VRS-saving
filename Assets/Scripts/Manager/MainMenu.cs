using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button continueButton;

    [SerializeField] private string gameplaySceneName = "MainScene";

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (continueButton != null)
        {
            continueButton.interactable = SaveManager.HasSave();
        }
    }

    public void NewGame()
    {
        PlayerPrefs.SetInt("LoadGame", 0);
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void ContinueGame()
    {
        PlayerPrefs.SetInt("LoadGame", 1);
        SceneManager.LoadScene(gameplaySceneName);
    }
    
    public void ExitGame()
    {
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}