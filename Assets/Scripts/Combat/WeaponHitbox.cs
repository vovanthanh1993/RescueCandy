using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WeaponHitbox : MonoBehaviour
{
    [SerializeField] private Collider hitCollider;
    [SerializeField] private bool debugLog = false;

    private int pendingDamage = 0;
    private bool isActive = false;
    private readonly HashSet<int> hitTargetsThisSwing = new HashSet<int>();
    private Rigidbody rb;

    private void Awake()
    {
        if (hitCollider == null)
        {
            hitCollider = GetComponent<Collider>();
        }

        // Hitbox chỉ dùng trigger
        if (hitCollider != null)
        {
            hitCollider.isTrigger = true;
            hitCollider.enabled = false;
        }

        // Unity trigger cần ít nhất 1 Rigidbody (kể cả kinematic) để OnTrigger hoạt động ổn định.
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void SetDamage(int damage)
    {
        pendingDamage = Mathf.Max(0, damage);
    }

    public void EnableHitbox()
    {
        isActive = true;
        hitTargetsThisSwing.Clear();
        if (hitCollider != null) hitCollider.enabled = true;

        if (debugLog)
        {
            Debug.Log($"WeaponHitbox[{name}] enabled. Damage={pendingDamage}");
        }
    }

    public void DisableHitbox()
    {
        isActive = false;
        if (hitCollider != null) hitCollider.enabled = false;
        hitTargetsThisSwing.Clear();

        if (debugLog)
        {
            Debug.Log($"WeaponHitbox[{name}] disabled.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // Nếu bật hitbox khi vũ khí đã overlap sẵn, OnTriggerEnter có thể không bắn.
        TryHit(other);
    }

    private void TryHit(Collider other)
    {
        if (!isActive) return;
        if (pendingDamage <= 0) return;

        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth == null)
        {
            if (debugLog)
            {
                Debug.Log($"WeaponHitbox[{name}] hit {other.name} but no EnemyHealth found in parents.");
            }
            return;
        }

        int id = enemyHealth.gameObject.GetInstanceID();
        if (hitTargetsThisSwing.Contains(id)) return;
        hitTargetsThisSwing.Add(id);

        enemyHealth.TakeDamage(pendingDamage);

        if (debugLog)
        {
            Debug.Log($"WeaponHitbox[{name}] damaged {enemyHealth.name} for {pendingDamage}.");
        }
    }
}

