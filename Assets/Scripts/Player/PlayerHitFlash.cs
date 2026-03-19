using System.Collections;
using UnityEngine;

public class PlayerHitFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private Color flashColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private float flashDuration = 0.12f;
    [SerializeField] private float hurtCooldown = 0.15f;

    [Tooltip("Nếu để trống sẽ tự lấy tất cả Renderer trong con")]
    [SerializeField] private Renderer[] renderers;

    [SerializeField] private string baseColorProperty = "_BaseColor";
    [SerializeField] private string colorProperty = "_Color";

    private MaterialPropertyBlock mpb;
    private Coroutine flashRoutine;
    private float lastFlashTime = -999f;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>();
        }

        mpb = new MaterialPropertyBlock();
    }

    public void Flash()
    {
        if (!gameObject.activeInHierarchy) return;

        // Chống spam nếu hit dồn dập
        if (Time.time - lastFlashTime < hurtCooldown) return;
        lastFlashTime = Time.time;

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        ApplyColorOverride(flashColor);
        yield return new WaitForSeconds(Mathf.Max(0.01f, flashDuration));
        ClearOverride();
        flashRoutine = null;
    }

    private void ApplyColorOverride(Color color)
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null) continue;

            r.GetPropertyBlock(mpb);
            mpb.SetColor(baseColorProperty, color);
            mpb.SetColor(colorProperty, color);
            r.SetPropertyBlock(mpb);
        }
    }

    private void ClearOverride()
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null) continue;
            r.SetPropertyBlock(null);
        }
    }
}

