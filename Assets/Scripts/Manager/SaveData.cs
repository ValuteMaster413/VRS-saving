using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public List<string> cassettesInHands;
    
    public List<CassetteSaveData> cassettesInWorld;
    
    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;
    
    public float playerRotY;
}
