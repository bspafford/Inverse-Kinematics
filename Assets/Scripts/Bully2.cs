using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

// n segments
// 0 - mouseJoint = calculate that joint as end effector.
// then make that bthe base for all proceeding joints, and calculate their angles like normal, froom mouseJoint - n
// make sure if something can't reach, then clamp the distance from base to joint to the sum of the all the segments

[Serializable]
struct SegmentData {
	public float length;
	public Transform segment;
	public Transform tip;
}

public class Bully2 : MonoBehaviour {

	[SerializeField]
	List<SegmentData> armData = new List<SegmentData>();

	[SerializeField]
	Vector3 desiredLoc = new Vector3(3, 1, 0);

	[SerializeField]
	Vector3 finalDesired = new Vector3(3, -1, 0);

	[SerializeField]
	bool useUpdate = false;
	[SerializeField]
	bool useMousePos = false;

	[SerializeField]
	float speed = 5f;

	List<float> angles = new List<float>();

	public int jointNum = -1;

	public float tempRotSpeed1 = 10f;

	InputAction next;

	public Transform debugPoint;

	void Start() {
		next = InputSystem.actions.FindAction("Next");
		next.started += Next;

		for (int i = 0; i < armData.Count; ++i) {
			angles.Add(armData[i].segment.localEulerAngles.z * Mathf.Deg2Rad);
		}
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
		if (jointNum != -1)
			desiredLoc = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - transform.position;
		else
			desiredLoc = finalDesired - transform.position;

		List<List<float>> dq = SolveIKStep(new float2(desiredLoc.x, desiredLoc.y), armData);
		List<List<float>> secondDq = SecondSolver(new float2(finalDesired.x, finalDesired.y), armData);

		if (dq != null) {
			for (int i = 0; i < dq.Count; ++i) {
				angles[i] += dq[i][0] * Time.deltaTime * speed;
			}
		}

		if (secondDq != null) {
			for (int i = 0; i < secondDq.Count; ++i) {
				angles[jointNum + i] += secondDq[i][0] * Time.deltaTime * speed;
			}
		}

		for (int i = 0; i < angles.Count; ++i) {
			armData[i].segment.localRotation = Quaternion.Euler(0f, 0f, angles[i] * Mathf.Rad2Deg);
		}
    }

	List<List<float>> SolveIKStep(float2 desiredLoc, List<SegmentData> armData, float alpha = 0.5f, float lambda = 0.05f) {

		// clamp length if too far for arm to reach
		float length = CalcLength(true);
		float dist = Vector2.Distance(this.desiredLoc, Vector2.zero);
		if (dist > length) {
			desiredLoc = math.normalize(desiredLoc) * length;
			this.desiredLoc = new Vector3(desiredLoc.x, desiredLoc.y, 0);
		}

		if (jointNum == 1) {
			Quaternion rot = Quaternion.Euler(0f, 0f, Mathf.Atan2(-desiredLoc.x, desiredLoc.y) * Mathf.Rad2Deg);
			armData[0].segment.localRotation = Quaternion.Slerp(armData[0].segment.localRotation, rot, speed * Time.deltaTime * tempRotSpeed1);

			angles[0] = armData[0].segment.localEulerAngles.z * Mathf.Deg2Rad;

			return null;
		}

		desiredLoc = new float2(desiredLoc.y, -desiredLoc.x);

		List<float> c = new List<float>(angles.Count);
		List<float> s = new List<float>(angles.Count);

		int jointCount = angles.Count;
		if (jointNum != -1)
			jointCount = Mathf.Min(jointNum, jointCount);

		float angleSum = 0f;
		for (int i = 0; i < jointCount; ++i) {
			angleSum += angles[i];
			c.Add(Mathf.Cos(angleSum));
			s.Add(Mathf.Sin(angleSum));
		}

		// Forward kinematics
		float2 current = new float2(0, 0);
		for (int i = 0; i < jointCount; ++i) {
			current.x += armData[i].length * c[i];
			current.y += armData[i].length * s[i];
		}

		// Error
		List<List<float>> error = new List<List<float>>(2);
		error.Add(new List<float>(new float[1] { desiredLoc.x - current.x }));
		error.Add(new List<float>(new float[1] { desiredLoc.y - current.y }));

		// Jacobian
		List<List<float>> J = new List<List<float>>(2);
		J.Add(new List<float>(new float[jointCount]));
		J.Add(new List<float>(new float[jointCount]));

		for (int i = jointCount - 1; i >= 0; --i) {
			float2 add = 0f;
			if (i < jointCount - 1)
				add = new float2(J[0][i + 1], J[1][i + 1]);

			J[0][i] = -armData[i].length * s[i] + add.x;
			J[1][i] = armData[i].length * c[i] + add.y;
		}

		List<List<float>> JT = Matrix.Transpose(J);
		List<List<float>> JJt = Matrix.Mul(J, JT);

		List<List<float>> inv = Matrix.Inverse(JJt);

		List<List<float>> J_pinv = Matrix.Mul(JT, inv);

		List<List<float>> dTheta = Matrix.Mul(J_pinv, error);

		return Matrix.Mul(dTheta, alpha);
	}



	List<List<float>> SecondSolver(float2 desiredLoc, List<SegmentData> armData, float alpha = 0.5f, float lambda = 0.05f) {
		if (jointNum == -1 || jointNum == armData.Count)
			return null;

        float2 currJointPos = new float2(armData[jointNum].segment.position.x, armData[jointNum].segment.position.y);
        desiredLoc -= currJointPos;// Forward(jointNum); // position of current joint



        // clamp length if too far for arm to reach
        float length = CalcLength(false);
        float dist = Vector2.Distance(this.desiredLoc, currJointPos);
        if (dist > length) {
            desiredLoc = math.normalize(desiredLoc) * length;
            this.desiredLoc = new Vector3(desiredLoc.x, desiredLoc.y, 0);
        }

		// make joint look at target
        if (armData.Count - jointNum == 1) {
            Quaternion rot = Quaternion.Euler(0f, 0f, Mathf.Atan2(-desiredLoc.x, desiredLoc.y) * Mathf.Rad2Deg);
			armData[armData.Count - 1].segment.rotation = Quaternion.Slerp(armData[armData.Count - 1].segment.rotation, rot, speed * Time.deltaTime * tempRotSpeed1);

            angles[armData.Count - 1] = armData[armData.Count - 1].segment.localEulerAngles.z * Mathf.Deg2Rad;

            return null;
        }



        desiredLoc = new float2(desiredLoc.y, -desiredLoc.x);
		if (debugPoint)
			debugPoint.position = new Vector3(desiredLoc.x + currJointPos.x, desiredLoc.y + currJointPos.y, -0.4f);

		List<float> c = new List<float>();
		List<float> s = new List<float>();

		float angleSum = angles[jointNum - 1];
		for (int i = jointNum; i < armData.Count; ++i) {
			print(i + ": angles: " + angles[i] * Mathf.Rad2Deg);
			angleSum += angles[i];
			c.Add(Mathf.Cos(angleSum));
			s.Add(Mathf.Sin(angleSum));
		}

		// Forward kinematics
		float2 current = new float2(0, 0);
		for (int i = jointNum; i < armData.Count; ++i) {
			current.x += armData[i].length * c[i - jointNum];
			current.y += armData[i].length * s[i - jointNum];
		}

		// Error
		List<List<float>> error = new List<List<float>>(2);
		error.Add(new List<float>(new float[1] { desiredLoc.x - current.x }));
		error.Add(new List<float>(new float[1] { desiredLoc.y - current.y }));

		// Jacobian
		List<List<float>> J = new List<List<float>>(2);
		J.Add(new List<float>(new float[armData.Count - jointNum]));
		J.Add(new List<float>(new float[armData.Count - jointNum]));

		for (int i = armData.Count - 1; i >= jointNum; --i) {
			float2 add = 0f;
			if (i < armData.Count - 1)
				add = new float2(J[0][i - jointNum + 1], J[1][i - jointNum + 1]);

			J[0][i - jointNum] = -armData[i].length * s[i - jointNum] + add.x;
			J[1][i - jointNum] = armData[i].length * c[i - jointNum] + add.y;
		}

		List<List<float>> JT = Matrix.Transpose(J);
		List<List<float>> JJt = Matrix.Mul(J, JT);

		List<List<float>> inv = Matrix.Inverse(JJt);

		List<List<float>> J_pinv = Matrix.Mul(JT, inv);

		List<List<float>> dTheta = Matrix.Mul(J_pinv, error);

		return Matrix.Mul(dTheta, alpha);
	}

    float2 Forward(int index = -1) {
		float3 pos = armData[index].segment.position - transform.position;
		print(pos);
		if (debugPoint)
			debugPoint.position = pos;
		return new float2(pos.x, pos.y);
    }

    float CalcLength(bool start) {
		float length = 0f;

		if (jointNum == -1)
			for (int i = 0; i < armData.Count; ++i) {
				length += armData[i].length;
			}
		else if (start) {
			for (int i = 0; i < jointNum; ++i) {
				length += armData[i].length;
			}
		} else {
			for (int i = jointNum; i < armData.Count; ++i) {
				length += armData[i].length;
			}
		}

		return length;
	}
}