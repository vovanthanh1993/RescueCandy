using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private float elapsedTime = 0f;
    private bool isRunning = false;

    private void Start()
    {
        if (timerText == null)
            timerText = GetComponentInChildren<TextMeshProUGUI>();

        ResetTimer();
        StartTimer();
    }

    private void Update()
    {
        if (!isRunning) return;

        elapsedTime += Time.deltaTime;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void StartTimer() => isRunning = true;
    public void StopTimer() => isRunning = false;

    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateDisplay();
    }

    public float GetElapsedTime() => elapsedTime;
}
