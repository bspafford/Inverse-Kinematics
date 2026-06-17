using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.Mathematics;

public class nJointsLiveUpdate : MonoBehaviour {

	[SerializeField]
	GameObject armObject1;
	[SerializeField]
	GameObject armObject2;

    [SerializeField]
	List<float> armLengths = new List<float>();

	Vector3 desiredLoc = new Vector3(3, 1, 0);

	[SerializeField]
	Transform target;

    [SerializeField]
	bool useUpdate = false;
	[SerializeField]
	bool useMousePos = false;

	[SerializeField]
	float speed = 5f;

	List<Transform> arms = new List<Transform>();
	List<float> angles = new List<float>();

	InputAction next;

	int prevCount = 0;

	List<bool> randList = new List<bool>();

	List<float> savedRot = new List<float>();

	void Start() {
		for (int i = 0; i < 100; ++i) {
			randList.Add(UnityEngine.Random.Range(0f, 1f) > 0.5f);
		}

		SetupSegments();

        next = InputSystem.actions.FindAction("Next");
        next.started += Next;
    }

	void SetupSegments() {
        Transform parent = transform;
        for (int i = 0; i < armLengths.Count; ++i) {
			GameObject armInstance;

            if (randList[i])
				armInstance = Instantiate(armObject1, transform);
			else
				armInstance = Instantiate(armObject2, transform);

            if (i != 0)
                armInstance.transform.localPosition = new Vector3(0f, armLengths[i - 1], 0f);

            armInstance.GetComponent<ArmSize2>().SetSize(armLengths[i]);

			if (arms.Count == 0) {
				armInstance.GetComponent<trackSegment>().trackingPoint = parent;
			} else
                armInstance.GetComponent<trackSegment>().trackingPoint = parent.GetChild(1);

            arms.Add(armInstance.transform);
            angles.Add(armInstance.transform.localEulerAngles.z * Mathf.Deg2Rad);

			if (i >= savedRot.Count)
				savedRot.Add(armInstance.transform.localEulerAngles.z);
			else
				armInstance.transform.localEulerAngles = new Vector3(0f, 0f, savedRot[i]);

            parent = armInstance.transform;
        }
    }

	void Update() {
		if (useMousePos)
			desiredLoc = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - transform.position;
		else
			desiredLoc = target.position - transform.position;

        //float lengthSum = 0f;
        //foreach (float length in armLengths) {
            //lengthSum += length;
        //}
        //if (desiredLoc.magnitude > lengthSum)
            //desiredLoc = desiredLoc.normalized * lengthSum; // clamp to max reach

        print(armLengths.Count + ", " + prevCount);
		if (prevCount != armLengths.Count) {
			print("updating segments");
            foreach (Transform arm in arms) {
				if (arm) Destroy(arm.gameObject);
			}
			arms.Clear();
			angles.Clear();

			SetupSegments();

            prevCount = armLengths.Count;
		}

		for (int i = 0; i < arms.Count; ++i) {
			arms[i].GetComponent<ArmSize2>().SetSize(armLengths[i]);
			savedRot[i] = arms[i].localEulerAngles.z;
        }

        if (useUpdate)
            Correct();
    }

	void Next(InputAction.CallbackContext context) {
		Correct();
	}

	void Correct() {
        List<List<float>> dq = SolveIKStep(new float2(desiredLoc.x, desiredLoc.y), angles, armLengths);

		float angleSum = 0f;
		for (int i = 0; i < dq.Count; ++i) {
			angles[i] += dq[i][0] * Time.deltaTime * speed;
			angleSum += angles[i];
			arms[i].localRotation = Quaternion.Euler(0f, 0f, angleSum * Mathf.Rad2Deg);
		}
	}

	List<List<float>> SolveIKStep(float2 desiredLoc, List<float> angles, List<float> armLengths, float alpha = 0.5f, float lambda = 0.05f) {
		desiredLoc = new float2(desiredLoc.y, -desiredLoc.x);

		// --- Forward kinematics ---
		List<float> c = new List<float>(angles.Count);
		List<float> s = new List<float>(angles.Count);

		float angleSum = 0f;
		for (int i = 0; i < angles.Count; ++i) {
			angleSum += angles[i];
			c.Add(Mathf.Cos(angleSum));
			s.Add(Mathf.Sin(angleSum));
		}

		float2 current = new float2(0, 0);
		// length1 * c1 + length2 * c2 + length3 * c3 + ... + lengthN * cN,
		// length1 * s1 + length2 * s2 + length3 * s3 + ... + lengthN * sN,
		for (int i = 0; i < angles.Count; ++i) {
			current.x += armLengths[i] * c[i];
			current.y += armLengths[i] * s[i];
		}

		// --- Error ---
		List<List<float>> error = new List<List<float>>(2);
		error.Add(new List<float>(new float[1] { desiredLoc.x - current.x }));
		error.Add(new List<float>(new float[1] { desiredLoc.y - current.y }));

		// --- Jacobian ---
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

		// --- Damped pseudoinverse ---

		List<List<float>> JT = Matrix.Transpose(J);
		List<List<float>> JJt = Matrix.Mul(J, JT);

        List<List<float>> inv = Matrix.Inverse(JJt);

		List<List<float>> J_pinv = Matrix.Mul(JT, inv);

		// --- Delta theta ---
		List<List<float>> dTheta = Matrix.Mul(J_pinv, error);

		// --- Step size ---
		return Matrix.Mul(dTheta, alpha);
	}
}