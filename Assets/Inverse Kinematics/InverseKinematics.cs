using UnityEngine;
using UnityEngine.InputSystem;

public class InverseKinematics : MonoBehaviour {

	[SerializeField]
	float angle = -90f;
	[SerializeField]
	public Vector2 targetLoc = new Vector2(3, 1);

	[SerializeField]
	float arm1Length = 3f;
	[SerializeField]
	float arm2Length = 2f;

	Transform arm1;
	Transform arm2;
	Transform arm3;

	[SerializeField]
	bool solution = true;

	public bool useMousePos = true;

	void Start() {
		arm1 = transform;
		arm2 = transform.GetChild(0);
		//arm3 = arm2.GetChild(0);
	}

    //Vector3 mousePos = Mouse.current.position.ReadValue();
    //targetLoc = Camera.main.ScreenToWorldPoint(mousePos);
	/*
    void Update() {
		

		float numerator = (targetLoc.x * targetLoc.x) + (targetLoc.y * targetLoc.y) + 5;
		float denominator = Mathf.Sqrt(Mathf.Pow((targetLoc.x * 6), 2) + Mathf.Pow((targetLoc.y * 6), 2));

		float theta1 = Mathf.Atan2(targetLoc.y, targetLoc.x) + Mathf.Acos(numerator / denominator);
		float theta2 = Mathf.Atan2((targetLoc.y - 3 * Mathf.Sin(theta1)), (targetLoc.x - 3 * Mathf.Cos(theta1))) - theta1;

		float angle1 = theta1 * Mathf.Rad2Deg;
		float angle2 = theta2 * Mathf.Rad2Deg;

		arm1.transform.rotation = Quaternion.Euler(0f, 0f, angle1 - 90);
		arm2.transform.rotation = Quaternion.Euler(0f, 0f, angle2 - 90 + angle1);
	}
	*/

	void Update() {
		Vector3 mousePos = Mouse.current.position.ReadValue();
		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
		Vector3 basePos = transform.position;

		if (useMousePos)
			targetLoc = mouseWorldPos - basePos;

		// theta1 + theta2 + theta3 = angle
		// loc.x = 3cos(theta1) + 2cos(theta1 + theta2)
		// loc.y = 3sin(theta1) + 2sin(theta1 + theta2)

		// 4 = (loc.x - 3cos(theta2))^2 + (loc.y - 3sin(theta2))^2


		// numerator = loc.x^2 + loc.y^2 + 9 - 4
		// denominator = sqrt((loc.x * 6)^2 + (loc.y * 6)^2)

		// theta1 = tan^-1(loc.y / loc.x) - cos^-1(numerator / denominator)
		// theta2 = tan^-1((loc.x - 3cos(theta1)) / (loc.y - 3sin(theta1))) - theta1
		// theta3 = angle - theta1 - theta2


		float numerator = (targetLoc.x * targetLoc.x) + (targetLoc.y * targetLoc.y) + 5;
		float denominator = Mathf.Sqrt(Mathf.Pow((targetLoc.x * 6), 2) + Mathf.Pow((targetLoc.y * 6), 2));

		float minus = 1;// loc.x < 0 ? -1 : 1;
		float theta1 = Mathf.Atan2(targetLoc.y, targetLoc.x) + Mathf.Acos(numerator / denominator) * (solution ? 1 : -1); // +/-
		float theta2 = Mathf.Atan2((targetLoc.y - 3 * Mathf.Sin(theta1)), (targetLoc.x - 3 * Mathf.Cos(theta1))) - theta1;
		float theta3 = angle * Mathf.Deg2Rad - theta1 - theta2;

		float angle1 = theta1 * Mathf.Rad2Deg;
		float angle2 = theta2 * Mathf.Rad2Deg;
		float angle3 = theta3 * Mathf.Rad2Deg;

		if (float.IsNaN(angle1) || float.IsNaN(angle2) || float.IsNaN(angle3)) {
			Quaternion rot = Quaternion.LookRotation(Vector3.forward, targetLoc.normalized);
			arm1.transform.rotation = rot;
			arm2.transform.rotation = rot;
		} else {
			arm1.transform.rotation = Quaternion.Euler(0f, 0f, angle1 - 90);
			arm2.transform.rotation = Quaternion.Euler(0f, 0f, angle2 - 90 + angle1);
		}

		print("angle1: " + angle1 + ", angle2: " + angle2 + ", angle3: " + angle3);
	}
}