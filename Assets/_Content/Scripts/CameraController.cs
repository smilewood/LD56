using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using vegeo;

public class CameraController : MonoBehaviour
{
    public bool AllowMouseInput;
    [SerializeField] private InputActionAsset _input;

    [Header("Config")]
    [SerializeField] private float _lerpingSpeed = 5f;

    [Header("Settings")]
    public AnimationCurve ZoomSensitivityCurve;
    public float MaxMoveSpeed = 10f;
    public float MinMoveSpeed = 2f;
    public float ScrollZoomAmount = 0.05f;
    public float MouseZoomSpeed = -0.005f;
    public float MouseRotationSpeed = 0.1f;
    public float KeyboardRotationSpeed = 100f;
    public float PanBorderThickness = 10f;

    [Header("References")]
    [SerializeField] private Rigidbody _focus;
    [SerializeField] private Transform _highPosition;
    [SerializeField] private Transform _lowPosition;
    [SerializeField] private Volume _closeDofVolume;
    [SerializeField] private Volume _farDofVolume;

    private Camera _camera;

    private Vector3 _targetPosition;
    private float _zoomValue = 1f;

    private InputAction _moveAction;
    private InputAction _rotateAction;

    private Vector2 _preservedCursorPosition;

    private void Start()
    {
        _camera = GetComponentInChildren<Camera>();

        _moveAction = _input.FindAction("Move");
        _moveAction.Enable();
        _rotateAction = _input.FindAction("Rotate");
        _rotateAction.Enable();
    }

    private void Update()
    {

        Physics.simulationMode = SimulationMode.FixedUpdate;
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            _preservedCursorPosition = Mouse.current.position.ReadValue();
        }

        if (Mouse.current.rightButton.isPressed)
        {
            // Lock and hide cursor
            Mouse.current.WarpCursorPosition(_preservedCursorPosition);
            Cursor.visible = false;
        }
        else
        {
            // Unlock and show cursor
            Cursor.visible = true;
        }

        if (Mouse.current.scroll.y.ReadValue() != 0 && AllowMouseInput)
            _zoomValue = Mathf.Clamp01(_zoomValue + Mathf.Sign(Mouse.current.scroll.y.ReadValue()) * ScrollZoomAmount);

        float moveSpeed = Util.map(ZoomSensitivityCurve.Evaluate(_zoomValue), 0f, 1f, MinMoveSpeed, MaxMoveSpeed);
        Vector3 moveDirection = _moveAction.ReadValue<Vector2>().ToXZ();
        moveDirection *= moveSpeed * Time.deltaTime;

        if (Mouse.current.rightButton.isPressed && AllowMouseInput)
        {
            _focus.transform.eulerAngles += new Vector3(0, Mouse.current.delta.x.ReadValue() * MouseRotationSpeed, 0);
        }
        _focus.transform.eulerAngles += new Vector3(0, _rotateAction.ReadValue<float>() * KeyboardRotationSpeed * Time.deltaTime, 0);

        _targetPosition = Vector3.Lerp(_lowPosition.position, _highPosition.position, _zoomValue);

        _closeDofVolume.weight = 1f - _zoomValue;
        _farDofVolume.weight = _zoomValue;
    }

    private void LateUpdate()
    {
        _camera.transform.position = Vector3.Lerp(_camera.transform.position, _targetPosition, _lerpingSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        float moveSpeed = Util.map(ZoomSensitivityCurve.Evaluate(_zoomValue), 0f, 1f, MinMoveSpeed, MaxMoveSpeed);
        Vector3 moveDirection = _moveAction.ReadValue<Vector2>().ToXZ();

        if (Mouse.current.position.y.ReadValue() >= Screen.height - PanBorderThickness)
        {
            moveDirection += Vector3.forward;
        }
        if (Mouse.current.position.y.ReadValue() <= PanBorderThickness)
        {
            moveDirection -= Vector3.forward;
        }
        if (Mouse.current.position.x.ReadValue() >= Screen.width - PanBorderThickness)
        {
            moveDirection += Vector3.right;
        }
        if (Mouse.current.position.x.ReadValue() <= PanBorderThickness)
        {
            moveDirection -= Vector3.right;
        }

        Debug.Log(moveDirection);

        moveDirection *= moveSpeed * Time.fixedDeltaTime;

        Vector3 velocity = _focus.transform.TransformDirection(moveDirection);
        velocity.y = 0;
        _focus.velocity = velocity;
    }
}
