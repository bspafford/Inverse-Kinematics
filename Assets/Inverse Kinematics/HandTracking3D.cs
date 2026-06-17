using System.IO;
using System.Net.Sockets;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class HandTracking3D : MonoBehaviour {

    [SerializeField]
    Transform target;

	[SerializeField]
	Vector3 desiredLoc = new Vector3(3, 1, 0);

	[SerializeField]
	bool useMousePos = false;

	[SerializeField]
	float speed = 5f;

	Transform arm1;
	Transform arm2;
	Transform arm3;

    float t1 = 0f;
	float t2 = 0f;
	float t3 = 0f;

	InputAction next;

	TcpClient client;
	StreamReader reader;

	public float x, y, depth, dist, w, h;

	[SerializeField]
	Transform debugPoint;

	void Start() {
		arm1 = transform;
		arm2 = transform.GetChild(0);
		arm3 = arm2.GetChild(0);

		t1 = 0;
		t2 = 0;
		t3 = 0;

		try {
			client = new TcpClient("127.0.0.1", 5052);
			reader = new StreamReader(client.GetStream());
		} catch {
			print("Could Not Connect to Server");
		}
	}

    void Update() {
        if (reader != null) {
            string line = reader.ReadLine();
            if (line == null) return;

            string[] p = line.Split(',');

            x = float.Parse(p[0]);
            y = float.Parse(p[1]);
            depth = float.Parse(p[2]);
            dist = float.Parse(p[3]);
            w = float.Parse(p[4]);
            h = float.Parse(p[5]);

            Vector2 percent = new Vector2(x / w, 1f - y / h) * 1.5f; // * 1.5f so i can move hand less

            // Camera Size in world units
            float height = 2f * Camera.main.orthographicSize;
            float width = height * Camera.main.aspect;


            float desiredHeight = Mouse.current.position.ReadValue().y / Screen.height * 10f;

            desiredLoc = new Vector3(percent.x * width - width / 2f, desiredHeight, percent.y * height - height / 2f);

            target.position = desiredLoc;

        } else
            desiredLoc = target.position;

            float3 p0 = arm1.position;
            float3 p1 = arm2.position;
            float3 p2 = arm3.position;
            float3 p3 = arm3.GetChild(0).GetChild(0).position;

            float3 axis0 = arm1.TransformDirection(Vector3.up);
            float3 axis1 = arm2.TransformDirection(Vector3.forward);
            float3 axis2 = arm3.TransformDirection(Vector3.forward);

            float3 delta = SolveIKStep3D(desiredLoc, p0, p1, p2, p3, axis0, axis1, axis2);

            print("delta: " + delta);

            float alpha = Time.deltaTime * speed;
            t1 += delta.x * alpha;
            t2 += delta.y * alpha;
            t3 += delta.z * alpha;

            arm1.localRotation = Quaternion.Euler(0f, t1 * Mathf.Rad2Deg, -25f);
            arm2.localRotation = Quaternion.Euler(0f, 0f, t2 * Mathf.Rad2Deg);
            arm3.localRotation = Quaternion.Euler(0f, 0f, t3 * Mathf.Rad2Deg);
    }

    float3 SolveIKStep3D(
    float3 target,
    float3 p0, float3 p1, float3 p2, float3 p3,
    float3 axis0, float3 axis1, float3 axis2,
    float lambda = 0.1f,
    float alpha = 0.5f) {
        float3 error = target - p3;

        float3 c0 = math.cross(axis0, (p3 - p0));
        float3 c1 = math.cross(axis1, (p3 - p1));
        float3 c2 = math.cross(axis2, (p3 - p2));

        float3x3 J = new float3x3(
            c0.x, c1.x, c2.x,
            c0.y, c1.y, c2.y,
            c0.z, c1.z, c2.z
        );

        float3x3 JT = math.transpose(J);
        float3x3 JTJ = math.mul(JT, J);
        float3x3 damping = new float3x3(
            lambda * lambda, 0, 0,
            0, lambda * lambda, 0,
            0, 0, lambda * lambda
        );

        float3x3 A = JTJ + damping;
        float3 rhs = math.mul(JT, error);
        float3 dTheta = math.mul(math.inverse(A), rhs);

        return alpha * dTheta;
    }
}