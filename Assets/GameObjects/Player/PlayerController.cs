using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Public Variables
    public int speed;
    public float jump;

    //Component References
    private Animator animationController;
    private Rigidbody2D rb;
    private Transform wallCheck;

    // Jump Constant ensuring jump doesn't get grounded by the raycast when jump starts
    private const float FORCED_JUMP_TIME = .11f;

    // Physics Variables
    private float forcedJumpTimer;
    private bool usedDoubleJump = false;
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
        // Check for Physics things to happen, Trigger them in FixedUpdate()
        checkJump();
        checkMovement();

        // Landing on the ground
        if (IsGrounded() && animationController.GetBool("isJumping") && forcedJumpTimer > FORCED_JUMP_TIME){
            animationController.SetBool("isJumping", false);
            usedDoubleJump = false;
        }

        forcedJumpTimer += Time.deltaTime;
    }

    bool IsGrounded() {
        // bitshift the index of the layer (8) to get the layer mask
        return Physics2D.Raycast(transform.position, -Vector2.up, 1.5f, 1 << 8); 
    }

    bool IsWallHitting(Vector2 direction) {
        Debug.Log( Physics2D.Raycast(wallCheck.position, direction, .7f, 1 << 8));
        return Physics2D.Raycast(wallCheck.position, direction, .7f, 1 << 8);
    }

    void checkJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (IsWallHitting(Vector2.right)) {
                triggerJump = true;
                usedDoubleJump = false;
                velX = -speed;
                if (transform.localScale.x > 0)
                    transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
                return;

            } 
            if (IsWallHitting(Vector2.left)){
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

        // If left or right inputted, this applies that to the character
        rb.velocity = new Vector2(velX, rb.velocity.y);
    }

    void Jump() {
        // Either player was on ground, or hadn't used doubel jump, so JUMP
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(new Vector2(0f, jump), ForceMode2D.Impulse);
        animationController.SetBool("isJumping", true);
        forcedJumpTimer = 0;
        triggerJump = false;
    }
}
