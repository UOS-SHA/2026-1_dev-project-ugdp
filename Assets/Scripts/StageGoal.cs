using UnityEngine;

/// <summary>
/// End 오브젝트에 부착된다. Spaceship이 3D Trigger에 들어오면
/// StageManager에게 클리어를 알리는 역할만 한다.
/// </summary>
public class StageGoal : MonoBehaviour
{
    [SerializeField] private StageManager _stageManager;
    [SerializeField] private string _spaceshipTag = "Spaceship";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(_spaceshipTag))
            return;

        _stageManager.StageClear();
    }
}