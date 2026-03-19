using UnityEngine;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Sweetie:
/// - Tất cả requiredDestroyedObjects == null -> kích hoạt Wave
/// - sau đó bật trigger, player đi vào trigger mới bay xoắn ốc + biến mất
/// Gắn script này lên object Sweetie (object có Collider).
/// </summary>
public class SweetieDanceOnTouch : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Animator của Sweetie (thường nằm trên model hoặc child)")]
    [SerializeField] private Animator animator;

    [Tooltip("Chỉ cho phép cứu khi TẤT CẢ objects trong list đã bị Destroy (null)")]
    [SerializeField] private List<GameObject> requiredDestroyedObjects = new List<GameObject>();

    [Header("Animation")]
    [Tooltip("Trigger parameter trong Animator để chuyển sang state Wave")]
    [SerializeField] private string waveTriggerParameter = "Wave";

    [Header("Behavior")]
    [Tooltip("Tắt collider sau khi bắt đầu bay để tránh va chạm không cần thiết")]
    [SerializeField] private bool disableColliderAfterTrigger = true;

    [Header("Move & Destroy")]
    [Tooltip("Bay lên bao cao (world units)")]
    [SerializeField] private float riseHeight = 3f;

    [Tooltip("Thời gian từ lúc trigger tới khi biến mất (giây)")]
    [SerializeField] private float disappearAfterSeconds = 3f;

    [Tooltip("Tốc độ bay (nếu bật thì ưu tiên riseSpeed; nếu không dùng Lerp theo disappearAfterSeconds)")]
    [SerializeField] private float riseSpeed = 8f;

    [Header("Spiral Settings")]
    [Tooltip("Bán kính xoắn ốc trên mặt phẳng XZ")]
    [SerializeField] private float spiralRadius = 0.6f;

    [Tooltip("Tốc độ xoắn (rad/giây)")]
    [SerializeField] private float spiralAngularSpeed = 10f;

    [Header("Speech Text")]
    [Tooltip("TextMeshPro hiển thị trạng thái (Canvas con của Sweetie)")]
    [SerializeField] private TextMeshProUGUI speechText;
    [SerializeField] private string waitingMessage = "Please kill the enemy!";
    [SerializeField] private string freeMessage = "Free me!";
    [SerializeField] private string thankMessage = "Thank you!";

    [Header("SFX")]
    [SerializeField] private string rescueSoundName = "se_rescue";
    [Range(0f, 1f)]
    [SerializeField] private float rescueSoundVolume = 1f;

    private Collider triggerCollider;
    private bool hasWaved = false;
    private bool hasStartedFly = false;
    private Vector3 startPosition;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            // Ban đầu chưa dùng trigger, chờ tới khi vào Wave mới bật.
            triggerCollider.isTrigger = false;
        }

        // Trigger trong Unity thường cần ít nhất 1 Rigidbody.
        // CharacterController không phải Rigidbody nên thêm kinematic Rigidbody cho chắc chắn.
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;

        startPosition = transform.position;

        if (speechText != null)
        {
            bool hasRequirements = requiredDestroyedObjects != null && requiredDestroyedObjects.Count > 0;
            speechText.text = hasRequirements ? waitingMessage : freeMessage;
        }
    }

    private void Update()
    {
        if (hasWaved) return;
        if (!AreAllRequiredObjectsDestroyed()) return;
        ActivateWaveOnly();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasWaved || hasStartedFly) return;
        if (other == null) return;
        if (!other.CompareTag("Player") && other.GetComponent<PlayerController>() == null)
            return;

        StartFlyAndDisappear();
    }

    private bool AreAllRequiredObjectsDestroyed()
    {
        if (requiredDestroyedObjects == null || requiredDestroyedObjects.Count == 0)
            return true;

        for (int i = 0; i < requiredDestroyedObjects.Count; i++)
        {
            if (requiredDestroyedObjects[i] != null)
                return false;
        }
        return true;
    }

    private void ActivateWaveOnly()
    {
        hasWaved = true;

        if (speechText != null)
            speechText.text = freeMessage;

        if (animator != null && !string.IsNullOrEmpty(waveTriggerParameter))
        {
            animator.SetTrigger(waveTriggerParameter);
        }

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void StartFlyAndDisappear()
    {
        hasStartedFly = true;

        if (speechText != null)
            speechText.text = thankMessage;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnSweetieRescued();
        }

        // Phát âm thanh cứu khi bắt đầu bay
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(rescueSoundName))
        {
            AudioManager.Instance.PlaySound(rescueSoundName, rescueSoundVolume);
        }

        if (disableColliderAfterTrigger && triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        StartCoroutine(BayLenVaBienMat());
    }

    private System.Collections.IEnumerator BayLenVaBienMat()
    {
        float t = 0f;
        float duration = Mathf.Max(0.01f, disappearAfterSeconds);

        // Nếu người dùng set riseSpeed > 0 thì dùng move theo vận tốc
        bool useSpeed = riseSpeed > 0f;

        float theta = 0f;
        Vector3 p = startPosition;

        while (t < duration)
        {
            float dt = Time.deltaTime;

            theta += spiralAngularSpeed * dt;

            // Di chuyển kiểu xoắn ốc (lấy world XZ)
            float x = startPosition.x + Mathf.Cos(theta) * spiralRadius;
            float z = startPosition.z + Mathf.Sin(theta) * spiralRadius;

            // Di chuyển lên cao
            float y;
            if (useSpeed)
            {
                // Tính y theo thời gian hiện tại để không phụ thuộc frame rate quá nhiều
                y = startPosition.y + riseSpeed * t;
            }
            else
            {
                float normalized = t / duration;
                y = Mathf.Lerp(startPosition.y, startPosition.y + riseHeight, normalized);
            }

            p = new Vector3(x, y, z);
            transform.position = p;

            t += dt;
            yield return null;
        }

        Destroy(gameObject);
    }
}

