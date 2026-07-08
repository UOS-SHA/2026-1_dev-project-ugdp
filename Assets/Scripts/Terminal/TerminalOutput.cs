using TMPro;
using UnityEngine;

/// <summary>
/// Terminal 씬에서 직전 플레이의 경과 시간을 출력한다.
/// GameSessionTimer에 저장된 값을 읽어 표시만 하며,
/// 씬 이동이나 게임 로직은 포함하지 않는다.
/// </summary>
public class TerminalOutput : MonoBehaviour
{
    [SerializeField] private TMP_Text _resultTimeText;

    private void Start()
    {
        float elapsed = GameSessionTimer.GetElapsedTime();
        int minutes = Mathf.FloorToInt(elapsed / 60f);
        int seconds = Mathf.FloorToInt(elapsed % 60f);
        if (seconds != 0)
        {
            _resultTimeText.text = $"Elapsed Time : {minutes:D2}:{seconds:D2}\n==================================\n\nAvailable Commands\n/start\n/help\n";
        }
    }
}