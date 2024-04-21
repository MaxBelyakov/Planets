using UnityEngine;

// Control camera behaviour.
// Listening for rotating, moving, zoom input.
// ReturnToDefault returns camera to origin position when we reset states

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    private float _rotationSpeed = 0.2f;
    private float _zoomSpeed = 2.0f;
    private float _moveSpeed = 3.0f;
    private float _minZoomDistance = 2.0f;
    private float _maxZoomDistance = 10.0f;

    private bool _isRotating = false;
    private Vector3 _lastMousePosition;

    private Vector3 _startPosition;
    private Quaternion _startRotation;

    public bool ReturnToDefault { get; set; } = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _startPosition = transform.position;
        _startRotation = transform.rotation;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsMouseOverGameObject())
            {
                ReturnToDefault = false;
                _isRotating = true;
                _lastMousePosition = Input.mousePosition;
            }
        }
        else if (Input.GetMouseButtonUp(0))
            _isRotating = false;

        // Rotating
        if (_isRotating)
        {
            Vector3 deltaMousePosition = Input.mousePosition - _lastMousePosition;

            float mouseX = deltaMousePosition.x * _rotationSpeed;
            float mouseY = deltaMousePosition.y * _rotationSpeed;

            transform.Rotate(Vector3.up, mouseX, Space.World);
            transform.Rotate(Vector3.left, mouseY, Space.Self);

            _lastMousePosition = Input.mousePosition;
        }

        // Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            Vector3 zoomDirection = transform.forward * scroll * _zoomSpeed;
            Vector3 newPosition = transform.position + zoomDirection;
            newPosition = Vector3.ClampMagnitude(newPosition - transform.position, _maxZoomDistance) + transform.position;
            transform.position = newPosition;
        }

        // Move
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 moveDirection = new Vector3(horizontalInput, 0.0f, verticalInput).normalized * _moveSpeed * Time.deltaTime;
        transform.Translate(moveDirection, Space.Self);

        if (ReturnToDefault)
        {
            transform.position = Vector3.Lerp(transform.position, _startPosition, _moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, _startRotation, _rotationSpeed * 5f * Time.deltaTime);

            if (Vector3.Distance(transform.position, _startPosition) < 1f && Quaternion.Angle(transform.rotation, _startRotation) < 1f)
                ReturnToDefault = false;
        }
    }

    private bool IsMouseOverGameObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
            return hit.collider.gameObject != gameObject;

        return false;
    }
}
