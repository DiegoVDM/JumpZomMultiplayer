using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : NetworkBehaviour
{
    public float walkSpeed = 5f;
    public float jumpForce = 8f;
    public LayerMask groundLayer;

    private Vector2 moveInput;
    private Rigidbody2D rb;

    public bool IsMoving { get; private set; }

    private bool _isFacingRight = true;

    public bool IsFacingRight
    {
        get => _isFacingRight;
        private set
        {
            if (_isFacingRight == value) return;
            _isFacingRight = value;

            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        // Optional but helpful:
        // freeze non-owner input state when spawned
        if (!IsOwner)
        {
            moveInput = Vector2.zero;
        }
    }

    private void Update()
    {
        // Only the owning player should read input
        if (!IsOwner) return;

        bool groundedNow = rb.IsTouchingLayers(groundLayer);

        if (Keyboard.current.wKey.wasPressedThisFrame && groundedNow)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void FixedUpdate()
    {
        // Only the owning player should apply movement
        if (!IsOwner) return;

        rb.linearVelocity = new Vector2(moveInput.x * walkSpeed, rb.linearVelocity.y);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // Only the owning player should respond to input actions
        if (!IsOwner) return;

        moveInput = context.ReadValue<Vector2>();
        IsMoving = moveInput != Vector2.zero;
        SetFacingDirection(moveInput);
    }

    private void SetFacingDirection(Vector2 input)
    {
        if (input.x > 0 && !IsFacingRight)
        {
            IsFacingRight = true;
        }
        else if (input.x < 0 && IsFacingRight)
        {
            IsFacingRight = false;
        }
    }
}