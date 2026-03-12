using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyWalker : MonoBehaviour
{
    public float speed = 2f;
    public bool startMovingRight = false;   

    // Keep enemy on screen
    public float screenMargin = 0.5f;

    Rigidbody2D rb;
    int dir; 
    float halfWidth;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 1f;             
        rb.freezeRotation = true;
        dir = startMovingRight ? 1 : -1;

       
        var rend = GetComponentInChildren<SpriteRenderer>();
        halfWidth = rend ? rend.bounds.extents.x : 0.5f;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

        
        var cam = Camera.main;
        if (!cam) return;
        Vector3 left = cam.ViewportToWorldPoint(new Vector3(0, 0.5f, 0));
        Vector3 right = cam.ViewportToWorldPoint(new Vector3(1, 0.5f, 0));

        if (transform.position.x < left.x + screenMargin + halfWidth) dir = 1;
        if (transform.position.x > right.x - screenMargin - halfWidth) dir = -1;

        
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (dir >= 0 ? 1 : -1);
        transform.localScale = s;
    }
}
