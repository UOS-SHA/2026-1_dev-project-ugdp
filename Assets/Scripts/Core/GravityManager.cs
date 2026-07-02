using System.Collections.Generic;
using UnityEngine;

public class GravityManager : MonoBehaviour
{
    [Header("Physics Settings")]
    [SerializeField] private float gravitationalConstant = 0.5f;
    [SerializeField] private float minimumDistance = 0.1f;

    private readonly List<GravityBody> bodies = new();

    [Header("Runtime Debug")]
    [SerializeField] private bool showDebugLog = false;

    [Tooltip("velocity º¤ÅÍ Ç¥½Ã ¹èÀ². velocity°¡ Å©¸é ÁÙÀÎ´Ù.")]
    [SerializeField] private float velocityVectorScale = 0.1f;

    [Tooltip("acceleration º¤ÅÍ Ç¥½Ã ¹èÀ². accelerationÀÌ ÀÛÀ¸¸é Å°¿î´Ù.")]
    [SerializeField] private float accelerationVectorScale = 20f;

    [Header("Gizmos - Marker")]
    [SerializeField] private Color planetMarkerColor = Color.yellow;
    [SerializeField] private float planetMarkerRadius = 0.3f;

    [Header("Gizmos - Gravity Rings")]
    [SerializeField] private Color gravityRingColor = Color.yellow;
    [SerializeField] private float[] gravityRingRadii = { 3f, 6f, 9f };

    [Header("Gizmos - Gravity Cross")]
    [SerializeField] private Color gravityCrossColor = Color.cyan;
    [SerializeField] private float gravityCrossArmLength = 2f;

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Cached Values
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private float _minDistanceSqr;

    /// <summary>
    /// GravityCalculator¿¡ Àü´ÞÇÒ °¡¼Óµµ Ãâ·Â ¹öÆÛ.
    /// ¸Å ÇÁ·¹ÀÓ ÀçÇÒ´çÇÏÁö ¾Ê±â À§ÇØ ¸â¹ö·Î À¯ÁöÇÑ´Ù.
    /// </summary>
    private Vector3[] _accelerations = System.Array.Empty<Vector3>();

    /// <summary>
    /// GravityCalculator¿¡ Àü´ÞÇÒ »óÅÂ ½º³À¼¦ ¹öÆÛ.
    /// GetSimulationState()¿Í º°µµ·Î ³»ºÎ °è»ê Àü¿ëÀ¸·Î »ç¿ëÇÑ´Ù.
    /// </summary>
    private GravityBodyState[] _stateBuffer = System.Array.Empty<GravityBodyState>();

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Public Accessors
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    public float GravitationalConstant => gravitationalConstant;
    public float MinimumDistance => minimumDistance;
    public IReadOnlyList<GravityBody> Bodies => bodies;

    public float MinimumDistanceSqr => _minDistanceSqr;

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Unity Lifecycle
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private void Start()
    {
        CacheMinDistanceSqr();
    }

    private void FixedUpdate()
    {
        if (bodies.Count < 2)
            return;

        StepPhysics(Time.fixedDeltaTime);
    }

    private void OnValidate()
    {
        CacheMinDistanceSqr();
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Physics Cycle
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    /// <summary>
    /// ÇÑ ¹°¸® ½ºÅÜÀÇ ÀüÃ¼ »çÀÌÅ¬À» ¼ø¼­´ë·Î ½ÇÇàÇÑ´Ù.
    /// °¡¼Óµµ °è»ê ¡æ velocity ¼öÁ¤ ¡æ position ÀÌµ¿ ¡æ µð¹ö±× Ç¥½Ã
    /// Pause, SlowMotion, OrbitPredictor µî ¿ÜºÎ¿¡¼­µµ Á÷Á¢ È£Ãâ °¡´ÉÇÏ´Ù.
    /// </summary>
    public void StepPhysics(float dt)
    {
        CalculateAllAccelerations();
        ApplyAccelerations(dt);
        MoveAllBodies(dt);
        DrawRuntimeDebug();
    }

    public void Register(GravityBody body)
    {
        if (body == null)
            return;

        if (!bodies.Contains(body))
            bodies.Add(body);
    }

    public void Unregister(GravityBody body)
    {
        bodies.Remove(body);
    }

    /// <summary>
    /// GravityCalculator¿¡ ÇöÀç »óÅÂ¸¦ Àü´ÞÇÏ¿© ¸ðµç ¹ÙµðÀÇ °¡¼Óµµ¸¦ °è»êÇÑ´Ù.
    /// °è»ê °á°ú´Â _accelerations ¹öÆÛ¿¡ Ã¤¿öÁø´Ù.
    ///
    /// null Body(ÆÄ±«µÇ¾ú°Å³ª Unregister ÀÌÀü ÇÑ ÇÁ·¹ÀÓ)´Â GetState() ´ë½Å
    /// default(Mass = 0)·Î Ã¤¿öÁö¸ç, GravityCalculator°¡ Mass <= 0ÀÎ
    /// Ç×¸ñÀ» °è»ê¿¡¼­ Á¦¿ÜÇÏ¹Ç·Î NaN/Infinity ÀüÆÄ¸¦ ¹æÁöÇÑ´Ù.
    /// </summary>
    private void CalculateAllAccelerations()
    {
        int count = bodies.Count;

        // ¹Ùµð ¼ö°¡ ¹Ù²ï °æ¿ì¿¡¸¸ ÀçÇÒ´ç (Æò»ó½Ã¿¡´Â GC ¾øÀ½)
        if (_accelerations.Length != count)
            _accelerations = new Vector3[count];

        if (_stateBuffer.Length != count)
            _stateBuffer = new GravityBodyState[count];

        // ÇöÀç »óÅÂ¸¦ ¹öÆÛ¿¡ º¹»ç
        for (int i = 0; i < count; i++)
        {
            _stateBuffer[i] = bodies[i] != null
                ? bodies[i].GetState()
                : default; // Mass = 0 ¡æ GravityCalculator¿¡¼­ ¾ÈÀüÇÏ°Ô Á¦¿ÜµÊ
        }

        // Áß·Â °è»êÀ» GravityCalculator¿¡ ¿ÏÀüÈ÷ À§ÀÓ
        GravityCalculator.CalculateAccelerations(
            _stateBuffer,
            _accelerations,
            count,
            gravitationalConstant,
            _minDistanceSqr
        );

        // µð¹ö±× ·Î±×
        if (showDebugLog)
        {
            for (int i = 0; i < count; i++)
            {
                if (bodies[i] == null) continue;
                Debug.Log(
                    $"[{bodies[i].name}]" +
                    $" Accel: {_accelerations[i].magnitude:F4}");
            }
        }
    }

    /// <summary>
    /// °è»êµÈ °¡¼Óµµ¸¦ °¢ ¹°Ã¼ÀÇ velocity¿¡ Àû¿ëÇÑ´Ù.
    /// </summary>
    private void ApplyAccelerations(float dt)
    {
        for (int i = 0; i < bodies.Count; i++)
        {
            if (bodies[i] == null)
                continue;

            bodies[i].ApplyAcceleration(_accelerations[i], dt);
        }
    }

    /// <summary>
    /// ¸ðµç ¹°Ã¼ÀÇ positionÀ» ÇÑ ½ºÅÜ ÀÌµ¿ÇÑ´Ù.
    /// </summary>
    private void MoveAllBodies(float dt)
    {
        for (int i = 0; i < bodies.Count; i++)
        {
            if (bodies[i] == null)
                continue;

            bodies[i].Move(dt);
        }
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Simulation State (Orbit Predictor Àü¿ë)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    /// <summary>
    /// ÇöÀç ½Ã¹Ä·¹ÀÌ¼Ç »óÅÂ¸¦ GravityBodyState ¹è¿­·Î º¹»çÇÏ¿© ¹ÝÈ¯ÇÑ´Ù.
    /// ¸Å È£Ãâ¸¶´Ù »õ ¹è¿­À» ÇÒ´çÇÏ¹Ç·Î(GC ¹ß»ý), ¸Å ÇÁ·¹ÀÓ È£ÃâÇÏ´Â
    /// ¿ëµµ(OrbitPredictor µî)¿¡´Â <see cref="CopySimulationStateTo"/>¸¦
    /// »ç¿ëÇÏ´Â °ÍÀ» ±ÇÀåÇÑ´Ù.
    /// </summary>
    public GravityBodyState[] GetSimulationState()
    {
        int count = bodies.Count;
        GravityBodyState[] states = new GravityBodyState[count];

        for (int i = 0; i < count; i++)
        {
            if (bodies[i] == null)
                continue;

            states[i] = bodies[i].GetState();
        }

        return states;
    }

    /// <summary>
    /// ÇöÀç ½Ã¹Ä·¹ÀÌ¼Ç »óÅÂ¸¦ È£ÃâÀÚ°¡ Á¦°øÇÑ ¹öÆÛ¿¡ º¹»çÇÑ´Ù.
    /// ¹öÆÛ ¿ë·®ÀÌ ºÎÁ·ÇÒ °æ¿ì¿¡¸¸ ÀçÇÒ´çÇÏ¸ç, ±× ¿Ü¿¡´Â GC°¡ ¹ß»ýÇÏÁö ¾Ê´Â´Ù.
    ///
    /// nullÀÎ Body(ÆÄ±«µÇ¾ú°Å³ª ¾ÆÁ÷ UnregisterµÇÁö ¾ÊÀº °æ¿ì)´Â °á°ú¿¡¼­
    /// Á¦¿ÜµÇ¹Ç·Î, ¹ÝÈ¯µÈ validCount´Â bodies.Countº¸´Ù ÀÛÀ» ¼ö ÀÖ´Ù.
    /// ÀÌ·Î ÀÎÇØ buffer ³»¿¡¼­ÀÇ ÀÎµ¦½º ÀÇ¹Ì°¡ ¸Å ÇÁ·¹ÀÓ ¹Ù²ð ¼ö ÀÖ´Ù´Â Á¡¿¡
    /// ÁÖÀÇÇØ¾ß ÇÑ´Ù (¿¹: trackedBodyIndex°¡ °¡¸®Å°´Â ´ë»óÀÌ ¹Ù²ð ¼ö ÀÖÀ½).
    /// </summary>
    /// <param name="buffer">Àç»ç¿ëÇÒ ¹öÆÛ. ¿ë·® ºÎÁ· ½Ã ³»ºÎ¿¡¼­ ÀçÇÒ´çÇÏ¿© ±³Ã¼ÇÑ´Ù.</param>
    /// <returns>buffer¿¡ Ã¤¿öÁø À¯È¿ÇÑ »óÅÂÀÇ °³¼ö.</returns>
    public int CopySimulationStateTo(ref GravityBodyState[] buffer)
    {
        int capacityNeeded = bodies.Count;

        if (buffer == null || buffer.Length < capacityNeeded)
            buffer = new GravityBodyState[capacityNeeded];

        int validCount = 0;
        for (int i = 0; i < bodies.Count; i++)
        {
            if (bodies[i] == null)
                continue;

            buffer[validCount] = bodies[i].GetState();
            validCount++;
        }

        return validCount;
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Debug Visualization
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private void DrawRuntimeDebug()
    {
        if (!showDebugLog)
            return;

        for (int i = 0; i < bodies.Count; i++)
        {
            if (bodies[i] == null)
                continue;

            bodies[i].DrawVelocity(Color.green, velocityVectorScale);

            Debug.DrawRay(
                bodies[i].Position,
                _accelerations[i] * accelerationVectorScale,
                Color.red);
        }
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Helpers
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private void CacheMinDistanceSqr()
    {
        _minDistanceSqr = minimumDistance * minimumDistance;
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Editor Gizmos
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private void OnDrawGizmos()
    {
        foreach (GravityBody body in bodies)
        {
            if (body == null)
                continue;

            Vector3 position = body.Position;

            DrawPlanetMarker(position);
            DrawGravityRings(position);
            DrawGravityCross(position);
        }
    }

    private void DrawPlanetMarker(Vector3 position)
    {
        Gizmos.color = planetMarkerColor;
        Gizmos.DrawSphere(position, planetMarkerRadius);
    }

    private void DrawGravityRings(Vector3 position)
    {
        if (gravityRingRadii == null)
            return;

        Gizmos.color = gravityRingColor;

        foreach (float radius in gravityRingRadii)
            Gizmos.DrawWireSphere(position, radius);
    }

    private void DrawGravityCross(Vector3 position)
    {
        float arm = gravityCrossArmLength;

        Gizmos.color = gravityCrossColor;
        Gizmos.DrawLine(
            position + Vector3.left * arm,
            position + Vector3.right * arm);
        Gizmos.DrawLine(
            position + Vector3.forward * arm,
            position + Vector3.back * arm);
    }


    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Body Lookup (Orbit Predictor Àü¿ë)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    /// <summary>
    /// ÁÖ¾îÁø GravityBody°¡ CopySimulationStateTo()·Î ¸¸µé¾îÁø
    /// ¾ÐÃàµÈ ¹è¿­¿¡¼­ ¸î ¹øÂ° ÀÎµ¦½º¿¡ À§Ä¡ÇÏ´ÂÁö ¹ÝÈ¯ÇÑ´Ù.
    ///
    /// bodies ¸®½ºÆ® ³» nullÀÎ Ç×¸ñµéÀº °Ç³Ê¶Ù°í Ä«¿îÆ®ÇÏ¹Ç·Î,
    /// CopySimulationStateTo()¿Í Á¤È®È÷ µ¿ÀÏÇÑ ¾ÐÃà ±ÔÄ¢À» µû¸¥´Ù.
    ///
    /// body°¡ nullÀÌ°Å³ª µî·ÏµÇ¾î ÀÖÁö ¾ÊÀ¸¸é -1À» ¹ÝÈ¯ÇÑ´Ù.
    /// </summary>
    public int GetCompactedIndex(GravityBody body)
    {
        if (body == null)
            return -1;

        int compactedIndex = 0;
        for (int i = 0; i < bodies.Count; i++)
        {
            if (bodies[i] == null)
                continue;

            if (bodies[i] == body)
                return compactedIndex;

            compactedIndex++;
        }

        return -1;
    }
}