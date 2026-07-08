using UnityEngine;

/// <summary>
/// 현재 시뮬레이션 상태의 스냅샷을 기반으로 미래 궤도를 예측하고
/// LineRenderer로 시각화하는 컴포넌트.
///
/// 실제 GravityBody, Transform, GameObject를 절대로 참조하지 않으며,
/// GravityBodyState[] 배열과 GravityCalculator만을 사용하여
/// 실제 시뮬레이션과 완전히 독립적으로 동작한다.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class OrbitPredictor : MonoBehaviour
{
    [Header("References")]
    [Tooltip("중력 시뮬레이션 관리자. 현재 상태 스냅샷을 가져오는 데 사용된다.")]
    [SerializeField] private GravityManager gravityManager;

    [Tooltip("추적할 GravityBody. 인덱스 대신 참조를 직접 지정한다.\n" +
             "Body가 런타임에 파괴/생성되어도 추적 대상이 바뀌지 않는다.")]
    [SerializeField] private GravityBody trackedBody;

    [Header("Prediction Settings")]
    [SerializeField] private int predictionSteps = 500;
    [SerializeField] private float predictionTimeStep = 0.02f;
    [SerializeField] private float maxPredictionDistance = 0f;

    [Header("Update Settings")]
    [SerializeField] private bool updateEveryFrame = true;

    [Header("Line Settings")]
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Color lineColor = new Color(0.4f, 0.8f, 1f, 0.6f);

    private LineRenderer _lineRenderer;
    private Vector3[] _predictedPositions;
    private Vector3[] _accelerationBuffer;
    private GravityBodyState[] _stateBuffer;
    private int _validPointCount;
    private bool _updateRequested;

    /// <summary>
    /// trackedBody가 유효하지 않아 직전 프레임에 경고를 출력했는지 여부.
    /// 동일 원인으로 매 프레임 경고가 중복 출력되는 것을 막기 위한 플래그.
    /// 원인이 해소되면(트래킹 재성공 시) 다시 false로 리셋된다.
    /// </summary>
    private bool _wasTrackingInvalid;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        InitializeLineRenderer();
        AllocateBuffers();
    }

    private void Update()
    {
        if (updateEveryFrame || _updateRequested)
        {
            UpdateOrbitPrediction();
            _updateRequested = false;
        }
    }

    public void RequestUpdate() => _updateRequested = true;

    public void SetVisible(bool visible) => _lineRenderer.enabled = visible;

    public void SetLineColor(Color color)
    {
        lineColor = color;
        ApplyLineColor();
    }

    public void SetLineMaterial(Material material)
    {
        lineMaterial = material;
        if (_lineRenderer != null)
            _lineRenderer.material = material;
    }

    /// <summary>
    /// 추적할 대상을 런타임에 교체한다 (예: 우주선 발사 직전 타겟 지정).
    /// </summary>
    public void SetTrackedBody(GravityBody body)
    {
        trackedBody = body;
    }

    private void UpdateOrbitPrediction()
    {
        if (gravityManager == null)
        {
            WarnOnce("[OrbitPredictor] gravityManager가 지정되지 않았습니다.");
            ClearOrbit();
            return;
        }

        if (trackedBody == null)
        {
            WarnOnce("[OrbitPredictor] trackedBody가 지정되지 않았습니다.");
            ClearOrbit();
            return;
        }

        int validCount = gravityManager.CopySimulationStateTo(ref _stateBuffer);

        if (validCount == 0)
        {
            ClearOrbit();
            return;
        }

        int trackedIndex = gravityManager.GetCompactedIndex(trackedBody);

        if (trackedIndex < 0)
        {
            WarnOnce(
                $"[OrbitPredictor] trackedBody({trackedBody.name})가 " +
                $"GravityManager에 등록되어 있지 않다. (비활성화 상태일 수 있음)");
            ClearOrbit();
            return;
        }

        _wasTrackingInvalid = false;

        EnsureAccelerationBufferCapacity(validCount);
        RunPredictionSimulation(_stateBuffer, validCount, trackedIndex);
        RenderPredictedOrbit();
    }

    /// <summary>
    /// 동일한 invalid 상태가 지속되는 동안 경고가 매 프레임 반복 출력되는 것을
    /// 막기 위해, 상태가 바뀐 시점(invalid로 처음 진입한 프레임)에만 로그를 남긴다.
    /// </summary>
    private void WarnOnce(string message)
    {
        if (_wasTrackingInvalid)
            return;

        Debug.LogWarning(message);
        _wasTrackingInvalid = true;
    }

    /// <summary>
    /// 추적 대상이 없을 때 궤도선을 비운다.
    /// LineRenderer의 enabled(가시성)는 건드리지 않는다 — "그릴 데이터가 없음"과
    /// "사용자가 숨기기로 결정함"은 서로 다른 개념이므로 분리해서 관리한다.
    /// </summary>
    private void ClearOrbit()
    {
        _validPointCount = 0;
        _lineRenderer.positionCount = 0;
    }

    private void RunPredictionSimulation(GravityBodyState[] states, int count, int trackedIndex)
    {
        Vector3 startPosition = states[trackedIndex].Position;
        _validPointCount = 0;

        for (int step = 0; step < predictionSteps; step++)
        {
            if (ShouldStopEarly(states[trackedIndex].Position, startPosition))
                break;

            _predictedPositions[_validPointCount] = states[trackedIndex].Position;
            _validPointCount++;

            SimulateStep(states, count);
        }
    }

    private void SimulateStep(GravityBodyState[] states, int count)
    {
        GravityCalculator.CalculateAccelerations(
            states,
            _accelerationBuffer,
            count,
            gravityManager.GravitationalConstant,
            gravityManager.MinimumDistanceSqr
        );

        for (int i = 0; i < count; i++)
        {
            states[i].Step(_accelerationBuffer[i], predictionTimeStep);
        }
    }

    private bool ShouldStopEarly(Vector3 currentPosition, Vector3 startPosition)
    {
        if (maxPredictionDistance <= 0f)
            return false;

        return (currentPosition - startPosition).sqrMagnitude
               > maxPredictionDistance * maxPredictionDistance;
    }

    private void InitializeLineRenderer()
    {
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.loop = false;
        _lineRenderer.startWidth = lineWidth;
        _lineRenderer.endWidth = lineWidth;

        if (lineMaterial != null)
            _lineRenderer.material = lineMaterial;

        ApplyLineColor();
    }

    private void RenderPredictedOrbit()
    {
        _lineRenderer.positionCount = _validPointCount;

        if (_validPointCount > 0)
            _lineRenderer.SetPositions(_predictedPositions);
    }

    private void ApplyLineColor()
    {
        if (_lineRenderer == null)
            return;

        _lineRenderer.startColor = lineColor;
        _lineRenderer.endColor = new Color(lineColor.r, lineColor.g, lineColor.b, 0f);
    }

    private void AllocateBuffers()
    {
        _predictedPositions = new Vector3[predictionSteps];
        _accelerationBuffer = System.Array.Empty<Vector3>();
        _stateBuffer = System.Array.Empty<GravityBodyState>();
    }

    private void EnsureAccelerationBufferCapacity(int validCount)
    {
        if (_predictedPositions == null || _predictedPositions.Length != predictionSteps)
            _predictedPositions = new Vector3[predictionSteps];

        if (_accelerationBuffer == null || _accelerationBuffer.Length != validCount)
            _accelerationBuffer = new Vector3[validCount];
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_lineRenderer == null)
            _lineRenderer = GetComponent<LineRenderer>();

        if (_lineRenderer == null)
            return;

        _lineRenderer.startWidth = lineWidth;
        _lineRenderer.endWidth = lineWidth;
        ApplyLineColor();
    }
#endif
}