using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillHotbarUI : MonoBehaviour
{
    [System.Serializable]
    public class SkillSlot
    {
        public string skillName;
        public Image cooldownOverlay;      // Слой затененной заливки (Image Type: Filled -> Radial 360)
        public TextMeshProUGUI cooldownText; // Текст секунд
        public Image skillIcon;            // Сама иконка (опционально, для прозрачности)
    }

    [Header("Skill Slots Config")]
    public SkillSlot shelfGuideSlot;  // Навык 1
    public SkillSlot insightSlot;     // Навык 2
    public SkillSlot autoShelvingSlot; // Навык 3
    public SkillSlot assembleSlot;    // Навык 4

    private void Update()
    {
        if (PlayerInventory.Instance == null) return;

        var player = PlayerInventory.Instance;

        // Обновляем состояния всех 4 скиллов
        UpdateSlotUI(
            shelfGuideSlot, 
            player.GetShelfGuideLvl(), 
            player.GetShelfGuideCooldownTimer(), 
            player.GetShelfGuideMaxCooldown()
        );

        UpdateSlotUI(
            insightSlot, 
            player.GetInsightLvl(), 
            player.GetInsightCooldownTimer(), 
            player.GetInsightMaxCooldown()
        );

        UpdateSlotUI(
            autoShelvingSlot, 
            player.GetAutoShelvingLvl(), 
            player.GetAutoShelvingCooldownTimer(), 
            player.GetAutoShelvingMaxCooldown()
        );

        UpdateSlotUI(
            assembleSlot, 
            player.GetAssembleLvl(), 
            player.GetAssembleCooldownTimer(), 
            player.GetAssembleMaxCooldown()
        );
    }

    private void UpdateSlotUI(SkillSlot slot, int skillLevel, float currentCD, float maxCD)
    {
        if (slot == null) return;

        // 1. Если навык еще не вкачан (Уровень 0)
        if (skillLevel <= 0)
        {
            if (slot.cooldownOverlay != null)
            {
                slot.cooldownOverlay.gameObject.SetActive(true);
                slot.cooldownOverlay.fillAmount = 1f; // Полностью закрыт
            }

            if (slot.cooldownText != null)
            {
                slot.cooldownText.text = "LOCKED";
            }

            if (slot.skillIcon != null)
            {
                slot.skillIcon.color = new Color(0.3f, 0.3f, 0.3f, 0.5f); // Иконка тусклая
            }
            return;
        }

        // 2. Навык вкачан и находится на Перезарядке (Cooldown)
        if (currentCD > 0f && maxCD > 0f)
        {
            if (slot.cooldownOverlay != null)
            {
                slot.cooldownOverlay.gameObject.SetActive(true);
                slot.cooldownOverlay.fillAmount = currentCD / maxCD; // Процент заполнения круга
            }

            if (slot.cooldownText != null)
            {
                slot.cooldownText.text = currentCD > 1f ? $"{currentCD:F0}s" : $"{currentCD:F1}s";
            }

            if (slot.skillIcon != null)
            {
                slot.skillIcon.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
            }
        }
        // 3. Навык Готов к использованию
        else
        {
            if (slot.cooldownOverlay != null)
            {
                slot.cooldownOverlay.gameObject.SetActive(false);
                slot.cooldownOverlay.fillAmount = 0f;
            }

            if (slot.cooldownText != null)
            {
                slot.cooldownText.text = ""; // Пусто, скилл готов!
            }

            if (slot.skillIcon != null)
            {
                slot.skillIcon.color = Color.white; // Яркая иконка
            }
        }
    }
}