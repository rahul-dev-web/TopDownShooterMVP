/// <summary>
/// CameraFollow - Player को smooth follow करता है
/// 
/// Features:
/// - Smooth camera movement
/// - Camera shake effects
/// - Optional offset
/// - Boundary constraints (future)
/// 
/// Usage:
/// Attach to Main Camera
/// </summary>

using UnityEngine;
using System;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    // ============== SERIALIZED FIELDS ==============

    [Header("Follow Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
    [SerializeField] private bool useVerticalOffset = false;
    [SerializeField] private float verticalOffset = 1f;

    [Header("Shake Settings")]
    [SerializeField] private bool enableShake = true;
    [SerializeField] private float defaultShakeIntensity = 0.1f;
    [SerializeField] private float defaultShakeDuration = 0.15f;

    [Header("Boundaries (Optional)")]
    [SerializeField] private bool useBoundaries = false;
    [SerializeField] private Vector2 minBounds = new Vector2(-10, -10);
    [SerializeField] private Vector2 maxBounds = new Vector2(10, 10);

    // ============== PRIVATE FIELDS ==============

    private Camera _camera;
    private Vector3 _targetPosition;
    private Vector3 _smoothVelocity = Vector3.zero;
    private bool _isShaking = false;

    // Events
    public static event Action OnCameraShake;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        Debug.Log("[CameraFollow] ✓ Initialized");
    }

    private void Start()
    {
        // Player को find करो अगर assign नहीं है
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                Debug.Log("[CameraFollow] Player found and assigned");
            }
            else
            {
                Debug.LogError("[CameraFollow] ✗ Player not found!");
                enabled = false;
                return;
            }
        }

        // Initial position set करो
        if (playerTransform != null)
        {
            UpdateCameraPosition();
        }
    }

    private void LateUpdate()
    {
        if (playerTransform == null)
            return;

        FollowPlayer();
    }

    // ============== FOLLOWING ==============

    private void FollowPlayer()
    {
        // Target position calculate करो
        _targetPosition = playerTransform.position + offset;

        // Vertical offset apply करो
        if (useVerticalOffset)
        {
            _targetPosition.y += verticalOffset;
        }

        // Boundaries apply करो
        if (useBoundaries)
        {
            _targetPosition.x = Mathf.Clamp(_targetPosition.x, minBounds.x, maxBounds.x);
            _targetPosition.y = Mathf.Clamp(_targetPosition.y, minBounds.y, maxBounds.y);
        }

        // Smooth damp करो (professional camera movement)
        transform.position = Vector3.SmoothDamp(
            transform.position,
            _targetPosition,
            ref _smoothVelocity,
            1f / smoothSpeed
        );
    }

    private void UpdateCameraPosition()
    {
        if (playerTransform == null)
            return;

        _targetPosition = playerTransform.position + offset;

        if (useVerticalOffset)
        {
            _targetPosition.y += verticalOffset;
        }

        if (useBoundaries)
        {
            _targetPosition.x = Mathf.Clamp(_targetPosition.x, minBounds.x, maxBounds.x);
            _targetPosition.y = Mathf.Clamp(_targetPosition.y, minBounds.y, maxBounds.y);
        }

        transform.position = _targetPosition;
    }

    // ============== CAMERA SHAKE ==============

    /// <summary>
    /// Camera को shake करता है
    /// </summary>
    public void Shake(float intensity = -1, float duration = -1)
    {
        if (!enableShake || _isShaking)
            return;

        float shakeIntensity = intensity < 0 ? defaultShakeIntensity : intensity;
        float shakeDuration = duration < 0 ? defaultShakeDuration : duration;

        StartCoroutine(ShakeCoroutine(shakeIntensity, shakeDuration));
    }

    private System.Collections.IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        _isShaking = true;
        OnCameraShake?.Invoke();

        Vector3 originalPos = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Random offset generate करो
            Vector3 randomOffset = new Vector3(
                (Mathf.PerlinNoise(Time.time * 10, 0) - 0.5f) * 2 * intensity,
                (Mathf.PerlinNoise(0, Time.time * 10) - 0.5f) * 2 * intensity,
                0
            );

            transform.position = originalPos + randomOffset;

            yield return null;
        }

        transform.position = originalPos;
        _isShaking = false;
    }

    // ============== PUBLIC METHODS ==============

    public void SetFollowSpeed(float speed)
    {
        smoothSpeed = Mathf.Max(0.1f, speed);
    }

    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }

    public void SetVerticalOffset(float newOffset)
    {
        verticalOffset = newOffset;
    }

    public void SetBoundaries(Vector2 min, Vector2 max)
    {
        minBounds = min;
        maxBounds = max;
        useBoundaries = true;
    }

    public void DisableBoundaries()
    {
        useBoundaries = false;
    }

    public void SetPlayerTransform(Transform playerTrans)
    {
        playerTransform = playerTrans;
    }

    // ============== GETTERS ==============

    public Vector3 GetCameraPosition() => transform.position;
    public Vector3 GetTargetPosition() => _targetPosition;
    public bool IsShaking() => _isShaking;
    public float GetSmoothSpeed() => smoothSpeed;

    // ============== ZOOM (Future Feature) ==============

    /// <summary>
    /// Camera को zoom करता है
    /// </summary>
    public void SetZoom(float newSize)
    {
        if (_camera != null)
        {
            _camera.orthographicSize = Mathf.Max(1, newSize);
        }
    }

    public void ResetZoom()
    {
        if (_camera != null)
        {
            _camera.orthographicSize = 5f; // Default
        }
    }

    // ============== DEBUG ==============

    public void PrintDebugInfo()
    {
        Debug.Log("=== CAMERA DEBUG INFO ===");
        Debug.Log($"Camera Position: {transform.position:F2}");
        Debug.Log($"Target Position: {_targetPosition:F2}");
        Debug.Log($"Smooth Speed: {smoothSpeed}");
        Debug.Log($"Offset: {offset}");
        Debug.Log($"Is Shaking: {_isShaking}");

        if (_camera != null)
        {
            Debug.Log($"Orthographic Size: {_camera.orthographicSize}");
        }
    }
}