using UnityEngine;
using System.Collections;

/// <summary>
/// Item hồi máu (HP) cho player, bay lên và biến mất khi nhặt.
/// </summary>
public class HealthItem : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Số máu (HP) hồi khi nhặt item")]
    [SerializeField] private int healAmount = 20;

    [Header("Fly Up Settings")]
    [Tooltip("Tốc độ bay lên")]
    [SerializeField] private float riseSpeed = 8f;

    [Tooltip("Thời gian từ lúc nhặt tới khi biến mất (giây)")]
    [SerializeField] private float disappearAfterSeconds = 0.6f;

    private bool isCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected || other == null) return;
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        CollectHealthItem(player);
    }

    private void CollectHealthItem(PlayerController player)
    {
        if (isCollected) return;
        isCollected = true;

        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        transform.SetParent(null);

        var itemScript = GetComponent<Item>();
        if (itemScript != null)
            itemScript.enabled = false;

        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.Heal(healAmount);
        }

        if (player != null)
        {
            player.SpawnHealthPickupVFX();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayHealSound();
        }

        StartCoroutine(FlyUpAndDisappear());
    }

    private IEnumerator FlyUpAndDisappear()
    {
        float t = 0f;
        float duration = Mathf.Max(0.01f, disappearAfterSeconds);
        Vector3 origin = transform.position;
        Vector3 originScale = transform.localScale;

        while (t < duration)
        {
            float dt = Time.deltaTime;
            t += dt;

            float y = origin.y + riseSpeed * t;
            transform.position = new Vector3(origin.x, y, origin.z);

            float normalized = 1f - Mathf.Clamp01(t / duration);
            transform.localScale = originScale * normalized;

            yield return null;
        }

        Destroy(gameObject);
    }
}
