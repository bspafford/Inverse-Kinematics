using UnityEngine;
using UnityEngine.InputSystem;

public class ArmScript : MonoBehaviour {

	Transform arm1;
    Transform arm2;
    Transform arm3;

    void Start() {
		arm1 = transform;
		arm2 = transform.GetChild(0);
		arm3 = arm2.GetChild(0);
    }

	void Update() {
		Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
		Vector2 dir = Vector2.Normalize(mousePos - screenSize);

		float arm1Rot = mousePos.x / 2f;
		float arm2Rot = mousePos.y / 2f + arm1Rot;
        arm1.transform.rotation = Quaternion.Euler(0f, 0f, arm1Rot);
		arm2.transform.rotation = Quaternion.Euler(0f, 0f, arm2Rot);
    }
}