/// <summary>
/// PlayerController - Player Movement और Animation System
/// 
/// Handles:
/// - WASD/Arrow key movement
/// - Mouse/Joystick aiming
/// - Sprint functionality
/// - Animation updates
/// - Sprite direction
/// 
/// Usage:
/// Attach to Player GameObject
/// Requires: Rigidbody2D, Animator, SpriteRenderer
/// </summary>

using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    // ============== SERIALIZED FIELDS ==============

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float acceleration = 20f;

    [Header("Animation")]
    [SerializeField] private bool useAnimations = true;
    [SerializeField] private float animationSmoothTime = 0.1f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    // ============== PRIVATE FIELDS ==============

    // Components
    private Rigidbody2D _rb;
    private CircleCollider2D _detectionCollider;

    // Movement state
    private Vector2 _moveDirection = Vector2.zero;
    private Vector2 _lookDirection = Vector2.right;
    private Vector2 _currentVelocity = Vector2.zero;
    private bool _isMoving = false;
    private bool _isSprinting = false;

    // Animation state
    private float _animMoveX = 0f;
    private float _animMoveY = 0f;

    // Game state
    private bool _canMove = true;
    private bool _isAlive = true;

    // Events
    public static event Action<Vector2> OnPlayerMoved;
    public static event Action<Vector2> OnPlayerLookedAt;
    public static event Action OnPlayerSprinted;

    private void Awake()
    {
        // Components initialize करो
        _rb = GetComponent<Rigidbody2D>();
        _detectionCollider = GetComponent<CircleCollider2D>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        Debug.Log("[PlayerController] ✓ Initialized");
    }

    private void OnEnable()
    {
        // Subscribe to input events
        InputManager.OnMovementInput += HandleMovementInput;
        InputManager.OnAimInput += HandleAimInput;
        InputManager.OnSprintInput += HandleSprintInput;
        GameManager.OnGameStateChanged += HandleGameStateChange;

        Debug.Log("[PlayerController] Events subscribed");
    }

    private void OnDisable()
    {
        // Unsubscribe
        InputManager.OnMovementInput -= HandleMovementInput;
        InputManager.OnAimInput -= HandleAimInput;
        InputManager.OnSprintInput -= HandleSprintInput;
        GameManager.OnGameStateChanged -= HandleGameStateChange;
    }

    private void FixedUpdate()
    {
        if (!_canMove || !_isAlive)
            return;

        MovePlayer();
    }

    private void Update()
    {
        if (!_isAlive)
            return;

        if (_canMove)
        {
            UpdateAnimation();
        }
    }

    // ============== INPUT HANDLING ==============

    private void HandleMovementInput(Vector2 input)
    {
        _moveDirection = input;
        _isMoving = input.magnitude > 0.05f;

        // Event भेजो
        OnPlayerMoved?.Invoke(_moveDirection);
    }

    private void HandleAimInput(Vector2 input)
    {
        // Aim direction determine करो
        if (input.magnitude > 0.1f)
        {
            _lookDirection = input.normalized;
        }
        else if (_isMoving)
        {
            // Moving हो तो movement direction में देखो
            _lookDirection = _moveDirection.normalized;
        }

        OnPlayerLookedAt?.Invoke(_lookDirection);
    }

    private void HandleSprintInput()
    {
        if (!_canMove || !_isMoving)
            return;

        _isSprinting = true;
        OnPlayerSprinted?.Invoke();
    }

    private void HandleGameStateChange(GameManager.GameState newState)
    {
        _canMove = (newState == GameManager.GameState.Playing);

        if (!_canMove)
        {
            _rb.linearVelocity = Vector2.zero;
            _isMoving = false;
        }
    }

    // ============== MOVEMENT ==============

    private void MovePlayer()
    {
        if (!_isMoving)
        {
            _rb.linearVelocity = Vector2.zero;
            _isSprinting = false;
            return;
        }

        // Current speed determine करो
        float currentSpeed = _isSprinting ? sprintSpeed : moveSpeed;

        // Velocity calculate करो (smooth acceleration के साथ)
        _currentVelocity = Vector2.Lerp(
            _currentVelocity,
            _moveDirection * currentSpeed,
            acceleration * Time.fixedDeltaTime
        );

        // Apply करो
        _rb.linearVelocity = _currentVelocity;

        // Sprint एक-बार का action है (reset करो)
        if (_isSprinting)
        {
            _isSprinting = false;
        }
    }

    // ============== ANIMATION ==============

    private void UpdateAnimation()
    {
        if (!useAnimations || animator == null)
            return;

        // Movement direction determine करो
        Vector2 displayDirection = _isMoving ? _moveDirection : _lookDirection;

        // Smooth animation values
        _animMoveX = Mathf.Lerp(_animMoveX, displayDirection.x, animationSmoothTime * 60f * Time.deltaTime);
        _animMoveY = Mathf.Lerp(_animMoveY, displayDirection.y, animationSmoothTime * 60f * Time.deltaTime);

        // Animator parameters update करो
        animator.SetBool("IsMoving", _isMoving);
        animator.SetFloat("MoveX", _animMoveX);
        animator.SetFloat("MoveY", _animMoveY);

        // Sprite flip करो (left/right)
        if (displayDirection.x < -0.1f && !spriteRenderer.flipX)
        {
            spriteRenderer.flipX = true;
        }
        else if (displayDirection.x > 0.1f && spriteRenderer.flipX)
        {
            spriteRenderer.flipX = false;
        }
    }

    // ============== PUBLIC GETTERS ==============

    public Vector2 GetMovementDirection() => _moveDirection;
    public Vector2 GetLookDirection() => _lookDirection;
    public bool IsMoving() => _isMoving;
    public bool IsSprinting() => _isSprinting;
    public bool IsAlive() => _isAlive;
    public Vector3 GetWorldPosition() => transform.position;
    public float GetCurrentSpeed() => _rb.linearVelocity.magnitude;

    // ============== PUBLIC SETTERS ==============

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0, speed);
    }

    public void SetSprintSpeed(float speed)
    {
        sprintSpeed = Mathf.Max(0, speed);
    }

    public void SetCanMove(bool canMove)
    {
        _canMove = canMove;
        if (!canMove)
        {
            _rb.linearVelocity = Vector2.zero;
            _isMoving = false;
        }
    }

    public void SetAlive(bool alive)
    {
        _isAlive = alive;
        if (!alive)
        {
            _rb.linearVelocity = Vector2.zero;
            animator?.SetBool("IsDead", true);
        }
    }

    // ============== UTILITIES ==============

    /// <summary>
    /// Player को given position पर teleport करता है
    /// </summary>
    public void Teleport(Vector3 newPosition)
    {
        transform.position = newPosition;
        _rb.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// Player को freeze करता है (किसी duration के लिए)
    /// </summary>
    public void Freeze(float duration)
    {
        StartCoroutine(FreezeCoroutine(duration));
    }

    private System.Collections.IEnumerator FreezeCoroutine(float duration)
    {
        SetCanMove(false);
        yield return new WaitForSeconds(duration);
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.Playing)
        {
            SetCanMove(true);
        }
    }

    /// <summary>
    /// Player को knockback देता है
    /// </summary>
    public void ApplyKnockback(Vector2 direction, float force)
    {
        _rb.linearVelocity = direction.normalized * force;
    }

    // ============== DEBUG ==============

    public void PrintDebugInfo()
    {
        Debug.Log("=== PLAYER DEBUG INFO ===");
        Debug.Log($"Position: {transform.position}");
        Debug.Log($"Movement: {_moveDirection}");
        Debug.Log($"Look Direction: {_lookDirection}");
        Debug.Log($"Is Moving: {_isMoving}");
        Debug.Log($"Is Sprinting: {_isSprinting}");
        Debug.Log($"Current Speed: {GetCurrentSpeed():F2}");
        Debug.Log($"Can Move: {_canMove}");
        Debug.Log($"Is Alive: {_isAlive}");
    }

    private void OnGUI()
    {
        // Debug information display करने के लिए (optional)
        if (GameManager.Instance == null)
            return;

        GUILayout.BeginArea(new Rect(10, 10, 200, 100));
        GUILayout.Box("Player Debug");
        GUILayout.Label($"Pos: {transform.position:F2}");
        GUILayout.Label($"Speed: {GetCurrentSpeed():F2}");
        GUILayout.Label($"Moving: {_isMoving}");
        GUILayout.Label($"Sprint: {_isSprinting}");
        GUILayout.EndArea();
    }
}