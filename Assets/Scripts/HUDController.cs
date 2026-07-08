using UnityEngine;
using TMPro;

/// <summary>
/// 게임 플레이 중 Fuel, Speed, Distance, Time 정보를 실시간으로 표시하는 HUD 전용 컨트롤러.
/// 게임플레이 로직(PlayerController, GravityBody, OrbitPredictor)은 참조만 하며 수정하지 않는다.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Gameplay References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GravityBody gravityBody;
    [SerializeField] private Transform endPoint;

    [Header("UI References (TextMeshPro)")]
    [SerializeField] private TMP_Text fuelText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text timeText;

    private Transform playerTransform;

    private void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        playerTransform = playerController.transform;
    }

    private void Update()
    {
        UpdateFuelText();
        UpdateSpeedText();
        UpdateDistanceText();
        UpdateTimeText();
    }

    private bool ValidateReferences()
    {
        bool isValid = true;

        if (playerController == null) { Debug.LogError($"[{nameof(HUDController)}] PlayerController 참조가 비어있습니다.", this); isValid = false; }
        if (gravityBody == null) { Debug.LogError($"[{nameof(HUDController)}] GravityBody 참조가 비어있습니다.", this); isValid = false; }
        if (endPoint == null) { Debug.LogError($"[{nameof(HUDController)}] End Transform 참조가 비어있습니다.", this); isValid = false; }
        if (fuelText == null) { Debug.LogError($"[{nameof(HUDController)}] Fuel TMP_Text 참조가 비어있습니다.", this); isValid = false; }
        if (speedText == null) { Debug.LogError($"[{nameof(HUDController)}] Speed TMP_Text 참조가 비어있습니다.", this); isValid = false; }
        if (distanceText == null) { Debug.LogError($"[{nameof(HUDController)}] Distance TMP_Text 참조가 비어있습니다.", this); isValid = false; }
        if (timeText == null) { Debug.LogError($"[{nameof(HUDController)}] Time TMP_Text 참조가 비어있습니다.", this); isValid = false; }

        return isValid;
    }

    private void UpdateFuelText()
    {
        fuelText.text = $"Fuel : {playerController.CurrentFuel:F0}";
    }

    private void UpdateSpeedText()
    {
        float speed = gravityBody.Velocity.magnitude;
        speedText.text = $"Speed : {speed:F2}";
    }

    private void UpdateDistanceText()
    {
        float distance = Vector3.Distance(playerTransform.position, endPoint.position);
        distanceText.text = $"Distance : {distance:F2}";
    }

    private void UpdateTimeText()
    {
        float elapsed = GameSessionTimer.GetElapsedTime();
        int minutes = Mathf.FloorToInt(elapsed / 60f);
        int seconds = Mathf.FloorToInt(elapsed % 60f);
        timeText.text = $"Time : {minutes:D2}:{seconds:D2}";
    }
}