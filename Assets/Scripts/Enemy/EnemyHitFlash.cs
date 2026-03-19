using System.Collections;
using UnityEngine;

public class EnemyHitFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private Color flashColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private float flashDuration = 0.3f;

    [Tooltip("Nếu để trống sẽ tự lấy tất cả Renderer trong con")]
    [SerializeField] private Renderer[] renderers;

    // Một số shader dùng _BaseColor (URP Lit), một số dùng _Color
    [SerializeField] private string baseColorProperty = "_BaseColor";
    [SerializeField] private string colorProperty = "_Color";

    private MaterialPropertyBlock mpb;
    private Coroutine flashRoutine;

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

            // Set cả 2 property để tăng khả năng tương thích shader
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

