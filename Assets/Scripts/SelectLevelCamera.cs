using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SelectLevelCamera : MonoBehaviour
{
    public static SelectLevelCamera Instance { get; private set; }

    [SerializeField] private float moveSpeed = 5f;

    [Header("Camera Points")]
    [Tooltip("Các vị trí Main Camera sẽ tới (theo thứ tự)")]
    [SerializeField] private List<Transform> cameraPoints = new List<Transform>();

    private int currentIndex = 0;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;
    private Camera targetCamera;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        Instance = this;
        Initialize();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Tìm Main Camera trong cùng scene với script (tag MainCamera, hoặc Camera.main đúng scene).
    /// </summary>
    private Camera FindMainCameraInThisScene()
    {
        Scene scene = gameObject.scene;
        if (!scene.IsValid()) return null;

        Camera[] all = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Camera fallbackEnabled = null;

        for (int i = 0; i < all.Length; i++)
        {
            Camera c = all[i];
            if (c == null || c.gameObject.scene != scene) continue;

            if (c.CompareTag("MainCamera"))
                return c;

            if (fallbackEnabled == null && c.enabled)
                fallbackEnabled = c;
        }

        if (Camera.main != null && Camera.main.gameObject.scene == scene)
            return Camera.main;

        return fallbackEnabled;
    }

    /// <summary>
    /// CameraFollowController chạy LateUpdate và ghi đè vị trí camera theo player — phải tắt khi chọn level.
    /// </summary>
    private void DisableCameraFollowOnTargetCamera()
    {
        if (targetCamera == null) return;
        var follow = targetCamera.GetComponent<CameraFollowController>()
            ?? targetCamera.GetComponentInParent<CameraFollowController>();
        if (follow != null)
            follow.enabled = false;
    }

    private void Initialize()
    {
        currentIndex = 0;
        isMoving = false;
        targetCamera = FindMainCameraInThisScene();
        DisableCameraFollowOnTargetCamera();

        if (cameraPoints.Count > 0 && targetCamera != null)
        {
            targetCamera.transform.position = cameraPoints[0].position;
            targetCamera.transform.rotation = cameraPoints[0].rotation;
            targetPosition = cameraPoints[0].position;
            targetRotation = cameraPoints[0].rotation;
        }
    }

    private void Update()
    {
        if (isMoving && (targetCamera == null || !targetCamera))
        {
            targetCamera = FindMainCameraInThisScene();
            DisableCameraFollowOnTargetCamera();
        }

        if (!isMoving || targetCamera == null) return;

        // Dùng unscaledDeltaTime để camera vẫn chạy nếu timeScale về 0 do lỗi/lệch flow (Lerp với deltaTime=0 sẽ đứng im).
        float dt = Time.unscaledDeltaTime;
        targetCamera.transform.position = Vector3.Lerp(targetCamera.transform.position, targetPosition, moveSpeed * dt);
        targetCamera.transform.rotation = Quaternion.Slerp(targetCamera.transform.rotation, targetRotation, moveSpeed * dt);

        if (Vector3.Distance(targetCamera.transform.position, targetPosition) < 0.05f)
        {
            targetCamera.transform.position = targetPosition;
            targetCamera.transform.rotation = targetRotation;
            isMoving = false;
        }
    }

    public void GoNext()
    {
        targetCamera = FindMainCameraInThisScene();
        DisableCameraFollowOnTargetCamera();
        if (targetCamera == null || cameraPoints.Count == 0) return;
        if (currentIndex >= cameraPoints.Count - 1) return;
        currentIndex++;
        MoveToPoint(currentIndex);
    }

    public void GoPrev()
    {
        targetCamera = FindMainCameraInThisScene();
        DisableCameraFollowOnTargetCamera();
        if (targetCamera == null || cameraPoints.Count == 0) return;
        if (currentIndex <= 0) return;
        currentIndex--;
        MoveToPoint(currentIndex);
    }

    public bool HasNext() => cameraPoints.Count > 0 && currentIndex < cameraPoints.Count - 1;
    public bool HasPrev() => currentIndex > 0;

    private void MoveToPoint(int index)
    {
        if (index < 0 || index >= cameraPoints.Count) return;
        targetPosition = cameraPoints[index].position;
        targetRotation = cameraPoints[index].rotation;
        isMoving = true;
    }
}
