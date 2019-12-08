using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    public int speed;
    public float jump;
    private bool forceJumpAnimation;
    private float forceJumpTimer;
    private float forceJumpTime = .11f;

    private Animator animationController;
    private Rigidbody2D rb;
    void Start()
    {
        animationController = GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Jump();
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(Vector3.left * Time.deltaTime * speed);
            animationController.SetBool("isRunning", true);
            if (transform.localScale.x > 0)
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(Vector3.right * Time.deltaTime * speed);
            animationController.SetBool("isRunning", true);
            if (transform.localScale.x < 0)
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);

        } else {
            animationController.SetBool("isRunning", false);
        }
            Debug.Log(IsGrounded());

        if (IsGrounded() && animationController.GetBool("isJumping") && forceJumpTimer > forceJumpTime){
            animationController.SetBool("isJumping", false);
        }

        forceJumpTimer += Time.deltaTime;
    }

    bool IsGrounded() {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up, 3.95f, 1 << 8);
        if (hit.collider != null)
            Debug.Log(hit.collider.gameObject.name);
        return hit;
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(new Vector2(0f, jump), ForceMode2D.Impulse);
            animationController.SetBool("isJumping", true);
            forceJumpTimer = 0;
        }
    }
}
