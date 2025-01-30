using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    private Rigidbody2D body;
    private Animator animatorplayer;
    private bool grounded;
    private bool canMove = true;

    public bool IsFacingRight { get; private set; } = true; // Tracks player direction
    private float horizontalInput; // Move horizontalInput to class level

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animatorplayer = GetComponent<Animator>();
    }

    public bool IsMoving()
    {
        return Mathf.Abs(horizontalInput) > 0.01f; // Returns true if there is movement input
    }


    public void DisableMovement()
    {
        canMove = false;
        body.velocity = Vector2.zero; // Stop movement
    }

    public void EnableMovement()
    {
        canMove = true;
    }

   private void Update()
{
    if (!canMove) return; // Prevent movement while aiming

    horizontalInput = Input.GetAxis("Horizontal");

    if (body != null)
    {
        body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);

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

        if (Input.GetKey(KeyCode.Space) && grounded)
        {
            Jump();
        }

        animatorplayer.SetBool("Run", horizontalInput != 0);
        animatorplayer.SetBool("Grounded", grounded);
    }
}

    public void Flip()
    {
        IsFacingRight = !IsFacingRight; // Toggle direction
        transform.localScale = new Vector2(IsFacingRight ? 4 : -4, 4);
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
