using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class nJoints : MonoBehaviour {

	[SerializeField]
	GameObject armObject;

	[SerializeField]
	List<float> armLengths = new List<float>();
	[SerializeField]
	List<Transform> armTransforms = new List<Transform>();

    [SerializeField]
	Vector3 desiredLoc = new Vector3(3, 1, 0);

	[SerializeField]
	bool useUpdate = false;
	[SerializeField]
	bool useMousePos = false;

	[SerializeField]
	float speed = 5f;

	List<Transform> arms = new List<Transform>();
	List<float> angles = new List<float>();

	InputAction next;

	void Start() {
		if (armTransforms.Count == 0) {
			Transform parent = transform;
			for (int i = 0; i < armLengths.Count; ++i) {
				GameObject armInstance = Instantiate(armObject, parent);

				if (i != 0)
					armInstance.transform.localPosition = new Vector3(0f, armLengths[i - 1], 0f);

				armInstance.GetComponent<ArmSize>().SetSize(armLengths[i]);

				arms.Add(armInstance.transform);
				angles.Add(armInstance.transform.localEulerAngles.z * Mathf.Deg2Rad);

				parent = armInstance.transform;
			}
		} else {
			foreach (Transform child in armTransforms) {
				arms.Add(child);
				angles.Add(child.localEulerAngles.z * Mathf.Deg2Rad);
			}
		}

		next = InputSystem.actions.FindAction("Next");
		next.started += Next;
	}

	void Update() {
		if (useMousePos)
			desiredLoc = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - transform.position;

		if (useUpdate)
			Correct();
	}

	void Next(InputAction.CallbackContext context) {
		Correct();
	}

	void Correct() {
		List<List<float>> dq = SolveIKStep(new float2(desiredLoc.x, desiredLoc.y), angles, armLengths);

		for (int i = 0; i < dq.Count; ++i) {
			angles[i] += dq[i][0] * Time.deltaTime * speed;
			arms[i].localRotation = Quaternion.Euler(0f, 0f, angles[i] * Mathf.Rad2Deg);
        }
	}

	List<List<float>> SolveIKStep(float2 desiredLoc, List<float> angles, List<float> armLengths, float alpha = 0.5f,float lambda = 0.05f) {
		desiredLoc = new float2(desiredLoc.y, -desiredLoc.x);

		List<float> c = new List<float>(angles.Count);
		List<float> s = new List<float>(angles.Count);

		float angleSum = 0f;
		for (int i = 0; i < angles.Count; ++i) {
			angleSum += angles[i];
            c.Add(Mathf.Cos(angleSum));
			s.Add(Mathf.Sin(angleSum));
		}

		// Forward kinematics
		float2 current = new float2(0, 0);
		for (int i = 0; i < angles.Count; ++i) {
			current.x += armLengths[i] * c[i];
			current.y += armLengths[i] * s[i];
        }

        // Error
        List<List<float>> error = new List<List<float>>(2);
		error.Add(new List<float>(new float[1] { desiredLoc.x - current.x }));
		error.Add(new List<float>(new float[1] { desiredLoc.y - current.y }));

		// Jacobian
		List<List<float>> J = new List<List<float>>(2);
		J.Add(new List<float>(new float[armLengths.Count]));
		J.Add(new List<float>(new float[armLengths.Count]));

        for (int i = armLengths.Count - 1; i >= 0; --i) {
			float2 add = 0f;
			if (i < armLengths.Count - 1)
				add = new float2(J[0][i + 1], J[1][i + 1]);

			J[0][i] = -armLengths[i] * s[i] + add.x;
			J[1][i] = armLengths[i] * c[i] + add.y;
        }

        List<List<float>> JT = Matrix.Transpose(J);
        List<List<float>> JJt = Matrix.Mul(J, JT);

		List<List<float>> inv = Matrix.Inverse(JJt);

		List<List<float>> J_pinv = Matrix.Mul(JT, inv);

		List<List<float>> dTheta = Matrix.Mul(J_pinv, error);

		return Matrix.Mul(dTheta, alpha);
	}
}