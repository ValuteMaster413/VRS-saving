using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInventory : MonoBehaviour
{ 
    public static PlayerInventory Instance { get; private set; }
    
    private List<CassetteData> _cassetteStack = new List<CassetteData>();
    private List<GameObject> _visualBoxes = new List<GameObject>();
    
    private Outline _lastSeenOutline;
    public Outline CurrentSeenOutline => _lastSeenOutline;
    
    private GameObject _cassettePreview;
    
    public Transform handHoldPoint;
    public GameObject cassettePrefab;
    public Material previewMaterial;
    
    public TextMeshProUGUI cassetteNameText;
    public TextMeshProUGUI cassetteHandedText;
    public TextMeshProUGUI amountCassettesInHandsText;
    public TextMeshProUGUI filledShelvesText;
    public TextMeshProUGUI placedCassettesText;
    
    public int maxCarryCount = 10;
    public float throwForce = 5f;
    public float thickness = 0.05f;
    
    private Shelf[] _allShelves;
    private PhysicalCassette[] _allCassettes;
    private int _totalSlots = 0;

    private int _totalCassettesInWorld = 0;
    private int _placedCassettes = 0;

    private float _lastSaveTime;
    public float autoSaveInterval = 900f;
    
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip pickUpSound;
    public AudioClip placeSound;
    public AudioClip dropSound;

    [Header("Tutorial Slides")] 
    public GameObject firstTutorialSlide;
    public GameObject pickUpTutorialSlide;
    public GameObject placeTutorialSlide;

    private bool _showTutorial;
    
    private bool _hasShownFirstTutorialSlide = false;
    private bool _hasShownPickUptTutorialSlide = false;
    private bool _hasShownPlaceTutorialSlide = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (PlayerPrefs.GetInt("LoadGame", 0) == 1)
        {
            LoadSavedGame();
        }
        
        _allShelves = FindObjectsOfType<Shelf>();
        _totalSlots = _allShelves.Length * 25;
        
        _allCassettes = FindObjectsOfType<PhysicalCassette>();
        _totalCassettesInWorld = _allCassettes.Length;
        
        _placedCassettes = 0;
        foreach(var cas in _allCassettes)
        {
            if(cas != null && cas.isShelved) _placedCassettes++;
        }
        
        InitPreviewAndUI();
        
        _lastSaveTime = Time.time;
        StartCoroutine(AutoSaveCoroutine());
        
        _showTutorial = PlayerPrefs.GetInt("ShowTutorial", 1) == 1;
        
        if (_showTutorial && firstTutorialSlide != null && !_hasShownFirstTutorialSlide)
        {
            _hasShownFirstTutorialSlide = true;
            firstTutorialSlide.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public int GetMinutesSinceLastSave()
    {
        return Mathf.FloorToInt((Time.time - _lastSaveTime) / 60f);
    }
    
    public void PerformSave()
    {
        SaveData data = SaveCreation();
        SaveManager.Save(data);
        _lastSaveTime = Time.time;
        Debug.Log("Game Saved Successfully!");
    }
    
    private IEnumerator AutoSaveCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            PerformSave();
        }
    }

    private void InitPreviewAndUI()
    {
        if (_cassettePreview != null) return;

        _cassettePreview = Instantiate(cassettePrefab);
        Rigidbody rb = _cassettePreview.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
            
        Collider col = _cassettePreview.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        _cassettePreview.GetComponentInChildren<MeshRenderer>().material = previewMaterial;
        _cassettePreview.transform.GetChild(0).gameObject.SetActive(false);
        
        if (_cassettePreview.TryGetComponent<PhysicalCassette>(out var pc)) 
        {
            Destroy(pc); 
        }

        UpdateCassetteCounterInterface();
        UpdateShelvesCounterInterface();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0f) return;
        
        SeenObject();
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DropCassette();
        }

        HandleScrollInventory();
    }

    private bool IsLineOfClear(RaycastHit targetHit)
    {
        Vector3 origin = Camera.main.transform.position;
        Vector3 direction = targetHit.point - origin;
        float distance = direction.magnitude;
        
        if (Physics.Raycast(origin, direction.normalized, out RaycastHit obstacleHit, distance - 0.05f, LayerMask.GetMask("Default")))
        {
            if (obstacleHit.collider.gameObject != targetHit.collider.gameObject && 
                !obstacleHit.transform.IsChildOf(targetHit.transform))
            {
                return false;
            }
        }

        return true;
    }

    public void SeenObject()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 3f, LayerMask.GetMask("Cassette")))
        {
            if (IsLineOfClear(hit))
            {
                Outline currentOutline = hit.collider.GetComponent<Outline>();

                if (currentOutline != null)
                {
                    if (_lastSeenOutline != currentOutline)
                    {
                        if (_lastSeenOutline != null)
                        {
                            _lastSeenOutline.enabled = false;
                        }

                        _lastSeenOutline = currentOutline;
                        _lastSeenOutline.enabled = true;

                        cassetteNameText.text = hit.collider.GetComponent<PhysicalCassette>().cassetteData.name;
                    }

                    _cassettePreview.SetActive(false);

                    return;
                }
            }
        }
        else if (Physics.Raycast(ray, out hit, 3f, LayerMask.GetMask("ShelfSlot")))
        {
            if (IsLineOfClear(hit))
            {
                ShelfSlotCollider slotCollider = hit.collider.GetComponent<ShelfSlotCollider>();
                int index = slotCollider.parentShelf.GetSlotIndex(slotCollider.slotID);
                var targetSlot = slotCollider.parentShelf.slots[index];

                if (_cassetteStack.Count > 0 && targetSlot.currentCount < targetSlot.countOfMovies)
                {
                    _cassettePreview.SetActive(true);

                    Transform slotTr = slotCollider.parentShelf.slots[index].slotTransform;
                    int currentCount = slotCollider.parentShelf.slots[index].currentCount;

                    Vector3 startPos = slotTr.position - (slotTr.right * 0.11f);
                    Vector3 previewPos = startPos + (slotTr.right * (currentCount * thickness));

                    _cassettePreview.transform.position = previewPos;
                    _cassettePreview.transform.rotation = slotTr.rotation;
                    _cassettePreview.transform.Rotate(270f, 0f, 270f);
                

                    if (_lastSeenOutline != null)
                    {
                        _lastSeenOutline.enabled = false;
                        _lastSeenOutline = null;
                        cassetteNameText.text = null;
                    }

                    return;
                }
            }
        }
        
        _cassettePreview.SetActive(false);

        if (_lastSeenOutline != null && (!hit.collider || hit.collider.gameObject.layer != LayerMask.NameToLayer("Cassette")))
        {
            _lastSeenOutline.enabled = false;
            _lastSeenOutline = null;
            cassetteNameText.text = null;
        }
    }

    public void UpdateCassetteCounterInterface()
    {
        placedCassettesText.text = _placedCassettes + "/" + _totalCassettesInWorld;
    }
    
    public void UpdateShelvesCounterInterface()
    {
        int correctSlots = 0;

        foreach (var shelf in _allShelves)
        {
            for (int i = 0; i < shelf.slots.Count; i++)
            {
                if (shelf.slots[i].IsRightFilled())
                {
                    correctSlots++;
                }
            }
        }
        
        if (filledShelvesText != null)
        {
            filledShelvesText.text = correctSlots + "/" + _totalSlots;
        }
    }

    public void PickUpCassette(GameObject cassette)
    {
        CassetteData data = cassette.GetComponent<PhysicalCassette>().cassetteData;
        _cassetteStack.Add(data);
        
        if (audioSource != null && pickUpSound != null)
        {
            audioSource.PlayOneShot(pickUpSound, 0.8f);
        }
        
        Destroy(cassette);
        
        RefreshHandVisuals();
        UpdateCassetteCounterInterface();
        UpdateShelvesCounterInterface();
    }

    public void Interact()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 3f, LayerMask.GetMask("Cassette")))
        {
            if (!IsLineOfClear(hit)) return;
            
            PhysicalCassette physicalCassette = hit.collider.GetComponent<PhysicalCassette>();

            if (physicalCassette != null)
            {
                if (_cassetteStack.Count < maxCarryCount)
                {
                    if (physicalCassette.isShelved)
                    {
                        CassetteData takenData =
                            physicalCassette.shelfRef.CassetteIsTaken(physicalCassette.slotIDForShelf);
                        if (takenData != null)
                        {
                            _cassetteStack.Add(takenData);
                            _placedCassettes--;
                            RefreshHandVisuals();

                        }
                    }
                    else
                    {
                        PickUpCassette(hit.collider.gameObject);
                    }

                    if (audioSource != null && pickUpSound != null)
                    {
                        audioSource.PlayOneShot(pickUpSound, 0.8f);
                    }
                    
                    UpdateCassetteCounterInterface();
                    UpdateShelvesCounterInterface();
                }
            }
            
            _showTutorial = PlayerPrefs.GetInt("ShowTutorial", 1) == 1;

            if (_showTutorial && pickUpTutorialSlide != null && !_hasShownPickUptTutorialSlide)
            {
                _hasShownPickUptTutorialSlide = true;
                pickUpTutorialSlide.SetActive(true);
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else if (Physics.Raycast(ray, out hit, 3f, LayerMask.GetMask("ShelfSlot")))
        {
            if (!IsLineOfClear(hit)) return;
            
            if (_cassetteStack.Count <= maxCarryCount)
            {
                if (_cassetteStack.Count > 0)
                {
                    ShelfSlotCollider slotCollider = hit.collider.GetComponent<ShelfSlotCollider>();

                    CassetteData topCassette = _cassetteStack[^1];
                    
                    string result = slotCollider.parentShelf.CassetteIsPlaced(slotCollider.slotID, topCassette);

                    if (result != null && result != "Slot Is Full")
                    {
                        _cassetteStack.RemoveAt(_cassetteStack.Count - 1);
                        _placedCassettes++;
                        RefreshHandVisuals();
                        UpdateCassetteCounterInterface();
                        UpdateShelvesCounterInterface();
                        
                        if (audioSource != null && placeSound != null)
                        {
                            audioSource.PlayOneShot(placeSound, 0.8f);
                        }
                        
                        _showTutorial = PlayerPrefs.GetInt("ShowTutorial", 1) == 1;

                        if (_showTutorial && placeTutorialSlide != null && !_hasShownPlaceTutorialSlide)
                        {
                            _hasShownPlaceTutorialSlide = true;
                            placeTutorialSlide.SetActive(true);
                            Time.timeScale = 0f;
                            Cursor.lockState = CursorLockMode.None;
                            Cursor.visible = true;
                        }
                    }
                }
            }
        }
        else if (Physics.Raycast(ray, out hit, 3f, LayerMask.GetMask("DeskReader")))
        {
            if (!IsLineOfClear(hit)) return;
            
            if (_cassetteStack.Count != 0)
            {
                DeskReader deskReader = hit.collider.GetComponent<DeskReader>();
                
                bool flag = deskReader.PlaceCassette(_cassetteStack[^1]);
                if (flag)
                {
                    _cassetteStack.RemoveAt(_cassetteStack.Count - 1);
                    RefreshHandVisuals();
                    UpdateCassetteCounterInterface();
                    UpdateShelvesCounterInterface();
                    
                    if (audioSource != null && placeSound != null)
                    {
                        audioSource.PlayOneShot(placeSound, 0.8f);
                    }
                }
            }
        }
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
        
        if (audioSource != null && dropSound != null)
        {
            audioSource.PlayOneShot(dropSound, 0.8f);
        }
        
        RefreshHandVisuals();
    }
    
    private void HandleScrollInventory()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        
        if (_cassetteStack.Count > 1)
        {
            if (scroll > 0f) 
            {
                CassetteData first = _cassetteStack[0];
                _cassetteStack.RemoveAt(0);
                _cassetteStack.Add(first);
            
                RefreshHandVisuals();
            }
            else if (scroll < 0f)
            {
                CassetteData last = _cassetteStack[^1];
                _cassetteStack.RemoveAt(_cassetteStack.Count - 1);
                _cassetteStack.Insert(0, last);
            
                RefreshHandVisuals();
            }
        }
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

        if (_cassetteStack.Count != 0)
        {
            cassetteHandedText.text = _cassetteStack[^1].name;
            amountCassettesInHandsText.text = _cassetteStack.Count.ToString() + "/10";
        }
        else
        {
            cassetteHandedText.text = "";
            amountCassettesInHandsText.text = "0/10";
        }
    }

    public SaveData SaveCreation()
    {
        SaveData saveData = new SaveData();
        saveData.cassettesInHands = new List<string>();
        saveData.cassettesInWorld = new List<CassetteSaveData>();

        foreach (var cassette in _cassetteStack)
        {
            saveData.cassettesInHands.Add(cassette.title);
        }
        
        PhysicalCassette[] allCassettes = FindObjectsOfType<PhysicalCassette>();

        foreach (var cassette in allCassettes)
        {
            if (_visualBoxes.Contains(cassette.gameObject)) continue;
            
            CassetteSaveData cData = new CassetteSaveData();
            
            cData.CassetteName = cassette.cassetteData.title;
            
            cData.position = cassette.transform.position;
            cData.rotation = cassette.transform.rotation;
            cData.scale = cassette.transform.localScale;
            
            if (cassette.isShelved)
            {
                cData.state = 2;
                cData.shelfGenre = cassette.shelfRef.shelfGenre;
                cData.slotID = cassette.slotIDForShelf;
                
                int slotIndex = cassette.shelfRef.GetSlotIndex(cassette.slotIDForShelf);
                var targetSlot = cassette.shelfRef.slots[slotIndex];
                cData.indexInSlot = targetSlot.spawnedCassettes.IndexOf(cassette.gameObject);
            }
            else
            {
                cData.state = 1;
                cData.shelfGenre = "";
                cData.slotID = 0;
                cData.indexInSlot = 0;
            }
            
            saveData.playerPosX = transform.position.x;
            saveData.playerPosY = transform.position.y;
            saveData.playerPosZ = transform.position.z;
            
            saveData.playerRotY = transform.eulerAngles.y;
            
            saveData.cassettesInWorld.Add(cData);
        }

        return saveData;
    }

    public void LoadSavedGame()
    {
        SaveData saveData = SaveManager.Load();
        Shelf[] allShelves = FindObjectsOfType<Shelf>();
        
        if (saveData == null)
        {
            return;
        }
        
        CassetteData[] allLoadedFilms = Resources.LoadAll<CassetteData>("Films");
        
        Dictionary<string, CassetteData> filmsDictionary = new Dictionary<string, CassetteData>();
        
        foreach (var film in allLoadedFilms)
        {
            if (film != null && !filmsDictionary.ContainsKey(film.title))
            {
                filmsDictionary.Add(film.title.Trim(), film); 
            }
        }
        
        PhysicalCassette[] allCassettes = FindObjectsOfType<PhysicalCassette>();

        foreach (var cassette in allCassettes)
        {
            DestroyImmediate(cassette.gameObject);
        }
        
        Debug.Log("ALL CASSETTES DESTROYED");

        foreach (var cassette in saveData.cassettesInHands)
        {
            if (filmsDictionary.TryGetValue(cassette.Trim(), out CassetteData handData))
            {
                _cassetteStack.Add(handData);
            }
        }
        
        Debug.Log("CASSETTES IN HANDS SPAWNED");
        
        RefreshHandVisuals();
        
        saveData.cassettesInWorld.Sort((a, b) => a.indexInSlot.CompareTo(b.indexInSlot));

        foreach (var cassette in saveData.cassettesInWorld)
        {
            if (cassette.state == 1)
            {
                GameObject newCassette = Instantiate(cassettePrefab, cassette.position, cassette.rotation);
                
                newCassette.transform.localScale = cassette.scale;

                if (filmsDictionary.TryGetValue(cassette.CassetteName.Trim(), out CassetteData data))
                {
                    newCassette.GetComponent<PhysicalCassette>().Init(data);
                }
            }

            if (cassette.state == 2)
            {
                Shelf targetShelf = System.Array.Find(allShelves, s => s.shelfGenre == cassette.shelfGenre);
                if (targetShelf != null)
                {
                    if (filmsDictionary.TryGetValue(cassette.CassetteName.Trim(), out CassetteData data))
                    {
                        targetShelf.CassetteIsPlaced(cassette.slotID, data);
                    }
                }
            }
        }
        
        CharacterController cc = transform.root.GetComponentInChildren<CharacterController>();
    
        if (cc != null)
        {
            cc.enabled = false;
            
            Vector3 cameraOffset = transform.position - cc.transform.position;
            
            Vector3 targetCapsulePos = new Vector3(saveData.playerPosX, saveData.playerPosY, saveData.playerPosZ) - cameraOffset;
            
            cc.transform.position = targetCapsulePos;
            cc.transform.rotation = Quaternion.Euler(0f, saveData.playerRotY, 0f);
            
            cc.enabled = true;
        }
        else
        {
            transform.root.position = new Vector3(saveData.playerPosX, saveData.playerPosY, saveData.playerPosZ);
        }
    }
    
    public bool IsTutorialActive()
    {
        bool isFirstActive = firstTutorialSlide != null && firstTutorialSlide.activeSelf;
        bool isPickUpActive = pickUpTutorialSlide != null && pickUpTutorialSlide.activeSelf;
        bool isPlaceActive = placeTutorialSlide != null && placeTutorialSlide.activeSelf;

        return isFirstActive || isPickUpActive || isPlaceActive;
    }
    
    public void CloseActiveTutorialSlide()
    {
        if (firstTutorialSlide != null && firstTutorialSlide.activeSelf) firstTutorialSlide.SetActive(false);
        if (pickUpTutorialSlide != null && pickUpTutorialSlide.activeSelf) pickUpTutorialSlide.SetActive(false);
        if (placeTutorialSlide != null && placeTutorialSlide.activeSelf) placeTutorialSlide.SetActive(false);
        
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}


