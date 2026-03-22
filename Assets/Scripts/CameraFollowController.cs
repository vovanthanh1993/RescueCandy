using UnityEngine;

/// <summary>
/// Camera Controller đơn giản để follow player
/// Dựa trên Cinemachine Follow settings: Binding Mode World Space, Position Damping (1,1,1), Follow Offset (0,4,-4)
/// </summary>
[DefaultExecutionOrder(100)]
public class CameraFollowController : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("Player Transform để follow (nếu null sẽ tự động tìm bằng tag 'Player')")]
    public Transform target;

    [Tooltip("Binding Mode: World Space (true) hoặc Local Space (false)")]
    public bool useWorldSpace = true;

    [Tooltip("Follow ngay lập tức, không có độ trễ (smooth damping)")]
    public bool instantFollow = true;

    [Header("Position Damping")]
    [Tooltip("Damping cho X, Y, Z (chỉ dùng khi instantFollow = false, giá trị càng lớn, camera di chuyển càng mượt)")]
    public Vector3 positionDamping = new Vector3(1f, 1f, 1f);

    [Header("Follow Offset")]
    [Tooltip("Offset từ player (X=0, Y=4, Z=-4)")]
    public Vector3 followOffset = new Vector3(0f, 7.13f, -7.71f);

    [Header("Giảm giật (platform / physics)")]
    [Tooltip("Khi Instant Follow bật: vẫn giữ X/Z khớp ngay, chỉ làm mượt trục Y của camera. Hữu ích khi player đứng trên vật di chuyển lên xuống (FixedUpdate vs Update lệch pha). 0 = tắt.")]
    [SerializeField] private float verticalSmoothTime = 0.12f;

    private Vector3 velocity;
    private float verticalSmoothVel;

    private void Start()
    {
        // Tự động tìm player nếu chưa gán
        FindAndSetPlayer();
    }

    private void LateUpdate()
    {
        // Target không cùng scene (ví dụ camera load scene mới nhưng reference cũ) → bỏ follow
        if (target != null && target.gameObject.scene != gameObject.scene)
        {
            target = null;
            velocity = Vector3.zero;
        }

        // Nếu chưa có target, tiếp tục tìm player (phòng trường hợp player spawn sau)
        if (target == null)
        {
            FindAndSetPlayer();
            return; // Chờ frame tiếp theo nếu vẫn chưa tìm thấy
        }

        // Tính toán vị trí mong muốn
        Vector3 desiredPosition;
        
        if (useWorldSpace)
        {
            // World Space: offset được áp dụng trực tiếp trong world space
            desiredPosition = target.position + followOffset;
        }
        else
        {
            // Local Space: offset được xoay theo rotation của target
            desiredPosition = target.position + target.rotation * followOffset;
        }

        // Follow ngay lập tức hoặc smooth với damping
        if (instantFollow)
        {
            if (verticalSmoothTime > 0.0001f)
            {
                float smoothY = Mathf.SmoothDamp(transform.position.y, desiredPosition.y, ref verticalSmoothVel, verticalSmoothTime, Mathf.Infinity, Time.deltaTime);
                transform.position = new Vector3(desiredPosition.x, smoothY, desiredPosition.z);
            }
            else
            {
                transform.position = desiredPosition;
            }
        }
        else
        {
            // Smooth follow với damping
            Vector3 currentPosition = transform.position;
            
            // Sử dụng Vector3.SmoothDamp với damping riêng cho từng trục
            float smoothX = positionDamping.x > 0 ? 1f / positionDamping.x : 0.1f;
            float smoothY = positionDamping.y > 0 ? 1f / positionDamping.y : 0.1f;
            float smoothZ = positionDamping.z > 0 ? 1f / positionDamping.z : 0.1f;

            // Smooth từng trục riêng biệt
            float newX = Mathf.SmoothDamp(currentPosition.x, desiredPosition.x, ref velocity.x, smoothX);
            float newY = Mathf.SmoothDamp(currentPosition.y, desiredPosition.y, ref velocity.y, smoothY);
            float newZ = Mathf.SmoothDamp(currentPosition.z, desiredPosition.z, ref velocity.z, smoothZ);

            transform.position = new Vector3(newX, newY, newZ);
        }
    }

    /// <summary>
    /// Set target để follow
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            // Reset velocity khi đổi target
            velocity = Vector3.zero;
            verticalSmoothVel = 0f;
        }
    }

    /// <summary>
    /// Tự động tìm và set target bằng tag "Player"
    /// </summary>
    public void FindAndSetPlayer()
    {
        if (target != null) return;

        GameObject player = GameObject.FindWithTag("Player");

        if (player == null)
        {
            var pc = Object.FindFirstObjectByType<PlayerController>();
            if (pc != null)
            {
                player = pc.gameObject;
                Debug.LogWarning($"CameraFollowController: Player không có tag 'Player', đã tìm bằng PlayerController: '{player.name}'");
            }
        }

        if (player != null)
        {
            SetTarget(player.transform);
            Debug.Log($"CameraFollowController: Đã tìm thấy Player '{player.name}'");
        }
    }
}

