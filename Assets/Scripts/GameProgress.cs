using UnityEngine;

/// <summary>
/// Stage 진행 상태(현재 Stage Index, 해금된 최고 Stage)를 관리하는
/// 유일한 진행도 소스(Single Source of Truth).
/// TerminalManager와 StageManager는 이 클래스를 통해서만 진행 상태를
/// 조회하거나 변경하며, 각자 currentStage / nextStage 같은 상태를
/// 직접 들고 있지 않는다.
/// </summary>
public static class GameProgress
{
    private const string SceneNamePrefix = "Stage_";
    private const string SceneNumberFormat = "00"; // 1 -> "01", 15 -> "15"

    private const int InitialStageIndex = 1;

    public static int CurrentStageIndex { get; private set; } = InitialStageIndex;
    public static int HighestUnlockedStage { get; private set; } = InitialStageIndex;

    /// <summary>
    /// 현재 CurrentStageIndex에 해당하는 Scene 이름. 예) Stage_03
    /// </summary>
    public static string CurrentSceneName => GetSceneName(CurrentStageIndex);

    /// <summary>
    /// 진행 상태를 초기값으로 되돌린다. (처음부터 다시 시작)
    /// </summary>
    public static void ResetProgress()
    {
        CurrentStageIndex = InitialStageIndex;
        HighestUnlockedStage = InitialStageIndex;
    }

    /// <summary>
    /// Stage 번호를 "Stage_XX" 형식의 Scene 이름으로 변환한다.
    /// </summary>
    public static string GetSceneName(int stage)
    {
        return $"{SceneNamePrefix}{stage.ToString(SceneNumberFormat)}";
    }

    /// <summary>
    /// 현재 Stage가 실제로 해금되어 있어서 진입 가능한지 확인한다.
    /// </summary>
    public static bool CanLaunchCurrentStage()
    {
        return CurrentStageIndex <= HighestUnlockedStage;
    }

    /// <summary>
    /// 현재 Stage를 클리어했을 때 호출한다.
    /// 해금 최고치는 단조 증가만 하도록 Mathf.Max로 방어한다.
    /// (이미 해금된 Stage를 다시 클리어해도 HighestUnlockedStage가 줄어들지 않는다.)
    /// </summary>
    public static void UnlockNextStage()
    {
        int candidate = CurrentStageIndex + 1;
        HighestUnlockedStage = Mathf.Max(HighestUnlockedStage, candidate);
    }

    /// <summary>
    /// Terminal에서 /next로 다음 Stage로 이동할 때 호출한다.
    /// 해금 여부 확인은 호출자(TerminalManager)의 책임이다.
    /// </summary>
    public static void AdvanceStage()
    {
        CurrentStageIndex++;
    }

    /// <summary>
    /// Terminal에서 /prev로 이전 Stage로 이동할 때 호출한다.
    /// 하한(1) 확인은 호출자(TerminalManager)의 책임이다.
    /// </summary>
    public static void RetreatStage()
    {
        CurrentStageIndex--;
    }
}