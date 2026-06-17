using UnityEngine;
using UnityEngine.InputSystem;

public class Attempt2 : MonoBehaviour {

	[SerializeField]
	float armNum = 3f;

    Transform arm1;
    Transform arm2;
    Transform arm3;

	[SerializeField]
	Transform target;

	[SerializeField]
	float rotStep = 0.1f;

    void Start() {
        arm1 = transform;
        arm2 = transform.GetChild(0);
        arm3 = arm2.GetChild(0);
    }

	void Update() {
		Transform end = arm3.GetChild(0).GetChild(0);

		UpdateArm(arm1, end, rotStep);
		UpdateArm(arm2, end, rotStep);
		//UpdateArm(arm3, end, rotStep);
    }

	void UpdateArm(Transform arm, Transform end, float rotStep) {
        float distToDesiredPrev = Vector3.Distance(end.position, target.position);

        float percent = Mathf.Clamp(distToDesiredPrev / 5f, 0f, 1f);
        rotStep *= percent;

        float randRot = Random.Range(-rotStep, rotStep);

        arm.transform.eulerAngles += new Vector3(0f, 0f, randRot);
        float distToDesired = Vector3.Distance(end.position, target.position);

        if (distToDesired > distToDesiredPrev) { // if original rotation was closer
            arm.transform.eulerAngles -= new Vector3(0f, 0f, randRot); // revert back to original
        }
    }

    /* before iterating, working, big step solution
    void UpdateArm(Transform arm, Transform end, float rotStep) {
        float randRot = Random.Range(-rotStep, rotStep);

        float distToDesiredPrev = Vector3.Distance(end.position, target.position);
        arm.transform.eulerAngles += new Vector3(0f, 0f, randRot);
        float distToDesired = Vector3.Distance(end.position, target.position);

        if (distToDesired > distToDesiredPrev) { // if original rotation was closer
            arm.transform.eulerAngles -= new Vector3(0f, 0f, randRot); // revert back to original
        }
    }
    */
}