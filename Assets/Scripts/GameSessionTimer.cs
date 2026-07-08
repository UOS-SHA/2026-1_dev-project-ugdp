using UnityEngine;

/// <summary>
/// 스테이지 시작부터 종료(성공 또는 실패)까지의 경과 시간을
/// 앱 실행 동안 메모리에 저장하는 정적 클래스.
/// 씬 전환 간에도 값이 유지되며, 앱을 종료하면 초기화된다.
/// StageManager(성공)와 GameManager(실패) 양쪽에서 공용으로 사용한다.
/// </summary>
public static class GameSessionTimer
{
    private static float _startTime;
    private static float _finalElapsedTime;
    private static bool _isRunning;
    private static bool _hasResult;

    /// <summary>
    /// 타이머를 시작한다. 이미 실행 중이거나 이미 결과가 기록된 상태라면
    /// 아무 동작도 하지 않는다 (스테이지가 여러 개로 이어져도 전체 플레이 시간을 유지하기 위함).
    /// </summary>
    public static void StartTimer()
    {
        if (_isRunning || _hasResult)
            return;

        _startTime = Time.time;
        _isRunning = true;
    }

    /// <summary>
    /// 타이머를 종료하고 최종 경과 시간을 고정한다.
    /// 성공(StageManager)과 실패(GameManager) 양쪽에서 호출되며,
    /// 중복 호출되어도 두 번째 호출은 무시된다.
    /// </summary>
    public static void StopTimer()
    {
        if (!_isRunning)
            return;

        _finalElapsedTime = Time.time - _startTime;
        _isRunning = false;
        _hasResult = true;
    }

    /// <summary>
    /// 현재 경과 시간을 반환한다.
    /// 타이머가 실행 중이면 실시간 값을, 종료되었으면 고정된 최종 값을 반환한다.
    /// HUD(실시간 표시)와 Terminal(최종 결과 표시) 양쪽에서 동일한 메서드로 사용 가능하다.
    /// </summary>
    public static float GetElapsedTime()
    {
        return _isRunning ? (Time.time - _startTime) : _finalElapsedTime;
    }

    /// <summary>
    /// 새로운 플레이 세션을 위해 완전히 초기화한다.
    /// 예: Terminal 씬에서 "다시 시작" 버튼을 눌러 Stage01로 돌아갈 때 호출.
    /// </summary>
    public static void ResetTimer()
    {
        _startTime = 0f;
        _finalElapsedTime = 0f;
        _isRunning = false;
        _hasResult = false;
    }
}