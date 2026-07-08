using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform lookAtTransform;
    [SerializeField] private float sensitivity = 0.2f;
    [SerializeField] private float maximumOrbitDistance = 10f;
    [SerializeField] private float minimumOrbitDistance = 2f;

    private float orbitRadius = 20f;

    private bool isOrbitCameraActive = false;

    // 추가
    private float yaw;
    private float pitch;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    private void Update()
    {
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            isOrbitCameraActive = !isOrbitCameraActive;

            if (isOrbitCameraActive)
            {
                orbitRadius = 20f;

                Vector3 angles = transform.eulerAngles;
                yaw = angles.y;
                pitch = angles.x;
            }
        }

        if (!isOrbitCameraActive)
            return;

        //-------------------------
        // 마우스 회전
        //-------------------------

        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();

            yaw += delta.x * sensitivity;
            pitch -= delta.y * sensitivity;

            pitch = Mathf.Clamp(pitch, -80f, 80f);
        }

        //-------------------------
        // 줌
        //-------------------------

        orbitRadius -= Mouse.current.scroll.ReadValue().y * 0.01f;

        orbitRadius = Mathf.Clamp(
            orbitRadius,
            minimumOrbitDistance,
            maximumOrbitDistance);

        //-------------------------
        // Orbit 계산
        //-------------------------

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        Vector3 offset =
            rotation * new Vector3(0, 0, -orbitRadius);

        transform.position =
            lookAtTransform.position + offset;

        transform.LookAt(lookAtTransform);
    }
}