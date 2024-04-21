using UnityEngine;

// Controls interactions with spheres and their movement.
// Contains sphere details info.

public class SphereController : MonoBehaviour
{
    [SerializeField] private string planetName;
    [SerializeField] private string planetRadius;

    public string PlanetName { get { return planetName; } }
    public string PlanetRadius { get { return planetRadius; } }

    [SerializeField] private GameObject centerPoint;

    private int _pointsInCircle = 300;

    private LineRenderer _trajectoryLine;
    private Vector3 _lastMousePosition;

    private bool _isDragging = false;
    private bool _isSelected = false;
    private Vector3 _forwardDirection;

    private float _radius;

    private Vector3 _lastPosition;

    private float _moveSpeed = 0.5f;
    private float _rotateSpeed;

    private bool _isScaled = false;

    private void Start()
    {
        _rotateSpeed = _moveSpeed * 30f / transform.localScale.magnitude;

        _trajectoryLine = GetComponent<LineRenderer>();
        _trajectoryLine.positionCount = _pointsInCircle + 1;

        CalculateForwardDirection();

        DrawTrajectoryCircle();
    }

    private void Update()
    {
        if (!_isDragging && !_isSelected)
        {
            // Calculates trajectory of movement depends on axis with connected object
            Vector3 newPosition = centerPoint.transform.position + Quaternion.AngleAxis(Time.deltaTime * _rotateSpeed, _forwardDirection) * (transform.position - centerPoint.transform.position);
            transform.position = newPosition;

            DrawTrajectoryCircle();
        }
        else if (_isSelected && !_isScaled)
        {
            // Move camera to selected object
            // It can be moved to camera controller
            Camera mainCamera = Camera.main;
            Vector3 startPosition = mainCamera.transform.position;

            if (Vector3.Distance(mainCamera.transform.position, transform.position) > 2f)
            {
                mainCamera.transform.position = Vector3.Lerp(startPosition, transform.position, _moveSpeed * Time.deltaTime);

                Vector3 lookDirection = (transform.position - mainCamera.transform.position).normalized;

                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, targetRotation, Time.deltaTime * _moveSpeed);
            }
            else
            {
                _isScaled = true;
            }
        }
    }

    private void OnMouseDown()
    {
        _lastMousePosition = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        _lastPosition = transform.position;
        _isDragging = false;
    }

    private void OnMouseDrag()
    {
        _isDragging = true;
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition - _lastMousePosition);

        if (!_isSelected)
        {
            CalculateForwardDirection();
            DrawTrajectoryCircle();
        }
    }

    private void OnMouseUp()
    {
        float dragDistance = Vector3.Distance(_lastPosition, transform.position);
        if (dragDistance < 0.1f && !_isSelected)
        {
            CameraController.Instance.ReturnToDefault = false;
            _isSelected = true;
            GetComponent<LineRenderer>().enabled = false;           
            ApplicationManager.Instance.OnObjectClick(this.gameObject);
        }

        _isDragging = false;
    }

    private void CalculateForwardDirection()
    {
        Vector3 rotationAxis = (transform.position - centerPoint.transform.position);
        _forwardDirection = Vector3.Cross(rotationAxis, Vector3.up);

        _radius = Vector3.Distance(transform.position, centerPoint.transform.position);
    }

    private void DrawTrajectoryCircle()
    {
        float angleIncrement = 360f / _pointsInCircle;

        // Do local calculations to draw correct lines on collisions
        Vector3 rotationAxis = (transform.position - centerPoint.transform.position);
        Vector3 updatedForwardDirection = Vector3.Cross(rotationAxis, Vector3.up);
        float updatedRadius = Vector3.Distance(transform.position, centerPoint.transform.position);

        for (int i = 0; i <= _pointsInCircle; i++)
        {
            float angle = i * angleIncrement;
            Quaternion rotation = Quaternion.AngleAxis(angle, updatedForwardDirection);
            Vector3 point = centerPoint.transform.position + rotation * (Vector3.up * updatedRadius);
            _trajectoryLine.SetPosition(i, point);
        }
    }
}
