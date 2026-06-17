using System.IO;
using System.Net.Sockets;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class HandTracking : MonoBehaviour {

	[SerializeField]
	float arm1Length = 3f;
	[SerializeField]
	float arm2Length = 2f;
	[SerializeField]
	float arm3Length = 2f;

	[SerializeField]
	Vector3 desiredLoc = new Vector3(3, 1, 0);

	[SerializeField]
	bool useUpdate = false;
	[SerializeField]
	bool useMousePos = false;

	[SerializeField]
	float speed = 5f;

	Transform arm1;
	Transform arm2;
	Transform arm3;
	public Transform leftClaw;
	public Transform rightClaw;

    float t1 = 0f;
	float t2 = 0f;
	float t3 = 0f;

	InputAction next;

	TcpClient client;
	StreamReader reader;

	public float x, y, dist, w, h;

	[SerializeField]
	Transform debugPoint;

	[SerializeField]
	Vector3 offset;
    [SerializeField]
	bool verticalFlip = false;
    [SerializeField]
    bool horizontalFlip = false;

    void Start() {
		arm1 = transform;
		arm2 = transform.GetChild(0);
		arm3 = arm2.GetChild(0);
		rightClaw = arm3.GetChild(0);
		leftClaw = arm3.GetChild(1);

        next = InputSystem.actions.FindAction("Next");
		next.started += Next;

		t1 = arm1.localEulerAngles.z * Mathf.Deg2Rad;
		t2 = arm2.localEulerAngles.z * Mathf.Deg2Rad;
		t3 = arm3.localEulerAngles.z * Mathf.Deg2Rad;

		try {
			client = new TcpClient("127.0.0.1", 5052);
			reader = new StreamReader(client.GetStream());
		} catch {
			print("Could Not Connect to Server");
		}
	}

	void Update() {
		if (client != null && client.Available > 0 && reader != null) {
			string line = reader.ReadLine();
			if (line == null) return;

			string[] p = line.Split(',');

			x = float.Parse(p[0]);
			y = float.Parse(p[1]);
			// depth = p[2]
			dist = float.Parse(p[3]);
			w = float.Parse(p[4]);
			h = float.Parse(p[5]);

			Vector2 percent = new Vector2(x / w, y / h); // * 1.5f so i can move hand less
			if (verticalFlip)
				percent.y = 1f - percent.y;
            if (horizontalFlip)
                percent.x = 1f - percent.x;

            // Camera Size in world units
            float height = 2f * Camera.main.orthographicSize;
			float width = height * Camera.main.aspect;

			desiredLoc = new Vector3(percent.x * width - width / 2f, percent.y * height - height / 2f, 0f) + offset;

			debugPoint.position = desiredLoc + transform.position;

			// set claw rotation based on distance
			// 0.03 -> 0.3
			float min = 0.03f;
			float max = 0.12f;
			float maxOpened = 60f;
			float maxCloseed = 25f;
			float t = Mathf.InverseLerp(min, max, dist);
			float rightAngle = Mathf.Lerp(maxCloseed, maxOpened, t);
			float leftAngle = -rightAngle;

			rightClaw.localEulerAngles = new Vector3(0f, 0f, rightAngle);
			leftClaw.localEulerAngles = new Vector3(0f, 0f, leftAngle);

        } else if (useMousePos)
			desiredLoc = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - transform.position;
		/*
		desiredLoc = debugPoint.position - transform.position;
		*/
		if (useUpdate)
			Correct();
	}

	void Next(InputAction.CallbackContext context) {
		Correct();
	}

	void Correct() {
		float3 dq = SolveIKStep(new float2(desiredLoc.x, desiredLoc.y), t1, t2, t3, arm1Length, arm2Length, arm3Length);

		t1 += dq.x * Time.deltaTime * speed;
		t2 += dq.y * Time.deltaTime * speed;
		t3 += dq.z * Time.deltaTime * speed;

		arm1.localRotation = Quaternion.Euler(0f, 0f, t1 * Mathf.Rad2Deg);
		arm2.localRotation = Quaternion.Euler(0f, 0f, t2 * Mathf.Rad2Deg);
		arm3.localRotation = Quaternion.Euler(0f, 0f, t3 * Mathf.Rad2Deg);
	}

	public float3 SolveIKStep(
		float2 desiredLoc,
		float t1, float t2, float t3,
		float l1, float l2, float l3,
		float alpha = 0.5f,
		float lambda = 0.05f) {

		desiredLoc = new float2(desiredLoc.y, -desiredLoc.x);

		// --- Forward kinematics ---
		float c1 = Mathf.Cos(t1);
		float s1 = Mathf.Sin(t1);
		float c12 = Mathf.Cos(t1 + t2);
		float s12 = Mathf.Sin(t1 + t2);
		float c123 = Mathf.Cos(t1 + t2 + t3);
		float s123 = Mathf.Sin(t1 + t2 + t3);

		float2 current = new float2(
			l1 * c1 + l2 * c12 + l3 * c123,
			l1 * s1 + l2 * s12 + l3 * s123
		);

		// --- Error ---
		float2 error = desiredLoc - current;

		// --- Jacobian (2x3) ---
		// Row 0: dX/dtheta
		// Row 1: dY/dtheta
		//float xt1 = -l1 * s1 - l2 * s12 - l3 * s123;
		//float xt2 = -l2 * s12 - l3 * s123;
		//float xt3 = -l3 * s123;
		float xt3 = -l3 * s123;
		float xt2 = -l2 * s12 + xt3;
		float xt1 = -l1 * s1 + xt2;

		//float yt1 = l1 * c1 + l2 * c12 + l3 * c123;
		//float yt2 = l2 * c12 + l3 * c123;
		//float yt3 = l3 * c123;
		float yt3 = l3 * c123;
		float yt2 = l2 * c12 + yt3;
		float yt1 = l1 * c1 + yt2;

		float2x3 J = new float2x3(
			xt1, xt2, xt3,
			yt1, yt2, yt3
		);

		// --- Damped pseudoinverse ---
		float3x2 JT = math.transpose(J);                  // 3x2
		float2x2 JJt = math.mul(J, JT);                   // 2x2

		float2x2 damping = new float2x2(
			lambda * lambda, 0,
			0, lambda * lambda
		);

		float2x2 inv = math.inverse(JJt/* + damping*/);     // 2x2

		float3x2 J_pinv = math.mul(JT, inv);                // 3x2

		// --- Delta theta ---
		float3 dTheta = math.mul(J_pinv, error);            // 3x1

		// --- Step size ---
		return alpha * dTheta;
	}
}