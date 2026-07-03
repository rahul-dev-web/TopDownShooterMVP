/// <summary>
/// InputManager - Input Handling System
/// Keyboard, Mouse, और Joystick inputs को handle करता है
/// 
/// Usage:
/// GameManager.Instance.GetInputManager().GetMovementInput();
/// GameManager.Instance.GetInputManager().GetAimDirection();
/// </summary>

using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    // Input events
    public static event Action<Vector2> OnMovementInput;
    public static event Action<Vector2> OnAimInput;
    public static event Action OnFireInput;
    public static event Action OnReloadInput;
    public static event Action OnJumpInput;
    public static event Action OnSprintInput;
    public static event Action OnWeaponSwitchInput;
    public static event Action OnPauseInput;
    public static event Action OnGrenade;

    // Input values (cache करते हैं ताकि frame के दौरान access करते रहें)
    private Vector2 _movementInput;
    private Vector2 _aimInput;
    private float _currentSensitivity = 1f;

    // Input settings
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float joystickDeadzone = 0.1f;
    [SerializeField] private bool useJoystick = true;

    // Platform detection
    private bool _isMobile;

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        Debug.Log("[InputManager] Initializing...");

        // Detect platform
        _isMobile = Application.platform == RuntimePlatform.Android || 
                    Application.platform == RuntimePlatform.IPhonePlayer;

        _currentSensitivity = mouseSensitivity;

        Debug.Log($"[InputManager] ✓ Initialized (Mobile: {_isMobile})");
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.Playing)
        {
            HandleMovementInput();
            HandleAimInput();
            HandleActionInput();
        }

        HandlePauseInput(); // Pause हर state में काम करेगा
    }

    // ============== MOVEMENT INPUT ==============

    private void HandleMovementInput()
    {
        _movementInput = Vector2.zero;

        // Keyboard input (WASD / Arrow Keys)
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            _movementInput.y += 1;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            _movementInput.y -= 1;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            _movementInput.x -= 1;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            _movementInput.x += 1;

        // Joystick input (left stick)
        if (useJoystick)
        {
            Vector2 joystickInput = new Vector2(
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical")
            );

            // Deadzone apply करो
            if (joystickInput.magnitude > joystickDeadzone)
            {
                _movementInput += joystickInput;
            }
        }

        // Normalize करो ताकि diagonal movement भी 1 speed maintain करे
        if (_movementInput.magnitude > 1f)
        {
            _movementInput = _movementInput.normalized;
        }

        OnMovementInput?.Invoke(_movementInput);
    }

    // ============== AIM INPUT ==============

    private void HandleAimInput()
    {
        // Mouse aim (Desktop)
        if (!_isMobile && !useJoystick)
        {
            Vector3 mousePosition = Input.mousePosition;
            _aimInput = new Vector2(mousePosition.x, mousePosition.y);
        }
        // Joystick aim (right stick)
        else if (useJoystick)
        {
            _aimInput = new Vector2(
                Input.GetAxis("Horizontal_Right"),
                Input.GetAxis("Vertical_Right")
            );

            // Deadzone apply करो
            if (_aimInput.magnitude < joystickDeadzone)
            {
                _aimInput = Vector2.zero;
            }
            else
            {
                _aimInput = _aimInput.normalized * (_aimInput.magnitude - joystickDeadzone) / (1f - joystickDeadzone);
            }
        }

        OnAimInput?.Invoke(_aimInput);
    }

    // ============== ACTION INPUT ==============

    private void HandleActionInput()
    {
        // Fire (Left Mouse / Gamepad RT)
        if (Input.GetMouseButton(0) || Input.GetButton("Fire1"))
        {
            OnFireInput?.Invoke();
        }

        // Reload (R key / Gamepad X)
        if (Input.GetKeyDown(KeyCode.R) || Input.GetButtonDown("Reload"))
        {
            OnReloadInput?.Invoke();
        }

        // Jump (Space / Gamepad A)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump"))
        {
            OnJumpInput?.Invoke();
        }

        // Sprint (Shift / Gamepad LB)
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetButton("Sprint"))
        {
            OnSprintInput?.Invoke();
        }

        // Weapon Switch (Numbers 1-4 / Gamepad D-Pad)
        if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Switch"))
        {
            OnWeaponSwitchInput?.Invoke();
        }

        // Grenade (G key / Gamepad Y)
        if (Input.GetKeyDown(KeyCode.G) || Input.GetButtonDown("Grenade"))
        {
            OnGrenade?.Invoke();
        }
    }

    // ============== PAUSE INPUT ==============

    private void HandlePauseInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnPauseInput?.Invoke();
        }
    }

    // ============== PUBLIC GETTERS ==============

    public Vector2 GetMovementInput() => _movementInput;
    public Vector2 GetAimInput() => _aimInput;

    /// <summary>
    /// Mouse position को world coordinates में convert करता है
    /// </summary>
    public Vector3 GetMouseWorldPosition(Camera camera = null)
    {
        if (camera == null)
            camera = Camera.main;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f; // Some distance from camera

        return camera.ScreenToWorldPoint(mousePos);
    }

    /// <summary>
    /// Joystick से aim direction निकालता है
    /// </summary>
    public Vector2 GetJoystickAimDirection()
    {
        return _aimInput.normalized;
    }

    // ============== SETTINGS ==============

    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 3f);
        _currentSensitivity = mouseSensitivity;
    }

    public void SetJoystickDeadzone(float deadzone)
    {
        joystickDeadzone = Mathf.Clamp01(deadzone);
    }

    public void SetUseJoystick(bool use)
    {
        useJoystick = use;
    }

    public float GetMouseSensitivity() => mouseSensitivity;
    public float GetJoystickDeadzone() => joystickDeadzone;
    public bool GetUseJoystick() => useJoystick;
    public bool IsMobileDevice() => _isMobile;

    // ============== DEBUG ==============

    public void PrintInputStatus()
    {
        Debug.Log("=== INPUT STATUS ===");
        Debug.Log($"Movement: {_movementInput}");
        Debug.Log($"Aim: {_aimInput}");
        Debug.Log($"Mouse Sensitivity: {mouseSensitivity}");
        Debug.Log($"Joystick Deadzone: {joystickDeadzone}");
        Debug.Log($"Is Mobile: {_isMobile}");
    }
}