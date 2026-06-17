using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Burst.Intrinsics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class ArmData {
	public float length;
	public Vector3 axis;
	public Vector2 angleLimits;
	public Vector2 hardLimits;
    public Transform arm;
	[HideInInspector]
	public float angle;
}

public class IK3DnDOF : MonoBehaviour {

	[SerializeField]
	float speed = 10f;

	[SerializeField]
	GameObject armObject;

	[SerializeField]
	public Transform target;

	[SerializeField]
	List<ArmData> arms;

	List<Transform> debugTemp = new List<Transform>();

	public float rotPenaltyWeight = 1f;

	public Transform rightClaw;

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
		} else {
			foreach(ArmData arm in arms) {
				if (arm.axis.x == 1)
					arm.angle = arm.arm.localEulerAngles.x * Mathf.Deg2Rad;
                else if (arm.axis.y == 1)
                    arm.angle = arm.arm.localEulerAngles.y * Mathf.Deg2Rad;
                else if (arm.axis.z == 1)
                    arm.angle = arm.arm.localEulerAngles.z * Mathf.Deg2Rad;
            }
		}
	}

	void Update() {
        float fps = 1.0f / Time.deltaTime;
        //Debug.Log("FPS: " + Mathf.RoundToInt(fps));

        List<List<float>> delta = SolveIKStep3D(target.position);

		float alpha = Time.deltaTime * speed;
		for (int i = 0; i < arms.Count; ++i) {
			arms[i].angle += delta[i][0] * alpha;

			
			if (arms[i].hardLimits.x != 0 || arms[i].hardLimits.y != 0) {
				arms[i].angle = Mathf.Clamp(arms[i].angle, arms[i].hardLimits.x * Mathf.Deg2Rad, arms[i].hardLimits.y * Mathf.Deg2Rad);
			}

			arms[i].arm.localRotation = Quaternion.AngleAxis(arms[i].angle * Mathf.Rad2Deg, arms[i].axis);
		}
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

	List<List<float>> SolveIKStep3D(float3 target, float lambda = 0.1f, float alpha = 0.5f) {

		// Forward Kinematics
		float3 current = Forward();
		List<List<float>> error = new List<List<float>> {
			new List<float>{ (target - current).x },
			new List<float>{ (target - current).y },
			new List<float>{ (target - current).z }
		};

		// Build Jacobian (3x3)
		for (int i = 0; i < debugTemp.Count; ++i) {
			if (debugTemp[i] == null)
                continue;
            debugTemp[i].position = Forward(i);
		}


		List<float3> c = new List<float3>(arms.Count);
		for (int i = 0; i < arms.Count; ++i) {
			float3 axis = arms[i].arm.TransformDirection(arms[i].axis);
			c.Add(math.cross(axis, (current - Forward(i))));
		}

		List<List<float>> J = new List<List<float>>(3);
		J.Add(new List<float>(c.Count));
		J.Add(new List<float>(c.Count));
		J.Add(new List<float>(c.Count));
		for (int i = 0; i < J.Count; ++i) {
			for (int j = 0; j < c.Count; ++j) {
				J[i].Add(c[j][i]);
			}
		}


        List<List<float>> JT = Matrix.Transpose(J);
		List<List<float>> JTJ = Matrix.Mul(JT, J);

		// Damping matrix
		List<List<float>> damping = new List<List<float>>(arms.Count);
		for (int i = 0; i < arms.Count; ++i) {
			damping.Add(new List<float>(arms.Count));
			for (int j = 0; j < arms.Count; ++j) {
				damping[i].Add(i == j ? lambda * lambda : 0f);
			}
		}

		List<List<float>> A = Matrix.Add(JTJ, damping);
		List<List<float>> rhs = Matrix.Mul(JT, error);
		List<List<float>> dTheta = Matrix.Mul(Matrix.Inverse(A), rhs);

		return Matrix.Mul(dTheta, alpha);
	}

	List<float> GetJointPenalty() {
		List<float> penaltyList = new List<float>(arms.Count);
		foreach (ArmData arm in arms) {
			float penalty = 0f;

			Vector2 angleLimitsRad = arm.angleLimits * Mathf.Deg2Rad;
			float wrappedAngle = Mathf.Atan2(Mathf.Sin(arm.angle), Mathf.Cos(arm.angle)); // makes sure angle is (-180, 180] but in radians
            
			if (wrappedAngle < angleLimitsRad.x)
				penalty = angleLimitsRad.x - wrappedAngle;
			else if (wrappedAngle > angleLimitsRad.y)
				penalty = wrappedAngle - angleLimitsRad.y;
            penaltyList.Add(penalty * rotPenaltyWeight);
        }
		return penaltyList;
	}

    List<float> GetJointPenaltyDerivative() {
        List<float> penaltyList = new List<float>(arms.Count);
        foreach (ArmData arm in arms) {
            float penalty = 0f;

			float wrappedAngle = Mathf.Atan2(Mathf.Sin(arm.angle), Mathf.Cos(arm.angle));
            Vector2 limits = arm.angleLimits * Mathf.Deg2Rad;
            
            if (wrappedAngle < limits.x)
                penalty = -1;
            else if (wrappedAngle > limits.y)
                penalty = 1;
            penaltyList.Add(penalty * rotPenaltyWeight);
        }
        return penaltyList;
    }
}