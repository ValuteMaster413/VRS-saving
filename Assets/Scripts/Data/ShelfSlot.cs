using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShelfSlot
{
    public int slotID;
    public CassetteData expectedMovie;
    public int countOfMovies;
    public int currentCount = 0;
    public List<CassetteData> storedCassettes = new List<CassetteData>();
    public Transform slotTransform;
    public List<GameObject> spawnedCassettes = new List<GameObject>();
}
