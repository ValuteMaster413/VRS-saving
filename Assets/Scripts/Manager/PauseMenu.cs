using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseController : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public GameObject quitConfirmationPanel;
    public TextMeshProUGUI quitWarningText;
    public GameObject settingsPanelUi;

    public static bool IsPaused { get; private set; } = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PlayerInventory.Instance != null && PlayerInventory.Instance.IsTutorialActive())
            {
                PlayerInventory.Instance.CloseActiveTutorialSlide();
                return;
            }
            
            if (settingsPanelUi != null && settingsPanelUi.activeSelf)
            {
                settingsPanelUi.SetActive(false);
                return;
            }
            
            if (IsPaused)
            {
                if (quitConfirmationPanel != null && quitConfirmationPanel.activeSelf)
                {
                    CancelQuit();
                }
                else
                {
                    ResumeGame();
                }
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        if (quitConfirmationPanel != null) 
            quitConfirmationPanel.SetActive(false);

        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        IsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Settings()
    {
        settingsPanelUi.SetActive(true);
    }

    void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        IsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void OpenQuitConfirmation()
    {
        int minutes = FindObjectOfType<PlayerInventory>().GetMinutesSinceLastSave();
        
        
        if (quitWarningText != null)
        {
            if (minutes <= 0)
            {
                quitWarningText.text = "You just saved.\nQuit?";
            }
            else
            {
                quitWarningText.text = $"Last save was {minutes} minutes ago.\nAre you sure?";
            }
        }

        if (quitConfirmationPanel != null)
        {
            quitConfirmationPanel.SetActive(true);
        }
    }
    
    public void SaveAndQuit()
    {
        FindObjectOfType<PlayerInventory>().PerformSave();
        ConfirmQuit();
    }
    
    public void ConfirmQuit()
    {
        Time.timeScale = 1f; 
        IsPaused = false;
        
        SceneManager.LoadScene("MenuScene"); 
    }
    
    public void CancelQuit()
    {
        if (quitConfirmationPanel != null)
        {
            quitConfirmationPanel.SetActive(false);
        }
    }

    public void ClickSaveButton()
    {
        SaveData save = FindObjectOfType<PlayerInventory>().SaveCreation();
        
        string result = SaveManager.Save(save);
        
        Debug.Log(result);
    }
}