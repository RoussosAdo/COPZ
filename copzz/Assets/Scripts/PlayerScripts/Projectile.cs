using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed; // Speed of the projectile
    private Rigidbody2D rb; // Reference to Rigidbody2D
    private bool hit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // Ensure Rigidbody2D exists
    }

    private void OnEnable()
    {
        rb.velocity = Vector2.zero; // Reset velocity every time the bullet is enabled
        hit = false;
    }

    public void SetDirection(Vector2 direction)
    {
        hit = false;

        // Normalize direction and apply speed
        Vector2 velocity = direction.normalized * speed;
        rb.velocity = velocity;

        // Rotate the bullet to face the direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        hit = true;

        // Stop the bullet on collision
        rb.velocity = Vector2.zero;

        // Deactivate the bullet
        gameObject.SetActive(false);
    }
}
