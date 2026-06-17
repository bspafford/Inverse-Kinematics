using UnityEngine;

public class monsterWalk : MonoBehaviour {

	IK3DnDOF script;

	public float stepDistance = 1.0f;
	public float stepLength = 1.0f;

	Vector3 currentTargetPos;

	void Start() {
		script = GetComponent<IK3DnDOF>();
		if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f)) {
			currentTargetPos = hit.point;
		}
	}

	void Update() {
		float distance = Vector3.Distance(
			new Vector3(transform.position.x, 0f, transform.position.z),
			new Vector3(currentTargetPos.x, 0f, currentTargetPos.z)
		);

		if (distance > stepDistance) {
			Vector3 newPos = transform.position - transform.forward * stepLength;

			if (Physics.Raycast(newPos, Vector3.down, out RaycastHit hit, 10f)) {
				print("updatingin position");
				currentTargetPos = hit.point;
				script.target.position = currentTargetPos;
			}
		}
	}
}