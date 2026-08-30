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

    public TextMeshProUGUI shelvesUntilNextSkill;
    public TextMeshProUGUI skillCounterText;
    
    public float throwForce = 5f;
    public float thickness = 0.05f;
    
    private Shelf[] _allShelves;
    private PhysicalCassette[] _allCassettes;
    private int _totalSlots = 0;

    private int _totalCassettesInWorld = 0;
    private int _placedCassettes = 0;
    private int _completedShelves= 0;
    private int _totalSkillPointsEarned = 0;
    private int _skillCounter = 0;

    private float _lastSaveTime;
    public float autoSaveInterval = 900f;

    private int _currentCarryCount = 5;
    private int _shelfGuideLvl = 0;
    private int _insightLvl = 0;
    private int _autoShelvingLvl = 0;
    private int _assembleLvl = 0;
    
    private int _cassetteLayerMask;
    private int _shelfSlotLayerMask;
    private int _defaultLayerMask;
    private int _deskReaderLayerMask;
    private int _cassetteLayerIndex;
    
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
    
    [Header("Shelf Guide Skill Settings")]
    public KeyCode shelfGuideSkillKey = KeyCode.Alpha1;
    private float _shelfGuideCooldownTimer = 0f;
    
    private readonly float[] _shelfGuideCooldowns = { 0f, 40f, 35f, 25f, 15f, 10f };
    private readonly float _shelfGuideDuration = 10f;

    [Header("Insight Skill Settings")]
    public KeyCode insightSkillKey = KeyCode.Alpha2;
    private float _insightCooldownTimer = 0f;
    
    private readonly float[] _insightCooldowns = { 0f, 45f, 35f, 25f, 15f, 10f };
    private readonly float _insightDuration = 10f;
    
    [Header("AutoShelving Skill Settings")]
    public KeyCode autoShelvingSkillKey = KeyCode.Alpha3;
    private float _autoShelvingCooldownTimer = 0f;
    
    private readonly float[] _autoShelvingCooldowns = { 0f, 45f, 35f, 25f, 15f, 10f };
    private readonly float _autoShelvingDistance = 5f;
    
    [Header("AutoShelving Skill Settings")]
    public KeyCode assembleSkillKey = KeyCode.Alpha4;
    private float _assembleCooldownTimer = 0f;
    
    private readonly float[] _assembleCooldowns = { 0f, 45f, 35f, 25f, 15f, 10f };
    private readonly float[] _assembleDistance = { 0f, 3f, 5f, 7f, 9f, 10f };
    
    [Header("Skill Menu Settings")]
    public GameObject skillMenu;
    public GameObject skillMenuButtonHint;

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
        
        _cassetteLayerMask = LayerMask.GetMask("Cassette");
        _shelfSlotLayerMask = LayerMask.GetMask("ShelfSlot");
        _defaultLayerMask = LayerMask.GetMask("Default");
        _deskReaderLayerMask = LayerMask.GetMask("DeskReader");
        _cassetteLayerIndex = LayerMask.NameToLayer("Cassette");
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
        ShelvesUntilNextSkillUpdate();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0f) return;
        
        if (_shelfGuideCooldownTimer > 0f)
        {
            _shelfGuideCooldownTimer -= Time.deltaTime;
        }

        if (_insightCooldownTimer > 0f)
        {
            _insightCooldownTimer -= Time.deltaTime;
        }
        
        if (_autoShelvingCooldownTimer > 0f)
        {
            _autoShelvingCooldownTimer -= Time.deltaTime;
        }
        
        if (_assembleCooldownTimer > 0f)
        {
            _assembleCooldownTimer -= Time.deltaTime;
        }
        
        SeenObject();
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DropCassette();
        }
        
        if (Input.GetKeyDown(shelfGuideSkillKey))
        {
            ShelfGuideSkill();
        }

        if (Input.GetKeyDown(insightSkillKey))
        {
            InsightSkill();
        }
        
        if (Input.GetKeyDown(autoShelvingSkillKey))
        {
            AutoShelvingSkill();
        }
        
        if (Input.GetKeyDown(assembleSkillKey))
        {
            AssembleSkill();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SkillMenu();
        }

        HandleScrollInventory();
    }

    private bool IsLineOfClear(RaycastHit targetHit)
    {
        Vector3 origin = Camera.main.transform.position;
        Vector3 direction = targetHit.point - origin;
        float distance = direction.magnitude;
        
        if (Physics.Raycast(origin, direction.normalized, out RaycastHit obstacleHit, distance - 0.05f, _defaultLayerMask))
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
        
        if (Physics.Raycast(ray, out hit, 3f, _cassetteLayerMask))
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
        else if (Physics.Raycast(ray, out hit, 3f, _shelfSlotLayerMask))
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
        ShelvesUntilNextSkillUpdate();
    }

    public void Interact()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 3f, _cassetteLayerMask))
        {
            if (!IsLineOfClear(hit)) return;
            
            PhysicalCassette physicalCassette = hit.collider.GetComponent<PhysicalCassette>();

            if (physicalCassette != null)
            {
                if (_cassetteStack.Count < _currentCarryCount)
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
                    ShelvesUntilNextSkillUpdate();
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
        else if (Physics.Raycast(ray, out hit, 3f, _shelfSlotLayerMask))
        {
            if (!IsLineOfClear(hit)) return;
            
            if (_cassetteStack.Count <= _currentCarryCount)
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
                        
                        int slotIndex = slotCollider.parentShelf.GetSlotIndex(slotCollider.slotID);
                        var targetSlot = slotCollider.parentShelf.slots[slotIndex];

                        if (targetSlot.CheckAndAward())
                        {
                            CheckForSkillPoint();
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
                        
                        ShelvesUntilNextSkillUpdate();
                    }
                }
            }
        }
        else if (Physics.Raycast(ray, out hit, 3f, _deskReaderLayerMask))
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
                    ShelvesUntilNextSkillUpdate();
                    
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
            amountCassettesInHandsText.text = _cassetteStack.Count.ToString() + "/" + _currentCarryCount.ToString();
        }
        else
        {
            cassetteHandedText.text = "";
            amountCassettesInHandsText.text = "0/" +  _currentCarryCount.ToString();
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
            
            saveData.completedShelves = _completedShelves;
            saveData.totalSkillPointsEarned = _totalSkillPointsEarned;
            saveData.skillCounter = _skillCounter;
            
            saveData.maxCarryCount =  _currentCarryCount;
            saveData.shelfGuideLvl = _shelfGuideLvl;
            saveData.insightLvl = _insightLvl;
            saveData.autoShelvingLvl = _autoShelvingLvl;
            saveData.assembleLvl = _assembleLvl;
            
            saveData.playerPosX = transform.position.x;
            saveData.playerPosY = transform.position.y;
            saveData.playerPosZ = transform.position.z;
            
            saveData.playerRotY = transform.eulerAngles.y;
            
            saveData.showTutorial = _showTutorial;
            saveData.hasShownFirstTutorialSlide = _hasShownFirstTutorialSlide;
            saveData.hasShownPickUptTutorialSlide = _hasShownPickUptTutorialSlide;
            saveData.hasShownPlaceTutorialSlide = _hasShownPlaceTutorialSlide;
            
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
        
        foreach (var shelf in allShelves)
        {
            foreach (var slot in shelf.slots)
            {
                if (slot.IsRightFilled())
                {
                    slot.isExpAwarded = true;
                }
            }
        }

        Debug.Log("SHELF SLOTS EXP FLAGS RESTORED");
        
        _completedShelves = saveData.completedShelves;
        _totalSkillPointsEarned = saveData.totalSkillPointsEarned;
        _skillCounter = saveData.skillCounter;
        
        _currentCarryCount =  saveData.maxCarryCount;
        _shelfGuideLvl = saveData.shelfGuideLvl;
        _insightLvl = saveData.insightLvl;
        _autoShelvingLvl = saveData.autoShelvingLvl;
        _assembleLvl = saveData.assembleLvl;
        
        _showTutorial = saveData.showTutorial;
        _hasShownFirstTutorialSlide = saveData.hasShownFirstTutorialSlide;
        _hasShownPickUptTutorialSlide = saveData.hasShownPickUptTutorialSlide;
        _hasShownPlaceTutorialSlide = saveData.hasShownPlaceTutorialSlide;
        
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
    
    private int GetRequiredSlotsForSP(int nextSkillPointIndex)
    {
        if (nextSkillPointIndex <= 1) return 1;
        if (nextSkillPointIndex >= 25) return 800;

        float n = nextSkillPointIndex;
        return Mathf.FloorToInt(1.258f * n * n - 0.258f * n);
    }
    
    public void CheckForSkillPoint()
    {
        _completedShelves += 1;

        if (_totalSkillPointsEarned < 25)
        {
            int nextSPIndex = _totalSkillPointsEarned + 1;
            int requiredSlots = GetRequiredSlotsForSP(nextSPIndex);
            
            if (_completedShelves >= requiredSlots)
            {
                _totalSkillPointsEarned++;
                _skillCounter++;
            
                Debug.Log($"SP earned: {_totalSkillPointsEarned}/25. Available SP: {_skillCounter}");
            }
        }
        Debug.Log($"Total completed shelf: {_completedShelves}, Total skill points: {_skillCounter}");
    }
    
    public bool TrySpendSkillPoint()
    {
        if (_skillCounter > 0)
        {
            _skillCounter--;
            return true;
        }
        return false;
    }

    public bool UpgradeInventory()
    {
        if (_currentCarryCount < 10)
        {
            if (TrySpendSkillPoint())
            {
                _currentCarryCount++;
                return true;
            }
        }
        return false;
    }

    public void ShelfGuideSkill()
    {
        if (_shelfGuideLvl <= 0)
        {
            Debug.Log("Shelf Guide skill is not unlocked!");
            return;
        }

        if (_shelfGuideCooldownTimer > 0f)
        {
            Debug.Log($"Shelf Guide is on cooldown! ({_shelfGuideCooldownTimer:F1}s left)");
            return;
        }

        if (_cassetteStack.Count == 0)
        {
            Debug.Log("No cassette in hand to guide!");
            return;
        }

        CassetteData currentCassette = _cassetteStack[^1];
        GameObject targetColliderObject = null;
        
        foreach (var shelf in _allShelves)
        {
            if (shelf.shelfGenre == currentCassette.genre)
            {
                ShelfSlotCollider[] slotColliders = shelf.GetComponentsInChildren<ShelfSlotCollider>();

                foreach (var slotCol in slotColliders)
                {
                    if (slotCol.slotID.ToString() == currentCassette.position)
                    {
                        targetColliderObject = slotCol.gameObject;
                        break;
                    }
                }
            }

            if (targetColliderObject != null) break;
        }

        if (targetColliderObject != null)
        {
            _shelfGuideCooldownTimer = _shelfGuideCooldowns[_shelfGuideLvl];

            float duration = _shelfGuideDuration;
            StartCoroutine(HighlightSlotCoroutine(targetColliderObject, duration));

            Debug.Log($"Guided to target slot collider for: {currentCassette.title}");
        }
        else
        {
            Debug.LogWarning($"No valid slot collider found with ID {currentCassette.position} for genre {currentCassette.genre}!");
        }
    }

    private IEnumerator HighlightSlotCoroutine(GameObject targetObject, float duration)
    {
        GameObject highlightBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        highlightBox.name = "ShelfSlot_HighlightMarker";
        
        highlightBox.layer = targetObject.layer;
        
        highlightBox.transform.position = targetObject.transform.position;
        highlightBox.transform.rotation = targetObject.transform.rotation;
        
        Collider targetCol = targetObject.GetComponent<Collider>();
        if (targetCol != null)
        {
            highlightBox.transform.localScale = targetCol.bounds.size;
        }
        else
        {
            highlightBox.transform.localScale = new Vector3(0.15f, 0.2f, 0.3f);
        }
        
        Destroy(highlightBox.GetComponent<Collider>());
        
        MeshRenderer mr = highlightBox.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.enabled = true;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            
            Material transparentMat = new Material(Shader.Find("Sprites/Default")); 
            transparentMat.color = new Color(0, 0, 0, 0);
            mr.material = transparentMat;
            
            mr.allowOcclusionWhenDynamic = false; 
        }

        Outline slotOutline = highlightBox.AddComponent<Outline>();
        
        slotOutline.OutlineMode = Outline.Mode.OutlineAndSilhouette; 
        slotOutline.OutlineColor = Color.cyan;
        slotOutline.OutlineWidth = 8f;
        slotOutline.enabled = true;

        yield return new WaitForSeconds(duration);
        
        if (highlightBox != null)
        {
            Destroy(highlightBox);
        }
    }

    public bool ShelfGuideSkillUpgrade()
    {
        if (_shelfGuideLvl < 5)
        {
            if (TrySpendSkillPoint())
            {
                _shelfGuideLvl++;
                return true;
            }
        }
        return false;
    }

   public void InsightSkill()
    {
        if (_insightLvl <= 0)
        {
            Debug.Log("Insight skill is not unlocked!");
            return;
        }

        if (_insightCooldownTimer > 0f)
        {
            Debug.Log($"Insight is on cooldown! ({_insightCooldownTimer:F1}s left)");
            return;
        }

        if (_cassetteStack.Count == 0)
        {
            Debug.Log("No cassette in hand to guide!");
            return;
        }
        
        CassetteData currentCassette = _cassetteStack[^1];
        
        _allCassettes = FindObjectsOfType<PhysicalCassette>();
        
        bool foundAny = false;
        float duration = _insightDuration;
        
        foreach (var cassette in _allCassettes)
        {
            if (cassette == null || cassette.cassetteData == null) continue;
            
            if (cassette.isShelved) continue;
            if (_visualBoxes.Contains(cassette.gameObject)) continue;
            
            if (cassette.cassetteData.title == currentCassette.title)
            {
                StartCoroutine(CassetteCoroutine(cassette.gameObject, duration));
                foundAny = true;
            }
        }

        if (foundAny)
        {
            _insightCooldownTimer = _insightCooldowns[_insightLvl];
            Debug.Log($"Insight active for: {currentCassette.title}");
        }
        else
        {
            Debug.Log("No free matching cassettes found in the world.");
        }
    }

    private IEnumerator CassetteCoroutine(GameObject targetObject, float duration)
    {
        if (targetObject == null) yield break;
        
        MeshRenderer mr = targetObject.GetComponentInChildren<MeshRenderer>();
        if (mr != null)
        {
            mr.allowOcclusionWhenDynamic = false;
        }
        
        Outline cassetteOutline = targetObject.GetComponent<Outline>();
        
        Color originalColor = Color.white;
        Outline.Mode originalMode = Outline.Mode.OutlineAll;
        bool hadOutlineComponent = cassetteOutline != null;

        if (hadOutlineComponent)
        {
            originalColor = cassetteOutline.OutlineColor;
            originalMode = cassetteOutline.OutlineMode;
        }
        else
        {
            cassetteOutline = targetObject.AddComponent<Outline>();
        }
        
        cassetteOutline.OutlineMode = Outline.Mode.OutlineAndSilhouette; 
        cassetteOutline.OutlineColor = Color.cyan;
        cassetteOutline.OutlineWidth = 8f;
        cassetteOutline.enabled = true;

        yield return new WaitForSeconds(duration);
        
        if (targetObject != null && cassetteOutline != null)
        {
            cassetteOutline.enabled = false;

            if (hadOutlineComponent)
            {
                cassetteOutline.OutlineColor = originalColor;
                cassetteOutline.OutlineMode = originalMode;
                cassetteOutline.OutlineWidth = 2f;
            }
            else
            {
                Destroy(cassetteOutline);
            }
        }
        
        if (mr != null)
        {
            mr.allowOcclusionWhenDynamic = true;
        }
    }

    public bool InsightSkillUpgrade()
    {
        if (_insightLvl < 5)
        {
            if (TrySpendSkillPoint())
            {
                _insightLvl++;
                return true;
            }
        }
        return false;
    }

    public void AutoShelvingSkill()
    {
        if (_autoShelvingLvl <= 0)
        {
            Debug.Log("AutoShelving skill is not unlocked!");
            return;
        }

        if (_autoShelvingCooldownTimer > 0f)
        {
            Debug.Log($"AutoShelving is on cooldown! ({_autoShelvingCooldownTimer:F1}s left)");
            return;
        }

        if (_cassetteStack.Count == 0)
        {
            Debug.Log("No cassettes in hand to auto-shelve!");
            return;
        }

        float maxDistance = _autoShelvingDistance;
        bool placedAny = false;
        
        for (int i = _cassetteStack.Count - 1; i >= 0; i--)
        {
            CassetteData currentCassette = _cassetteStack[i];
            
            Shelf targetShelf = null;
            ShelfSlotCollider targetCollider = null;
            
            foreach (var shelf in _allShelves)
            {
                if (shelf.shelfGenre == currentCassette.genre)
                {
                    ShelfSlotCollider[] slotColliders = shelf.GetComponentsInChildren<ShelfSlotCollider>();

                    foreach (var slotCol in slotColliders)
                    {
                        if (slotCol.slotID.ToString() == currentCassette.position)
                        {
                            targetShelf = shelf;
                            targetCollider = slotCol;
                            break;
                        }
                    }
                }

                if (targetCollider != null) break;
            }
            
            if (targetCollider != null && targetShelf != null)
            {
                Vector3 slotPosition = targetCollider.transform.position;
                Vector3 playerPosition = Camera.main.transform.position;
                
                if (Vector3.Distance(playerPosition, slotPosition) <= maxDistance)
                {
                    Vector3 direction = slotPosition - playerPosition;
                    float distance = direction.magnitude;
                    
                    if (!Physics.Raycast(playerPosition, direction.normalized, distance - 0.1f, _defaultLayerMask))
                    {
                        string result = targetShelf.CassetteIsPlaced(targetCollider.slotID, currentCassette);

                        if (result != null && result != "Slot Is Full")
                        {
                            _cassetteStack.RemoveAt(i);
                            _placedCassettes++;
                            placedAny = true;
                            
                            int slotIndex = targetShelf.GetSlotIndex(targetCollider.slotID);
                            var targetSlot = targetShelf.slots[slotIndex];

                            if (targetSlot.CheckAndAward())
                            {
                                CheckForSkillPoint();
                            }
                        }
                    }
                }
            }
        }
        
        if (placedAny)
        {
            _autoShelvingCooldownTimer = _autoShelvingCooldowns[_autoShelvingLvl];

            if (audioSource != null && placeSound != null)
            {
                audioSource.PlayOneShot(placeSound, 0.8f);
            }

            RefreshHandVisuals();
            UpdateCassetteCounterInterface();
            UpdateShelvesCounterInterface();
            ShelvesUntilNextSkillUpdate();

            Debug.Log("AutoShelving skill used successfully!");
        }
        else
        {
            Debug.Log("No valid shelf slots nearby or wall obstacle blocking line of sight!");
        }
    }

    public bool AutoShelvingSkillUpgrade()
    {
        if (_autoShelvingLvl < 5)
        {
            if (TrySpendSkillPoint())
            {
                _autoShelvingLvl++;
                return true;
            }
        }
        return false;
    }

    public void AssembleSkill()
    {
        if (_assembleLvl <= 0)
        {
            Debug.Log("Assemble skill is not unlocked!");
            return;
        }

        if (_assembleCooldownTimer > 0f)
        {
            Debug.Log($"Assemble is on cooldown! ({_assembleCooldownTimer:F1}s left)");
            return;
        }

        if (_cassetteStack.Count == 0)
        {
            Debug.Log("No cassette in hand to start assembly!");
            return;
        }

        if (_cassetteStack.Count >= _currentCarryCount)
        {
            Debug.Log("Hands are full! Cannot assemble more cassettes.");
            return;
        }
        
        string targetGenre = _cassetteStack[^1].genre;
        float maxDistance = _assembleDistance[_assembleLvl];
        
        
        int collectedCount = 0;
        Vector3 playerPos = transform.position;
        
        foreach (var physicalCassette in _allCassettes)
        {
            if (physicalCassette == null || physicalCassette.isShelved) continue;
            if (_visualBoxes.Contains(physicalCassette.gameObject)) continue;
            
            if (physicalCassette.cassetteData != null && physicalCassette.cassetteData.genre == targetGenre)
            {
                float distance = Vector3.Distance(playerPos, physicalCassette.transform.position);

                if (distance <= maxDistance)
                {
                    PickUpCassette(physicalCassette.gameObject);
                    collectedCount++;
                    
                    if (_cassetteStack.Count >= _currentCarryCount)
                    {
                        Debug.Log("Hands became full during assembly.");
                        break;
                    }
                }
            }
        }
        
        if (collectedCount > 0)
        {
            _assembleCooldownTimer = _assembleCooldowns[_assembleLvl];

            if (audioSource != null && pickUpSound != null)
            {
                audioSource.PlayOneShot(pickUpSound, 1f);
            }

            Debug.Log($"Assemble skill collected {collectedCount} cassettes of genre '{targetGenre}'!");
        }
        else
        {
            Debug.Log($"No free cassettes with genre '{targetGenre}' found within {maxDistance}m radius!");
        }
    }

    public bool AssembleSkillUpgrade()
    {
        if (_assembleLvl < 5)
        {
            if (TrySpendSkillPoint())
            {
                _assembleLvl++;
                return true;
            }
        }
        return false;
    }
    
    public void ShelvesUntilNextSkillUpdate()
    {
        shelvesUntilNextSkill.text = $"Shelves until next Skill: {GetRequiredSlotsForSP(_totalSkillPointsEarned + 1)}";
        skillCounterText.text = $"Skill Points: {_skillCounter}";
    }

    public bool SkillMenu() 
    {
        return true;
    }
    
    public int GetSkillCounter() => _skillCounter;
    public int GetCurrentCarryCount() => _currentCarryCount;
    public int GetShelfGuideLvl() => _shelfGuideLvl;
    public int GetInsightLvl() => _insightLvl;
    public int GetAutoShelvingLvl() => _autoShelvingLvl;
    public int GetAssembleLvl() => _assembleLvl;
    
    public float GetShelfGuideCooldownTimer() => _shelfGuideCooldownTimer;
    public float GetShelfGuideMaxCooldown() => _shelfGuideLvl > 0 ? _shelfGuideCooldowns[_shelfGuideLvl] : 1f;
    
    public float GetInsightCooldownTimer() => _insightCooldownTimer;
    public float GetInsightMaxCooldown() => _insightLvl > 0 ? _insightCooldowns[_insightLvl] : 1f;
    
    public float GetAutoShelvingCooldownTimer() => _autoShelvingCooldownTimer;
    public float GetAutoShelvingMaxCooldown() => _autoShelvingLvl > 0 ? _autoShelvingCooldowns[_autoShelvingLvl] : 1f;
    
    public float GetAssembleCooldownTimer() => _assembleCooldownTimer;
    public float GetAssembleMaxCooldown() => _assembleLvl > 0 ? _assembleCooldowns[_assembleLvl] : 1f;
}


