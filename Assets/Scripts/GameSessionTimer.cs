using UnityEngine;

public static class GameSessionTimer
{
    private static float _startTime;
    private static float _finalElapsedTime;
    private static bool _isRunning;
    private static bool _hasResult;

    /// <summary>
    /// 타이머를 시작슨
    /// </summary>
    public static void StartTimer()
    {
        if (_isRunning || _hasResult)
            return;

        _startTime = Time.time;
        _isRunning = true;
    }

    
    public static void StopTimer()
    {
        if (!_isRunning)
            return;

        _finalElapsedTime = Time.time - _startTime;
        _isRunning = false;
        _hasResult = true;
    }

    public static float GetElapsedTime()
    {
        return _isRunning ? (Time.time - _startTime) : _finalElapsedTime;
    }

    public static void ResetTimer()
    {
        _startTime = 0f;
        _finalElapsedTime = 0f;
        _isRunning = false;
        _hasResult = false;
    }
}