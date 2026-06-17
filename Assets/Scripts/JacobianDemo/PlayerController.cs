using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour {
    [Header("References")]
    public Transform cameraHolder;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float sprintSpeed = 10f;
    public float jumpForce = 5f;
    public float airMultiplier = 0.4f;

    [Header("Look")]
    public float sensitivity = 0.1f;
    public float maxLookAngle = 90f;

    private Rigidbody rb;

    InputAction moveAction;
    InputAction lookAction;
    InputAction jumpAction;
    InputAction sprintAction;
    
    private Vector2 moveInput;
    private Vector2 lookInput;

    private bool sprintHeld;

    private float xRotation;

    [Header("Compass")]
    public Transform compass;
    public TMP_Text distText;
    public Transform dirObj;
    public float lookDir = 0;
    public float compassLength = 1f;
    public float dirObjClamp;

    [Header("Desired Loc")]
    public Vector3 desiredLoc;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

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

        jumpAction.started += GenerateRandomLoc;
    }

    private void OnDisable() {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();

        jumpAction.started -= GenerateRandomLoc;
    }

    void GenerateRandomLoc(InputAction.CallbackContext context) {
        float desiredLocRange = 100f;
        //desiredLoc = new Vector3(Random.Range(-desiredLocRange, desiredLocRange), 0f, Random.Range(-desiredLocRange, desiredLocRange));
        desiredLoc = new Vector3(5136, 1f, 612);
    }

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update() {
        moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();
        sprintHeld = sprintAction.ReadValue<float>() != 0f;

        HandleMouseLook();

        // compass
        lookDir = cameraHolder.eulerAngles.y;// (cameraHolder.eulerAngles.y + 180f) % 360f;
        compass.localPosition = new Vector3(Mathf.Lerp(compassLength, -compassLength, lookDir / 360f), 0f, 0f);

        distText.SetText(Vector3.Distance(transform.position, desiredLoc).ToString("F0"));

        //desiredLoc = transform.forward + transform.position;

        float angle = Mathf.Atan2(
            desiredLoc.x - transform.position.x,
            desiredLoc.z - transform.position.z
        ) * Mathf.Rad2Deg;

        // player's current yaw
        //float lookDir = transform.eulerAngles.y;

        // shortest angle difference (-180 to 180)
        float delta = Mathf.DeltaAngle(lookDir, angle);

        // normalize to -1 to 1
        float normalized = delta / 180f;

        // move compass object
        float clamped = Mathf.Clamp(normalized * compassLength, -dirObjClamp, dirObjClamp);

        dirObj.localPosition = new Vector3(clamped, dirObj.localPosition.y, dirObj.localPosition.z);

        print($"LookDir: {lookDir}, Target: {angle}, Delta: {delta}");
        Debug.DrawRay(transform.position + new Vector3(0, -0.5f, 0), desiredLoc - transform.position, Color.red);
    }

    private void FixedUpdate() {
        HandleMovement();
    }

    void HandleMovement() {
        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;

        float currentSpeed = sprintHeld ? sprintSpeed : moveSpeed;

        Vector3 targetVelocity = moveDirection * currentSpeed;

        Vector3 velocity = rb.linearVelocity;
        Vector3 velocityChange = targetVelocity - new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);

        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }

    void HandleMouseLook() {
        float mouseX = lookInput.x * sensitivity;
        float mouseY = lookInput.y * sensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }
}