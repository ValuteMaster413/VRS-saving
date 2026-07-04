using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    public GameObject pauseMenuPanel; 

    public static bool IsPaused { get; private set; } = false;

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        IsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        IsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void QuitToMainMenu()
    {
        Time.timeScale = 1f; 
        IsPaused = false;
        
        SceneManager.LoadScene("MenuScene"); 
    }
    
    public void ClickSaveButton()
    {
        SaveData save = FindObjectOfType<PlayerInventory>().SaveCreation();
        
        string result = SaveManager.Save(save);
        
        Debug.Log(result);
    }
}