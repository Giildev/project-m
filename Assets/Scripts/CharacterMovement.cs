using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles basic 2.5D side-scrolling movement on the XY plane for the player character.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed multiplier.")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Tooltip("Force applied when jumping.")]
    [SerializeField] private float jumpForce = 7f;
    
    [Tooltip("Distance to check for ground below the character. Should be slightly more than half the capsule's height.")]
    [SerializeField] private float groundCheckDistance = 1.1f;

    private Rigidbody rb;
    private float moveX;
    private bool jumpRequested;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Ensure Rigidbody respects gravity and is constrained correctly for 2.5D
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
    }

    private void Update()
    {
        moveX = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;

            if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                jumpRequested = true;
            }
        }
    }

    private void FixedUpdate()
    {
        CheckGrounded();

        // Apply horizontal velocity, keep existing vertical velocity for gravity
        Vector3 newVelocity = new Vector3(moveX * moveSpeed, rb.linearVelocity.y, 0f);

        if (jumpRequested)
        {
            if (isGrounded)
            {
                // Set explicit vertical velocity for consistent jump heights regardless of falling speed
                newVelocity.y = jumpForce;
            }
            jumpRequested = false;
        }

        rb.linearVelocity = newVelocity;
    }

    private void CheckGrounded()
    {
        // Simple raycast downwards from the center of the object to detect floor
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
    }
}
