using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public List<string> cassettesInHands;
    
    public List<CassetteSaveData> cassettesInWorld;
    
    public int completedShelves;
    public int totalSkillPointsEarned;
    public int skillCounter;
    
    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;
    
    public float playerRotY;

    public int maxCarryCount;
    public int shelfGuideLvl;
    public int insightLvl;
    public int autoShelvingLvl;
    public int assembleLvl;
    
    public bool showTutorial;
    public bool hasShownFirstTutorialSlide = false;
    public bool hasShownPickUptTutorialSlide = false;
    public bool hasShownPlaceTutorialSlide = false;
}
