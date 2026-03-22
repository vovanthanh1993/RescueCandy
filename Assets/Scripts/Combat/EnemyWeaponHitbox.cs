using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyWeaponHitbox : MonoBehaviour
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
            hitCollider = GetComponent<Collider>();

        if (hitCollider != null)
        {
            hitCollider.isTrigger = true;
            hitCollider.enabled = false;
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
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
            Debug.Log($"EnemyWeaponHitbox[{name}] enabled. Damage={pendingDamage}");
    }

    public void DisableHitbox()
    {
        isActive = false;
        if (hitCollider != null) hitCollider.enabled = false;
        hitTargetsThisSwing.Clear();

        if (debugLog)
            Debug.Log($"EnemyWeaponHitbox[{name}] disabled.");
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryHit(other);
    }

    private void TryHit(Collider other)
    {
        if (!isActive) return;
        if (pendingDamage <= 0) return;
        if (PlayerHealth.Instance == null) return;

        bool isPlayer = other.CompareTag("Player") || other.GetComponent<PlayerController>() != null;
        if (!isPlayer)
        {
            PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();
            if (ph == null) return;
        }

        int id = PlayerHealth.Instance.gameObject.GetInstanceID();
        if (hitTargetsThisSwing.Contains(id)) return;
        hitTargetsThisSwing.Add(id);

        PlayerHealth.Instance.TakeDamage(pendingDamage);

        if (debugLog)
            Debug.Log($"EnemyWeaponHitbox[{name}] damaged Player for {pendingDamage}.");
    }
}
