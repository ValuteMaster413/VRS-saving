using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    
[System.Serializable]
public class Shelf : MonoBehaviour
{
    public string shelfGenre;
    public List<ShelfSlot> slots;
    public GameObject cassettePrefab;
    public float thickness = 0.05f;
    
    private Color _genreMatchColor = Color.white; 
    private Color _perfectMatchColor = new Color(1f, 0.75f, 0f);
    
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
        GameObject newlySpawned = SpawnCassette(slotID, cassette);
        
        if (slots[GetSlotIndex(slotID)].expectedMovie.title == cassette.title)
        {
            StartCoroutine(FlashCassette(newlySpawned, _perfectMatchColor));
            return ("Cassette Is Matched");
        }
        
        if (shelfGenre == cassette.genre)
        {
            StartCoroutine(FlashCassette(newlySpawned, _genreMatchColor));
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

        CassetteData data = slots[index].spawnedCassettes[^1].GetComponent<PhysicalCassette>().cassetteData;
        Destroy(slots[index].spawnedCassettes[^1]);
        slots[index].spawnedCassettes.RemoveAt(slots[index].spawnedCassettes.Count - 1);
        
        return data;
    }

    public GameObject SpawnCassette(int slotID, CassetteData cassette)
    {
        int index = GetSlotIndex(slotID);

        Vector3 startPos = slots[index].slotTransform.position - (slots[index].slotTransform.right * 0.10f); 

        Vector3 spawnPos = startPos + (slots[index].slotTransform.right * ((slots[index].currentCount - 1) * thickness));
            
        GameObject addedCassette = Instantiate(cassettePrefab, spawnPos, slots[GetSlotIndex(slotID)].slotTransform.rotation);
        
        addedCassette.transform.Rotate(270f, 0f, 270f);
        
        addedCassette.GetComponent<PhysicalCassette>().isShelved = true;
        Rigidbody rb = addedCassette.GetComponent<Rigidbody>();
        
        if (rb != null) rb.isKinematic = true;
        
        slots[GetSlotIndex(slotID)].spawnedCassettes.Add(addedCassette);
        
        addedCassette.GetComponent<PhysicalCassette>().Init(cassette);
        
        PhysicalCassette pc = addedCassette.GetComponent<PhysicalCassette>();
        pc.shelfRef = this;
        pc.slotIDForShelf = slotID;
        return addedCassette;
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
    
    private IEnumerator FlashCassette(GameObject cassette, Color flashColor)
    {
        if (cassette == null) yield break;
        
        Outline outline = cassette.GetComponent<Outline>();
        
        if (outline == null) outline = cassette.GetComponentInChildren<Outline>();
        
        if (outline == null) yield break;

        Color originalColor = outline.OutlineColor;
        float originalWidth = outline.OutlineWidth;
        bool originalEnabled = outline.enabled;
        
        outline.OutlineColor = flashColor;
        outline.OutlineWidth = 6f;
        outline.enabled = true;
        
        yield return new WaitForSeconds(0.25f);
        
        float duration = 0.4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (outline == null) yield break;

            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            outline.OutlineColor = Color.Lerp(flashColor, new Color(flashColor.r, flashColor.g, flashColor.b, 0f), progress);
            outline.OutlineWidth = Mathf.Lerp(6f, 0f, progress);

            yield return null;
        }
        
        if (outline != null)
        {
            PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
            
            outline.OutlineColor = Color.white;
            outline.OutlineWidth = 2f;
            
            if (inventory != null && inventory.enabled && inventory.CurrentSeenOutline == outline)
            {
                outline.enabled = true;
            }
            else
            {
                outline.enabled = false; 
            }
        }
    }
}

