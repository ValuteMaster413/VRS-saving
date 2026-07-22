using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Slider volumeSlider;
    public Toggle tutorialToggle;

    private List<Resolution> _filteredResolutions = new List<Resolution>();
    private bool _isInitializing = true;

    void Start()
    {
        _isInitializing = true;
        
        Resolution[] allResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < allResolutions.Length; i++)
        {
            if (!_filteredResolutions.Exists(r => r.width == allResolutions[i].width && r.height == allResolutions[i].height))
            {
                _filteredResolutions.Add(allResolutions[i]);
                string option = allResolutions[i].width + " x " + allResolutions[i].height;
                options.Add(option);

                if (allResolutions[i].width == Screen.currentResolution.width &&
                    allResolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = _filteredResolutions.Count - 1;
                }
            }
        }

        resolutionDropdown.AddOptions(options);

        int savedResIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        
        if (savedResIndex >= _filteredResolutions.Count) savedResIndex = currentResolutionIndex;

        resolutionDropdown.value = savedResIndex;
        resolutionDropdown.RefreshShownValue();
        
        bool isFullscreen = PlayerPrefs.GetInt("IsFullscreen", 1) == 1;
        fullscreenToggle.isOn = isFullscreen;
        
        ApplyScreenMode(isFullscreen, savedResIndex);

        // 3. Звук
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume;

        // 4. Обучение
        bool showTutorial = PlayerPrefs.GetInt("ShowTutorial", 1) == 1;
        tutorialToggle.isOn = showTutorial;
        
        _isInitializing = false;
    }

    public void SetResolution(int resolutionIndex)
    {
        if (_isInitializing) return;

        if (resolutionIndex >= 0 && resolutionIndex < _filteredResolutions.Count)
        {
            PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
            ApplyScreenMode(fullscreenToggle.isOn, resolutionIndex);
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        if (_isInitializing) return;

        PlayerPrefs.SetInt("IsFullscreen", isFullscreen ? 1 : 0);
        int currentResIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
        ApplyScreenMode(isFullscreen, currentResIndex);
    }

    private void ApplyScreenMode(bool isFullscreen, int resIndex)
    {
        if (resIndex >= 0 && resIndex < _filteredResolutions.Count)
        {
            Resolution res = _filteredResolutions[resIndex];
            
            FullScreenMode mode = isFullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;
            Screen.SetResolution(res.width, res.height, mode);
        }
    }

    public void SetVolume(float volume)
    {
        if (_isInitializing) return;
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetTutorialEnabled(bool enabled)
    {
        if (_isInitializing) return;
        PlayerPrefs.SetInt("ShowTutorial", enabled ? 1 : 0);
    }

    public void CloseSettings()
    {
        gameObject.SetActive(false);
    }
}