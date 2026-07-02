using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    private Vector3 offset;

    private void Start()
    {
        offset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        transform.position = new Vector3(
            target.position.x + offset.x,
            transform.position.y,          // Y´Â °íÁ¤
            target.position.z + offset.z
        );
    }
}