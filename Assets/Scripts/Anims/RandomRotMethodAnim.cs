using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.HableCurve;

public class RandomRotMethodAnim : MonoBehaviour {

	InputAction startAction;

	[SerializeField]
	Transform segment1;

	[SerializeField]
	float toRot = -11f;

    void Start() {
		startAction = InputSystem.actions.FindAction("start");
		startAction.started += StartAnim;
	}

	void StartAnim(InputAction.CallbackContext context) {
        StartCoroutine(Anim());
    }

    IEnumerator Anim() {
        float duration = 1f;

        yield return AnimateToRot(duration, segment1, toRot); // -33.74f -> -11f
    }

	IEnumerator AnimateToRot(float duration, Transform trans, float rot) {
		float elapsed = 0f;

		float startRot = trans.localEulerAngles.z;
		if (startRot > 180)
			startRot -= 360;
		print(startRot + ", " + rot);

        while (elapsed < duration) {
			elapsed += Time.deltaTime;

			float percent = elapsed / duration;
			float logistic = Math.Logistical(percent);

			trans.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(startRot, rot, logistic));
			yield return null;
		}

        trans.localEulerAngles = new Vector3(0f, 0f, rot);
    }
}