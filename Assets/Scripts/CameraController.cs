using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the Main Camera in a 2.5D environment.
/// Keeps a target (Meca) centered, allows for edge panning, and scroll-based zoom.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("The Transform the camera will follow (e.g., the Capsule/Meca).")]
    [SerializeField] private Transform target;
    [Tooltip("How smoothly the camera follows the target.")]
    [SerializeField] private float followSpeed = 5f;

    [Header("Edge Panning")]
    [Tooltip("Maximum offset the camera can pan away from the target center.")]
    [SerializeField] private float maxEdgePanOffset = 2f;
    [Tooltip("How fast the camera pans when the cursor is at the screen edge.")]
    [SerializeField] private float edgePanSpeed = 2f;
    [Tooltip("Distance from the screen border in pixels to trigger panning.")]
    [SerializeField] private float edgePanThreshold = 20f;

    [Header("Zoom Settings")]
    [Tooltip("Minimum Z position (closest to the scene).")]
    [SerializeField] private float minZoom = -5f;
    [Tooltip("Maximum Z position (furthest from the scene).")]
    [SerializeField] private float maxZoom = -20f;
    [Tooltip("How fast the zoom changes with the scroll wheel.")]
    [SerializeField] private float zoomSpeed = 10f;
    [Tooltip("How smoothly the camera interpolates towards the target zoom.")]
    [SerializeField] private float zoomSmoothing = 5f;

    private float currentZoom;
    private Vector2 edgePanOffset;

    private void Start()
    {
        // Initialize zoom to current camera Z position, clamped within limits.
        currentZoom = Mathf.Clamp(transform.position.z, Mathf.Min(minZoom, maxZoom), Mathf.Max(minZoom, maxZoom));
    }

    private void Update()
    {
        HandleZoom();
        HandleEdgePanning();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 1. Follow the target on X and Y, with edge pan offset
        Vector3 targetPosition = target.position;
        float finalX = targetPosition.x + edgePanOffset.x;
        float finalY = targetPosition.y + edgePanOffset.y;

        // 3. Smoothly move the camera to the target position on X and Y
        float newX = Mathf.Lerp(transform.position.x, finalX, Time.deltaTime * followSpeed);
        float newY = Mathf.Lerp(transform.position.y, finalY, Time.deltaTime * followSpeed);

        // 4. Smoothly move the camera on Z using zoomSmoothing
        float newZ = Mathf.Lerp(transform.position.z, currentZoom, Time.deltaTime * zoomSmoothing);

        transform.position = new Vector3(newX, newY, newZ);
    }

    /// <summary>
    /// Adjusts the target Z position based on mouse scroll input.
    /// </summary>
    private void HandleZoom()
    {
        if (Mouse.current != null)
        {
            // The scroll value in the new Input System is typically larger (e.g., 120 per notch)
            float rawScroll = Mouse.current.scroll.y.ReadValue();
            
            if (Mathf.Abs(rawScroll) > 0.1f)
            {
                // Normalize the scroll to a reasonable multiplier (-1 to 1 range typically per notch)
                float scrollNormalized = Mathf.Clamp(rawScroll * 0.01f, -1f, 1f);
                
                // Zooming in/out changes the target zoom value
                currentZoom += scrollNormalized * zoomSpeed * 0.5f;
                // Clamp currentZoom between minZoom and maxZoom. 
                // Note: Since Z is negative, maxZoom is often a more negative value than minZoom.
                float trueMin = Mathf.Min(minZoom, maxZoom);
                float trueMax = Mathf.Max(minZoom, maxZoom);
                currentZoom = Mathf.Clamp(currentZoom, trueMin, trueMax);
            }
        }
    }

    /// <summary>
    /// Calculates the pan offset based on cursor position relative to screen edges.
    /// </summary>
    private void HandleEdgePanning()
    {
        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 targetPanOffset = Vector2.zero;

        // Check horizontal edges
        if (mousePos.x <= edgePanThreshold)
        {
            targetPanOffset.x = -maxEdgePanOffset;
        }
        else if (mousePos.x >= Screen.width - edgePanThreshold)
        {
            targetPanOffset.x = maxEdgePanOffset;
        }

        // Check vertical edges
        if (mousePos.y <= edgePanThreshold)
        {
            targetPanOffset.y = -maxEdgePanOffset;
        }
        else if (mousePos.y >= Screen.height - edgePanThreshold)
        {
            targetPanOffset.y = maxEdgePanOffset;
        }

        // Smoothly interpolate the current offset towards the target offset
        edgePanOffset = Vector2.Lerp(edgePanOffset, targetPanOffset, Time.deltaTime * edgePanSpeed);
    }
}
