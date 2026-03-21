using UnityEngine;

public class SkullItem : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damage = 30;

    [Header("VFX")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private float effectDuration = 2f;

    [Header("SFX")]
    [SerializeField] private string hitSoundName = "se_hurt";
    [Range(0f, 1f)]
    [SerializeField] private float hitSoundVolume = 1f;

    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;
        if (!other.CompareTag("Player") && other.GetComponent<PlayerController>() == null) return;

        isTriggered = true;

        if (PlayerHealth.Instance != null && !PlayerHealth.Instance.IsShielded)
        {
            PlayerHealth.Instance.TakeDamage(damage);
        }

        if (hitEffect != null)
        {
            GameObject vfx = Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(vfx, effectDuration);
        }

        if (AudioManager.Instance != null && !string.IsNullOrEmpty(hitSoundName))
        {
            AudioManager.Instance.PlaySound(hitSoundName, hitSoundVolume);
        }

        Destroy(gameObject);
    }
}
