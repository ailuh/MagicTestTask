using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private float _smoothSpeed = 0.125f;

    private Vector3 _offset;

    private void Start()
    {
        if (_playerTransform == null)
        {
            Debug.LogWarning("Player Transform is not assigned.");
            return;
        }

        _offset = transform.position - _playerTransform.position;
    }

    private void FixedUpdate()
    {
        if (_playerTransform == null)
        {
            return;
        }

        var desiredPosition = _playerTransform.position + _offset;
        var smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed);

        transform.position = smoothedPosition;
    }
}