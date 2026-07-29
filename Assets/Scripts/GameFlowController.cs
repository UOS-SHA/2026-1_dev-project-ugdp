using System;
using UnityEngine;

namespace Game.Flow
{
    public enum GameResult
    {
        None,
        Cleared,
        Failed
    }

    /// <summary>
    /// 게임의 성공/실패 결과를 유일하게 판정하는 단일 권위 지점.
    /// StageManager, GameManager 등 어떤 스크립트도 자체적으로
    /// "게임이 끝났다"는 상태를 따로 들고 있어서는 안 되며,
    /// 반드시 이 클래스를 통해 결과를 확정해야 한다.
    /// </summary>
    public sealed class GameFlowController : MonoBehaviour
    {
        public static GameFlowController Instance { get; private set; }

        public GameResult CurrentResult { get; private set; } = GameResult.None;
        public bool IsGameplayActive => CurrentResult == GameResult.None;

        public event Action<GameResult> OnResultFinalized;

        private void Awake()
        {
            Debug.Log($"[GameFlowController] Awake called at frame {Time.frameCount}");
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        /// <summary>
        /// 결과 확정을 시도한다. 최초 호출만 성공하며,
        /// 이미 결과가 확정된 이후의 호출은 모두 무시된다.
        /// </summary>
        /// <returns>이 호출로 결과가 새로 확정되었으면 true.</returns>
        public bool TryFinalize(GameResult result)
        {
            if (result == GameResult.None)
            {
                Debug.LogWarning("GameFlowController.TryFinalize called with GameResult.None.");
                return false;
            }

            if (CurrentResult != GameResult.None)
                return false;

            CurrentResult = result;
            OnResultFinalized?.Invoke(result);
            return true;
        }
    }
}