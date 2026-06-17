using UnityEngine;

public class Billboard : MonoBehaviour {
	void Start() {
		
	}

	void Update() {
		transform.localEulerAngles = new Vector3(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
	}
}