using UnityEngine;

/// <summary>
/// Làm UI/world object luôn hướng về camera.
/// Gắn lên Canvas world-space của thanh máu enemy.
/// </summary>
public class FaceCameraBillboard : MonoBehaviour
{
    [Tooltip("Nếu để trống sẽ tự lấy Camera.main")]
    [SerializeField] private Camera targetCamera;

    [Tooltip("Giữ trục Y của object không nghiêng theo camera")]
    [SerializeField] private bool lockYAxis = true;

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null) return;

        Vector3 toCamera = transform.position - targetCamera.transform.position;
        if (toCamera.sqrMagnitude < 0.0001f) return;

        if (lockYAxis)
        {
            toCamera.y = 0f;
            if (toCamera.sqrMagnitude < 0.0001f) return;
        }

        transform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
    }
}

