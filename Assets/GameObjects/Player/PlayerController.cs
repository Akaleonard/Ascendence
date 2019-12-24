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
    private Transform topRight;
    private Transform bottomRight;
    private Transform topLeft;
    private Transform bottomLeft;



    // Jump Constant ensuring jump doesn't get grounded by the raycast when jump starts
    private const float FORCED_JUMP_TIME = .11f;

    // Physics Variables
    private float forcedJumpTimer;
    private bool usedDoubleJump = false;
    private bool triggerJump;
    private float velX;

    private float deltaTime;
    void Start()
    {
        animationController = gameObject.GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        topRight = gameObject.transform.Find("TopRight");
        topLeft = gameObject.transform.Find("TopLeft");
        bottomRight = gameObject.transform.Find("BottomRight");
        bottomLeft = gameObject.transform.Find("BottomLeft");
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
        RaycastHit2D hit1 = Physics2D.Raycast(bottomRight.position, Vector2.down, .1f, 1 << 8);
        if (hit1)
            return hit1;
        RaycastHit2D hit2 = Physics2D.Raycast(bottomLeft.position, Vector2.down, .1f, 1 << 8);
        if (hit2)
            return hit2;
        return false;
    }

    bool IsWallHitting(Vector2 direction) {
        Transform child1, child2;
        if (direction == Vector2.right) {
            child1 = transform.localScale.x > 0 ? topRight : topLeft;
            child2 = transform.localScale.x > 0 ? bottomRight : bottomLeft;

            RaycastHit2D hit1 = Physics2D.Raycast(child1.position, direction, .1f, 1 << 8);
            if (hit1)
                return hit1;
            RaycastHit2D hit2 = Physics2D.Raycast(child2.position, direction, .1f, 1 << 8);
            if (hit2)
                return hit2;
        } else {
            child1 = transform.localScale.x > 0 ? topLeft : topRight;
            child2 = transform.localScale.x > 0 ? bottomLeft : bottomRight;

            RaycastHit2D hit3 = Physics2D.Raycast(child1.position, direction, .1f, 1 << 8);
            //Color color = hit3 ? Color.green : Color.red;
            //Debug.DrawRay(topLeft.position, direction * .1f, color);

            if (hit3)
                return hit3;
            RaycastHit2D hit4 = Physics2D.Raycast(child2.position, direction, .1f, 1 << 8);
            if (hit4)
                return hit4;
        }

        return false;
    }

    void checkJump()
    {
        if (Input.GetButtonDown("Jump"))
        {   
            //Vector2 right = new Vector2(Vector2.right.x * transform.localScale.x, Vector2.right.y);
            //Vector2 left = new Vector2(right.x * -1, Vector2.left.y);
            // Check if there's a wall to our right
            if (IsWallHitting(Vector2.left)){
                triggerJump = true;
                usedDoubleJump = false;
                velX = speed;
                if (transform.localScale.x < 0)
                    transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
                return;
            }
            if (IsWallHitting(Vector2.right)) {
                triggerJump = true;
                usedDoubleJump = false;
                velX = -speed;
                if (transform.localScale.x > 0)
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
