using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    [Tooltip("Điểm tuần tra thứ nhất (nếu để trống sẽ tự tạo dựa trên vị trí ban đầu)")]
    public Transform pointA;

    [Tooltip("Điểm tuần tra thứ hai (nếu để trống sẽ tự tạo dựa trên vị trí ban đầu)")]
    public Transform pointB;

    [Tooltip("Tốc độ di chuyển tuần tra")]
    public float moveSpeed = 2f;

    [Tooltip("Khoảng cách tuần tra tính từ vị trí ban đầu (enemy đứng giữa, đi ra trước và sau)")]
    public float autoPatrolDistance = 4f;

    [Tooltip("Khoảng cách tối thiểu để coi như đã tới điểm tuần tra")]
    public float reachPointDistance = 0.1f;

    [Tooltip("Stopping distance khi tuần tra bằng NavMeshAgent (nên nhỏ, ví dụ 0.05)")]
    public float patrolStoppingDistance = 0.05f;

    [Header("Player Detection & Attack")]
    [Tooltip("Transform của Player (lấy tự động từ PlayerController.Instance)")]
    private Transform player;

    [Tooltip("Khoảng cách bắt đầu đuổi/ tấn công player")]
    public float chaseRange = 5f;

    [Tooltip("Khoảng cách để mất mục tiêu (nên lớn hơn Chase Range để tránh giật/nhấp nhả)")]
    public float loseChaseRange = 7f;

    [Tooltip("Khoảng cách được coi là tấn công trúng player")]
    public float attackRange = 1.2f;

    [Tooltip("Thời gian giữa 2 lần tấn công")]
    public float attackCooldown = 2f;

    [Tooltip("Thời gian enemy đứng yên khi đánh (giây)")]
    public float attackLockDuration = 0.8f;

    [Tooltip("Damage gây ra mỗi lần enemy tấn công")]
    public int enemyDamage = 10;

    [Tooltip("Delay (giây) từ lúc đánh tới lúc tính trúng đòn/dính damage")]
    public float damageDelay = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;
    [SerializeField] private bool showGizmos = true;

    [Header("Ground / Physics")]
    [Tooltip("Giữ enemy ở đúng độ cao ban đầu (khóa trục Y) để tránh bị lún/rơi khi move bằng Transform")]
    [SerializeField] private bool lockYPosition = true;

    private float lockedY;
    private Rigidbody rb;
    private NavMeshAgent navAgent;

    [Header("Animation")]
    [Tooltip("Animator của enemy (kéo Animator vào đây)")]
    [SerializeField] private Animator animator;

    [Tooltip("Tên parameter float dùng cho speed (ví dụ: \"Speed\")")]
    [SerializeField] private string speedParam = "Speed";

    [Tooltip("Tên trigger dùng cho attack (ví dụ: \"Attack\")")]
    [SerializeField] private string attackTrigger = "Attack";

    private Transform currentTarget;
    private float lastAttackTime = -999f;
    private Vector3 lastFramePosition;
    private bool isAttacking = false;
    private string debugState = "Patrol";
    private bool isChasingPlayer = false;
    private bool wasChasingPlayer = false;

    private void Start()
    {
        // Giữ độ cao ban đầu
        lockedY = transform.position.y;

        // Nếu có Rigidbody thì cấu hình để không bị physics kéo rơi/lún (vì script đang move bằng Transform)
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        // NavMeshAgent (nếu có thì ưu tiên dùng để di chuyển)
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
            navAgent.stoppingDistance = Mathf.Max(0.01f, patrolStoppingDistance);
            navAgent.isStopped = false;

            // Khi dùng NavMeshAgent thì không cần lockYPosition kiểu Transform nữa
            lockYPosition = false;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        AcquirePlayer();

        // Nếu chưa gán pointA/pointB thì tự tạo 2 điểm dựa trên vị trí ban đầu và hướng nhìn
        if (pointA == null || pointB == null)
        {
            CreateAutoPatrolPoints();
        }

        // Nếu chưa gán target thì mặc định đi từ pointA
        currentTarget = pointA;

        wasChasingPlayer = isChasingPlayer;

        // Lưu vị trí khung hình trước để tính tốc độ cho animation
        lastFramePosition = transform.position;
    }

    private void OnDisable()
    {
        // Khi enemy chết -> EnemyHealth.Die() sẽ disable EnemyAI.
        // Hủy mọi Invoke pending để không còn gây damage/tấn công sau khi đã chết.
        CancelInvoke();
        isAttacking = false;

        if (navAgent != null && navAgent.enabled)
        {
            navAgent.isStopped = true;
        }
    }

    private void AcquirePlayer()
    {
        // Ưu tiên singleton
        if (PlayerController.Instance != null)
        {
            player = PlayerController.Instance.transform;
            return;
        }

        // Fallback theo tag nếu singleton chưa sẵn sàng
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    /// <summary>
    /// Tự tạo pointA/pointB nếu người dùng không gán, dựa trên vị trí ban đầu của enemy.
    /// Enemy đứng giữa, pointA phía sau, pointB phía trước theo hướng nhìn (forward).
    /// </summary>
    private void CreateAutoPatrolPoints()
    {
        // Hướng tuần tra theo local forward trên mặt phẳng XZ
        Vector3 forward = transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
        {
            // Nếu forward bị lỗi, fallback sang trục Z
            forward = Vector3.forward;
        }
        forward.Normalize();

        float halfDistance = Mathf.Max(0.1f, autoPatrolDistance * 0.5f);

        Vector3 origin = transform.position;
        Vector3 posA = origin - forward * halfDistance;
        Vector3 posB = origin + forward * halfDistance;

        if (pointA == null)
        {
            GameObject a = new GameObject($"{name}_PatrolPointA");
            a.transform.position = posA;
            pointA = a.transform;
        }

        if (pointB == null)
        {
            GameObject b = new GameObject($"{name}_PatrolPointB");
            b.transform.position = posB;
            pointB = b.transform;
        }
    }

    private void Update()
    {
        UpdateAnimatorSpeed();

        // Đang tấn công thì không di chuyển, chỉ update animation
        if (isAttacking)
        {
            debugState = "AttackLock";
            return;
        }

        if (player == null)
        {
            AcquirePlayer();
        }

        if (player == null)
        {
            debugState = "Patrol(NoPlayer)";
            PatrolBetweenPoints();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canReachPlayer = CanReachPosition(player.position);

        // Hysteresis + điều kiện đường đi:
        // - Chỉ bắt đầu đuổi khi trong tầm phát hiện và có đường đi hợp lệ tới player.
        // - Đang đuổi thì vẫn cần giữ được đường đi; mất đường đi sẽ hủy chase ngay.
        if (!isChasingPlayer)
        {
            if (distanceToPlayer <= chaseRange && canReachPlayer)
            {
                isChasingPlayer = true;
            }
        }
        else
        {
            float loseRange = Mathf.Max(loseChaseRange, chaseRange);
            if (distanceToPlayer > loseRange || !canReachPlayer)
            {
                isChasingPlayer = false;
            }
        }

        // Vừa rời khỏi trạng thái đuổi: chọn A hoặc B gần nhất để quay về rồi patrol tiếp
        if (wasChasingPlayer && !isChasingPlayer)
        {
            SelectClosestPatrolPointToCurrentPosition();
        }

        // Nếu đang đuổi -> đuổi theo, tới tầm đánh -> đứng lại và đánh
        if (isChasingPlayer)
        {
            debugState = distanceToPlayer <= attackRange ? "Attack" : "Chase";
            ChaseAndAttackPlayer(distanceToPlayer);
            return;
        }

        // Player ngoài tầm đuổi -> quay về tuần tra
        debugState = "Patrol";
        PatrolBetweenPoints();

        // Khóa trục Y nếu bật
        if (lockYPosition)
        {
            Vector3 p = transform.position;
            if (!Mathf.Approximately(p.y, lockedY))
            {
                transform.position = new Vector3(p.x, lockedY, p.z);
            }
        }

        wasChasingPlayer = isChasingPlayer;
    }

    private void SelectClosestPatrolPointToCurrentPosition()
    {
        if (pointA == null || pointB == null) return;

        float distA = Vector3.Distance(transform.position, pointA.position);
        float distB = Vector3.Distance(transform.position, pointB.position);

        currentTarget = distA <= distB ? pointA : pointB;
    }

    private void PatrolBetweenPoints()
    {
        if (pointA == null || pointB == null)
            return;

        // Xác định target hiện tại
        if (currentTarget == null)
        {
            currentTarget = pointA;
        }

        // Nếu có NavMeshAgent, dùng stoppingDistance nhỏ cho tuần tra
        if (navAgent != null && navAgent.enabled)
        {
            navAgent.stoppingDistance = Mathf.Max(0.01f, patrolStoppingDistance);
        }

        MoveTowards(currentTarget.position);

        // Đảo hướng khi tới gần điểm
        if (HasReachedDestination(currentTarget.position))
        {
            currentTarget = currentTarget == pointA ? pointB : pointA;
        }
    }

    private void ChaseAndAttackPlayer(float distanceToPlayer)
    {
        // Đang trong tầm truy đuổi nhưng không còn đường hợp lệ tới player -> hủy chase và quay về patrol
        if (navAgent != null && navAgent.enabled && !CanReachPosition(player.position))
        {
            if (debugLog)
            {
                Debug.Log($"EnemyAI[{name}]: Không tìm được đường tới player, quay về patrol.");
            }

            isChasingPlayer = false;
            SelectClosestPatrolPointToCurrentPosition();
            ResumeMovement();
            return;
        }

        // Trong tầm đánh: dừng lại và đánh
        if (distanceToPlayer <= attackRange)
        {
            StopMovement();
            FaceTowards(player.position);

            if (Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time;
                PerformAttack();
            }

            return;
        }

        // Ngoài tầm đánh nhưng còn trong tầm đuổi: đuổi theo player
        ResumeMovement();
        if (navAgent != null && navAgent.enabled)
        {
            navAgent.stoppingDistance = Mathf.Max(0.01f, attackRange);
        }
        MoveTowards(player.position);

        if (debugLog)
        {
            Debug.Log($"EnemyAI[{name}] Chase: dist={distanceToPlayer:F2}, chaseRange={chaseRange:F2}, attackRange={attackRange:F2}");
        }
    }

    private void FaceTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position);
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
            return;

        direction.Normalize();
        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        // Ưu tiên NavMeshAgent nếu có
        if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
        {
            navAgent.SetDestination(targetPosition);
            return;
        }

        Vector3 direction = (targetPosition - transform.position);
        direction.y = 0f; // Giữ enemy trên mặt phẳng

        if (direction.sqrMagnitude < 0.0001f)
            return;

        direction.Normalize();

        // Di chuyển
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Xoay mặt về hướng di chuyển
        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }
    }

    private void PerformAttack()
    {
        isAttacking = true;
        StopMovement();

        // Animation attack
        if (animator != null && !string.IsNullOrEmpty(attackTrigger))
        {
            animator.SetTrigger(attackTrigger);
        }

        Debug.Log("EnemyAI: Tấn công player!");

        // Sau một khoảng delay, nếu player vẫn trong tầm đánh thì mới tính damage
        CancelInvoke(nameof(ApplyDamageIfPlayerStillInRange));
        Invoke(nameof(ApplyDamageIfPlayerStillInRange), Mathf.Max(0f, damageDelay));

        // Sau một khoảng thời gian, cho phép di chuyển lại
        Invoke(nameof(EndAttackLock), attackLockDuration);
    }

    private void ApplyDamageIfPlayerStillInRange()
    {
        if (player == null)
        {
            AcquirePlayer();
        }

        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRange) return;

        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.TakeDamage(enemyDamage);
        }
    }

    private void EndAttackLock()
    {
        isAttacking = false;
        ResumeMovement();
    }

    private void StopMovement()
    {
        if (navAgent != null && navAgent.enabled)
        {
            navAgent.isStopped = true;
        }
    }

    private void ResumeMovement()
    {
        if (navAgent != null && navAgent.enabled)
        {
            navAgent.isStopped = false;
        }
    }

    private bool HasReachedDestination(Vector3 targetPosition)
    {
        if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
        {
            if (navAgent.pathPending) return false;
            // remainingDistance là khoảng cách còn lại trên đường đi
            float remaining = navAgent.remainingDistance;
            float threshold = Mathf.Max(reachPointDistance, navAgent.stoppingDistance + reachPointDistance);
            return remaining <= threshold;
        }

        return Vector3.Distance(transform.position, targetPosition) <= reachPointDistance;
    }

    private bool CanReachPosition(Vector3 targetPosition)
    {
        if (navAgent == null || !navAgent.enabled || !navAgent.isOnNavMesh)
            return true;

        NavMeshPath path = new NavMeshPath();
        if (!navAgent.CalculatePath(targetPosition, path))
            return false;

        // PathComplete: đi tới đích được.
        // PathPartial/Invalid: không đi tới đích trọn vẹn -> coi như không tìm được đường.
        return path.status == NavMeshPathStatus.PathComplete;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = new Color(1f, 0.9f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (pointA != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pointA.position, 0.15f);
        }

        if (pointB != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(pointB.position, 0.15f);
        }
    }

    /// <summary>
    /// Cập nhật tham số speed cho Animator dựa trên vận tốc hiện tại.
    /// </summary>
    private void UpdateAnimatorSpeed()
    {
        if (animator == null || string.IsNullOrEmpty(speedParam))
            return;

        float currentSpeed;
        if (navAgent != null && navAgent.enabled)
        {
            currentSpeed = navAgent.velocity.magnitude;
        }
        else
        {
            float distance = Vector3.Distance(transform.position, lastFramePosition);
            currentSpeed = distance / Mathf.Max(Time.deltaTime, 0.0001f);
        }

        // Chuẩn hóa về 0-1 để dùng cho blend tree walk/run nếu cần
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / moveSpeed);
        animator.SetFloat(speedParam, normalizedSpeed);

        lastFramePosition = transform.position;
    }
}

