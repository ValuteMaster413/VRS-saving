using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{ 
    private List<CassetteData> _cassetteStack = new List<CassetteData>();
    private List<GameObject> _visualBoxes = new List<GameObject>();
    public Transform handHoldPoint;
    public GameObject cassettePrefab;
    
    public int maxCarryCount = 10;
    public float throwForce = 5f;
    public float thickness = 0.05f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 3f, LayerMask.GetMask("Cassette")))
            {
                PhysicalCassette physicalCassette = hit.collider.GetComponent<PhysicalCassette>();
        
                if (physicalCassette != null)
                {
                    if (_cassetteStack.Count < maxCarryCount)
                    {
                        if (physicalCassette.isShelved)
                        {
                            CassetteData takenData = physicalCassette.shelfRef.CassetteIsTaken(physicalCassette.slotIDForShelf);
                            if (takenData != null)
                            {
                                _cassetteStack.Add(takenData);
                                RefreshHandVisuals();
                            }
                        }
                        else
                        {
                            PickUpCassette(hit.collider.gameObject);
                        }
                    }
                }
            }
            else if (Physics.Raycast(ray, out hit, 3f, LayerMask.GetMask("ShelfSlot")))
            {
                if (_cassetteStack.Count < maxCarryCount)
                {
                    if (_cassetteStack.Count > 0)
                    {
                        ShelfSlotCollider slotCollider = hit.collider.GetComponent<ShelfSlotCollider>();

                        CassetteData topCassette = _cassetteStack[^1];
                        
                        string result = slotCollider.parentShelf.CassetteIsPlaced(slotCollider.slotID, topCassette);

                        if (result != null && result != "Slot Is Full")
                        {
                            _cassetteStack.RemoveAt(_cassetteStack.Count - 1);
                            RefreshHandVisuals();
                        }
                    }
                }
                
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DropCassette();
        }
    }

    public void PickUpCassette(GameObject cassette)
    {
        CassetteData data = cassette.GetComponent<PhysicalCassette>().cassetteData;
        _cassetteStack.Add(data);
        
        Destroy(cassette);
        
        RefreshHandVisuals();
    }
    
    public void DropCassette()
    {
        if (_cassetteStack.Count == 0)
        {
            return;
        }
        
        GameObject spawnedCassette = Instantiate(cassettePrefab, handHoldPoint.position, handHoldPoint.rotation);
        spawnedCassette.GetComponent<PhysicalCassette>().Init(_cassetteStack[^1]);
        
        Rigidbody rb = spawnedCassette.GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        }
        
        _cassetteStack.RemoveAt(_cassetteStack.Count - 1);
        
        RefreshHandVisuals();
    }

    public void RefreshHandVisuals()
    {
        for (int i = 0; i < _visualBoxes.Count; i++)
        {
            Destroy( _visualBoxes[i]);
        }
        
        _visualBoxes.Clear();
        
        for (int i = 0; i < _cassetteStack.Count; i++)
        {
            Vector3 position = handHoldPoint.position + (handHoldPoint.up * (i * thickness));
            GameObject newBox = Instantiate(cassettePrefab, position, handHoldPoint.rotation);
            
            newBox.transform.SetParent(handHoldPoint);
            
            newBox.GetComponent<PhysicalCassette>().Init(_cassetteStack[i]);
            
            Rigidbody rb = newBox.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            
            Collider col = newBox.GetComponent<Collider>();
            if (col != null) col.enabled = false;
            
            _visualBoxes.Add(newBox);
        }
    }
}
