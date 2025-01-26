using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    private Rigidbody2D body;
    private Animator animatorplayer;
    private bool grounded;


    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animatorplayer = GetComponent<Animator>();
    }

    private void Update()
    {

        float horizontalInput = Input.GetAxis("Horizontal");


        if(body != null)
        {
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);

            //For Flipping Sides,Change Vector In depend of Sprite Scale
            if(horizontalInput > 0.01f)
            {
                    transform.localScale = new Vector2(4,4);
            }else if(horizontalInput < -0.01f)
            {
                transform.localScale = new Vector2(-4, 4); 
            }


            if(Input.GetKey(KeyCode.Space) && grounded)
            {
                Jump();
            }

            animatorplayer.SetBool("Run", horizontalInput != 0); //This will be walk animation [Cause it happens every time you press "d" so x axis]
            animatorplayer.SetBool("Grounded",grounded);
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
        if(collision.gameObject.tag == "Ground")
        {
            grounded = true;    
        }
    }
}
