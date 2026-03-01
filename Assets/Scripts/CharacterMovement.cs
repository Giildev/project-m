using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles basic 4-directional movement on the XY plane for the player character placeholder.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed multiplier.")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody rb;
    private Vector2 movementInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveY -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveY += 1f;
        }
        
        // Normalize input to prevent moving faster diagonally
        movementInput = new Vector2(moveX, moveY).normalized;
    }

    private void FixedUpdate()
    {
        // Apply velocity to the Rigidbody. We set Z velocity to 0 to ensure it stays on the 2D plane.
        rb.linearVelocity = new Vector3(movementInput.x * moveSpeed, movementInput.y * moveSpeed, 0f);
    }
}
