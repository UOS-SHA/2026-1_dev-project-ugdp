using UnityEngine;

/// <summary>
/// GravityBody의 물리 상태를 순수 데이터로 표현한다.
/// Transform(씬 오브젝트)과 분리되어 있으므로 Orbit Predictor에서
/// 실제 GameObject를 움직이지 않고 미래 궤도를 계산할 수 있다.
/// </summary>
public struct GravityBodyState
{
    public Vector3 Position;
    public Vector3 Velocity;
    public float Mass;

    /// <summary>
    /// Explicit Euler로 한 스텝 이동한다.
    /// GravityManager.StepPhysics()와 동일한 적분 방식을 사용한다.
    /// </summary>
    /// <param name="acceleration">이번 스텝에 적용할 가속도</param>
    /// <param name="dt">시간 간격</param>
    public void Step(Vector3 acceleration, float dt)
    {
        Velocity += acceleration * dt;
        Position += Velocity * dt;
    }
}