using TMPro;
using UnityEngine;

public class DeskReader : MonoBehaviour
{
    private CassetteData _currentCassette;
    private GameObject _placedCassetteObject;
    
    public GameObject cassettePrefab;
    
    public Transform spawnPoint;
    
    public TextMeshProUGUI cassetteName;
    public TextMeshProUGUI cassetteShelf;
    public TextMeshProUGUI cassetteSlot;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if ((_currentCassette != null) && (_placedCassetteObject == null))
        {
            TakeCassette();
        }
    }

    public bool PlaceCassette(CassetteData data)
    {
        if (_currentCassette != null)
        {
            return false;
        }
        else
        {
            _currentCassette = data;
            
            cassetteName.text = _currentCassette.name; 
            cassetteShelf.text = _currentCassette.genre;
            cassetteSlot.text = _currentCassette.position;
            
            _placedCassetteObject = Instantiate(cassettePrefab,spawnPoint.position,spawnPoint.rotation);
        
            _placedCassetteObject.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
        
            _placedCassetteObject.GetComponent<PhysicalCassette>();
            Rigidbody rb = _placedCassetteObject.GetComponent<Rigidbody>();
        
            if (rb != null) rb.isKinematic = true;
            
            _placedCassetteObject.GetComponent<PhysicalCassette>().Init(_currentCassette);
            
            return true;
        }
    }

    public CassetteData TakeCassette()
    {
        CassetteData tempCassette = _currentCassette;
        
        cassetteName.text = " ";
        cassetteShelf.text = " ";
        cassetteSlot.text = " ";

        _currentCassette = null;
        
        return tempCassette;
    }
}
