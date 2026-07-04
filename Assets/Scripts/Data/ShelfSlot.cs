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
    public List<GameObject> spawnedCassettes = new List<GameObject>();

    public bool IsRightFilled()
    {
        int cassetteCount = 0;
            
        foreach (var cas in spawnedCassettes)
        {
            if (cas.GetComponent<PhysicalCassette>().cassetteData == expectedMovie)
            {
                cassetteCount++;
            }
        }

        if (cassetteCount == countOfMovies)
        {
            return true;
        }
        else 
        {
            return false;
        }
    }
}


