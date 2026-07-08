using TMPro;
using UnityEngine;

/// <summary>
/// Stage 진행 상황을 화면에 출력하는 역할만 한다.
/// 씬 이동, 게임 진행 로직은 포함하지 않는다.
/// </summary>
public class StageUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _messageText;

    public void ShowMissionComplete()
    {
        _messageText.text = "MISSION COMPLETE";
    }

    public void ShowLoading(string nextSceneName)
    {
        _messageText.text = $"Loading {nextSceneName}...";
    }
}