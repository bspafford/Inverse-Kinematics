using UnityEngine;

public class ArmSize3D : MonoBehaviour {
	public void SetSize(float size) {
		Transform arm = transform.GetChild(0);
		Vector3 scale = arm.localScale;
		arm.localScale = new Vector3(scale.x, size, scale.z);
		arm.localPosition = new Vector3(0f, size, 0f);

		Transform joint = arm.GetChild(0);
		joint.localScale = new Vector3(1.333f, 1f / size, 1.333f);
	}
}