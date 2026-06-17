using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.Windows;

public class threeJointConstraint : MonoBehaviour {

	[SerializeField]
	float arm1Length = 3f;
	[SerializeField]
	float arm2Length = 2f;
	[SerializeField]
	float arm3Length = 2f;

	[SerializeField]
	Vector2 arm1RotLimit = new Vector2(0f, 0f);
	[SerializeField]
	Vector2 arm2RotLimit = new Vector2(0f, 0f);
	[SerializeField]
	Vector2 arm3RotLimit = new Vector2(0f, 0f);

    [SerializeField]
	Vector3 desiredLoc = new Vector3(3, 1, 0);

	[SerializeField]
	bool useUpdate = false;
	[SerializeField]
	bool useMousePos = false;

	[SerializeField]
	float speed = 5f;

	[SerializeField]
	float jointPenaltyWeight = 1f;

	[SerializeField]
	LiveGraph graph;

	Transform arm1;
	Transform arm2;
	Transform arm3;

	float t1 = 0f;
	float t2 = 0f;
	float t3 = 0f;

	InputAction next;

	void Start() {
		arm1 = transform;
		arm2 = transform.GetChild(0);
		arm3 = arm2.GetChild(0);

		next = InputSystem.actions.FindAction("Next");
		next.started += Next;

		t1 = arm1.localEulerAngles.z * Mathf.Deg2Rad;
		t2 = arm2.localEulerAngles.z * Mathf.Deg2Rad;
		t3 = arm3.localEulerAngles.z * Mathf.Deg2Rad;
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
		List<List<float>> dq = SolveIKStep(new float2(desiredLoc.x, desiredLoc.y), t1, t2, t3, arm1Length, arm2Length, arm3Length);

		t1 += dq[0][0] * Time.deltaTime * speed;
		t2 += dq[1][0] * Time.deltaTime * speed;
		t3 += dq[2][0] * Time.deltaTime * speed;

        arm1.localRotation = Quaternion.Euler(0f, 0f, t1 * Mathf.Rad2Deg);
		arm2.localRotation = Quaternion.Euler(0f, 0f, t2 * Mathf.Rad2Deg);
		arm3.localRotation = Quaternion.Euler(0f, 0f, t3 * Mathf.Rad2Deg);
	}

	public List<List<float>> SolveIKStep(
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

        float3 jointPenalty = GetJointPenalty();
		float3 derivativeJointPenalty = GetJointPenaltyDerivative();

		// --- Error ---
		List<List<float>> residuals = new List<List<float>> {
            new List<float>{ (current - desiredLoc).x },
			new List<float>{ (current - desiredLoc).y },
			new List<float>{ jointPenalty.x },
			new List<float>{ jointPenalty.y },
			new List<float>{ jointPenalty.z },
        };

		float cost = Matrix.Length(residuals);
        graph.Add(cost);

        // --- Jacobian (2x3) ---
        float xt3 = -l3 * s123;
		float xt2 = -l2 * s12 + xt3;
		float xt1 = -l1 * s1 + xt2;

		float yt3 = l3 * c123;
		float yt2 = l2 * c12 + yt3;
		float yt1 = l1 * c1 + yt2;

        List<List<float>> J = new List<List<float>>{ // constraints X joints
			new List<float>{ xt1, xt2, xt3 },
			new List<float>{ yt1, yt2, yt3 },
			new List<float>{ derivativeJointPenalty.x, 0f, 0f },
			new List<float>{ 0f, derivativeJointPenalty.y, 0f },
			new List<float>{ 0f, 0f, derivativeJointPenalty.z },
        };

		// --- Damped pseudoinverse ---
		List<List<float>> JT = Matrix.Transpose(J);
		List<List<float>> JtJ = Matrix.Mul(JT, J);

		List<List<float>> damping = new List<List<float>>{
			new List<float>{ lambda * lambda, 0, 0 },
            new List<float>{ 0, lambda * lambda, 0 },
            new List<float>{ 0, 0, lambda * lambda }
        };

		List<List<float>> inv = Matrix.Inverse(Matrix.Add(JtJ, damping));

		List<List<float>> J_pinv = Matrix.Mul(inv, JT);

		// --- Delta theta ---
		List<List<float>> dTheta = Matrix.Mul(Matrix.Mul(J_pinv, residuals), -1f);

		// --- Step size ---
		return Matrix.Mul(dTheta, alpha);

		// JT * (J * JT)^-1 * error

		// optimization-base IK
		// build residual matrix:
			// [ (desiredLoc - current).x,
			//	 (desiredLoc - current).y,
			//	 orientation error
			//   joint limit violation
			//   collision penetration]
		// from the residual matrix we can calculate the Cost by ||r||^2
			// I think the cost is more of a "look at me to check how wrong you still are" but not something that actually goes in the LM equation
			// cost = wp * ||desired - current||^2 + wr * orientation error + wl * joint limit penalty
				// wp (weighted position) = single scalar (how much i want position to be correct over other constraints)
				// wr (weighted rotation = single scalar (how much i want rotation to be correct over other constraints)
				// wl (weighted limits) = ^
				// ||desired - current||^2 = the error from jacobian just in a single scalar value
		// then use Lavenberg-Marquardt algorithm to reduce cost (almost jacobian inverse but different)
			// change in theta = -(J^T * J + damping)^-1 * J^T * r

		// constraints / residuals are directly built into the jacobian matrix
			// rows = constraints / residuals
			// columns = joints
	}

    // t1 = min1 + (max1 - min1) * 0.5f * (1 + Mathf.Tanh(t1_raw));


    float3 GetJointPenalty() {
		// error = min - current	when	current < min
		// error = 0				when	mint <= current <= max
		// error = current - max	when	current > max

		// Deg 2 Rad
		Vector2 arm1Rot = arm1RotLimit * Mathf.Deg2Rad;
		Vector2 arm2Rot = arm1RotLimit * Mathf.Deg2Rad;
        Vector2 arm3Rot = arm1RotLimit * Mathf.Deg2Rad;

		float x = 0f;
		if (t1 < arm1Rot.x)
			x = arm1Rot.x - t1;
		else if (t1 > arm1Rot.y)
			x = t1 - arm1Rot.y;

		float y = 0f;
        if (t2 < arm2Rot.x)
            y = arm2Rot.x - t2;
        else if (t2 > arm2Rot.y)
            y = t2 - arm2Rot.y;

        float z = 0f;
        if (t3 < arm3Rot.x)
            z = arm3Rot.x - t3;
        else if (t3 > arm3Rot.y)
            z = t3 - arm3Rot.y;

        return new float3(x, y, z) * jointPenaltyWeight;
	}

	float3 GetJointPenaltyDerivative() {
        Vector2 arm1Rot = arm1RotLimit * Mathf.Deg2Rad;
        Vector2 arm2Rot = arm1RotLimit * Mathf.Deg2Rad;
        Vector2 arm3Rot = arm1RotLimit * Mathf.Deg2Rad;

        float x = 0f;
        if (t1 < arm1Rot.x)
            x = -1;
        else if (t1 > arm1Rot.y)
            x = 1;

        float y = 0f;
        if (t2 < arm2Rot.x)
            y = -1;
        else if (t2 > arm2Rot.y)
            y = 1;

        float z = 0f;
        if (t3 < arm3Rot.x)
            z = -1;
        else if (t3 > arm3Rot.y)
            z = 1;


        return new float3(x, y, z) * jointPenaltyWeight;
	}
}