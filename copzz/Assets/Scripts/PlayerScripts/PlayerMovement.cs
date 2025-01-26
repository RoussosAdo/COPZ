using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    private Rigidbody2D body;
    private Animator animatorplayer;
    private bool grounded;

    public bool IsFacingRight { get; private set; } = true; // Tracks player direction
    private float horizontalInput; // Move horizontalInput to class level

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animatorplayer = GetComponent<Animator>();
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal"); // Update the class-level variable

        if (body != null)
        {
            // Move the player horizontally
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);

            // Flip the player based on movement direction
            if (horizontalInput > 0.01f)
            {
                transform.localScale = new Vector2(4, 4);
                IsFacingRight = true;
            }
            else if (horizontalInput < -0.01f)
            {
                transform.localScale = new Vector2(-4, 4);
                IsFacingRight = false;
            }

            // Handle jump input
            if (Input.GetKey(KeyCode.Space) && grounded)
            {
                Jump();
            }

            // Update animations
            animatorplayer.SetBool("Run", horizontalInput != 0);
            animatorplayer.SetBool("Grounded", grounded);
        }
    }

    private void Jump()
    {
        body.velocity = new Vector2(body.velocity.x, jumpForce);
        animatorplayer.SetTrigger("Jump");
        grounded = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            grounded = true;
        }
    }

    public bool canShoot()
    {
        return horizontalInput == 0 && grounded; // Now horizontalInput can be accessed
    }
}
