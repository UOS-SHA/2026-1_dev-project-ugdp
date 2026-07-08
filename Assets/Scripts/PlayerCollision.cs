using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private const string ObstacleTag = "Obstacles";

    [SerializeField] private GameManager gm;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Hit : {other.name}");

        if (other.CompareTag(ObstacleTag))
        {
            Debug.Log("Obstacle Hit");
            gm.EndGame();
        }
    }
}