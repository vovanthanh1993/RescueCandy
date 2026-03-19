using UnityEngine;
using System.Collections;

/// <summary>
/// Power item: khi player nhặt sẽ cộng mana, bay lên và biến mất.
/// </summary>
public class PowerItem : MonoBehaviour
{
    [Header("Mana Settings")]
    [Tooltip("Số mana cộng thêm khi nhặt")]
    [SerializeField] private int manaAmount = 50;

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

        CollectPower();
    }

    private void CollectPower()
    {
        if (isCollected) return;
        isCollected = true;

        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        transform.SetParent(null);

        var itemScript = GetComponent<Item>();
        if (itemScript != null)
            itemScript.enabled = false;

        if (PlayerMana.Instance != null)
        {
            PlayerMana.Instance.AddMana(manaAmount);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCollectSound();
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
