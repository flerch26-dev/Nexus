using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Bools to determine what features the player can use
    [Header("Options")]
    public bool canSprint;
    public bool canCrouch;
    public bool canUseHeadbob;
    public bool canDoubleJump;

    //Keybinds
    [Header("KeyBinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode ladderKey = KeyCode.L;
    public KeyCode doorKey = KeyCode.O;
    public KeyCode aimKey = KeyCode.X;
    public KeyCode dropItemKey = KeyCode.Q;

    //Movement settings
    [Header("Movement")]
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;
    private float moveSpeed;

    //Jump settings
    [Header("Jump")]
    public float jumpForce;
    public float jumpCoolDown;
    public float airMultiplier;
    int jumpCounter;
    bool readyToJump;

    //crouch settings
    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    //Ground Check references and variables
    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask ground;
    [HideInInspector] public bool grounded;

    //Slope settings
    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    //private bool exitingSlope;

    //Headbob settings
    [Header("Headbob")]
    public float walkBobSpeed = 14f;
    public float walkBobAmount = 0.05f;
    public float sprintBobSpeed = 18f;
    public float sprintBobAmount = 0.1f;
    public float crouchBobSpeed = 8f;
    public float crouchBobAmount = 0.025f;
    public float defaultYPos = 0;
    private float timer;

    //References to player object
    [Space]
    public Transform orientation;
    public Transform playerCamera;

    //variables to keep track of keyboard inputs
    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Rigidbody rb;

    [HideInInspector] public int gravityDirection = 1;
    [HideInInspector] public int upDir = 180;

    //Enum and variable to keep track of which state the player is in (walking, sprinting, crouching or in the air)
    private MovementState state;
    private enum MovementState { walking, sprinting, crouching, air };

    private void Awake()
    {
        //stepRayUpper.transform.position = new Vector3(stepRayUpper.transform.position.x, stepHeight, stepRayUpper.transform.position.z);
    }

    private void Start()
    {
        transform.rotation = Quaternion.Euler(upDir, 0, 0);
        //Refernce to the rigidbody
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        //Reset variables
        ResetJump();
        startYScale = transform.localScale.y;

        defaultYPos = 0.676f;
    }

    public void CheckGrounded()
    {
        //Check if the player is on the ground
        if (gravityDirection == 1) grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, ground);
        else grounded = Physics.Raycast(transform.position, Vector3.up, playerHeight * 0.5f + 0.2f, ground);
    }

    //Runs every frame
    private void Update()
    {
        CheckGrounded();

        HandleInput(); //Get keyboard input
        SpeedControl(); //Limit player speed
        StateHandler(); //Change player state
        if (canUseHeadbob) HandleHeadbob(); //Check if headbob is enable and handle it if yes

        //Handle drag
        if (grounded) rb.drag = groundDrag;
        else rb.drag = 0;
    }

    //Since using rigidbody, player movement has to be called in Fixed update
    //This ensures that we will move the same amount each time no matter the frame rate
    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void HandleInput()
    {
        //Get keyboard input (arrow keys or WASD)
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //Check for jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            //Handle jump
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCoolDown);
            jumpCounter = 1;
        }

        //Check for double jump
        if (Input.GetKeyDown(jumpKey) && jumpCounter == 1 && !grounded && readyToJump && canDoubleJump)
        {
            //Handle double jump
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCoolDown);
            jumpCounter = 0;
        }

        //Check for crouch
        if (Input.GetKeyDown(crouchKey) && canCrouch)
        {
            //Shrink the player and move him down so he stays on the floor
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        //Check if we are not crouching anymore
        if (Input.GetKeyUp(crouchKey))
        {
            //Set the player back to his normal height
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }

        /*if (Input.GetKeyDown(KeyCode.P))
        {
            SwitchGravity();
        }*/
    }

    public void SwitchGravity()
    {
        upDir = 180 - upDir;

        transform.rotation = Quaternion.Euler(upDir, 0, 0);
        gravityDirection = (gravityDirection == 1) ? gravityDirection = -1 : gravityDirection = 1;
        Vector3 newGravity = new Vector3(0, -14.0f * gravityDirection, 0); 
        Physics.gravity = newGravity;
    }

    private void HandleHeadbob()
    {
        //Don't use headbob if in the air
        if (!grounded) return;

        //Check if we are going faster than a minimum speed threshold (so as to not headbob if the player is going very very slowly
        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            float bobSpeed = state == MovementState.crouching ? crouchBobSpeed : state == MovementState.sprinting ? sprintBobSpeed : walkBobSpeed; //Determine the bob speed based on the current state of the player
            float bobAmount = state == MovementState.crouching ? crouchBobAmount : state == MovementState.sprinting ? sprintBobAmount : walkBobAmount; //Determine the bob amount based on the current state of the player

            //Increase the timer used for the headbob by the bob speed
            //This determines how fast the camera moves up and down
            timer += Time.deltaTime * bobSpeed;

            //change the position of the camera on the y axis based on a sin wave that takes in the timer variable
            playerCamera.transform.localPosition = new Vector3
                (playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * bobAmount,
                playerCamera.transform.localPosition.z);

        }
    }

    private void Jump()
    {
        //exitingSlope = true;
        //Changes the velocity and adds a force to the player rigidbody
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    //Resets the jump variables
    private void ResetJump()
    {
        readyToJump = true;
        //exitingSlope = false;
    }

    private void StateHandler()
    {
        //Set the state and moveSpeed to the corresponding crouch settings if we are pressing the crouch key
        if (Input.GetKey(crouchKey) && canCrouch)
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }

        //Set the state and moveSpeed to the corresponding sprint settings if we are pressing the sprint key
        else if (grounded && Input.GetKey(sprintKey) && canSprint)
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }

        //Set the state and moveSpeed to the corresponding default settings if we are on the ground and neither sprinting not crouching (walking)
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        //if we are not on the ground, set the state of the player to air
        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        //Determines the direction the player is moving based on the orientation and the keyboard input
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //Make sure the player doesn't slide down a slope
        /*if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }*/
        //Moves the player in the correct direction on the ground and in the air
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        //Diables gravity if we are on a slope so the player doens't slide down
        bool useGravity = true;//!OnSlope();
        if (useGravity)
        {
            rb.AddForce(Physics.gravity * (rb.mass * rb.mass));
        }
    }

    private void SpeedControl()
    {
        //Makes sure the speed on the slope isn't greater than the max movement speed
        /*if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        else
        {
            //Determines the speed and clamps it to the max movement speed
            Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }*/
        //Determines the speed and clamps it to the max movement speed
        Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    //Helper method to determine if the player is standing on a slope
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    //Helper method to determine the direction of the slope
    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}