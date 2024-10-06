using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputActionAsset _inputActions;
    [SerializeField] private Transform _buildUI;
    [SerializeField] private GameObject _testPrefab;
    [Header("Building Mode")]
    [SerializeField] private Transform _buildModeGroup;
    [SerializeField] private Transform _previewParent;
    [SerializeField] private Material _previewMaterial;
    [SerializeField] private LayerMask _buildingRaycastMask;
    [SerializeField] private float _buildingRotateAmount = 15f;

    private InputAction _rotateBuildingAction;
    private GameObject _currentBuilding;

    private float _timeStartedBuilding;

    public PlayerState State { get; private set; }

    private void Awake()
    {
        _rotateBuildingAction = _inputActions.FindAction("RotateBuilding");
        _rotateBuildingAction.Enable();
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

        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time - _timeStartedBuilding > 1f)
        {
            PlaceBuilding();
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

        _buildModeGroup.gameObject.SetActive(true);
        _timeStartedBuilding = Time.time;
    }

    private void PlaceBuilding()
    {
        if (State != PlayerState.Placing) return;
        State = PlayerState.None;

        _currentBuilding.transform.SetParent(null);
        _currentBuilding.SetActive(true);

        _buildModeGroup.gameObject.SetActive(false);

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
