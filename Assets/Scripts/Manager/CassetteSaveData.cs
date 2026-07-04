using UnityEngine;

[System.Serializable]
public class CassetteSaveData
{
    public string CassetteName;
    
    // 1:OnFloor, 2:OnShelf, 3:OnInventory
    public int state;
    
    public Vector3  position;
    public Quaternion rotation;
    public Vector3 scale;

    public string shelfGenre;
    public int slotID;
    public int indexInSlot;
}