using UnityEngine;

/// <summary>
/// 중력 시뮬레이션에 참여하는 모든 물체의 공통 물리 상태를 담당한다.
/// </summary>
public class GravityBody : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private float mass = 1f;
    [SerializeField] private Vector3 initialVelocity = Vector3.zero;

    [Header("Manager")]
    [SerializeField] private GravityManager gravityManager;

    public float Mass => mass;

    /// <summary>
    /// 현재 속도
    /// </summary>
    public Vector3 Velocity { get; private set; }

    /// <summary>
    /// 현재 위치
    /// </summary>
    public Vector3 Position
    {
        get => transform.position;
        set => transform.position = value;
    }

    private void Awake()
    {
        Velocity = initialVelocity;

        // Manager를 지정하지 않았다면 자동 탐색
        if (gravityManager == null)
            gravityManager = FindFirstObjectByType<GravityManager>();
    }

    private void OnEnable()
    {
        if (gravityManager != null)
            gravityManager.Register(this);
    }

    private void OnDisable()
    {
        if (gravityManager != null)
            gravityManager.Unregister(this);
    }

    /// <summary>
    /// 속도 증가
    /// </summary>
    public void AddVelocity(Vector3 delta)
    {
        Velocity += delta;
    }

    /// <summary>
    /// 속도 직접 설정
    /// </summary>
    public void SetVelocity(Vector3 value)
    {
        Velocity = value;
    }

    /// <summary>
    /// 가속도 적용
    /// </summary>
    public void ApplyAcceleration(Vector3 acceleration, float dt)
    {
        Velocity += acceleration * dt;
    }

    /// <summary>
    /// 이동
    /// </summary>
    public void Move(float dt)
    {
        Position += Velocity * dt;
    }

    /// <summary>
    /// 현재 상태 복사
    /// </summary>
    public GravityBodyState GetState()
    {
        return new GravityBodyState
        {
            Position = Position,
            Velocity = Velocity,
            Mass = Mass
        };
    }

    /// <summary>
    /// Velocity 디버그 표시
    /// </summary>
    public void DrawVelocity(Color color, float scale = 1f)
    {
        Debug.DrawRay(Position, Velocity * scale, color);
    }
}