using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed; // Speed of the projectile
    private float direction;
    private Rigidbody2D rb; // Reference to Rigidbody2D
    private bool hit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // Ensure Rigidbody2D exists
    }

    private void OnEnable()
    {
        // Reset velocity every time the bullet is enabled
        rb.velocity = Vector2.zero;
        hit = false;
    }

    public void SetDirection(float _direction)
    {
        direction = _direction;
        hit = false;

        // Ensure bullet faces the right direction
        float localScaleX = Mathf.Abs(transform.localScale.x) * Mathf.Sign(direction);
        transform.localScale = new Vector3(localScaleX, transform.localScale.y, transform.localScale.z);

        // Add force to the Rigidbody2D
        rb.AddForce(new Vector2(speed * direction, 0), ForceMode2D.Impulse);
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
