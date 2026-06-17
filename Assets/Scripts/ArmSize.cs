using UnityEngine;

public class ArmSize : MonoBehaviour {
	public void SetSize(float size) {
		Transform arm = transform.GetChild(0);
		Vector3 scale = arm.localScale;
		arm.localScale = new Vector3(scale.x, size, scale.z);
		arm.localPosition = new Vector3(0f, size / 2f, 0f);

		Transform joint = arm.GetChild(0);
		joint.localScale = new Vector3(2f, 1f / size, 0f);
	}

	public float GetSize() {
		Transform arm = transform.GetChild(0);
		return arm.localScale.y;
	}

}