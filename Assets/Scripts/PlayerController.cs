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

    private Vector2 moveInput;
    private Rigidbody2D rb;

    public bool IsMoving { get; private set; }

    public bool _isFacingRight = true;

    public bool IsFacingRight
    {
        get => _isFacingRight;
        private set
        {
            if (_isFacingRight == value) return;
            _isFacingRight = value;

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
        Debug.Log(
            $"PlayerController.OnNetworkSpawn | name={name} | OwnerClientId={OwnerClientId} | " +
            $"IsOwner={IsOwner} | IsServer={IsServer} | IsClient={IsClient}"
        );

        if (!IsOwner)
        {
            moveInput = Vector2.zero;
        }
    }

    public override void OnNetworkDespawn()
    {
        Debug.Log($"PlayerController.OnNetworkDespawn | name={name}");
    }

    private void Update()
    {
        if (!IsOwner) return;

        float horizontal = 0f;

        if (Keyboard.current.aKey.isPressed)
            horizontal = -1f;
        else if (Keyboard.current.dKey.isPressed)
            horizontal = 1f;

        moveInput = new Vector2(horizontal, 0f);
        IsMoving = horizontal != 0f;

        if (horizontal > 0f && !IsFacingRight)
            IsFacingRight = true;
        else if (horizontal < 0f && IsFacingRight)
            IsFacingRight = false;

        bool groundedNow = rb.IsTouchingLayers(groundLayer);

        if (Keyboard.current.wKey.wasPressedThisFrame && groundedNow)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (Keyboard.current.jKey.wasPressedThisFrame && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        rb.linearVelocity = new Vector2(moveInput.x * walkSpeed, rb.linearVelocity.y);
    }

    private void Fire()
    {
        if (!bulletPrefab || !muzzle) return;

        // Single-player/local-only bullet spawn kept as reference for later multiplayer conversion.
        // Later this should become server/network spawning.
        var b = Instantiate(bulletPrefab, muzzle.position, Quaternion.identity);
        b.direction = IsFacingRight ? 1 : -1;

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
        // not used right now, keeping it so it doesn't break your setup
    }
}