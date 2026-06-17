using UnityEngine;
using UnityEngine.InputSystem;

public class monsterController : MonoBehaviour {

	InputAction moveAction;
    InputAction lookAction;
    InputAction jumpAction;
    InputAction sprintAction;
    
    private Vector2 moveInput;
    private Vector2 lookInput;

	[Header("Movement")]
    public float moveSpeed = 6f;
    public float sprintSpeed = 10f;
    public float jumpForce = 5f;
    public float airMultiplier = 0.4f;

    [Header("Look")]
    public float sensitivity = 0.1f;
    public float maxLookAngle = 90f;


	private void Awake() {
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        jumpAction = InputSystem.actions.FindAction("Jump");
        sprintAction = InputSystem.actions.FindAction("Sprint");
    }

	private void OnEnable() {
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
    }

    private void OnDisable() {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
    }

	void Update() {
		moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();

        HandleMouseLook();
    }

    private void FixedUpdate() {
        HandleMovement();
    }

    void HandleMovement() {
        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;

        float currentSpeed = moveSpeed;

        Vector3 targetVelocity = moveDirection * currentSpeed;

		transform.Translate(targetVelocity);
    }

    void HandleMouseLook() {
        float mouseX = lookInput.x * sensitivity;
        float mouseY = lookInput.y * sensitivity;

        // xRotation -= mouseY;
        // xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        // cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }
}