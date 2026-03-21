using UnityEngine;

public class SelectLevelSceneInit : MonoBehaviour
{
    private void Start()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowSelectLevelPanel(true);
            UIManager.Instance.ShowHomePanel(false);
            UIManager.Instance.ShowGamePlayPanel(false);
        }
    }
}
