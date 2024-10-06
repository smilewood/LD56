using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputActionAsset _inputActions;
    [SerializeField] private Transform _buildUI;
    [SerializeField] private GameObject _testPrefab;
    [SerializeField] private CameraController _cameraController;
    [Header("Building Mode")]
    [SerializeField] private Transform _buildModeGroup;
    [SerializeField] private Transform _previewParent;
    [SerializeField] private Material _previewMaterial;
    [SerializeField] private LayerMask _buildingRaycastMask;
    [SerializeField] private float _buildingRotateAmount = 15f;

    private InputAction _rotateBuildingAction;
    private GameObject _currentBuilding;

    private BuildingPreview _buildingPreview;

    private float _timeStartedBuilding;

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
        }
    }

    private void HandleNoneState()
    {
        _cameraController.AllowMouseInput = true;

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (_buildUI.gameObject.activeSelf)
            {
                _buildUI.gameObject.SetActive(false);
            }

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            Physics.Raycast(ray, out hit);

            Transform hoveredObject = hit.collider?.transform;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _buildUI.gameObject.SetActive(true);
                _buildUI.position = Mouse.current.position.ReadValue();
            }
        }
    }

    private void HandlePlacingState()
    {
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

    public void StartBuildingPlacement()
    {
        State = PlayerState.Placing;

        _buildUI.gameObject.SetActive(false);

        _currentBuilding = Instantiate(_testPrefab, _buildModeGroup);
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
        Placing
    }
}
