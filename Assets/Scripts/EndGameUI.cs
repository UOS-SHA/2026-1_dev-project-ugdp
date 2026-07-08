using TMPro;
using UnityEngine;

/// <summary>
/// 게임 오버(실패) 관련 텍스트 출력만 담당한다.
/// 씬 이동, 게임 진행 로직은 포함하지 않는다.
/// </summary>
public class EndGameUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _messageText;

    public void ShowMissionFailed()
    {
        _messageText.text = "MISSION FAILED";
    }

    public void ShowEndLoading(string nextSceneName)
    {
        _messageText.text = $"Loading {nextSceneName}...";
    }
}