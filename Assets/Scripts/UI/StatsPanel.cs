using UnityEngine;
using TMPro;

public class StatsPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private TextMeshProUGUI damageText;

    private void OnEnable()
    {
        UpdateStats();
    }

    private void Update()
    {
        UpdateStats();
    }

    private void UpdateStats()
    {
        if (healthText != null)
        {
            int hp = PlayerHealth.Instance != null ? PlayerHealth.Instance.MaxHealth : 0;
            healthText.text = hp.ToString();
        }

        if (manaText != null)
        {
            int mana = PlayerMana.Instance != null ? PlayerMana.Instance.MaxMana : 0;
            manaText.text = mana.ToString();
        }

        if (damageText != null)
        {
            int dmg = PlayerCombat.Instance != null ? PlayerCombat.Instance.GetPlayerDamage() : 0;
            damageText.text = dmg.ToString();
        }
    }
}
