using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float _lerpingSpeed = 5f;

    [Header("Settings")]
    public float MaxMoveSpeed = 10f;
    public float MinMoveSpeed = 2f;
    public float ZoomSpeed = 5f;
    public float RotationSpeed = 5f;
    public float PanBorderThickness = 10f;

    [Header("References")]
    [SerializeField] private Transform _focusTransform;
    [SerializeField] private Transform _highPosition;
    [SerializeField] private Transform _lowPosition;
    private Camera _camera;

    private Vector3 _targetPosition;
    private float _zoomValue = 0.8f;

    private void Start()
    {
        _camera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        _zoomValue = Mathf.Clamp01(_zoomValue + Mouse.current.scroll.ReadValue().y * ZoomSpeed);
        float moveSpeed = Mathf.Lerp(MinMoveSpeed, MaxMoveSpeed, _zoomValue);

        if (Mouse.current.position.y.ReadValue() >= Screen.height - PanBorderThickness)
        {
            _focusTransform.position += _focusTransform.forward * moveSpeed * Time.deltaTime;
        }
        if (Mouse.current.position.y.ReadValue() <= PanBorderThickness)
        {
            _focusTransform.position -= _focusTransform.forward * moveSpeed * Time.deltaTime;
        }
        if (Mouse.current.position.x.ReadValue() >= Screen.width - PanBorderThickness)
        {
            _focusTransform.position += _focusTransform.right * moveSpeed * Time.deltaTime;
            _camera.transform.position += _focusTransform.right * moveSpeed * Time.deltaTime;
        }
        if (Mouse.current.position.x.ReadValue() <= PanBorderThickness)
        {
            _focusTransform.position -= _focusTransform.right * moveSpeed * Time.deltaTime;
            _camera.transform.position -= _focusTransform.right * moveSpeed * Time.deltaTime;
        }

        if (Mouse.current.rightButton.isPressed)
        {
            _focusTransform.eulerAngles += new Vector3(0, Mouse.current.delta.x.ReadValue() * RotationSpeed, 0);
        }



        _targetPosition = Vector3.Lerp(_lowPosition.position, _highPosition.position, _zoomValue);
        _camera.transform.position = Vector3.Lerp(_camera.transform.position, _targetPosition, Time.deltaTime * _lerpingSpeed);
    }
}
