using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private GameObject model;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("Jump Settings")]
    [Tooltip("Lực nhảy")]
    [SerializeField] private float jumpForce = 5f;
    [Tooltip("Khoảng cách kiểm tra mặt đất")]
    [SerializeField] private float groundCheckDistance = 0.1f;
    [Tooltip("Điểm kiểm tra mặt đất (đặt ở chân nhân vật). Nếu null sẽ dùng vị trí gốc của player.")]
    [SerializeField] private Transform groundCheckPoint;
    [Tooltip("Layer mặt đất")]
    [SerializeField] private LayerMask groundLayer = 1; // Default layer
    [Tooltip("Hệ số nhân lực rơi (càng lớn càng rơi nhanh)")]
    [SerializeField] private float gravityMultiplier = 2f;
    
    [Header("Attack Settings")]
    [Tooltip("Phím dùng để tấn công")]
    [SerializeField] private KeyCode attackKey = KeyCode.J;
    [Tooltip("Thời gian cooldown giữa các lần tấn công (giây)")]
    [SerializeField] private float attackCooldown = 0.5f;
    [Tooltip("Tên state attack trong Animator (phải trùng với tên state trong Animator). Chỉ cho di chuyển lại khi state này chạy xong.")]
    [SerializeField] private string attackStateName = "Attack";
    [Tooltip("Layer của Animator chứa state attack (thường là 0).")]
    [SerializeField] private int attackAnimatorLayer = 0;
    [Tooltip("Thời gian tối đa chờ animation attack (fallback nếu không nhận diện được state).")]
    [SerializeField] private float attackDurationMax = 2f;
    
    private float baseMoveSpeed; // Tốc độ gốc
    private bool isSpeedBoosted = false; // Đang trong trạng thái speed boost
    private float verticalVelocity = 0f; // Vận tốc theo trục Y (cho jump và gravity)
    private bool isGrounded = false; // Đang trên mặt đất hay không
    private bool isAttacking = false; // Đang trong animation tấn công
    private float lastAttackTime = -999f; // Thời điểm tấn công lần cuối (để cooldown)
    private bool cachedApplyRootMotion = true; // Giá trị applyRootMotion gốc (khôi phục sau khi tấn công)
    
    [Header("Camera Settings")]
    [SerializeField] private Transform camTarget;

    [Header("Item Collection")]
    [Tooltip("Điểm để hiển thị item khi đã nhặt")]
    [SerializeField] private Transform itemPoint;
    
    // Item đang được mang theo (chỉ 1 item)
    private Item carriedItem = null;

    [Header("Spawn Settings")]
    [Tooltip("Vị trí spawn point (nếu null sẽ dùng vị trí ban đầu của player)")]
    [SerializeField] private Transform spawnPoint;
    
    [Header("Input Control")]
    [Tooltip("Cho phép nhận input từ người chơi hay không")]
    [SerializeField] private bool canReceiveInput = false;
    [SerializeField] private bool isDisable = false;
    
    // Components
    public PlayerAnimation playerAnimation;
    
    private Vector3 initialSpawnPosition;
    
    // Flag để tránh trừ mạng nhiều lần khi va chạm với nhiều ResetTag cùng lúc
    private bool isReturningToSpawn = false;
    private float lastSpawnReturnTime = 0f;
    [Header("Spawn Return Settings")]
    [Tooltip("Thời gian cooldown sau khi về spawn point (giây)")]
    [SerializeField] private float spawnReturnCooldown = 0.5f;
    
    [Tooltip("Thời gian chờ death animation trước khi spawn lại (giây)")]
    [SerializeField] private float deathAnimationDuration = 1f;
    
    [Header("Pickup VFX Settings")]
    [Tooltip("Vị trí spawn VFX khi nhặt item (health, speed)")]
    [SerializeField] private Transform pickupVFXPoint;
    
    [Tooltip("VFX effect khi nhặt Health Item")]
    [SerializeField] private GameObject healthPickupVFXPrefab;
    
    [Tooltip("VFX effect khi nhặt Speed Item")]
    [SerializeField] private GameObject speedPickupVFXPrefab;

    [Header("Speed Skill Settings")]
    [Tooltip("Tốc độ tăng thêm khi kích hoạt skill")]
    [SerializeField] private float speedSkillBoostAmount = 2f;

    [Tooltip("Thời gian skill hoạt động (giây)")]
    [SerializeField] private float speedSkillDuration = 5f;

    [Tooltip("Thời gian cooldown (giây)")]
    [SerializeField] private float speedSkillCooldown = 60f;

    // Speed Skill state
    private bool isSpeedSkillActive = false;
    private bool isSpeedSkillOnCooldown = false;
    private float speedSkillCooldownTimer = 0f;
    private float speedSkillTimer = 0f;

    // UI Callbacks
    public System.Action<float> OnSpeedSkillCooldownChanged; // (cooldownProgress 0-1)
    public System.Action<bool> OnSpeedSkillStateChanged; // (isActive)

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        // Get components
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        
        playerAnimation = GetComponent<PlayerAnimation>();
        
        // Tự động tìm ItemPoint nếu chưa được assign
        if (itemPoint == null)
        {
            itemPoint = transform.Find("ItemPoint");
            if (itemPoint == null)
            {
                // Tìm trong tất cả các con
                foreach (Transform child in transform)
                {
                    if (child.name == "ItemPoint")
                    {
                        itemPoint = child;
                        break;
                    }
                }
            }
        }
        
        // Tự động tìm PickupVFXPoint nếu chưa được assign
        if (pickupVFXPoint == null)
        {
            pickupVFXPoint = transform.Find("PickupVFXPoint");
            if (pickupVFXPoint == null)
            {
                // Tìm trong tất cả các con
                foreach (Transform child in transform)
                {
                    if (child.name == "PickupVFXPoint" || child.name == "VFXPoint")
                    {
                        pickupVFXPoint = child;
                        break;
                    }
                }
            }
        }
        
        // Tự động tìm GroundCheckPoint (điểm ở chân) nếu chưa được assign
        if (groundCheckPoint == null)
        {
            groundCheckPoint = transform.Find("GroundCheckPoint");
            if (groundCheckPoint == null)
            {
                foreach (Transform child in transform)
                {
                    if (child.name == "GroundCheckPoint" || child.name == "Feet")
                    {
                        groundCheckPoint = child;
                        break;
                    }
                }
            }
        }
        
        // Đảm bảo CharacterController tồn tại
        if (characterController == null)
        {
            Debug.LogError("PlayerController: CharacterController component is missing! Please add CharacterController to the player GameObject.");
        }
    }

    private void Start()
    {
        // Lưu tốc độ gốc
        baseMoveSpeed = moveSpeed;
        
        // Lưu vị trí spawn point
        if (spawnPoint != null)
        {
            initialSpawnPosition = spawnPoint.position;
        }
        else
        {
            initialSpawnPosition = transform.position;
        }
        
        // Reset flag khi bắt đầu level mới
        isReturningToSpawn = false;
        lastSpawnReturnTime = 0f;
        
        // Lưu giá trị applyRootMotion gốc để khôi phục sau khi tấn công
        if (playerAnimation != null && playerAnimation.animator != null)
            cachedApplyRootMotion = playerAnimation.animator.applyRootMotion;
        
        // Camera sẽ được quản lý bởi CameraFollowController tự động
    }

    private void Update()
    {
        if (isDisable || !canReceiveInput)
        {
            // Nếu không cho phép input, dừng animation
            if (playerAnimation != null)
            {
                playerAnimation.SetMovement(false, 0f);
            }
            return;
        }

        HandleInput();
        UpdateSpeedSkill();
    }

    private void LateUpdate()
    {
        if (!isDisable)
        {
            UpdateCameraTarget();
        }
    }

    #region Initialization
    #endregion

    #region Input Handling

    private void HandleInput()
    {
        if (InputManager.Instance == null) return;

        HandleAttack();
        HandleJump();
        HandleMovement();
    }
    
    /// <summary>
    /// Xử lý nhảy khi nhấn Space
    /// </summary>
    private void HandleJump()
    {
        // Kiểm tra mặt đất
        CheckGrounded();
        
        // Nhấn Space và đang trên mặt đất thì nhảy (không cho nhảy khi đang tấn công)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && canReceiveInput && !isDisable && !isAttacking)
        {
            verticalVelocity = jumpForce;
            
            // Phát animation jump nếu có
            if (playerAnimation != null)
            {
                // Nếu PlayerAnimation có method Jump(), gọi nó
                // playerAnimation.Jump();
            }
        }
    }
    
    /// <summary>
    /// Xử lý tấn công khi nhấn phím attack
    /// </summary>
    private void HandleAttack()
    {
        if (!canReceiveInput || isDisable || isAttacking) return;
        if (Time.time - lastAttackTime < attackCooldown) return;
        
        if (Input.GetKeyDown(attackKey))
        {
            lastAttackTime = Time.time;
            isAttacking = true;
            
            if (playerAnimation != null)
            {
                playerAnimation.SetAttack();
            }
            
            StartCoroutine(ResetAttackStateAfterDuration());
        }
    }
    
    /// <summary>
    /// Chờ animation attack chạy xong (thoát khỏi state attack trong Animator) rồi mới cho di chuyển lại.
    /// </summary>
    private IEnumerator ResetAttackStateAfterDuration()
    {
        Animator anim = playerAnimation != null ? playerAnimation.animator : null;
        float timeout = Time.time + attackDurationMax;
        
        if (anim != null && !string.IsNullOrEmpty(attackStateName))
        {
            // Đợi vào state attack (tránh trường hợp transition chưa kịp)
            while (Time.time < timeout && !anim.GetCurrentAnimatorStateInfo(attackAnimatorLayer).IsName(attackStateName))
            {
                yield return null;
            }
            // Đợi thoát khỏi state attack (animation đã chạy xong)
            while (Time.time < timeout && anim.GetCurrentAnimatorStateInfo(attackAnimatorLayer).IsName(attackStateName))
            {
                yield return null;
            }
        }
        else
        {
            // Fallback: không có Animator hoặc chưa đặt tên state thì chờ attackDurationMax
            yield return new WaitForSeconds(attackDurationMax);
        }
        
        isAttacking = false;
        if (playerAnimation != null && playerAnimation.animator != null)
            playerAnimation.animator.applyRootMotion = cachedApplyRootMotion;
    }
    
    /// <summary>
    /// Kiểm tra xem player có đang trên mặt đất không.
    /// Nếu có gán groundCheckPoint (ở chân nhân vật) thì raycast từ đó.
    /// </summary>
    private void CheckGrounded()
    {
        // Dùng điểm ở chân nếu đã gán, không thì dùng vị trí gốc
        Vector3 origin = groundCheckPoint != null ? groundCheckPoint.position : transform.position;
        // Khoảng cách raycast: từ điểm kiểm tra xuống một chút
        float rayDistance = groundCheckPoint != null ? groundCheckDistance + 0.05f : characterController.height * 0.5f + 0.1f;
        isGrounded = Physics.Raycast(origin, Vector3.down, rayDistance, groundLayer);
        
        // Nếu raycast không phát hiện, dùng CharacterController.isGrounded làm backup
        if (!isGrounded)
        {
            isGrounded = characterController.isGrounded;
        }
    }

    private void HandleMovement()
    {
        if (characterController == null || InputManager.Instance == null)
        {
            return;
        }
        
        // Đang tấn công: tắt root motion (tránh animation đẩy nhân vật), chỉ gravity, không di chuyển
        if (isAttacking)
        {
            if (playerAnimation != null && playerAnimation.animator != null)
                playerAnimation.animator.applyRootMotion = false;
            CheckGrounded();
            ApplyVerticalMovementOnly();
            if (playerAnimation != null) playerAnimation.SetMovement(false, 0f);
            return;
        }
        
        // Áp dụng gravity cho vertical velocity
        if (!isGrounded)
        {
            // Đang ở trên không - áp dụng gravity với hệ số nhân để rơi nhanh hơn
            verticalVelocity += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
            
            // Giới hạn tốc độ rơi tối đa để tránh rơi quá nhanh
            float maxFallSpeed = -20f; // Tốc độ rơi tối đa
            if (verticalVelocity < maxFallSpeed)
            {
                verticalVelocity = maxFallSpeed;
            }
        }
        else
        {
            // Đã chạm đất - chỉ reset khi verticalVelocity đang âm (đang rơi xuống)
            // Và chỉ reset khi đã thực sự chạm đất (kiểm tra khoảng cách rất nhỏ)
            if (verticalVelocity < 0f)
            {
                // Kiểm tra lại bằng raycast với khoảng cách rất nhỏ để chắc chắn đã chạm đất
                Vector3 origin = groundCheckPoint != null ? groundCheckPoint.position : transform.position;
                float verySmallDistance = 0.05f; // Khoảng cách rất nhỏ
                float rayDist = groundCheckPoint != null ? groundCheckDistance + verySmallDistance : characterController.height * 0.5f + verySmallDistance;
                bool reallyGrounded = Physics.Raycast(origin, Vector3.down, rayDist, groundLayer);
                
                if (reallyGrounded)
                {
                    // Chỉ reset khi thực sự chạm đất
                    verticalVelocity = -2f; // Giữ lực nhỏ xuống để bám đất
                }
                // Nếu chưa thực sự chạm đất, tiếp tục áp dụng gravity
                else
                {
                    verticalVelocity += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
                }
            }
        }

        // Tính toán vertical movement vector (dùng chung cho cả 2 trường hợp)
        Vector3 verticalMovement = new Vector3(0f, verticalVelocity, 0f);

        // Không áp dụng input di chuyển khi đang tấn công (kiểm tra lần nữa để chắc chắn)
        if (isAttacking) return;
        
        Vector2 moveInput = InputManager.Instance.InputMoveVector();
        
        if (moveInput.magnitude < 0.1f)
        {
            // Không có input - chỉ áp dụng vertical velocity (gravity/jump)
            characterController.Move(verticalMovement * Time.deltaTime);
            
            // Cập nhật animation
            if (playerAnimation != null)
            {
                playerAnimation.SetMovement(false, 0f);
            }
            return;
        }

        // Tính toán hướng di chuyển tương đối với camera rotation
        Vector3 worldDirection = GetWorldDirection(moveInput);
        
        // Xoay player theo hướng di chuyển
        if (worldDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(worldDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Di chuyển player (horizontal + vertical) — chỉ khi không đang tấn công
        if (isAttacking) return;
        Vector3 horizontalVelocity = worldDirection * moveSpeed;
        Vector3 totalVelocity = horizontalVelocity + verticalMovement;
        characterController.Move(totalVelocity * Time.deltaTime);

        // Cập nhật animation walk
        if (playerAnimation != null)
        {
            float moveSpeedValue = moveInput.magnitude;
            playerAnimation.SetMovement(true, moveSpeedValue);
        }
    }
    
    /// <summary>
    /// Chỉ áp dụng trọng lực và di chuyển theo trục Y (dùng khi đang tấn công).
    /// </summary>
    private void ApplyVerticalMovementOnly()
    {
        if (!isGrounded)
        {
            verticalVelocity += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
            float maxFallSpeed = -20f;
            if (verticalVelocity < maxFallSpeed) verticalVelocity = maxFallSpeed;
        }
        else if (verticalVelocity < 0f)
        {
            Vector3 origin = groundCheckPoint != null ? groundCheckPoint.position : transform.position;
            float verySmallDistance = 0.05f;
            float rayDist = groundCheckPoint != null ? groundCheckDistance + verySmallDistance : characterController.height * 0.5f + verySmallDistance;
            if (Physics.Raycast(origin, Vector3.down, rayDist, groundLayer))
                verticalVelocity = -2f;
            else
                verticalVelocity += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
        characterController.Move(new Vector3(0f, verticalVelocity, 0f) * Time.deltaTime);
    }
    
    /// <summary>
    /// Nhận damage từ fireball và chết
    /// </summary>
    public void TakeFireballDamage()
    {
        TakeDamage(true); // true = phát âm thanh nổ
    }
    
    /// <summary>
    /// Nhận damage từ laser và chết (không phát âm thanh nổ)
    /// </summary>
    public void TakeLaserDamage()
    {
        TakeDamage(false); // false = không phát âm thanh nổ (đã phát ở laser)
    }
    
    /// <summary>
    /// Nhận damage và chết (internal method)
    /// </summary>
    /// <param name="playExplosionSound">Có phát âm thanh nổ không</param>
    private void TakeDamage(bool playExplosionSound)
    {
        // Disable input ngay lập tức để player không thể di chuyển thêm
        canReceiveInput = false;
        
        // Dừng movement ngay lập tức
        SetIdleAnimation();
        
        // Trigger death animation
        if (playerAnimation != null)
        {
            playerAnimation.SetDie();
        }
        
        // Nếu đang mang item, cho item bay về vị trí ban đầu
        if (carriedItem != null)
        {
            carriedItem.ReturnToOriginalPosition();
            carriedItem = null; // Reset carried item
        }
        
        // Chờ death animation xong rồi mới spawn lại
        StartCoroutine(HandleDeathAnimation(playExplosionSound));
        
        Debug.LogWarning($"PlayerController: Player bị trúng damage! Chết và sẽ spawn lại. PlayExplosionSound: {playExplosionSound}");
    }
    
    /// <summary>
    /// Nhận damage từ BoomItem và chết
    /// </summary>
    public void TakeBoomDamage()
    {
        // Disable input ngay lập tức để player không thể di chuyển thêm
        canReceiveInput = false;
        
        // Dừng movement ngay lập tức
        SetIdleAnimation();
        
        // Trigger death animation
        if (playerAnimation != null)
        {
            playerAnimation.SetDie();
        }
        
        // Nếu đang mang item, cho item bay về vị trí ban đầu
        if (carriedItem != null)
        {
            carriedItem.ReturnToOriginalPosition();
            carriedItem = null; // Reset carried item
        }
        
        // Chờ death animation xong rồi mới spawn lại
        StartCoroutine(HandleBoomDeath());
        
        Debug.LogWarning("PlayerController: Player chạm vào BoomItem! Chết và sẽ spawn lại.");
    }
    
    /// <summary>
    /// Xử lý death animation và spawn lại sau khi chết vì BoomItem
    /// </summary>
    private System.Collections.IEnumerator HandleBoomDeath()
    {
        AudioManager.Instance.PlayExplosion();
        // Đảm bảo player không di chuyển trong lúc animation chết
        // Disable CharacterController để player không bị ảnh hưởng bởi gravity hoặc physics
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        
        // Mạng đã được trừ trong BoomItem.Explode(), chỉ cần kiểm tra xem còn mạng không
        bool stillHasLives = true;
        if (HealthPanel.Instance != null)
        {
            stillHasLives = HealthPanel.Instance.GetCurrentLives() > 0;
            
            // Nếu hết mạng, hiển thị lose panel ngay và dừng, không chờ animation
            if (!stillHasLives)
            {
                Debug.LogWarning("PlayerController: Đã hết mạng! Hiển thị lose panel ngay, không chờ death animation.");
                
                // Đảm bảo lose panel đã được hiển thị
                if (UIManager.Instance != null && UIManager.Instance.gamePlayPanel != null)
                {
                    UIManager.Instance.gamePlayPanel.ShowLosePanel(true);
                    Time.timeScale = 0f;
                }
                
                yield break;
            }
        }
        
        // Chỉ chờ death animation nếu còn mạng
        yield return new WaitForSeconds(3.5f);
        
        Debug.LogWarning("PlayerController: Death animation hoàn thành, bắt đầu spawn lại...");

        // Bật lại CharacterController trước khi spawn
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        
        // Spawn lại (không cần trừ mạng nữa vì đã trừ trong BoomItem)
        ReturnToSpawnPointWithoutLosingLife();
        
        // Chờ một chút để đảm bảo spawn hoàn tất
        yield return new WaitForSeconds(0.2f);
    }
    
    /// <summary>
    /// Xử lý death animation và spawn lại sau khi chết
    /// </summary>
    private System.Collections.IEnumerator HandleDeathAnimation(bool playExplosionSound = true)
    {
        // Chỉ phát âm thanh nổ nếu được yêu cầu (fireball), không phát nếu chết bởi laser
        if (playExplosionSound && AudioManager.Instance != null)
    {
        AudioManager.Instance.PlayExplosion();
        }
        // Đảm bảo player không di chuyển trong lúc animation chết
        // Disable CharacterController để player không bị ảnh hưởng bởi gravity hoặc physics
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        
        // Trừ mạng trước để kiểm tra xem còn mạng không
        bool stillHasLives = true;
        if (HealthPanel.Instance != null)
        {
            // Lưu số mạng trước khi trừ
            int livesBefore = HealthPanel.Instance.GetCurrentLives();
            
            // Trừ mạng
            stillHasLives = HealthPanel.Instance.LoseLife();
            
            // Nếu hết mạng, hiển thị lose panel ngay và dừng, không chờ animation
            if (!stillHasLives)
            {
                Debug.LogWarning("PlayerController: Đã hết mạng! Hiển thị lose panel ngay, không chờ death animation.");
                
                // Đảm bảo lose panel đã được hiển thị
                if (UIManager.Instance != null && UIManager.Instance.gamePlayPanel != null)
                {
                    UIManager.Instance.gamePlayPanel.ShowLosePanel(true);
                    Time.timeScale = 0f;
                }
                
                yield break;
            }
        }
        
        // Chỉ chờ death animation nếu còn mạng
        yield return new WaitForSeconds(3.5f);
        
        Debug.LogWarning("PlayerController: Death animation hoàn thành, bắt đầu spawn lại...");

        // Bật lại CharacterController trước khi spawn
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        
        // Spawn lại (không cần trừ mạng nữa vì đã trừ ở trên)
        ReturnToSpawnPointWithoutLosingLife();
        
        // Chờ một chút để đảm bảo spawn hoàn tất
        yield return new WaitForSeconds(0.2f);
    }
    
    /// <summary>
    /// Spawn lại tại spawn point mà không trừ mạng (đã trừ ở HandleDeathAnimation)
    /// </summary>
    private void ReturnToSpawnPointWithoutLosingLife()
    {
        // Đánh dấu đang trong quá trình về spawn để tránh xử lý nhiều lần
        if (isReturningToSpawn)
        {
            return;
        }
        
        isReturningToSpawn = true;
        lastSpawnReturnTime = Time.time;
        
        // Tắt CharacterController tạm thời để teleport
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        
        // Teleport về spawn point
        Vector3 targetPosition = spawnPoint != null ? spawnPoint.position : initialSpawnPosition;
        transform.position = targetPosition;

        // Reset máu (HP) khi spawn lại
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.ResetHealth();
        }
        
        // Bật lại CharacterController
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        
        // Disable input ngay lập tức
        canReceiveInput = false;
        
        // Set idle animation
        SetIdleAnimation();
        
        Debug.Log("Player đã quay về spawn point!");
        
        // Chờ 1 giây rồi mới cho phép điều khiển lại
        StartCoroutine(EnableInputAfterSpawn());
        
        // Reset flag sau một khoảng thời gian ngắn để cho phép xử lý lần tiếp theo
        StartCoroutine(ResetSpawnReturnFlag());
    }

    /// <summary>
    /// Chuyển đổi input direction sang world direction dựa trên camera rotation
    /// </summary>
    private Vector3 GetWorldDirection(Vector2 inputDirection)
    {
        // Lấy rotation từ camera hoặc từ player rotation
        Quaternion rotation = Quaternion.identity;
        
        if (Camera.main != null)
        {
            // Dùng camera yaw để tính hướng di chuyển
            float cameraYaw = Camera.main.transform.eulerAngles.y;
            rotation = Quaternion.Euler(0f, cameraYaw, 0f);
        }
        else
        {
            // Nếu không có camera, dùng player rotation
            rotation = transform.rotation;
        }
        
        // Chuyển đổi input direction sang world direction
        Vector3 direction = new Vector3(inputDirection.x, 0f, inputDirection.y);
        return rotation * direction;
    }


    #endregion

    #region Visual & Camera

    private void UpdateCameraTarget()
    {
        // Không cần xoay camTarget nữa vì camera top-down chỉ follow position
        // Giữ hàm này để không phá vỡ code khác nhưng không làm gì
    }

    #endregion

    #region Collision Detection
        
    /// <summary>
    /// Xử lý va chạm với collider khi dùng Character Controller
    /// Character Controller không trigger OnCollisionEnter, cần dùng OnControllerColliderHit
    /// </summary>
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Va chạm với cổng kết thúc (EndTag) bằng collider thường (không phải trigger)
        if (hit.collider != null && hit.collider.CompareTag("EndTag"))
        {
            // Chỉ cho qua màn nếu đã nhặt đủ EnergyItem (nếu level có yêu cầu)
            if (LevelManager.Instance == null || LevelManager.Instance.HasCollectedEnoughEnergy())
            {
                ShowVictory();
            }
            else
            {
                Debug.Log("PlayerController: Chưa nhặt đủ EnergyItem để qua màn (collision)!");
                // TODO: Có thể hiển thị UI thông báo ở đây (popup, text, v.v.)
            }
        }

        // Xử lý checkpoint bằng collider thường
        Checkpoint checkpoint = hit.collider != null ? hit.collider.GetComponent<Checkpoint>() : null;
        if (checkpoint != null)
        {
            checkpoint.OnPlayerEnter(this);
        }

        // Kiểm tra va chạm với item (item không phải trigger)
        Item item = hit.gameObject.GetComponent<Item>();
        if (item != null && carriedItem == null && !item.IsPickedUp && !item.IsCollected)
        {
            // Chỉ lượm được nếu chưa có item nào đang mang
            // Lấy ItemPoint
            Transform itemPointTransform = GetItemPoint();
            
            // Lượm item (chưa tính điểm)
            item.PickupItem(itemPointTransform);
            
            // Thông báo cho PlayerController
            PickupItem(item);
        }
    }
    
    /// <summary>
    /// Lượm item (chỉ lượm được 1 item) - được gọi từ Item
    /// </summary>
    public void PickupItem(Item item)
    {
        if (carriedItem != null)
        {
            Debug.Log("Đã có item đang mang theo, không thể lượm thêm!");
            return;
        }
        
        carriedItem = item;
    }
    
    /// <summary>
    /// Thả item tại checkpoint
    /// </summary>
    public void DropItemAtCheckpoint(Transform checkpointPosition)
    {
        if (carriedItem == null)
        {
            Debug.Log("Không có item để thả!");
            return;
        }
        
        // Lưu item type trước khi thả
        ItemType itemType = carriedItem.ItemType;
        
        // Thả item tại checkpoint với callback để tính điểm sau khi animation hoàn thành
        carriedItem.DropItemAtCheckpoint(checkpointPosition, () =>
        {
            // Tính điểm khi animation thả item hoàn thành
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnItemCollected(itemType);
            }
        });
        
        // Reset carried item
        carriedItem = null;
    }
    
    /// <summary>
    /// Kiểm tra xem có đang mang item không
    /// </summary>
    public bool HasCarriedItem()
    {
        return carriedItem != null;
    }
    
    /// <summary>
    /// Lấy item đang mang theo
    /// </summary>
    public Item GetCarriedItem()
    {
        return carriedItem;
    }
    
    /// <summary>
    /// Quay về spawn point
    /// </summary>
    private void ReturnToSpawnPoint()
    {
        // Đánh dấu đang trong quá trình về spawn để tránh xử lý nhiều lần
        if (isReturningToSpawn)
        {
            return;
        }
        
        isReturningToSpawn = true;
        lastSpawnReturnTime = Time.time;
        
        
        
        // Trừ 1 mạng khi về spawn point
        if (HealthPanel.Instance != null)
        {
            bool stillHasLives = HealthPanel.Instance.LoseLife();
            
            // Nếu hết mạng, không cần teleport nữa vì đã hiển thị lose panel
            if (!stillHasLives)
            {
                Debug.Log("Player đã hết mạng! Không thể tiếp tục.");
                
                // Dừng tất cả coroutines và disable input
                StopAllCoroutines();
                canReceiveInput = false;
                isReturningToSpawn = false; // Reset flag
                
                // Đảm bảo lose panel đã được hiển thị
                if (UIManager.Instance != null && UIManager.Instance.gamePlayPanel != null)
                {
                    UIManager.Instance.gamePlayPanel.ShowLosePanel(true);
                    Time.timeScale = 0f;
                }
                
                return;
            }
        }
        
        // Tắt CharacterController tạm thời để teleport
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        
        // Teleport về spawn point
        Vector3 targetPosition = spawnPoint != null ? spawnPoint.position : initialSpawnPosition;
        transform.position = targetPosition;

        // Reset máu (HP) khi spawn lại
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.ResetHealth();
        }
        
        // Bật lại CharacterController
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        
        // Disable input ngay lập tức
        canReceiveInput = false;
        
        // Set idle animation
        SetIdleAnimation();
        
        Debug.Log("Player đã quay về spawn point!");
        
        // Chờ 1 giây rồi mới cho phép điều khiển lại
        StartCoroutine(EnableInputAfterSpawn());
        
        // Reset flag sau một khoảng thời gian ngắn để cho phép xử lý lần tiếp theo
        StartCoroutine(ResetSpawnReturnFlag());
    }
    
    /// <summary>
    /// Reset flag sau khi hoàn thành quá trình về spawn point
    /// </summary>
    private System.Collections.IEnumerator ResetSpawnReturnFlag()
    {
        yield return new WaitForSeconds(spawnReturnCooldown);
        isReturningToSpawn = false;
    }
    
    /// <summary>
    /// Enable lại input sau 1.5 giây khi về spawn point
    /// </summary>
    private System.Collections.IEnumerator EnableInputAfterSpawn()
    {
        // Chờ 1.5 giây
        yield return new WaitForSeconds(1.5f);
        
        // Enable lại input (chỉ nếu không bị disable bởi lý do khác)
        if (!isDisable)
        {
            canReceiveInput = true;
        }
        
        // Đảm bảo animation ở trạng thái idle
        SetIdleAnimation();
    }
    
    /// <summary>
    /// Spawn VFX effect khi nhặt Health Item tại VFX point
    /// </summary>
    public void SpawnHealthPickupVFX()
    {
        if (healthPickupVFXPrefab == null)
            return;
        
        // Xác định vị trí spawn VFX
        Vector3 spawnPosition = transform.position;
        Transform parentTransform = null;
        if (pickupVFXPoint != null)
        {
            spawnPosition = pickupVFXPoint.position;
            parentTransform = pickupVFXPoint;
        }
        
        // Spawn VFX và set làm con của pickupVFXPoint
        GameObject vfx = Instantiate(healthPickupVFXPrefab, spawnPosition, Quaternion.identity, parentTransform);
        
        // Tự động destroy VFX sau một khoảng thời gian (nếu VFX không tự destroy)
        Destroy(vfx, 0.5f);
    }
    
    /// <summary>
    /// Spawn VFX effect khi nhặt Speed Item tại VFX point
    /// </summary>
    public void SpawnSpeedPickupVFX()
    {
        if (speedPickupVFXPrefab == null)
            return;
        
        // Xác định vị trí spawn VFX
        Vector3 spawnPosition = transform.position;
        Transform parentTransform = null;
        if (pickupVFXPoint != null)
        {
            spawnPosition = pickupVFXPoint.position;
            parentTransform = pickupVFXPoint;
        }
        
        // Spawn VFX và set làm con của pickupVFXPoint
        GameObject vfx = Instantiate(speedPickupVFXPrefab, spawnPosition, Quaternion.identity, parentTransform);
        
        // Tự động destroy VFX sau một khoảng thời gian (nếu VFX không tự destroy)
        Destroy(vfx, 4f);
    }
    
    /// <summary>
    /// Hiển thị victory panel khi đến end gate
    /// </summary>
    private void ShowVictory()
    {
        // Kiểm tra xem đã collect đủ animal và đến endgate chưa
        if (QuestManager.Instance != null)
        {
            // Kiểm tra và hoàn thành quest nếu đã collect đủ
            QuestManager.Instance.CheckAndCompleteQuest();
        }
        else
        {
            Debug.LogWarning("QuestManager không tồn tại!");
        }
    }
    
    /// <summary>
    /// Set spawn point mới
    /// </summary>
    public void SetSpawnPoint(Transform newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
        if (spawnPoint != null)
        {
            initialSpawnPosition = spawnPoint.position;
        }
    }

    #endregion

    #region Public Methods

    public void SetDisable(bool disable)
    {
        isDisable = disable;
        
        if (characterController != null)
        {
            characterController.enabled = !disable;
        }
        
        if (disable)
        {
            SetIdleAnimation();
        }
    }

    /// <summary>
    /// Set cho phép nhận input hay không
    /// </summary>
    public void SetCanReceiveInput(bool canReceive)
    {
        canReceiveInput = canReceive;
    }

    public void SetIdleAnimation()
    {
        // Set movement to idle (speed = 0)
        playerAnimation?.SetMovement(false, 0f);
    }

    public GameObject GetModel()
    {
        return model;
    }

    /// <summary>
    /// Lấy ItemPoint transform
    /// </summary>
    public Transform GetItemPoint()
    {
        return itemPoint;
    }
    
    /// <summary>
    /// Kích hoạt speed boost cho player
    /// </summary>
    /// <param name="boostAmount">Tốc độ tăng thêm</param>
    /// <param name="duration">Thời gian boost (giây)</param>
    public void ActivateSpeedBoost(float boostAmount, float duration)
    {
        // Nếu đang có speed boost, reset lại thời gian
        if (isSpeedBoosted)
        {
            StopCoroutine("SpeedBoostCoroutine");
        }
        
        StartCoroutine(SpeedBoostCoroutine(boostAmount, duration));
    }
    
    /// <summary>
    /// Coroutine để tăng tốc độ trong thời gian nhất định
    /// </summary>
    private IEnumerator SpeedBoostCoroutine(float boostAmount, float duration)
    {
        isSpeedBoosted = true;
        moveSpeed = baseMoveSpeed + boostAmount;
        
        Debug.Log($"Speed Boost activated! Tốc độ: {moveSpeed} (tăng {boostAmount})");
        
        yield return new WaitForSeconds(duration);
        
        // Trở về tốc độ gốc
        moveSpeed = baseMoveSpeed;
        isSpeedBoosted = false;
        
        Debug.Log($"Speed Boost hết hạn! Tốc độ về: {baseMoveSpeed}");
    }

    #endregion

    #region Speed Skill

    /// <summary>
    /// Cập nhật speed skill (cooldown và timer)
    /// </summary>
    private void UpdateSpeedSkill()
    {
        // Cập nhật cooldown timer
        if (isSpeedSkillOnCooldown)
        {
            speedSkillCooldownTimer -= Time.deltaTime;
            if (speedSkillCooldownTimer <= 0f)
            {
                speedSkillCooldownTimer = 0f;
                isSpeedSkillOnCooldown = false;
                OnSpeedSkillCooldownChanged?.Invoke(0f);
                Debug.Log("PlayerController: Speed Skill cooldown đã hết, có thể sử dụng!");
            }
            else
            {
                float cooldownProgress = 1f - (speedSkillCooldownTimer / speedSkillCooldown);
                OnSpeedSkillCooldownChanged?.Invoke(cooldownProgress);
            }
        }

        // Cập nhật skill timer
        if (isSpeedSkillActive)
        {
            speedSkillTimer -= Time.deltaTime;
            if (speedSkillTimer <= 0f)
            {
                DeactivateSpeedSkill();
            }
        }
    }

    /// <summary>
    /// Thử kích hoạt speed skill (kiểm tra cooldown trước)
    /// </summary>
    public bool TryActivateSpeedSkill()
    {
        // Kiểm tra cooldown
        if (isSpeedSkillOnCooldown)
        {
            Debug.Log($"PlayerController: Speed Skill đang trong cooldown! Còn lại: {speedSkillCooldownTimer:F1}s");
            return false;
        }

        // Kiểm tra nếu đang disable
        if (isDisable || !canReceiveInput)
        {
            Debug.LogWarning("PlayerController: Không thể kích hoạt skill khi player đang disable!");
            return false;
        }

        // Kích hoạt skill
        ActivateSpeedSkill();
        return true;
    }

    /// <summary>
    /// Kích hoạt speed skill
    /// </summary>
    private void ActivateSpeedSkill()
    {
        if (isSpeedSkillActive)
        {
            Debug.LogWarning("PlayerController: Speed Skill đang hoạt động, không thể kích hoạt lại!");
            return;
        }

        isSpeedSkillActive = true;
        speedSkillTimer = speedSkillDuration;

        // Bắt đầu cooldown ngay khi kích hoạt skill
        isSpeedSkillOnCooldown = true;
        speedSkillCooldownTimer = speedSkillCooldown;
        OnSpeedSkillCooldownChanged?.Invoke(0f); // Bắt đầu từ 0 (fillAmount = 1)

        // Kích hoạt speed boost
        ActivateSpeedBoost(speedSkillBoostAmount, speedSkillDuration);

        // Spawn effect tại pickupVFXPoint (dùng speedPickupVFXPrefab)
        if (speedPickupVFXPrefab != null)
        {
            Vector3 spawnPosition = transform.position;
            Transform parentTransform = null;
            
            if (pickupVFXPoint != null)
            {
                spawnPosition = pickupVFXPoint.position;
                parentTransform = pickupVFXPoint;
            }
            
            // Spawn VFX và set làm con của pickupVFXPoint
            GameObject vfx = Instantiate(speedPickupVFXPrefab, spawnPosition, Quaternion.identity, parentTransform);
            
            // Tự động destroy VFX sau 5 giây (nếu VFX không tự destroy)
            Destroy(vfx, 5f);
        }

        // Phát âm thanh tương tự lúc nhặt speed item
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySpeedSound();
        }

        OnSpeedSkillStateChanged?.Invoke(true);
        Debug.Log($"PlayerController: Speed Skill đã kích hoạt! Tăng tốc {speedSkillBoostAmount} trong {speedSkillDuration}s, Cooldown: {speedSkillCooldown}s");
    }

    /// <summary>
    /// Tắt speed skill (được gọi tự động khi hết thời gian)
    /// </summary>
    private void DeactivateSpeedSkill()
    {
        if (!isSpeedSkillActive)
            return;

        isSpeedSkillActive = false;
        speedSkillTimer = 0f;

        // Cooldown đã bắt đầu từ khi kích hoạt skill, không cần bắt đầu lại

        OnSpeedSkillStateChanged?.Invoke(false);
        Debug.Log($"PlayerController: Speed Skill đã hết hạn! Cooldown đang tiếp tục...");
    }

    /// <summary>
    /// Lấy thời gian cooldown còn lại (giây)
    /// </summary>
    public float GetSpeedSkillCooldownRemaining()
    {
        return isSpeedSkillOnCooldown ? speedSkillCooldownTimer : 0f;
    }

    /// <summary>
    /// Lấy tiến độ cooldown (0-1)
    /// </summary>
    public float GetSpeedSkillCooldownProgress()
    {
        if (!isSpeedSkillOnCooldown)
            return 0f;
        return 1f - (speedSkillCooldownTimer / speedSkillCooldown);
    }

    /// <summary>
    /// Kiểm tra skill có đang hoạt động không
    /// </summary>
    public bool IsSpeedSkillActive()
    {
        return isSpeedSkillActive;
    }

    /// <summary>
    /// Lấy tiến độ skill timer (0-1), 1 = vừa kích hoạt, 0 = hết thời gian
    /// </summary>
    public float GetSpeedSkillTimerProgress()
    {
        if (!isSpeedSkillActive || speedSkillDuration <= 0f)
            return 0f;
        return speedSkillTimer / speedSkillDuration;
    }

    /// <summary>
    /// Kiểm tra skill có đang trong cooldown không
    /// </summary>
    public bool IsSpeedSkillOnCooldown()
    {
        return isSpeedSkillOnCooldown;
    }

    /// <summary>
    /// Reset speed skill (dùng khi bắt đầu level mới)
    /// </summary>
    public void ResetSpeedSkill()
    {
        isSpeedSkillActive = false;
        isSpeedSkillOnCooldown = false;
        speedSkillCooldownTimer = 0f;
        speedSkillTimer = 0f;
        
        // Dừng speed boost nếu đang active
        if (isSpeedBoosted)
        {
            StopCoroutine("SpeedBoostCoroutine");
            moveSpeed = baseMoveSpeed;
            isSpeedBoosted = false;
        }
        
        OnSpeedSkillStateChanged?.Invoke(false);
        OnSpeedSkillCooldownChanged?.Invoke(0f);
        Debug.Log("PlayerController: Speed Skill đã được reset!");
    }

    #endregion
}
