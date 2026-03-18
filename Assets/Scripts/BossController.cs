using UnityEngine;

/// <summary>
/// Controller cho Boss
/// Boss không di chuyển, chỉ xoay mặt
/// </summary>
public class BossController : MonoBehaviour
{
    
    [Header("Fireball Settings")]
    [Tooltip("Prefab fireball để bắn")]
    [SerializeField] private GameObject fireballPrefab;
    
    [Tooltip("Vị trí pivot để bắn fireball (nếu null sẽ dùng transform.position)")]
    [SerializeField] private Transform fireballPivot;
    
    [Header("Shoot Effects")]
    [Tooltip("Effect 1 khi bắn fireball")]
    [SerializeField] private GameObject shootEffect1;
    
    [Tooltip("Effect 2 khi bắn fireball")]
    [SerializeField] private GameObject shootEffect2;
    
    [Header("Shoot Settings")]
    [Tooltip("Thời gian cooldown giữa các lần bắn (giây) - tránh bắn nhiều lần")]
    [SerializeField] private float shootCooldown = 0.5f;
    
    [Tooltip("Tốc độ xoay khi nhắm vào player (độ/giây)")]
    [SerializeField] private float aimRotationSpeed = 300f;
    
    [Header("Debug")]
    [Tooltip("Hiển thị log trong Console")]
    [SerializeField] private bool debugLog = false;
    
    // Đang trong quá trình xoay để nhắm bắn
    private bool isAiming = false;
    
    // Cooldown để tránh bắn nhiều lần
    private float lastShootTime = 0f;

    /// <summary>
    /// Bắn fireball đến player
    /// </summary>
    public void ShootFireballAtPlayer()
    {
        // Kiểm tra cooldown để tránh bắn nhiều lần
        if (Time.time - lastShootTime < shootCooldown)
        {
            if (debugLog)
            {
                Debug.Log($"BossController: Đang trong cooldown, không thể bắn. Thời gian còn lại: {shootCooldown - (Time.time - lastShootTime):F2}s");
            }
            return;
        }
        
        if (fireballPrefab == null)
        {
            Debug.LogWarning("BossController: Fireball prefab chưa được gán!");
            return;
        }
        
        if (PlayerController.Instance == null)
        {
            Debug.LogWarning("BossController: Không tìm thấy Player!");
            return;
        }
        
        // Nếu đang xoay để nhắm, không bắn
        if (isAiming)
        {
            if (debugLog)
            {
                Debug.Log("BossController: Đang xoay để nhắm, chờ xong rồi bắn.");
            }
            return;
        }
        
        // Bắt đầu xoay đến hướng player trước khi bắn
        StartCoroutine(AimAndShoot());
    }
    
    /// <summary>
    /// Xoay đến hướng player rồi mới bắn
    /// </summary>
    private System.Collections.IEnumerator AimAndShoot()
    {
        if (PlayerController.Instance == null)
            yield break;
        
        isAiming = true;
        
        // Tính toán hướng đến player
        Vector3 directionToPlayer = (PlayerController.Instance.transform.position - transform.position).normalized;
        directionToPlayer.y = 0f; // Chỉ xoay trên trục Y
        
        // Tính rotation mục tiêu
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        
        // Xoay mượt mà đến hướng player
        Quaternion startRotation = transform.rotation;
        float elapsedTime = 0f;
        float rotationDuration = Quaternion.Angle(startRotation, targetRotation) / aimRotationSpeed;
        
        if (debugLog)
        {
            Debug.Log($"BossController: Bắt đầu xoay đến hướng player. Thời gian: {rotationDuration:F2}s");
        }
        
        while (elapsedTime < rotationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / rotationDuration);
            
            // Sử dụng Slerp để quay mượt mà
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            
            yield return null;
        }
        
        // Đảm bảo đã xoay đến đúng hướng
        transform.rotation = targetRotation;
        
        isAiming = false;
        
        // Sau khi xoay xong, bắn fireball
        PerformShoot();
    }
    
    /// <summary>
    /// Thực hiện bắn fireball (sau khi đã xoay xong)
    /// </summary>
    private void PerformShoot()
    {
        // Cập nhật thời gian bắn
        lastShootTime = Time.time;
        
        // Xác định vị trí bắn (pivot hoặc transform.position)
        Vector3 spawnPosition = fireballPivot != null ? fireballPivot.position : transform.position;
        
        // Play effect 1 khi bắn
        if (shootEffect1 != null)
        {
            GameObject effect1 = Instantiate(shootEffect1, spawnPosition, Quaternion.identity);
            // Tự động destroy effect sau một khoảng thời gian (nếu effect không tự destroy)
            Destroy(effect1, 5f);
        }
        
        // Play effect 2 khi bắn
        if (shootEffect2 != null)
        {
            GameObject effect2 = Instantiate(shootEffect2, spawnPosition, Quaternion.identity);
            // Tự động destroy effect sau một khoảng thời gian (nếu effect không tự destroy)
            Destroy(effect2, 5f);
        }
        
        // Spawn fireball
        GameObject fireballObj = Instantiate(fireballPrefab, spawnPosition, Quaternion.identity);
        Fireball fireball = fireballObj.GetComponent<Fireball>();
        
        if (fireball != null)
        {
            fireball.Initialize(PlayerController.Instance.transform, spawnPosition);
            
            if (debugLog)
            {
                Debug.Log("BossController: Bắn fireball đến player!");
            }
        }
        else
        {
            Debug.LogWarning("BossController: Fireball prefab không có component Fireball!");
            Destroy(fireballObj);
        }
    }
    
}
