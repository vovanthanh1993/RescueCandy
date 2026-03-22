using UnityEngine;

/// <summary>
/// Gắn cùng GameObject có <b>Sphere Collider (Is Trigger)</b> trên enemy (ví dụ quanh thân).
/// Khi player (CharacterController) nằm trong trigger → đẩy ngang ra xa tâm collider.
/// <para>
/// Unity: trigger + CharacterController cần ít nhất một bên có <see cref="Rigidbody"/> —
/// script tự thêm <c>Rigidbody</c> kinematic trên object này nếu chưa có.
/// </para>
/// </summary>
[RequireComponent(typeof(Collider))]
public class EnemyPushPlayerTrigger : MonoBehaviour
{
    [Tooltip("Độ mạnh đẩy (nhân với Time.deltaTime trong OnTriggerStay)")]
    [SerializeField] private float pushSpeed = 8f;

    [Tooltip("Giới hạn quãng đường đẩy tối đa mỗi frame (0 = không giới hạn)")]
    [SerializeField] private float maxPushDistancePerFrame = 0.2f;

    private Collider _triggerCollider;

    private void Awake()
    {
        _triggerCollider = GetComponent<Collider>();
        if (_triggerCollider != null && !_triggerCollider.isTrigger)
            Debug.LogWarning($"{nameof(EnemyPushPlayerTrigger)}: Collider nên bật Is Trigger.", this);

        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other == null)
            return;

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null)
            return;

        Vector3 center = _triggerCollider != null ? _triggerCollider.bounds.center : transform.position;
        Vector3 away = player.transform.position - center;
        away.y = 0f;

        if (away.sqrMagnitude < 0.0001f)
        {
            Vector2 r = Random.insideUnitCircle;
            away = new Vector3(r.x, 0f, r.y);
        }

        away.Normalize();

        Vector3 delta = away * pushSpeed * Time.deltaTime;
        if (maxPushDistancePerFrame > 0f && delta.magnitude > maxPushDistancePerFrame)
            delta = away * maxPushDistancePerFrame;

        player.ApplyHorizontalPushFromTrigger(delta);
    }
}
