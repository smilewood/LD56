using Unity.Scenes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputActionAsset _inputActions;
    [SerializeField] private Transform _contextMenu;
    [SerializeField] private Transform _buildMenu;
    [SerializeField] private Transform _creatureBuyMenu;
    [SerializeField] private GameObject _testPrefab;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private GameObject[] _creaturePrefabs;
    [Header("Building Mode")]
    [SerializeField] private Transform _buildModeGroup;
    [SerializeField] private Transform _previewParent;
    [SerializeField] private Material _previewMaterial;
    [SerializeField] private LayerMask _buildingRaycastMask;
    [SerializeField] private LayerMask _interactionRaycastMask;
    [SerializeField] private float _buildingRotateAmount = 15f;
    [Header("References")]
    [SerializeField] private Highlighter _highlighter;
    [SerializeField] private Button _capUpgrade;
    [SerializeField] private Button _opRateUpgrade;
    [SerializeField] private Button _deleteBuildingButton;
    [SerializeField] private Button _creatureBuy;
    [SerializeField] private SubScene _entityScene;
    [SerializeField] private Button[] _buildingBuysButtons;
    [SerializeField] private Button[] _creatureBuyButtons;
    [SerializeField] private Transform _creatureSpawnLocation;

    private InputAction _rotateBuildingAction;
    private GameObject _currentBuilding;

    private BuildingPreview _buildingPreview;

    private float _timeStartedBuilding;
    private Transform _lastHovered;

    public PlayerState State { get; private set; }

    private void Awake()
    {
        _rotateBuildingAction = _inputActions.FindAction("RotateBuilding");
        _rotateBuildingAction.Enable();
        _buildingPreview = _previewParent.GetComponent<BuildingPreview>();
    }

    private void Update()
    {
        switch (State)
        {
            case PlayerState.None:
                HandleNoneState();
                break;
            case PlayerState.Placing:
                HandlePlacingState();
                break;
            case PlayerState.UI:
                HandleUIState();
                break;
        }
    }

    private void HandleNoneState()
    {
        _cameraController.AllowMouseInput = true;
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Physics.Raycast(ray, out hit, 75f, _interactionRaycastMask);

        Transform hoveredObject = null;
        if (hit.collider != null)
        {
            if (hit.collider.transform.parent != null)
                hoveredObject = hit.collider.transform.parent;
            else
                hoveredObject = hit.collider.transform;
        }

        if (hoveredObject)
        {
            if (_lastHovered != hoveredObject)
            {
                _highlighter.Unhighlight();
            }
            _highlighter.Highlight(hoveredObject.transform);
            _lastHovered = hoveredObject;
        }
        else
        {
            if (_lastHovered != null)
            {
                _highlighter.Unhighlight();
                _lastHovered = null;
            }
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (hoveredObject == null)
            {
                OpenContextMenuNoContext();
            }
            else
            {
                OpenContextMenu(hoveredObject);
            }
        }
    }

    private void HandlePlacingState()
    {
        if (_lastHovered != null)
        {
            _highlighter.Unhighlight();
            _lastHovered = null;
        }

        _cameraController.AllowMouseInput = false;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Physics.Raycast(ray, out hit, Mathf.Infinity, _buildingRaycastMask);

        if (hit.collider != null)
        {
            _buildModeGroup.position = hit.point;
        }

        if (_rotateBuildingAction.ReadValue<float>() != 0)
        {
            _buildModeGroup.Rotate(Vector3.up, _buildingRotateAmount * Mathf.Sign(_rotateBuildingAction.ReadValue<float>()));
        }

        _previewMaterial.SetFloat("_Valid", _buildingPreview.ValidPlacement ? 1 : 0);

        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time - _timeStartedBuilding > 0.1f)
        {
            PlaceBuilding();
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelBuilding();
        }
    }

    private void HandleUIState()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            _contextMenu.gameObject.SetActive(false);
            _buildMenu.gameObject.SetActive(false);
            _creatureBuyMenu.gameObject.SetActive(false);
            State = PlayerState.None;
        }
    }

    private void OpenContextMenuNoContext()
    {
        _capUpgrade.gameObject.SetActive(false);
        _opRateUpgrade.gameObject.SetActive(false);
        _creatureBuy.gameObject.SetActive(false);
        _deleteBuildingButton.gameObject.SetActive(false);

        _contextMenu.position = Mouse.current.position.ReadValue();

        State = PlayerState.UI;

        _contextMenu.gameObject.SetActive(true);
    }

    private void OpenContextMenu(Transform hoveredObject)
    {
        /*
        BuildingManager buildingManager = hoveredObject?.GetComponent<BuildingManager>();
        if (buildingManager != null)
        {
            _capUpgrade.gameObject.SetActive(!buildingManager.IsCapacityUpgraded);
            _capUpgrade.interactable = buildingManager.CanUpgradeCapacity();
            _opRateUpgrade.gameObject.SetActive(!buildingManager.IsOperationRateUpgraded);
            _opRateUpgrade.interactable = buildingManager.CanUpgradeOperationRate();
            _deleteBuildingButton.gameObject.SetActive(true);
        }
        else
        {
            _capUpgrade.gameObject.SetActive(false);
            _opRateUpgrade.gameObject.SetActive(false);
            _creatureBuy.gameObject.SetActive(false);
        }
        */

        bool canBuyCreatures = false;

        // Check if food and water tags exist in scene
        if (GameObject.FindGameObjectsWithTag("Food").Length > 0 && GameObject.FindGameObjectsWithTag("Water").Length > 0)
        {
            canBuyCreatures = true;
        }

        if (hoveredObject.tag == "Home")
        {
            _creatureBuy.gameObject.SetActive(true);
            _creatureBuy.interactable = canBuyCreatures;
        }
        else
        {
            _creatureBuy.gameObject.SetActive(false);
        }

        _contextMenu.gameObject.SetActive(true);

        _contextMenu.position = Mouse.current.position.ReadValue();

        State = PlayerState.UI;
    }

    public void AttemptCapUpgrade()
    {
        AudioManager.Instance.PlayUIInteract();
        _contextMenu.gameObject.SetActive(false);
        if (_lastHovered == null) return;
        if (_lastHovered.TryGetComponent(out BuildingManager buildingManager))
        {
            buildingManager.TryUpgradeCapacity();
        }
    }

    public void AttemptOpRateUpgrade()
    {
        AudioManager.Instance.PlayUIInteract();
        _contextMenu.gameObject.SetActive(false);
        if (_lastHovered == null) return;
        if (_lastHovered.TryGetComponent(out BuildingManager buildingManager))
        {
            buildingManager.TryUpgradeOperationRate();
        }
    }

    public void StartBuildingMenu()
    {
        AudioManager.Instance.PlayUIInteract();
        for (int i = 0; i < _buildingBuysButtons.Length; i++)
        {
            _buildingBuysButtons[i].interactable = EconomyManager.Instance.CanPurchase(EconomyManager.Instance.GetBuildingMetadata((BuildingType)i));
        }

        _contextMenu.gameObject.SetActive(false);
        _buildMenu.gameObject.SetActive(true);

        _buildMenu.position = Mouse.current.position.ReadValue();

        State = PlayerState.UI;
    }

    public void StartCreatureBuyMenu()
    {
        AudioManager.Instance.PlayUIInteract();
        RefreshCreatureBuyMenuInteractable();
        _contextMenu.gameObject.SetActive(false);
        _creatureBuyMenu.gameObject.SetActive(true);

        _creatureBuyMenu.position = Mouse.current.position.ReadValue();

        State = PlayerState.UI;
    }

    public void RefreshCreatureBuyMenuInteractable()
    {
        _creatureBuyButtons[0].interactable = EconomyManager.Instance.CanPurchase(new CurrencyData { Biomass = 100, Ore = 50, Bread = 0 });
        _creatureBuyButtons[1].interactable = EconomyManager.Instance.CanPurchase(new CurrencyData { Biomass = 75, Ore = 250, Bread = 50 });
        _creatureBuyButtons[2].interactable = EconomyManager.Instance.CanPurchase(new CurrencyData { Biomass = 500, Ore = 500, Bread = 100 });
    }

    public void BuyCreature(int index)
    {
        AudioManager.Instance.PlayUIInteract();
        AudioManager.Instance.PlayTransmutiteCall();

        switch (index)
        {
            case 0:
                EconomyManager.Instance.TryPurchase(new CurrencyData { Biomass = 100, Ore = 50, Bread = 0 });
                CreatureSpawnManager.Instance.CleanerCount += 10;
                break;
            case 1:
                EconomyManager.Instance.TryPurchase(new CurrencyData { Biomass = 75, Ore = 250, Bread = 50 });
                CreatureSpawnManager.Instance.HaulerCount += 10;
                break;
            case 2:
                EconomyManager.Instance.TryPurchase(new CurrencyData { Biomass = 500, Ore = 500, Bread = 100 });
                CreatureSpawnManager.Instance.ProducerCount += 10;
                break;
        }
    }

    public void StartBuildingPlacement(int index)
    {
        AudioManager.Instance.PlayUIInteract();
        State = PlayerState.Placing;

        _buildMenu.gameObject.SetActive(false);

        if (!EconomyManager.Instance.TryPurchase(EconomyManager.Instance.GetBuildingMetadata((BuildingType)index)))
        {
            State = PlayerState.None;
            return;
        }

        GameObject prefab = EconomyManager.Instance.GetBuildingMetadata((BuildingType)index).Prefab;

        _currentBuilding = Instantiate(prefab, _buildModeGroup);
        _currentBuilding.transform.localPosition = Vector3.zero;
        _currentBuilding.transform.localRotation = Quaternion.identity;
        _currentBuilding.SetActive(false);

        foreach (MeshRenderer meshRenderer in _currentBuilding.GetComponentsInChildren<MeshRenderer>())
        {
            GameObject newObj = Instantiate(meshRenderer.gameObject, _previewParent, true);
            MeshRenderer newMeshRenderer = newObj.GetComponent<MeshRenderer>();
            Material[] newMaterials = new Material[newMeshRenderer.materials.Length];
            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = _previewMaterial;
            }
            newMeshRenderer.materials = newMaterials;
        }

        Collider collider = _currentBuilding.transform.Find("Bounds")?.GetComponent<Collider>();
        if (collider)
        {
            Instantiate(collider.gameObject, _previewParent, true);
        }

        _buildModeGroup.gameObject.SetActive(true);
        _timeStartedBuilding = Time.time;
    }

    private void PlaceBuilding()
    {
        if (State != PlayerState.Placing) return;
        if (!_buildingPreview.ValidPlacement) return;

        AudioManager.Instance.PlayPlaceStructure();

        State = PlayerState.None;

        _currentBuilding.transform.SetParent(null);
        _currentBuilding.SetActive(true);

        _buildModeGroup.gameObject.SetActive(false);

        EmptyPreview();
    }

    private void CancelBuilding()
    {
        if (State != PlayerState.Placing) return;
        State = PlayerState.None;

        Destroy(_currentBuilding);

        _buildModeGroup.gameObject.SetActive(false);

        EmptyPreview();
    }

    private void EmptyPreview()
    {

        foreach (Transform preview in _previewParent)
        {
            Destroy(preview.gameObject);
        }
    }

    public enum PlayerState
    {
        None,
        Placing,
        UI
    }
}
