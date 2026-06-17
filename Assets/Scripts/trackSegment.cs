using UnityEngine;

public class trackSegment : MonoBehaviour {

	public Transform trackingPoint;

    private void OnValidate() {
        Update();
    }

    void Update() {
		transform.position = new Vector3(trackingPoint.position.x, trackingPoint.position.y, 0f);
	}
}