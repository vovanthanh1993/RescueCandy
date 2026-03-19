using UnityEngine;
using System.Collections;

/// <summary>
/// Coin item: khi player nhặt sẽ cộng tiền, bay lên và biến mất.
/// </summary>
public class CoinItem : MonoBehaviour
{
    [Header("Coin Settings")]
    [Tooltip("Số tiền cộng thêm khi nhặt coin")]
    [SerializeField] private int coinAmount = 10;

    [Header("Fly Up Settings")]
    [Tooltip("Tốc độ bay lên")]
    [SerializeField] private float riseSpeed = 8f;

    [Tooltip("Thời gian từ lúc nhặt tới khi biến mất (giây)")]
    [SerializeField] private float disappearAfterSeconds = 0.6f;

    private bool isCollected = false;
    private Vector3 startPosition;

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected || other == null) return;
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        CollectCoin();
    }

    private void CollectCoin()
    {
        if (isCollected) return;
        isCollected = true;

        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        transform.SetParent(null);

        var itemScript = GetComponent<Item>();
        if (itemScript != null)
            itemScript.enabled = false;

        int amount = Mathf.Max(0, coinAmount);

        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.playerData != null)
        {
            PlayerDataManager.Instance.playerData.totalReward += amount;
            PlayerDataManager.Instance.Save();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("se_pickup");
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
