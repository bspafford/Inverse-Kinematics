using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class createArmAnim : MonoBehaviour {

	InputAction startAction;

	[SerializeField]
	Transform floorLine;
	[SerializeField]
	Transform floorBackground;

	[SerializeField]
	Transform hemicircle;

	[SerializeField]
	Transform segment1;
	[SerializeField]
	Transform joint1;

	[SerializeField]
	Transform segment2;
	[SerializeField]
	Transform joint2;

    [SerializeField]
    Transform segment3;
    [SerializeField]
    Transform joint3;

    void Awake() {
        startAction = InputSystem.actions.FindAction("start");
    }

    private void OnEnable() {
        startAction.started += StartAnim;
        startAction.Enable();
    }

    private void OnDisable() {
        startAction.started -= StartAnim;
        startAction.Disable();
    }

    void Start() {
		SpriteRenderer renderer = floorBackground.GetComponent<SpriteRenderer>();
		Color color = renderer.color;
		color.a = 0;
		renderer.color = color;

		floorLine.localScale = Vector3.zero;
		hemicircle.localScale = Vector3.zero;
		segment1.localScale = Vector3.zero;
		joint1.localScale = Vector3.zero;
		segment2.localScale = Vector3.zero;
		joint2.localScale = Vector3.zero;
		if (segment3 && joint3) {
			segment3.localScale = Vector3.zero;
			joint3.localScale = Vector3.zero;
		}
    }

	void StartAnim(InputAction.CallbackContext context) {
		StartCoroutine(Anim());
	}

	IEnumerator Anim() {
		float duration = 1f;
		float startAfter = 0.7f;

		StartCoroutine(FloorAnim(duration));
		yield return new WaitForSeconds(duration * startAfter);

		StartCoroutine(PartAnim(duration * 0.75f, new List<Transform>{ segment1 }, Vector2.up));
		yield return new WaitForSeconds(duration * 0.75f * startAfter);

		StartCoroutine(PartAnim(duration * 0.5625f, new List<Transform>{ joint1 }, Vector2.one));
		yield return new WaitForSeconds(duration * 0.5625f * startAfter * 0.8f);

		StartCoroutine(PartAnim(duration * 0.75f, new List<Transform> { segment2 }, Vector2.up));
		yield return new WaitForSeconds(duration * 0.75f * startAfter);

		StartCoroutine(PartAnim(duration, new List<Transform> { joint2 }, Vector2.one));
		yield return new WaitForSeconds(duration * startAfter * 0.8f);

		if (!segment3 || !joint3)
			yield break;

		yield return new WaitForSeconds(1f);

        StartCoroutine(PartAnim(duration * 0.75f, new List<Transform> { segment3 }, Vector2.up));
        yield return new WaitForSeconds(duration * 0.75f * startAfter);

        StartCoroutine(PartAnim(duration, new List<Transform> { joint3 }, Vector2.one));
        yield return new WaitForSeconds(duration * startAfter * 0.8f);
    }

	IEnumerator PartAnim(float duration, List<Transform> transforms, Vector2 axis) {
		yield return PartAnim(duration, transforms, new List<Vector2> { axis });
	}

	IEnumerator PartAnim(float duration, List<Transform> transforms, List<Vector2> axis) {
		float elapsed = 0f;
		while (elapsed < duration) {
			elapsed += Time.deltaTime;

			float percent = elapsed / duration;

			float logistic = Math.Logistical(percent);

			for (int i = 0; i < transforms.Count; ++i) {
				transforms[i].localScale = new Vector3(axis[i].x == 0 ? 1f : logistic, axis[i].y == 0 ? 1f : logistic, logistic);
			}

			yield return null;
		}
	}

	IEnumerator FloorAnim(float duration) {
		float elapsed = 0f;
		while (elapsed < duration) {
			elapsed += Time.deltaTime;

			float percent = elapsed / duration;

			float logistic = Math.Logistical(percent);

			hemicircle.localScale = new Vector3(logistic, logistic, logistic);
			floorLine.localScale = new Vector3(logistic * 21.04f, 0.08883516f, logistic);

			SpriteRenderer renderer = floorBackground.GetComponent<SpriteRenderer>();
			Color color = renderer.color;
			color.a = logistic;
			renderer.color = color;

			yield return null;
		}
	}
}