using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// 물리 시뮬레이션 상태를 실시간으로 표시하는 디버그 UI.
/// GravityManager.Bodies를 순회하므로 물체 수 변화에 자동 대응한다.
/// </summary>
public class DebugUI : MonoBehaviour
{
    public enum DebugLevel
    {
        Basic,
        Detailed,
        Physics
    }

    [Header("References")]
    [SerializeField] private TMP_Text debugText;
    [SerializeField] private GravityManager gravityManager;

    [Header("Display")]
    [SerializeField] private DebugLevel debugLevel = DebugLevel.Detailed;

    private readonly StringBuilder sb = new StringBuilder();

    private void Update()
    {
        if (debugText == null || gravityManager == null)
            return;

        IReadOnlyList<GravityBody> bodies = gravityManager.Bodies;

        if (bodies == null || bodies.Count == 0)
            return;

        debugText.text = BuildDebugText(bodies);
    }

    private string BuildDebugText(IReadOnlyList<GravityBody> bodies)
    {
        sb.Clear();

        float totalKE = 0f;
        float totalPE = 0f;
        Vector3 totalMomentum = Vector3.zero;

        // -----------------------------
        // 물체별 정보
        // -----------------------------
        foreach (GravityBody body in bodies)
        {
            if (body == null)
                continue;

            float ke = 0.5f * body.Mass * body.Velocity.sqrMagnitude;
            Vector3 momentum = body.Mass * body.Velocity;

            totalKE += ke;
            totalMomentum += momentum;

            if (debugLevel >= DebugLevel.Detailed)
            {
                sb.AppendLine($"[{body.name}]");
                sb.AppendLine($"Position : {body.Position}");
                sb.AppendLine($"Velocity : {body.Velocity}");
                sb.AppendLine($"Speed    : {body.Velocity.magnitude:F3}");
                sb.AppendLine($"Mass     : {body.Mass:F2}");
                sb.AppendLine($"KE       : {ke:F3}");
                sb.AppendLine();
            }
        }

        // -----------------------------
        // 퍼텐셜 에너지
        // -----------------------------
        float G = gravityManager.GravitationalConstant;

        for (int i = 0; i < bodies.Count; i++)
        {
            if (bodies[i] == null)
                continue;

            for (int j = i + 1; j < bodies.Count; j++)
            {
                if (bodies[j] == null)
                    continue;

                float distance = Vector3.Distance(
                    bodies[i].Position,
                    bodies[j].Position);

                if (distance < gravityManager.MinimumDistance)
                    continue;

                float pe =
                    -G *
                    bodies[i].Mass *
                    bodies[j].Mass /
                    distance;

                totalPE += pe;

                if (debugLevel == DebugLevel.Physics)
                {
                    sb.AppendLine(
                        $"{bodies[i].name} ↔ {bodies[j].name}");

                    sb.AppendLine(
                        $"Distance : {distance:F3}");

                    sb.AppendLine(
                        $"PE       : {pe:F3}");

                    sb.AppendLine();
                }
            }
        }

        // -----------------------------
        // 총합
        // -----------------------------
        sb.AppendLine("-----------------------");
        sb.AppendLine($"Total KE : {totalKE:F3}");
        sb.AppendLine($"Total PE : {totalPE:F3}");
        sb.AppendLine($"Energy   : {(totalKE + totalPE):F3}");
        sb.AppendLine($"Momentum : {totalMomentum}");

        return sb.ToString();
    }
}