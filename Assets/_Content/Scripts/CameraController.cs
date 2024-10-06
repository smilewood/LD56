using UnityEngine;
using UnityEngine.InputSystem;
using vegeo;

public class CameraController : MonoBehaviour
{
    [SerializeField] private InputActionAsset _input;

    [Header("Config")]
    [SerializeField] private float _lerpingSpeed = 5f;

    [Header("Settings")]
    public AnimationCurve ZoomSensitivityCurve;
    public float MaxMoveSpeed = 10f;
    public float MinMoveSpeed = 2f;
    public float MouseZoomSpeed = -0.005f;
    public float MouseRotationSpeed = 0.1f;
    public float KeyboardRotationSpeed = 100f;
    public float PanBorderThickness = 10f;

    [Header("References")]
    [SerializeField] private Transform _focusTransform;
    [SerializeField] private Transform _highPosition;
    [SerializeField] private Transform _lowPosition;
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

        _zoomValue = Mathf.Clamp01(_zoomValue + (Mouse.current.rightButton.isPressed ? Mouse.current.delta.y.ReadValue() * MouseZoomSpeed : 0));
        //float moveSpeed = Mathf.Lerp(MinMoveSpeed, MaxMoveSpeed, _zoomValue);
        float moveSpeed = Util.map(ZoomSensitivityCurve.Evaluate(_zoomValue), 0f, 1f, MinMoveSpeed, MaxMoveSpeed);
        Vector3 moveDirection = _moveAction.ReadValue<Vector2>().ToXZ();
        moveDirection *= moveSpeed * Time.deltaTime;

        if (Mouse.current.position.y.ReadValue() >= Screen.height - PanBorderThickness)
        {
            moveDirection += Vector3.forward * moveSpeed * Time.deltaTime;
        }
        if (Mouse.current.position.y.ReadValue() <= PanBorderThickness)
        {
            moveDirection -= Vector3.forward * moveSpeed * Time.deltaTime;
        }
        if (Mouse.current.position.x.ReadValue() >= Screen.width - PanBorderThickness)
        {
            moveDirection += Vector3.right * moveSpeed * Time.deltaTime;
        }
        if (Mouse.current.position.x.ReadValue() <= PanBorderThickness)
        {
            moveDirection -= Vector3.right * moveSpeed * Time.deltaTime;
        }

        _focusTransform.position += _focusTransform.TransformDirection(moveDirection);
        moveDirection.z = 0;
        _camera.transform.position += _focusTransform.TransformDirection(moveDirection);

        if (Mouse.current.rightButton.isPressed)
        {
            _focusTransform.eulerAngles += new Vector3(0, Mouse.current.delta.x.ReadValue() * MouseRotationSpeed, 0);
        }
        _focusTransform.eulerAngles += new Vector3(0, _rotateAction.ReadValue<float>() * KeyboardRotationSpeed * Time.deltaTime, 0);


        _targetPosition = Vector3.Lerp(_lowPosition.position, _highPosition.position, _zoomValue);
        _camera.transform.position = Vector3.Lerp(_camera.transform.position, _targetPosition, Time.deltaTime * _lerpingSpeed);
    }
}
