using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using Unity.Burst.Intrinsics;
using Unity.Mathematics;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class threeDnDOFHandTracking : MonoBehaviour {

	[SerializeField]
	float speed = 10f;

	[SerializeField]
	GameObject armObject;
	[SerializeField]
	GameObject claw;

	[SerializeField]
	Transform target;

	[SerializeField]
	List<ArmData> arms;

	[SerializeField]
	List<Transform> debugTemp;

    TcpClient client;
    StreamReader reader;

	[SerializeField]
	List<float> values = new List<float>();

    void Start() {
		if (arms[0].arm == null) {
			Transform parent = transform;
			for (int i = 0; i < arms.Count; i++) {
				GameObject armInstance = Instantiate(armObject, parent);
				if (i != 0)
					armInstance.transform.localPosition = new Vector3(0f, arms[i - 1].length, 0f);

				armInstance.GetComponent<ArmSize3D>().SetSize(arms[i].length / 2f);

				arms[i].arm = armInstance.transform;

				parent = armInstance.transform;
			}

			GameObject clawInstance = Instantiate(claw, parent);
			clawInstance.transform.localPosition = new Vector3(0f, arms[arms.Count - 1].length, 0f);

			arms[arms.Count - 1].length += 1f; // make longer for claw
		}

        try {
            client = new TcpClient("127.0.0.1", 5052);
            reader = new StreamReader(client.GetStream());
        } catch {
            print("Could Not Connect to Server");
        }
    }

	void Update() {
		Vector3 handNormal = Vector3.zero;
        if (reader != null) {
            string line = reader.ReadLine();
            if (line == null) return;

            string[] p = line.Split(',');

            float x = float.Parse(p[0]);
            float y = float.Parse(p[1]);
            float depth = float.Parse(p[2]);
            float dist = float.Parse(p[3]);
            float w = float.Parse(p[4]);
            float h = float.Parse(p[5]);

            values = new List<float> { x, y, depth, dist, w, h };

            debugTemp[0].eulerAngles = new Vector3(0f, 0f, handNormal.x * 10f);

            Vector3 percent = new Vector3(x / w, depth / h, 1f - y / h) * 1.5f; // * 1.5f so i can move hand less
			if (math.isnan(percent.x) || math.isnan(percent.y) || math.isnan(percent.z))
				percent = new Vector3(0f, 0f, 0f);

            // Camera Size in world units
            float height = 2f * Camera.main.orthographicSize;
            float width = height * Camera.main.aspect;


            float desiredHeight = Mouse.current.position.ReadValue().y / Screen.height * 10f;

            target.position = new Vector3(percent.x * width - width / 2f, percent.y * height - height / 2f, percent.z * height - height / 2f);
        }

        List<List<float>> delta = SolveIKStep3D(target.position);

		float alpha = Time.deltaTime * speed;
		for (int i = 0; i < arms.Count; ++i) {
			arms[i].angle += delta[i][0] * alpha;
			arms[i].arm.localRotation = Quaternion.AngleAxis(arms[i].angle * Mathf.Rad2Deg, arms[i].axis);
		}

		//arms[arms.Count - 1].arm.localEulerAngles = new Vector3(0f, Quaternion.LookRotation(handNormal, Vector3.up).eulerAngles.x * 5f, 0f);
    }

	float3 Forward(int index = -1) {
		Quaternion rot = Quaternion.identity;
		Vector3 pos = transform.position;

		for (int i = 0; i < arms.Count; ++i) {
			if (index != -1 && i >= index) // early return to get position at specific joint
				return pos;

			rot *= Quaternion.AngleAxis(arms[i].angle * Mathf.Rad2Deg, arms[i].axis);
			pos += rot * Vector3.up * arms[i].length;
		}

		return pos;
	}

	List<List<float>> SolveIKStep3D(
	float3 target,
	float lambda = 0.1f,
	float alpha = 0.5f) {

		List<float> jointPenalty = GetJointPenalty();
		List<float> derivativeJointPenalty = GetJointPenaltyDerivative();

		float3 current = Forward();
		List<List<float>> error = new List<List<float>> {
			new List<float>{ (target - current).x },
			new List<float>{ (target - current).y },
			new List<float>{ (target - current).z }
		};

		foreach (float penalty in jointPenalty) {
			error.Add(new List<float> { penalty });
		}

		// -------------------------
		// 2. build Jacobian (3x3)
		// J columns = axis x (end - joint)
		// -------------------------

		List<float3> c = new List<float3>(arms.Count);
		for (int i = 0; i < arms.Count; ++i) {
			float3 axis = arms[i].arm.TransformDirection(arms[i].axis);
			c.Add(math.cross(axis, (current - Forward(i))));
		}

		List<List<float>> J = new List<List<float>>(3); // dimensions x
		J.Add(new List<float>(c.Count));
		J.Add(new List<float>(c.Count));
		J.Add(new List<float>(c.Count));
		for (int i = 0; i < J.Count; ++i) {
			for (int j = 0; j < c.Count; ++j) {
				J[i].Add(c[j][i]);
			}
		}

		for (int i = 0; i < derivativeJointPenalty.Count; ++i) {
			List<float> list = new List<float>(c.Count);
			for (int j = 0; j < c.Count; ++j) {
				list.Add(i == j ? derivativeJointPenalty[j] : 0f);
			}
			J.Add(list);
		}

		// -------------------------
		// 3. compute J^T
		// -------------------------
		List<List<float>> JT = Matrix.Transpose(J);

		// -------------------------
		// 4. compute J^T J
		// -------------------------
		List<List<float>> JTJ = Matrix.Mul(JT, J);

		// -------------------------
		// 5. damping matrix
		// -------------------------
		List<List<float>> damping = new List<List<float>>(arms.Count);
		for (int i = 0; i < arms.Count; ++i) {
			damping.Add(new List<float>(arms.Count));
			for (int j = 0; j < arms.Count; ++j) {
				damping[i].Add(i == j ? lambda * lambda : 0f);
			}
		}

		List<List<float>> A = Matrix.Add(JTJ, damping);

		// -------------------------
		// 6. solve linear system:
		// (J^T J + λ²I) Δθ = J^T e
		// -------------------------
		List<List<float>> rhs = Matrix.Mul(JT, error);

		List<List<float>> dTheta = Matrix.Mul(Matrix.Inverse(A), rhs);

		// -------------------------
		// 7. step scale
		// -------------------------
		return Matrix.Mul(dTheta, alpha);
	}

	List<float> GetJointPenalty() {
		List<float> penaltyList = new List<float>(arms.Count);
		foreach (ArmData arm in arms) {
			float penalty = 0f;
			if (arm.angle < arm.angleLimits.x)
				penalty = arm.angleLimits.x - arm.angle;
			else if (arm.angle > arm.angleLimits.y)
				penalty = arm.angle - arm.angleLimits.y;
			penaltyList.Add(penalty);
		}

		return penaltyList;
	}

	List<float> GetJointPenaltyDerivative() {
		List<float> penaltyList = new List<float>(arms.Count);
		foreach (ArmData arm in arms) {
			float penalty = 0f;
			if (arm.angle < arm.angleLimits.x)
				penalty = -1;
			else if (arm.angle > arm.angleLimits.y)
				penalty = 1;
			penaltyList.Add(penalty);
		}

		return penaltyList;
	}
}