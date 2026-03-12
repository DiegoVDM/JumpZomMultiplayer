using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : NetworkBehaviour
{
    public float walkSpeed = 5f;

    public float jumpForce = 8f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    public Bullet bulletPrefab;
    public Transform muzzle;
    public float fireCooldown = 0.15f;
    float nextFireTime = 0f;

    public AudioClip shootClip;
    private AudioSource audioSource;

    Vector2 moveInput;

    public bool IsMoving { get; private set; }

    Rigidbody2D rb;

    public bool _isFacingRight = true;

    public bool IsFacingRight
    {
        get => _isFacingRight;
        private set
        {
            if (_isFacingRight == value) return;
            _isFacingRight = value;

            // Flip the sprite by mirroring X scale
            var scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            moveInput = Vector2.zero;
        }
    }

    void Update()
    {
        // Only the owning player should read input
        if (!IsOwner) return;

        // grounded check
        bool groundedNow = rb.IsTouchingLayers(groundLayer);

        // Jump on W press if grounded
        if (Keyboard.current.wKey.wasPressedThisFrame && groundedNow)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // fire with J
        if (Keyboard.current.jKey.wasPressedThisFrame && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    private void FixedUpdate()
    {
        // Only the owning player should apply movement
        if (!IsOwner) return;

        rb.linearVelocity = new Vector2(moveInput.x * walkSpeed, rb.linearVelocity.y);
    }

    void Fire()
    {
        if (!bulletPrefab || !muzzle) return;

        // For now this still does local bullet spawning.
        // We can network shooting next after movement is confirmed working.
        var b = Instantiate(bulletPrefab, muzzle.position, Quaternion.identity);
        b.direction = IsFacingRight ? 1 : -1;

        // Optionally flip bullet sprite to face travel
        var s = b.transform.localScale;
        s.x = Mathf.Abs(s.x) * (b.direction >= 0 ? 1 : -1);
        b.transform.localScale = s;

        if (audioSource != null && shootClip != null)
        {
            audioSource.PlayOneShot(shootClip);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // Only the owning player should respond to input actions
        if (!IsOwner) return;

        moveInput = context.ReadValue<Vector2>();

        IsMoving = moveInput != Vector2.zero;

        SetFacingDirection(moveInput);
    }

    private void SetFacingDirection(Vector2 moveInput)
    {
        if (moveInput.x > 0 && !IsFacingRight)
        {
            IsFacingRight = true;
        }
        else if (moveInput.x < 0 && IsFacingRight)
        {
            IsFacingRight = false;
        }
    }
}