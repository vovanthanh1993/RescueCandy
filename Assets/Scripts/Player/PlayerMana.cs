using UnityEngine;

public class PlayerMana : MonoBehaviour
{
    public static PlayerMana Instance { get; private set; }

    [SerializeField] private int maxMana = 100;
    [SerializeField] private int currentMana = 0;

    public int MaxMana => maxMana;
    public int CurrentMana => currentMana;
    public System.Action<int, int> OnManaChanged; // (current, max)

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ResetMana()
    {
        currentMana = 0;
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    public void AddMana(int amount)
    {
        if (amount <= 0) return;
        currentMana = Mathf.Min(currentMana + amount, maxMana);
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    /// <summary>
    /// Tiêu mana. Trả về true nếu đủ mana để tiêu.
    /// </summary>
    public bool UseMana(int amount)
    {
        if (amount <= 0) return true;
        if (currentMana < amount) return false;

        currentMana -= amount;
        OnManaChanged?.Invoke(currentMana, maxMana);
        return true;
    }

    public bool HasEnoughMana(int amount)
    {
        return currentMana >= amount;
    }
}
