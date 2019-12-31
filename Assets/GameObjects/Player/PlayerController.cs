using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Public Variables
    public int speed;
    public float jump;
    public float dashSpeed;

    //Component References
    private Animator animationController;
    private Rigidbody2D rb;
    private Transform topRight;
    private Transform bottomRight;
    private Transform middleLeft;
    private Transform middleRight;
    private Transform topLeft;
    private Transform bottomLeft;



    // Jump Constant ensuring jump doesn't get grounded by the raycast when jump starts
    private const float FORCED_JUMP_TIME = .11f;
    private const float WALL_RAYCAST_DISTANCE = .2f;
    private const float GROUND_RAYCAST_DISTANCE = .1f;
    private const int PLATFORM_LAYER_MASK = 8;
    private const float WALL_JUMP_LOCKED_INPUT_TIME = .3f;
    private const float Dash_Locked_Input_Time = .3f;
    // Physics Variables
    private float lockedInputTimer;
    private bool lockedInputJumpEscapable;
    private bool isGroundedThisFrame;
    private float forcedJumpTimer;
    private bool usedDoubleJump = false;
    private bool triggerJump;
    private bool triggerDash;
    private float velX;
    private bool overRideVelX;

    private float deltaTime;
    void Start()
    {
        animationController = gameObject.GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        topRight = gameObject.transform.Find("TopRight");
        topLeft = gameObject.transform.Find("TopLeft");
        middleLeft = gameObject.transform.Find("MiddleLeft");
        middleRight = gameObject.transform.Find("MiddleRight");
        bottomRight = gameObject.transform.Find("BottomRight");
        bottomLeft = gameObject.transform.Find("BottomLeft");
        lockedInputTimer = -1;
    }

    // Update is called once per frame
    void Update()
    {
        // Stores results into private var isGroundedThisFrame for optimization
        checkGrounded(); 

        // Check for Physics things to happen, Trigger them in FixedUpdate()
        if (lockedInputTimer < 0) {
            overRideVelX = false;
            checkMovement();
            checkJump();
            checkDash();
        // If movement locked out but can escape it with jump
        } else if (lockedInputJumpEscapable) {
            checkJump();
        }

        // Handle Landing on the ground
        checkLanding();

        // Update Timers
        forcedJumpTimer += Time.deltaTime;
        lockedInputTimer -= Time.deltaTime;
    }

    // Needed for correctly interacting with physics engine
    void FixedUpdate()
    {
        if (triggerJump) {
            Jump();
        }
        if (triggerDash)
        {
            Dash();
        }
        // Apply velocity from input or environmental factors
        if(!overRideVelX) {
            rb.velocity = new Vector2(velX, rb.velocity.y);
        }
    }

    void checkGrounded() {
        // bitshift the index of the layer (8) to get the layer mask
        RaycastHit2D hit1 = Physics2D.Raycast(bottomRight.position, Vector2.down, GROUND_RAYCAST_DISTANCE, 1 << 8);
        if (hit1) {
            isGroundedThisFrame = hit1;
            return;
        }
        RaycastHit2D hit2 = Physics2D.Raycast(bottomLeft.position, Vector2.down, GROUND_RAYCAST_DISTANCE, 1 << 8);
        if (hit2){
            isGroundedThisFrame = hit2;
            return;
        }
        if (animationController.GetBool("isRunning")){
            animationController.SetBool("isJumping", true);
            animationController.SetBool("isRunning", false);
        }

        isGroundedThisFrame = false;
    }

    void checkJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            // Check and exit if this is a wall jump
            if (checkWallJump() == true)
                return;

            // In the air, haven't double jumped yet.. Use double jump
            if(!isGroundedThisFrame && usedDoubleJump == false){
                usedDoubleJump = true;
            // In the air, double jumped used... Exit without jump
            }else if(!isGroundedThisFrame && usedDoubleJump == true) {
                return;
            }

            // Jump in FixedUpdate()
            triggerJump = true;
            // Escape jumpEscapable input lockout e.g. wall jump
            lockedInputTimer = -1;
        }
    }

    bool checkWallJump() {
        bool wallHit = false;

        // Left Wall Specific
        if (checkWallHitting(Vector2.left)){
            wallHit = true;
            velX = speed;
            if (transform.localScale.x < 0)
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        // Right Wall Specific
        }else if (checkWallHitting(Vector2.right)) {
            wallHit = true;
            velX = -speed;
            if (transform.localScale.x > 0)
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }

        // Wall Jump General
        if (wallHit) {
            triggerJump = true;
            usedDoubleJump = false;
            lockoutInput(WALL_JUMP_LOCKED_INPUT_TIME, true, false);
        }

        return wallHit;
    }

    bool checkWallHitting(Vector2 direction) {
        if (isGroundedThisFrame)
            return false;

        Transform child1, child2, child3;
        if (direction == Vector2.right) {
            child1 = transform.localScale.x > 0 ? topRight : topLeft;
            child2 = transform.localScale.x > 0 ? bottomRight : bottomLeft;
            child3 = transform.localScale.x > 0 ? middleRight: middleLeft;

            RaycastHit2D hit1 = Physics2D.Raycast(child1.position, direction, WALL_RAYCAST_DISTANCE, 1 << PLATFORM_LAYER_MASK);
            // Uncomment below for debugging the raycasts
            /*Color color = hit1 ? Color.green : Color.red;
            Debug.DrawRay(child1.position, direction * WALL_RAYCAST_DISTANCE, color, 3);
            Debug.DrawRay(child2.position, direction * WALL_RAYCAST_DISTANCE, color, 3);
            Debug.DrawRay(child3.position, direction * WALL_RAYCAST_DISTANCE, color, 3);*/
            if (hit1)
                return hit1;
            RaycastHit2D hit2 = Physics2D.Raycast(child2.position, direction, WALL_RAYCAST_DISTANCE, 1 << PLATFORM_LAYER_MASK);
            if (hit2)
                return hit2;

            RaycastHit2D hit5 = Physics2D.Raycast(child3.position, direction, WALL_RAYCAST_DISTANCE, 1 << PLATFORM_LAYER_MASK);
            if (hit5)
                return hit5;
        } else {
            child1 = transform.localScale.x > 0 ? topLeft : topRight;
            child2 = transform.localScale.x > 0 ? bottomLeft : bottomRight;
            child3 = transform.localScale.x > 0 ? middleLeft: middleRight;

            RaycastHit2D hit3 = Physics2D.Raycast(child1.position, direction, WALL_RAYCAST_DISTANCE, 1 << PLATFORM_LAYER_MASK);
            if (hit3)
                return hit3;

            RaycastHit2D hit4 = Physics2D.Raycast(child2.position, direction, WALL_RAYCAST_DISTANCE, 1 << PLATFORM_LAYER_MASK);

            // Uncomment below for debugging the raycasts
            /*Color color = hit4 ? Color.green : Color.red;
            Debug.DrawRay(child1.position, direction * WALL_RAYCAST_DISTANCE, color, 3);
            Debug.DrawRay(child2.position, direction * WALL_RAYCAST_DISTANCE, color, 3);
            Debug.DrawRay(child3.position, direction * WALL_RAYCAST_DISTANCE, color, 3);*/
            if (hit4)
                return hit4;

            RaycastHit2D hit6 = Physics2D.Raycast(child3.position, direction, WALL_RAYCAST_DISTANCE, 1 << PLATFORM_LAYER_MASK);
            if (hit6)
                return hit6;
        }

        // No Wall Collision
        return false;
    }

    void checkMovement() {
        // Left Input
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            velX = -speed;
            animationController.SetBool("isRunning", true);
            if (transform.localScale.x > 0)
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        // Right Input
        } else if (Input.GetKey(KeyCode.RightArrow))
        {
            velX = speed;
            animationController.SetBool("isRunning", true);
            if (transform.localScale.x < 0)
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        // No Left or Right input
        } else {
            // Walked off a platform, in midair now
            if (isGroundedThisFrame) 
                animationController.SetBool("isRunning", false);
            velX = 0;
        }
    }

    void checkLanding() {
        if (isGroundedThisFrame && animationController.GetBool("isJumping") && forcedJumpTimer > FORCED_JUMP_TIME){
            animationController.SetBool("isJumping", false);
            usedDoubleJump = false;
        }
    }

    // Actions
    void Jump() {
        // Either player was on ground, or hadn't used double jump, so JUMP
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(new Vector2(0f, jump), ForceMode2D.Impulse);
        animationController.SetBool("isJumping", true);
        forcedJumpTimer = 0;
        triggerJump = false;
    }

    public void lockoutInput(float lockoutTime, bool jumpEscapable, bool overRideVelX) {
        this.lockedInputTimer = lockoutTime;
        this.lockedInputJumpEscapable = jumpEscapable;
        this.overRideVelX = overRideVelX; 
    }

    void checkDash()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            triggerDash = true;
            lockoutInput(Dash_Locked_Input_Time, true, true);
        }
    }

    void Dash()
    {
        rb.AddForce(new Vector2(transform.localScale.x, 0) * dashSpeed, ForceMode2D.Impulse);
        triggerDash = false;
    }
}
