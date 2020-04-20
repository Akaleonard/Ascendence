using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Public Variables
    public float acceleration;
    public float maxSpeed;
    public int jumpSquatFrames;
    public int freeFallStartupFrames;
    public float jump;
    public float hopModifier;
    public float dashSpeed;
    public float runStartSpeed;
    public float gravity;


    //Component References
    private Animator animationController;
    private InputHandler inputHandler;
    private Rigidbody2D rb;
    private Transform topRight;
    private Transform bottomRight;
    private Transform middleLeft;
    private Transform middleRight;
    private Transform topLeft;
    private Transform bottomLeft;

    private string state;
    private int jumpFrameCounter;
    private float hop;

    // Jump Constant ensuring jump doesn't get grounded by the raycast when jump starts
    private const float WALL_RAYCAST_DISTANCE = .2f;
    private const float GROUND_RAYCAST_DISTANCE = .1f;
    private const int PLATFORM_LAYER_MASK = 8;

    public int WALL_JUMP_LOCKED_INPUT_TIME = 10;
    public int Dash_Locked_Input_Time = 10;
    // Physics Variables
    private int lockedInputTimer;
    private bool lockedInputJumpEscapable;
    private bool isGroundedThisFrame;
    private bool usedDoubleJump = false;
    private bool triggerJump;
    private bool triggerDash;
    private float velX;
    private bool overRideVelX;
    private Vector3 newVelocity;
    private float inputX;
    private float inputY;
    private Vector2 dashVelocity;
    private float totalTime;
    private int totalFrameCount;
    void Start()
    {
        animationController = gameObject.GetComponent<Animator>();
        inputHandler = gameObject.GetComponent<InputHandler>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        topRight = gameObject.transform.Find("TopRight");
        topLeft = gameObject.transform.Find("TopLeft");
        middleLeft = gameObject.transform.Find("MiddleLeft");
        middleRight = gameObject.transform.Find("MiddleRight");
        bottomRight = gameObject.transform.Find("BottomRight");
        bottomLeft = gameObject.transform.Find("BottomLeft");
        lockedInputTimer = -1;
        newVelocity = new Vector3(0, 0, -1);

        totalTime = 0;
        totalFrameCount = 0;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (state == "freeFallStartup" && lockedInputTimer == 0){
            state = "isFreeFalling";
        } else if (state == "isDashing" && lockedInputTimer == 0) {
            state = "freeFallStartup";
            lockoutInput(freeFallStartupFrames, false, false);
        }

        gameObject.layer = rb.velocity.y > 0 ? 10 : 0;

        // Stores results into private var isGroundedThisFrame for optimization
        checkGrounded(); 

        // Check for Physics things to happen, Trigger them in FixedUpdate()
        if (lockedInputTimer <= 0) {
            overRideVelX = false;
            checkMovement();
            checkJump();
            checkDash();

            if (lockedInputTimer == 0 && state == "isJumpSquatting") {
                triggerJump = true;
                Debug.Log("triggered Jump");
            }
        // If movement locked out but can escape it with jump
        } else if (lockedInputJumpEscapable) {
            checkJump();
        }

        //Debug.Log(state);
        // Handle Landing on the ground
        checkLanding();

        // Update Timers
        jumpFrameCounter++;
        lockedInputTimer--;
        totalFrameCount++;
        totalTime += Time.deltaTime;
        //Debug.Log("Frame: " + totalFrameCount + " Total Time: " + totalTime);
    }

    // Needed for correctly interacting with physics engine
    void FixedUpdate()
    {
        if (newVelocity.z > 0) {
            rb.velocity = new Vector2(newVelocity.x, newVelocity.y);
            newVelocity = new Vector3(0, 0, -1);
        }
        //Debug.Log(inputHandler.GetInputObject().jumpPressed);
        if (triggerJump) {
            
            Jump();

        }
        if (triggerDash)
        {
            Dash();
        }

        if (state == "freeFallStartup"){
            rb.gravityScale = 0;
            rb.velocity = new Vector2(0, 0);
        }else if(state == "isFreeFalling" && rb.gravityScale != gravity) {
            rb.gravityScale = gravity;
        } if (state == "isDashing") {
            float t = (float) lockedInputTimer / Dash_Locked_Input_Time;
            rb.velocity = new Vector2(dashVelocity.x * t, dashVelocity.y * t);
            
        }else if(!overRideVelX && velX != 0) {
            rb.velocity = new Vector2(rb.velocity.x + (velX), rb.velocity.y);
            if(state != "isDashing" && rb.velocity.x > maxSpeed) {
                rb.velocity = new Vector2(maxSpeed, rb.velocity.y);
            } else if (state != "isDashing" && rb.velocity.x < -maxSpeed) {
                rb.velocity = new Vector2(-maxSpeed, rb.velocity.y);
            }
        }
    }

    void checkWaveland(RaycastHit2D hit) {

        if (hit) {
            if (!isGroundedThisFrame) {
                state = "standing";
            }
            isGroundedThisFrame = hit;
        }
    }

    void checkGrounded() {
        // bitshift the index of the layer (8) to get the layer mask
        RaycastHit2D hit1 = Physics2D.Raycast(bottomRight.position, Vector2.down, GROUND_RAYCAST_DISTANCE, (1 << 8 | 1 << 9));
        if (hit1){
            checkWaveland(hit1);
            return;
        } 

        RaycastHit2D hit2 = Physics2D.Raycast(bottomLeft.position, Vector2.down, GROUND_RAYCAST_DISTANCE, (1 << 8 | 1 << 9));
        if (hit2){
            checkWaveland(hit2);
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
        var inputObj = inputHandler.GetInputObject();
        if (inputObj.jumpPressed)
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
            startJumpSquat();
        } else if(state == "isJumpSquatting" && !inputObj.jumpHeld) {
            hop = hopModifier;
        }
    }

    bool checkWallJump() {
        bool wallHit = false;

        // Left Wall Specific
        if (checkWallHitting(Vector2.left)){
            wallHit = true;
            velX = acceleration;
            if (transform.localScale.x < 0)
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
            
            state = "WallSliding";
        // Right Wall Specific
        }else if (checkWallHitting(Vector2.right)) {
            wallHit = true;
            velX = -acceleration;
            if (transform.localScale.x > 0)
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);

            state = "wallSliding";
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

            RaycastHit2D hit1 = Physics2D.Raycast(child1.position, direction, WALL_RAYCAST_DISTANCE, 1 << PLATFORM_LAYER_MASK | 1);
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
        if (state == "isJumpSquatting")
            return;
        // Left Input
        inputX = inputHandler.GetInputObject().inputX;
        if (inputX < 0)
        {
            if (isGroundedThisFrame) {
                animationController.SetBool("isRunning", true);
                state = "running";
            }

            velX = -acceleration;
            if (transform.localScale.x > 0){
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);

                if (state == "running") {
                    newVelocity = new Vector3(-runStartSpeed, rb.velocity.y, 1);
                }
            }

        // Right Input
        } else if (inputX > 0)
        {
            if (isGroundedThisFrame) {
                animationController.SetBool("isRunning", true);
                state = "running";
            }
            velX = acceleration;
            if (transform.localScale.x < 0) {
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
                if (state == "running") {
                    newVelocity = new Vector3(runStartSpeed, rb.velocity.y, 1);
                }
            }
        // No Left or Right input
        } else {
            // Walked off a platform, in midair now
            if (isGroundedThisFrame) {
                animationController.SetBool("isRunning", false);
                state = "standing";
            }
            velX = 0;
        }
    }

    void checkLanding() {
        if (isGroundedThisFrame && animationController.GetBool("isJumping") ){
            animationController.SetBool("isJumping", false);
            usedDoubleJump = false;
        }
    }

    void startJumpSquat() {
        lockoutInput(jumpSquatFrames, false, true);
        state = "isJumpSquatting";
        hop = 1;
        //Debug.Log("Start Jump Squat");
    }

    // Actions
    void Jump() {
        // Either player was on ground, or hadn't used double jump, so JUMP
        var input = inputHandler.GetInputObject();
        float angle = input.inputX != 0 ? Mathf.Atan2(input.inputY, input.inputX) : Mathf.PI / 2;
        rb.velocity = new Vector2(Mathf.Cos(angle) * maxSpeed, jump * hop);
        //rb.AddForce(new Vector2(0f, jump), ForceMode2D.Impulse);
        if (!animationController.GetBool("isJumping"))
            animationController.SetBool("isJumping", true);
        triggerJump = false;
        state = "isJumping";
    }

    public void lockoutInput(int lockoutTime, bool jumpEscapable, bool overRideVelX) {
        this.lockedInputTimer = lockoutTime;
        this.lockedInputJumpEscapable = jumpEscapable;
        this.overRideVelX = overRideVelX; 
    }

    void checkDash()
    {
        if (state == "isJumping" && inputHandler.GetInputObject().dashPressed)
        {
            triggerDash = true;
            newVelocity = new Vector3(0, 0, 1);
            state = "isDashing";
            dashVelocity = new Vector2(inputHandler.GetInputObject().inputX, inputHandler.GetInputObject().inputY) * dashSpeed;
            lockoutInput(Dash_Locked_Input_Time, true, true);
        }
    }

    void Dash()
    {
        //rb.AddForce(new Vector2(inputHandler.GetInputObject().inputX, inputHandler.GetInputObject().inputY) * dashSpeed, ForceMode2D.Impulse);
        triggerDash = false;

    }
}
