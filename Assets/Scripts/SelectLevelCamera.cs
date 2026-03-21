using UnityEngine;
using System.Collections.Generic;

public class SelectLevelCamera : MonoBehaviour
{
    public static SelectLevelCamera Instance { get; private set; }

    [Header("Camera")]
    [SerializeField] private Camera cam;
    [SerializeField] private float moveSpeed = 5f;

    [Header("Camera Points")]
    [Tooltip("Các vị trí camera đặt sẵn trong scene")]
    [SerializeField] private List<Transform> cameraPoints = new List<Transform>();

    private int currentIndex = 0;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;
    private bool initialized = false;

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

    private void Initialize()
    {
        currentIndex = 0;
        isMoving = false;

        if (cam == null)
            cam = Camera.main;
        if (cam == null)
            cam = FindFirstObjectByType<Camera>();

        if (cameraPoints.Count > 0 && cam != null)
        {
            cam.transform.position = cameraPoints[0].position;
            cam.transform.rotation = cameraPoints[0].rotation;
            targetPosition = cameraPoints[0].position;
            targetRotation = cameraPoints[0].rotation;
        }

        initialized = true;
        Debug.Log($"SelectLevelCamera: Initialized. cam={cam != null}, points={cameraPoints.Count}");
    }

    private void Update()
    {
        if (!initialized && cam == null)
        {
            cam = Camera.main;
            if (cam == null) cam = FindFirstObjectByType<Camera>();
            if (cam != null) Initialize();
        }

        if (!isMoving || cam == null) return;

        cam.transform.position = Vector3.Lerp(cam.transform.position, targetPosition, moveSpeed * Time.deltaTime);
        cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, targetRotation, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(cam.transform.position, targetPosition) < 0.05f)
        {
            cam.transform.position = targetPosition;
            cam.transform.rotation = targetRotation;
            isMoving = false;
        }
    }

    public void GoNext()
    {
        if (cam == null || cameraPoints.Count == 0) return;
        if (currentIndex >= cameraPoints.Count - 1) return;
        currentIndex++;
        MoveToPoint(currentIndex);
    }

    public void GoPrev()
    {
        if (cam == null || cameraPoints.Count == 0) return;
        if (currentIndex <= 0) return;
        currentIndex--;
        MoveToPoint(currentIndex);
    }

    public bool HasNext() => currentIndex < cameraPoints.Count - 1;
    public bool HasPrev() => currentIndex > 0;

    private void MoveToPoint(int index)
    {
        if (index < 0 || index >= cameraPoints.Count) return;
        targetPosition = cameraPoints[index].position;
        targetRotation = cameraPoints[index].rotation;
        isMoving = true;
    }
}
