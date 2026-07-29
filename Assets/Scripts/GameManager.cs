using Game.Flow;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private EndGameUI _endGameUI;
    [SerializeField] private float _endSceneDelay = 2f;

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            ReturnToTerminal();
        }
    }
    public void EndGame()
    {
        if (!GameFlowController.Instance.TryFinalize(GameResult.Failed))
            return;

        _endGameUI.ShowMissionFailed();
        StartCoroutine(LoadEndSceneAfterDelay());
    }

    private void ReturnToTerminal()
    {
        if (!GameFlowController.Instance.IsGameplayActive)
            return;

        GameSessionTimer.StopTimer();
        SceneManager.LoadScene("Terminal");
    }

    private IEnumerator LoadEndSceneAfterDelay()
    {
        yield return new WaitForSeconds(_endSceneDelay);
        _endGameUI.ShowMissionFailed() ;
        _endGameUI.ShowEndLoading("Terminal");
        SceneManager.LoadScene("Terminal");
    }
}