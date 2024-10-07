using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputActionAsset _inputActions;
    [SerializeField] private Transform _contextMenu;
    [SerializeField] private Transform _buildMenu;
    [SerializeField] private GameObject _testPrefab;
    [SerializeField] private CameraController _cameraController;
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

        Transform hoveredObject = hit.collider?.transform.parent;

        if (hoveredObject)
        {
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
            OpenContextMenu(hoveredObject?.GetComponent<BuildingManager>());
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

        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time - _timeStartedBuilding > 1f)
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
            State = PlayerState.None;
        }
    }

    private void OpenContextMenu(BuildingManager buildingManager)
    {
        if (buildingManager != null)
        {
            _capUpgrade.gameObject.SetActive(!buildingManager.IsCapacityUpgraded);
            _capUpgrade.interactable = buildingManager.CanUpgradeCapacity();
            _opRateUpgrade.gameObject.SetActive(!buildingManager.IsOperationRateUpgraded);
            _opRateUpgrade.interactable = buildingManager.CanUpgradeOperationRate();
        }
        else
        {
            _capUpgrade.gameObject.SetActive(false);
            _opRateUpgrade.gameObject.SetActive(false);
        }

        _contextMenu.gameObject.SetActive(true);

        _contextMenu.position = Mouse.current.position.ReadValue();

        State = PlayerState.UI;
    }

    public void AttemptCapUpgrade()
    {
        _contextMenu.gameObject.SetActive(false);
        if (_lastHovered == null) return;
        if (_lastHovered.TryGetComponent(out BuildingManager buildingManager))
        {
            buildingManager.TryUpgradeCapacity();
        }
    }

    public void AttemptOpRateUpgrade()
    {
        _contextMenu.gameObject.SetActive(false);
        if (_lastHovered == null) return;
        if (_lastHovered.TryGetComponent(out BuildingManager buildingManager))
        {
            buildingManager.TryUpgradeOperationRate();
        }
    }

    public void StartBuildingMenu()
    {
        _contextMenu.gameObject.SetActive(false);
        _buildMenu.gameObject.SetActive(true);

        _buildMenu.position = Mouse.current.position.ReadValue();

        State = PlayerState.UI;
    }

    public void StartBuildingPlacement(int index)
    {
        State = PlayerState.Placing;

        _buildMenu.gameObject.SetActive(false);

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
