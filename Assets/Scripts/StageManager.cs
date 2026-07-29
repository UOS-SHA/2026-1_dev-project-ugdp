using Game.Flow;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform _startPoint;
    [SerializeField] private GravityBody _spaceship;
    [SerializeField] private StageUI _stageUI;
    [Header("Stage Settings")]
    [SerializeField] private string _nextSceneName;
    [SerializeField] private float _clearDelaySeconds = 2f;

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
    public void StageClear()
    {
        if (!GameFlowController.Instance.TryFinalize(GameResult.Cleared))
            return;

        GameSessionTimer.StopTimer();

        // StageManager는 다음 Stage 번호나 해금 상태를 직접 알지 않는다.
        // 진행도 갱신은 전적으로 GameProgress의 책임이다.
        GameProgress.UnlockNextStage();

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