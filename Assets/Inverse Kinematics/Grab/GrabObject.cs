using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class GrabObject : MonoBehaviour {

    [Header("Debug")]
	public Transform leftClaw = null;
    public Transform rightClaw = null;

    public float clawRot = 0;

	bool pickedUp = false;

	Rigidbody2D rb2D;
	Rigidbody rb;

    [SerializeField]
	float dropRange = 5f;

	float beginRot = 0f;

	int velocityListSize = 5;
    List<Vector3> prevVelocities;
	Vector3 prevPos;

	void Start() {
        rb2D = GetComponent<Rigidbody2D>();
        rb = GetComponent<Rigidbody>();
        prevVelocities = new List<Vector3>();
    }

	void Update() {
		if (rightClaw) {
            clawRot = rightClaw.parent.localEulerAngles.z; // rightClaw.parent.parent.localEulerAngles.z;
            print(clawRot);
			// if all the way open or dropRange more than starting range
			if ((clawRot <= 340 && clawRot >= 300) || clawRot >= beginRot + dropRange)
				Drop();
		}

		if (pickedUp) {
            Vector3 velocity = (transform.position - prevPos) / Time.deltaTime;
            prevVelocities.Add(velocity);
            if (prevVelocities.Count > velocityListSize)
                prevVelocities.RemoveAt(0);
        }

        prevPos = transform.position;
    }

	void BeginPickup() {
        print("picked up!: " + gameObject.name);
		beginRot = rightClaw.parent.localEulerAngles.z;

		transform.SetParent(rightClaw.parent);
        if (GetComponent<MeshCollider>())
            GetComponent<MeshCollider>().enabled = false;

        if (rb2D)
            rb2D.gravityScale = 0f;
        else {
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        pickedUp = true;
    }

    void Drop() {
        print("dropped!");
        if (GetComponent<MeshCollider>())
            GetComponent<MeshCollider>().enabled = true;

        if (rb2D)
            rb2D.gravityScale = 1f;
        else
            rb.useGravity = true;
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
                rb2D.linearVelocity = avg;
            else
                rb.linearVelocity = avg;
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

    public void OnCollisionEnter(Collision collision) {
        if (rightClaw && leftClaw)
            return;

        if (collision.transform.tag == "clawLeft")
            leftClaw = collision.transform;
        if (collision.transform.tag == "clawRight")
            rightClaw = collision.transform;

        if (rightClaw && leftClaw)
            BeginPickup();
    }

    public void OnCollisionExit(Collision collision) {
        if (pickedUp)
            return;

        if (collision.transform.tag == "clawLeft")
            leftClaw = null;
        if (collision.transform.tag == "clawRight")
            rightClaw = null;
    }
}