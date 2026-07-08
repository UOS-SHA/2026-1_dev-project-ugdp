using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 우주선의 입력, 3축 회전, 추진, 연료를 관리합니다.
/// 입력 캐싱과 FixedUpdate 기반의 물리 동기화 구조를 따릅니다.
/// </summary>
[RequireComponent(typeof(GravityBody))]
public class PlayerController : MonoBehaviour
{
    [Header("Rotation Speed (Pitch / Yaw / Roll)")]
    [Tooltip("X = Pitch, Y = Yaw, Z = Roll")]
    [SerializeField] private Vector3 rotationSpeed = new Vector3(60f, 60f, 60f);

    [Header("Thrust Settings")]
    [Tooltip("FixedUpdate마다 적용되는 속도 증가량")]
    [SerializeField] private float thrustStrength = 10f;
    [SerializeField] private Vector3 localThrustDirection = Vector3.forward;
    [SerializeField] private float maxFuel = 1000f;
    [SerializeField] private float fuelConsumptionRate = 10f;

    [Header("References")]
    [SerializeField] private OrbitPredictor orbitPredictor;

    [Header("Status")]
    [SerializeField] private float _currentFuel;

    [SerializeField] private GameManager gm;


    private InputAction _pitchAction;
    private InputAction _yawAction;
    private InputAction _rollAction;
    private InputAction _thrustAction;

    private float _pitchInput;
    private float _yawInput;
    private float _rollInput;
    private bool _thrustInput;
    private bool _orbitNeedsRebuild;

    private GravityBody _gravityBody;
    private PlayerInput _playerInput;

    public float CurrentFuel => _currentFuel;
    public float FuelPercent => maxFuel > 0 ? Mathf.Clamp01(_currentFuel / maxFuel) : 0f;
    public bool HasFuel => _currentFuel > 0.01f;
    public bool IsThrusting => _thrustInput && HasFuel;

    private void Awake()
    {


        _gravityBody = GetComponent<GravityBody>();
        _playerInput = GetComponent<PlayerInput>();

        if (_gravityBody == null)
        {
            Debug.LogError($"GravityBody missing on {gameObject.name}");
            enabled = false;
            return;
        }

        if (_playerInput == null)
        {
            Debug.LogError($"PlayerInput missing on {gameObject.name}");
            enabled = false;
            return;
        }



        _pitchAction = _playerInput.actions.FindAction("Pitch");
        _yawAction = _playerInput.actions.FindAction("Yaw");
        _rollAction = _playerInput.actions.FindAction("Roll");
        _thrustAction = _playerInput.actions.FindAction("Thrust");


        if (_pitchAction == null || _yawAction == null || _rollAction == null || _thrustAction == null)
        {
            Debug.LogError("Missing one or more required Input Actions.");
            enabled = false;
            return;
        }

        // Action 명시적 활성화
        _pitchAction.Enable();
        _yawAction.Enable();
        _rollAction.Enable();
        _thrustAction.Enable();

        localThrustDirection.Normalize();
        _currentFuel = maxFuel;
    }

    private void OnDestroy()
    {
        // Action 명시적 비활성화
        _pitchAction?.Disable();
        _yawAction?.Disable();
        _rollAction?.Disable();
        _thrustAction?.Disable();
    }

    public void RefillFuel() => _currentFuel = maxFuel;

    private void Update()
    {
        _pitchInput = _pitchAction.ReadValue<float>();
        _yawInput = _yawAction.ReadValue<float>();
        _rollInput = _rollAction.ReadValue<float>();
        _thrustInput = _thrustAction.IsPressed();

    }

    private void FixedUpdate()
    {
        _orbitNeedsRebuild = false;

        HandleRotation();
        HandleThrust();

        if (_orbitNeedsRebuild && orbitPredictor != null)
        {
            orbitPredictor.RequestUpdate();
        }
    }

    private void HandleRotation()
    {
        float pitch = _pitchInput * rotationSpeed.x * Time.fixedDeltaTime;
        float yaw = _yawInput * rotationSpeed.y * Time.fixedDeltaTime;
        float roll = _rollInput * rotationSpeed.z * Time.fixedDeltaTime;

        if (Mathf.Approximately(pitch, 0f) &&
            Mathf.Approximately(yaw, 0f) &&
            Mathf.Approximately(roll, 0f))
        {
            return;
        }

        transform.Rotate(Vector3.right, pitch, Space.Self);
        transform.Rotate(Vector3.up, yaw, Space.Self);
        transform.Rotate(Vector3.forward, roll, Space.Self);

        _orbitNeedsRebuild = true;
    }

    private void HandleThrust()
    {
        if (!IsThrusting)
        {
            return;
        }

        _currentFuel = Mathf.Max(0f, _currentFuel - fuelConsumptionRate * Time.fixedDeltaTime);

        Vector3 thrustDirection = transform.TransformDirection(localThrustDirection);
        Vector3 thrustVelocity = thrustDirection * thrustStrength;

        _gravityBody.AddVelocity(thrustVelocity);

        _orbitNeedsRebuild = true;
    }
}