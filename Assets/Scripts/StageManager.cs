using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Stage01의 전체 진행을 담당한다.
/// Spaceship을 시작 위치로 배치하고, 클리어 시 UI 출력 후 다음 씬을 로드한다.
/// </summary>
public class StageManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform _startPoint;
    [SerializeField] private GravityBody _spaceship;
    [SerializeField] private StageUI _stageUI;
    [Header("Stage Settings")]
    [SerializeField] private string _nextSceneName;
    [SerializeField] private float _clearDelaySeconds = 2f;
    private bool _isCleared;

    private void Start()
    {
        PlaceSpaceshipAtStart();
        GameSessionTimer.StartTimer();
    }

    private void PlaceSpaceshipAtStart()
    {
        _spaceship.Position = _startPoint.position;
        _spaceship.SetVelocity(Vector3.zero);
    }

    /// <summary>
    /// StageGoal에서 우주선 도착을 감지했을 때 호출된다.
    /// Trigger가 여러 프레임 동안 겹쳐있어도 한 번만 처리되도록 방어한다.
    /// </summary>
    public void StageClear()
    {
        if (_isCleared)
            return;
        _isCleared = true;
        GameSessionTimer.StopTimer();
        _stageUI.ShowMissionComplete();
        StartCoroutine(LoadNextSceneAfterDelay());
    }

    private IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(_clearDelaySeconds);
        _stageUI.ShowLoading(_nextSceneName);
        SceneManager.LoadScene(_nextSceneName);
    }
}