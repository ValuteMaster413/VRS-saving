using UnityEngine;

public class ShelfSlotCollider : MonoBehaviour
{
    public Shelf parentShelf;
    public int slotID;

    public void Awake()
    { 
        parentShelf = GetComponentInParent<Shelf>();
    }
}

