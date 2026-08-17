using UnityEngine;

public class PlayerMovement : MonoBehaviour{

    [Header("Movement")]
    private float _movementSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundedDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    private bool _readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYscale;
    private float _startYscale;

    [Header("KeyBinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;


    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    private bool _grounded;



    public Transform orientation;

    private float _horizontalInput;
    private float _verticalInput;

    private Vector3 _moveDirection;

    private Rigidbody _rb;

    public MovementState state;

    public enum MovementState{
        Walking,
        Sprinting,
        Air,
        Crouching
    }

    private void FixedUpdate(){
        Move();
    }
    private void Start(){
        ResetJump();

        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;

        _startYscale = transform.localScale.y;
    }
    private void Update(){
        _grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        HandleInput();

        SpeedControl();

        StateHandler();

        if (_grounded){
            _rb.linearDamping = groundedDrag;
        }
        else _rb.linearDamping = 0;
    }

    private void HandleInput(){
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && _readyToJump && _grounded){
            _readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey)){
            transform.localScale = new Vector3(transform.localScale.x, crouchYscale, transform.localScale.z);

            playerHeight *= 0.5f;

            _rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(crouchKey)){
            transform.localScale = new Vector3(transform.localScale.x, _startYscale, transform.localScale.z);

            playerHeight *= 2f;
        }
    }
    public void StateHandler(){

        if (_grounded && Input.GetKey(sprintKey)){
            state = MovementState.Sprinting;
            _movementSpeed = sprintSpeed;
        }
        else if (Input.GetKey(crouchKey)){
            state = MovementState.Crouching;
            _movementSpeed = crouchSpeed;
        }
        else if (_grounded){
            state = MovementState.Walking;
            _movementSpeed = walkSpeed;
        }
        else{
            state = MovementState.Air;
        }
    }
    private void Move(){
        _moveDirection = orientation.forward * _verticalInput + orientation.right * _horizontalInput;

        if (_grounded)
            _rb.AddForce(_moveDirection.normalized * _movementSpeed * 10f, ForceMode.Force);

        else if (!_grounded)
            _rb.AddForce(_moveDirection.normalized * _movementSpeed * airMultiplier * 10f, ForceMode.Force);

    }
    private void SpeedControl(){
        Vector3 flatVel = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);

        if (flatVel.magnitude > _movementSpeed){
            Vector3 limitedVel = flatVel.normalized * _movementSpeed;

            _rb.linearVelocity = new Vector3(limitedVel.x, _rb.linearVelocity.y, limitedVel.z);

        }
    }

    private void Jump(){
        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);

        _rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

    }
    private void ResetJump(){
        _readyToJump = true;
    }
}
