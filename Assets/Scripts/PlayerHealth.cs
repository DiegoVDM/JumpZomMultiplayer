using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    public int maxHearts = 3;
    public float invincibilityDuration = 5f;

    public HealthUI healthUI;  

    int currentHearts;
    float invincibleUntil = 0f;

    void Start()
    {
        currentHearts = maxHearts;

        if (healthUI != null)
            healthUI.SetHearts(currentHearts);
    }

    public void TakeHit()
    {
        if (Time.time < invincibleUntil)
            return;

        currentHearts = Mathf.Max(0, currentHearts - 1);

        if (healthUI != null)
            healthUI.SetHearts(currentHearts);

        invincibleUntil = Time.time + invincibilityDuration;


        if (currentHearts <= 0)
        {
            var controller = GetComponent<PlayerController>();
            if (controller != null)
                controller.enabled = false;

       
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeHit();
        }
    }
}
