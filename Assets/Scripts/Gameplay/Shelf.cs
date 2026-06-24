using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Shelf : MonoBehaviour
{
    public string shelfGenre;
    public List<ShelfSlot> slots;
    public GameObject cassettePrefab;
    public float thickness = 0.05f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string CassetteIsPlaced(int slotID, CassetteData cassette)
    {
        if (slots[GetSlotIndex(slotID)].currentCount >= slots[GetSlotIndex(slotID)].countOfMovies)
        {
            return ("Slot Is Full");
        }
        
        slots[GetSlotIndex(slotID)].currentCount += 1;
        SpawnCassette(slotID, cassette);
        
        if (slots[GetSlotIndex(slotID)].expectedMovie.title == cassette.title)
        {
            return ("Cassette Is Matched");
        }
        
        if (shelfGenre == cassette.genre)
        {
            return ("Genre Is Matched");
        }
        
        return ("No Match");
    }
    
    public CassetteData CassetteIsTaken(int slotID)
    {
        int index = GetSlotIndex(slotID);
        
        if (slots[GetSlotIndex(slotID)].currentCount == 0)
        {
            return null;
        }

        slots[index].currentCount -= 1;

        Destroy(slots[index].spawnedCassettes[^1]);
        slots[index].spawnedCassettes.RemoveAt(slots[index].spawnedCassettes.Count - 1);
        
        return slots[GetSlotIndex(slotID)].expectedMovie;
    }

    public void SpawnCassette(int slotID, CassetteData cassette)
    {
        Vector3 spawnPos = slots[GetSlotIndex(slotID)].slotTransform.position + (slots[GetSlotIndex(slotID)].slotTransform.right * (slots[GetSlotIndex(slotID)].currentCount - 1) * thickness);
            
        GameObject addedCassette = Instantiate(cassettePrefab,spawnPos ,slots[GetSlotIndex(slotID)].slotTransform.rotation);
        
        addedCassette.transform.Rotate(270f, 0f, 270f);
        
        addedCassette.GetComponent<PhysicalCassette>().isShelved = true;
        Rigidbody rb = addedCassette.GetComponent<Rigidbody>();
        
        if (rb != null) rb.isKinematic = true;
        
        slots[GetSlotIndex(slotID)].spawnedCassettes.Add(addedCassette);
        
        addedCassette.GetComponent<PhysicalCassette>().Init(cassette);
        
        PhysicalCassette pc = addedCassette.GetComponent<PhysicalCassette>();
        pc.shelfRef = this;
        pc.slotIDForShelf = slotID;
    }

    public int GetSlotIndex(int slotID)
    {
        int index = 0;
        
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].slotID == slotID)
            {
                index = i;
            } 
        }
        
        return index;
    }
}
