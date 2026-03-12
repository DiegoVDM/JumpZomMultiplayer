using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
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

    public AudioClip shootClip;   // drag your wav here in Inspector
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
            var scale = transform.localScale; // Vector3
            scale.x *= -1f;
            transform.localScale = scale;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Simple grounded check using current colliders
        bool isGrounded = rb.IsTouchingLayers(groundLayer);

        // Jump on W press if grounded
        if (Keyboard.current.wKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // fire with J (or Space/LeftMouse if you prefer)
        if (Keyboard.current.jKey.wasPressedThisFrame && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireCooldown;
        }

    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * walkSpeed, rb.linearVelocity.y);
    }




    void Fire()
    {
        if (!bulletPrefab || !muzzle) return;

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
        moveInput = context.ReadValue<Vector2>();

        IsMoving = moveInput != Vector2.zero;

        SetFacingDirection(moveInput);
    }

    private void SetFacingDirection(Vector2 moveInput)
    {
        if(moveInput.x > 0 && !IsFacingRight)
        {
            // Face the right
            IsFacingRight = true;
        }
        else if (moveInput.x < 0 && IsFacingRight)
        {
            // Face the left
            IsFacingRight = false;
        }


    }




}
