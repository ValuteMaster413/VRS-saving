using UnityEngine;

public class PhysicalCassette : MonoBehaviour
{
    public CassetteData cassetteData;
    public SpriteRenderer coverImageRenderer;
    public bool isShelved = false;
    public Shelf shelfRef;
    public int slotIDForShelf;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (cassetteData != null)
        {
            Init(cassetteData);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void Init(CassetteData data)
    {
        cassetteData = data;
        
        if (coverImageRenderer != null && cassetteData.coverArt != null)
        {
            coverImageRenderer.sprite = cassetteData.coverArt;
        }
    }
}
