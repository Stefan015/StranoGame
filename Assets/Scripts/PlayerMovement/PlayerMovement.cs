using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("Movement")]
    private float movementSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundedDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYscale;
    private float startYscale;

    [Header("KeyBinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;


    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

   

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;

    public enum MovementState {
        walking,
        sprinting,
        air,
        crouching
    }

    private void FixedUpdate() {
        move();
    }
    private void Start() {
        ResetJump();

        rb = GetComponent<Rigidbody>(); 
        rb.freezeRotation = true;

        startYscale = transform.localScale.y;
    }
    private void Update() {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        input();

        speedControl();

        stateHandler();

        if (grounded)
        {
            rb.drag = groundedDrag;

        }else rb.drag = 0;
    }

    private void input() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKey(jumpKey) && readyToJump && grounded) {
            readyToJump = false;

            jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey)) {
            transform.localScale = new Vector3(transform.localScale.x, crouchYscale, transform.localScale.z);

            playerHeight *= 0.5f;

            rb.AddForce(Vector3.down* 5f, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(crouchKey)) {
            transform.localScale = new Vector3(transform.localScale.x, startYscale, transform.localScale.z);

            playerHeight *= 2f;
        }
    }
    public void stateHandler() {
     

        if (grounded && Input.GetKey(sprintKey)) {
            state = MovementState.sprinting;
            movementSpeed = sprintSpeed;
        }
        else if (Input.GetKey(crouchKey)) {
            state = MovementState.crouching;
            movementSpeed = crouchSpeed;
        }
        else if (grounded) {
            state = MovementState.walking;
            movementSpeed = walkSpeed;
        }
        else {
             state = MovementState.air; 
        }
    }
    private void move() {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(grounded)
            rb.AddForce(moveDirection.normalized * movementSpeed * 10f,ForceMode.Force);
        
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * movementSpeed * airMultiplier * 10f, ForceMode.Force);

    }
    private void speedControl() {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if(flatVel.magnitude > movementSpeed) {
            Vector3 limitedVel = flatVel.normalized * movementSpeed;

            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);

        }
    }

    private void jump() {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

    }
    private void ResetJump() {
        readyToJump = true;
    }
}
