using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillMenuUI : MonoBehaviour
{
    [Header("Menu Key & Main Panel")]
    public KeyCode toggleMenuKey = KeyCode.Tab;
    public GameObject skillMenuPanel;

    [Header("Skill Points Text")]
    public TextMeshProUGUI availableSPText;

    [Header("Inventory Skill")]
    public TextMeshProUGUI inventoryLvlText;
    public Button inventoryUpgradeBtn;

    [Header("Shelf Guide Skill")]
    public TextMeshProUGUI shelfGuideLvlText;
    public Button shelfGuideUpgradeBtn;

    [Header("Insight Skill")]
    public TextMeshProUGUI insightLvlText;
    public Button insightUpgradeBtn;

    [Header("Auto Shelving Skill")]
    public TextMeshProUGUI autoShelvingLvlText;
    public Button autoShelvingUpgradeBtn;

    [Header("Assemble Skill")]
    public TextMeshProUGUI assembleLvlText;
    public Button assembleUpgradeBtn;
    
    public bool IsMenuOpen => skillMenuPanel != null && skillMenuPanel.activeSelf;

    private void Start()
    {
        if (inventoryUpgradeBtn) inventoryUpgradeBtn.onClick.AddListener(OnUpgradeInventory);
        if (shelfGuideUpgradeBtn) shelfGuideUpgradeBtn.onClick.AddListener(OnUpgradeShelfGuide);
        if (insightUpgradeBtn) insightUpgradeBtn.onClick.AddListener(OnUpgradeInsight);
        if (autoShelvingUpgradeBtn) autoShelvingUpgradeBtn.onClick.AddListener(OnUpgradeAutoShelving);
        if (assembleUpgradeBtn) assembleUpgradeBtn.onClick.AddListener(OnUpgradeAssemble);
        
        if (skillMenuPanel != null)
        {
            skillMenuPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (PlayerInventory.Instance != null && PlayerInventory.Instance.IsTutorialActive()) 
            return;

        if (Input.GetKeyDown(toggleMenuKey))
        {
            ToggleMenu();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape) && IsMenuOpen)
        {
            CloseMenu();
        }
    }

    public void ToggleMenu()
    {
        if (skillMenuPanel == null) return;

        bool isOpening = !skillMenuPanel.activeSelf;
        skillMenuPanel.SetActive(isOpening);
        
        Time.timeScale = isOpening ? 0f : 1f;
        Cursor.lockState = isOpening ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpening;

        if (isOpening)
        {
            UpdateUI();
        }
    }
    
    public void CloseMenu()
    {
        if (skillMenuPanel == null) return;

        skillMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UpdateUI()
    {
        if (Time.timeScale == 0f && !IsMenuOpen)
            return;
        
        if (PlayerInventory.Instance != null && PlayerInventory.Instance.IsTutorialActive()) 
            return;

        var inv = PlayerInventory.Instance;
        
        if (availableSPText != null) 
            availableSPText.text = $"Skill Points: {inv.GetSkillCounter()}";

        if (inventoryLvlText != null) 
            inventoryLvlText.text = $"Max Inventory: {inv.GetCurrentCarryCount()}/10";
        
        if (inventoryUpgradeBtn != null) 
            inventoryUpgradeBtn.interactable = (inv.GetSkillCounter() > 0 && inv.GetCurrentCarryCount() < 10);
        
        if (shelfGuideLvlText != null) 
            shelfGuideLvlText.text = $"Shelf GuideLvl: {inv.GetShelfGuideLvl()}";
        if (shelfGuideUpgradeBtn != null) 
            shelfGuideUpgradeBtn.interactable = (inv.GetSkillCounter() > 0 && inv.GetShelfGuideLvl() < 5);
        
        if (insightLvlText != null) 
            insightLvlText.text = $"Insight Lvl: {inv.GetInsightLvl()}";
        if (insightUpgradeBtn != null) 
            insightUpgradeBtn.interactable = (inv.GetSkillCounter() > 0 && inv.GetInsightLvl() < 5);
        
        if (autoShelvingLvlText != null) 
            autoShelvingLvlText.text = $"Auto ShelvingLvl: {inv.GetAutoShelvingLvl()}";
        if (autoShelvingUpgradeBtn != null) 
            autoShelvingUpgradeBtn.interactable = (inv.GetSkillCounter() > 0 && inv.GetAutoShelvingLvl() < 5);
        
        if (assembleLvlText != null) 
            assembleLvlText.text = $"Assemble Lvl: {inv.GetAssembleLvl()}";
        if (assembleUpgradeBtn != null) 
            assembleUpgradeBtn.interactable = (inv.GetSkillCounter() > 0 && inv.GetAssembleLvl() < 5);
    }

    #region Button Callbacks
    private void OnUpgradeInventory()
    {
        if (PlayerInventory.Instance.UpgradeInventory()) UpdateUI();
    }

    private void OnUpgradeShelfGuide()
    {
        if (PlayerInventory.Instance.ShelfGuideSkillUpgrade()) UpdateUI();
    }

    private void OnUpgradeInsight()
    {
        if (PlayerInventory.Instance.InsightSkillUpgrade()) UpdateUI();
    }

    private void OnUpgradeAutoShelving()
    {
        if (PlayerInventory.Instance.AutoShelvingSkillUpgrade()) UpdateUI();
    }

    private void OnUpgradeAssemble()
    {
        if (PlayerInventory.Instance.AssembleSkillUpgrade()) UpdateUI();
    }
    #endregion
}