using UnityEngine;

/// <summary>
/// 중력 가속도 계산만 담당하는 순수 계산 클래스.
/// 실제 GameObject를 전혀 참조하지 않는다.
/// </summary>
public static class GravityCalculator
{
    /// <summary>
    /// states[0..count) 범위의 Body에 대해 가속도를 계산하여
    /// accelerations[0..count)에 저장한다.
    ///
    /// states.Length나 accelerations.Length가 count보다 클 수 있다.
    /// (버퍼 재사용 시 실제 유효 데이터 범위만 count로 지정한다.)
    ///
    /// Mass가 0 이하인 항목은 계산에서 완전히 제외한다.
    /// (파괴되었거나 아직 초기화되지 않은 Body가 0으로 나누기를
    /// 유발하여 NaN/Infinity가 전체 시뮬레이션으로 전파되는 것을 방지한다.)
    /// </summary>
    public static void CalculateAccelerations(
        GravityBodyState[] states,
        Vector3[] accelerations,
        int count,
        float gravitationalConstant,
        float minimumDistanceSqr)
    {
        // 이전 프레임 값 제거 (유효 범위만)
        System.Array.Clear(accelerations, 0, count);

        // N(N-1)/2 계산
        for (int i = 0; i < count; i++)
        {
            if (states[i].Mass <= 0f)
                continue; // 유효하지 않은 Body는 힘의 발생원/대상에서 제외

            for (int j = i + 1; j < count; j++)
            {
                if (states[j].Mass <= 0f)
                    continue;

                Vector3 direction = states[j].Position - states[i].Position;
                float sqrDistance = direction.sqrMagnitude;

                if (sqrDistance < minimumDistanceSqr)
                    continue;

                float distance = Mathf.Sqrt(sqrDistance);
                Vector3 forceDir = direction / distance;

                float forceMagnitude =
                    gravitationalConstant *
                    (states[i].Mass * states[j].Mass) /
                    sqrDistance;

                // a = F / m
                accelerations[i] += forceDir * (forceMagnitude / states[i].Mass);
                accelerations[j] -= forceDir * (forceMagnitude / states[j].Mass);
            }
        }
    }
}