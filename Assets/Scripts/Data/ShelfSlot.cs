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
    public Transform slotTransform;
    public bool isExpAwarded = false;
    public List<GameObject> spawnedCassettes = new List<GameObject>();

    public bool IsRightFilled()
    {
        int cassetteCount = 0;
            
        foreach (var cas in spawnedCassettes)
        {
            if (cas != null && cas.GetComponent<PhysicalCassette>().cassetteData == expectedMovie)
            {
                cassetteCount++;
            }
        }

        return cassetteCount == countOfMovies;
    }

    public bool CheckAndAward()
    {
        if (IsRightFilled() && !isExpAwarded)
        {
            isExpAwarded = true;
            return true;
        }

        return false;
    }
    
    public void RestoreAwardedStatusOnLoad()
    {
        if (IsRightFilled())
        {
            isExpAwarded = true;
        }
    }
}


