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

    private bool usedDoubleJump = false;

    private Animator animationController;
    private Rigidbody2D rb;
    private Transform wallCheck;

    private bool triggerJump;
    private float velX;

    void Start()
    {
        animationController = gameObject.GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        wallCheck = gameObject.transform.Find("WallCheck");
    }

    // Update is called once per frame
    void Update()
    {
        checkJump();
        checkMovement();
        

        // Landing on the ground
        if (IsGrounded() && animationController.GetBool("isJumping") && forceJumpTimer > forceJumpTime){
            animationController.SetBool("isJumping", false);
            usedDoubleJump = false;
        }

        forceJumpTimer += Time.deltaTime;
    }

    bool IsGrounded() {
        // bitshift the index of the layer (8) to get the layer mask
        return Physics2D.Raycast(transform.position, -Vector2.up, 1.5f, 1 << 8); 
    }

    bool isWallHitting(Vector2 direction) {
        Debug.Log( Physics2D.Raycast(wallCheck.position, direction, .7f, 1 << 8));
        return Physics2D.Raycast(wallCheck.position, direction, .7f, 1 << 8);
    }

    void checkJump()
    {
        if (Input.GetButtonDown("Jump"))
        {   

            if (isWallHitting(Vector2.right)) {
                triggerJump = true;
                usedDoubleJump = false;
                velX = -speed;
                if (transform.localScale.x > 0)
                    transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
                return;

            } 
            if (isWallHitting(Vector2.left)){
                triggerJump = true;
                usedDoubleJump = false;
                velX = speed;
                if (transform.localScale.x < 0)
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
                return;
            }

            // In the air, haven't double jumped yet.. Use double jump
            if(!IsGrounded() && usedDoubleJump == false){
                usedDoubleJump = true;
            // In the air, double jumped used... Exit without jump
            }else if(!IsGrounded() && usedDoubleJump == true) {
                return;
            }

            triggerJump = true;
        }
    }

    void checkMovement() {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            velX = -speed;
            //transform.Translate(Vector3.left * Time.deltaTime * speed);
            animationController.SetBool("isRunning", true);
            if (transform.localScale.x > 0)
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            velX = speed;
            animationController.SetBool("isRunning", true);
            if (transform.localScale.x < 0)
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);

        } else if(IsGrounded()) {
            animationController.SetBool("isRunning", false);
            velX = 0;
        }
    }

    void FixedUpdate()
    {
        if (triggerJump){
            Jump();
        }

        rb.velocity = new Vector2(velX, rb.velocity.y);

    }

    void Jump() {
        // Either player was on ground, or hadn't used doubel jump, so JUMP
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(new Vector2(0f, jump), ForceMode2D.Impulse);
        animationController.SetBool("isJumping", true);
        forceJumpTimer = 0;
        triggerJump = false;
    }
}
