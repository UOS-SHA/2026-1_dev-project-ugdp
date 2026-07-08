using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // 추가

public class GameManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private GravityBody _spaceship;
    [SerializeField] private EndGameUI _endGameUI;

    [Header("Fail Settings")]
    [SerializeField] private string _nextSceneName;
    [SerializeField] private float _restartDelay = 2f;

    private bool _gameHasEnded;

    private void Update()
    {
        // 디버그/테스트용: R키를 누르면 강제로 Mission Failed를 트리거한다.
        // Keyboard.current가 null인 경우(키보드 미연결 등)를 대비해 null 체크 포함.
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            EndGame();
        }
    }

    public void EndGame()
    {
        if (_gameHasEnded)
            return;

        if (_spaceship == null || _endGameUI == null)
        {
            Debug.LogError($"[{nameof(GameManager)}] 필수 참조가 비어있어 EndGame을 진행할 수 없습니다. Inspector 연결을 확인하세요.", this);
            return;
        }

        _gameHasEnded = true;
        GameSessionTimer.StopTimer();

        _spaceship.SetVelocity(Vector3.zero);

        PlayerController playerController = _spaceship.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        _endGameUI.ShowMissionFailed();
        StartCoroutine(LoadEndSceneAfterDelay());
    }

    private IEnumerator LoadEndSceneAfterDelay()
    {
        yield return new WaitForSeconds(_restartDelay);

        _endGameUI.ShowEndLoading(_nextSceneName);

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(_nextSceneName);
    }
}