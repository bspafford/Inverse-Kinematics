using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class grabObject2D : MonoBehaviour {

    [Header("Debug")]
    public Transform leftClaw = null;
    public Transform rightClaw = null;

    public float clawRot = 0;

    bool pickedUp = false;

    Rigidbody2D rb2D;

    [SerializeField]
    float dropRange = 5f;

    float beginRot = 0f;

    int velocityListSize = 5;
    List<Vector3> prevVelocities = new List<Vector3>();
    Vector3 prevPos;

    public float velocityMultiplier = 1f;

    Vector3 startPos;

    InputAction startAction;

    // have to do this in Awake, onEnable, and OnDisable to make inputs work correctly without having to do scene/domain reload on play
    void Awake() {
        startAction = InputSystem.actions.FindAction("start");
    }

    private void OnEnable() {
        startAction.started += ResetPos;
        startAction.Enable();
    }

    private void OnDisable() {
        startAction.started -= ResetPos;
        startAction.Disable();
    }

    void ResetPos(InputAction.CallbackContext context) {
        print("resetting pos!");
        Drop();

        if (rb2D) {
            rb2D.angularVelocity = 0f;
            rb2D.linearVelocity = Vector3.zero;
        }

        transform.position = startPos;
    }

    void Start() {
        startPos = transform.position;
        rb2D = GetComponent<Rigidbody2D>();
    }

    void Update() {
        if (rightClaw) {
            clawRot = rightClaw.parent.parent.localEulerAngles.z;
            // if all the way open or dropRange more than starting range
            if ((clawRot <= 340 && clawRot >= 300) || clawRot >= beginRot + dropRange)
                Drop();
        }

        if (pickedUp) {
            Vector3 velocity = (transform.position - prevPos) / Time.deltaTime;
            prevVelocities.Add(velocity);
            if (prevVelocities.Count > velocityListSize)
                prevVelocities.RemoveAt(0);

            if (rb2D) {
                rb2D.angularVelocity = 0f;
                rb2D.linearVelocity = Vector3.zero;
            }
        }

        prevPos = transform.position;


        Vector3 avg = Vector3.zero;
        foreach (Vector3 vel in prevVelocities)
            avg += vel;
        if (prevVelocities.Count != 0) {
            avg /= prevVelocities.Count;
        }
    }

    void BeginPickup() {
        print("picked up!: " + gameObject.name);
        beginRot = rightClaw.parent.parent.localEulerAngles.z;

        transform.SetParent(rightClaw.parent.parent);
        if (GetComponent<Collider2D>())
            GetComponent<Collider2D>().enabled = false;

        if (rb2D)
            rb2D.gravityScale = 0f;
        pickedUp = true;
    }

    void Drop() {
        print("dropped!");
        if (GetComponent<Collider2D>())
            GetComponent<Collider2D>().enabled = true;

        if (rb2D)
            rb2D.gravityScale = 1f;
        transform.SetParent(null);
        leftClaw = null;
        rightClaw = null;
        pickedUp = false;

        Vector3 avg = Vector3.zero;
        foreach (Vector3 vel in prevVelocities) {
            avg += vel;
        }

        if (prevVelocities.Count != 0) {
            avg /= prevVelocities.Count;
            if (rb2D)
                rb2D.linearVelocity = avg * velocityMultiplier;
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (rightClaw && leftClaw)
            return;

        if (collision.transform.tag == "clawLeft")
            leftClaw = collision.transform;
        if (collision.transform.tag == "clawRight")
            rightClaw = collision.transform;

        if (rightClaw && leftClaw)
            BeginPickup();
    }

    private void OnCollisionExit2D(Collision2D collision) {
        if (pickedUp)
            return;

        if (collision.transform.tag == "clawLeft")
            leftClaw = null;
        if (collision.transform.tag == "clawRight")
            rightClaw = null;
    }
}