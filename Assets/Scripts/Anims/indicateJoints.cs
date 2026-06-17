using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class indicateJoints : MonoBehaviour {

	public Transform joint1;
	public Transform joint2;
	public Transform joint3;

	public Transform joint1Tracker;
	public Transform joint2Tracker;

	public TMP_Text fKText;

    public float scaleMultiplier = 1.5f;
	public float duration = 1f;
	public Color indicateColor = Color.yellow;
	public float laggedStart = 0.1f;
	public float jointLaggedStart = 0.05f;

	[Header("Rot")]
	public float totalRot = 10f;

	InputAction startAction;
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

	void StartAnim(InputAction.CallbackContext context) {
		print("starting!");
		StartCoroutine(Anim());
	}

	void Update() {
		float x1 = joint2.position.x;
		float x2 = joint1.position.x;
		float diff = x1 - x2;
		float startRot = -33.74f;

		transform.eulerAngles = new Vector3(0f, 0f, startRot + Mathf.LerpUnclamped(-totalRot, totalRot, diff));
    }

	IEnumerator Anim() {
		yield return StartCoroutine(Animations.Rotate(joint1.parent.parent.parent, new Vector3(0f, 0f, -104.9f + 360f * 2f), 2f, true));

        yield return new WaitForSeconds(1f);

		StartCoroutine(Animations.Fade(() => fKText.GetComponent<TMP_Text>().color, c => fKText.GetComponent<TMP_Text>().color = c, true, duration));
        StartCoroutine(Animations.Scale(joint1, Vector3.one * scaleMultiplier, duration));
		StartCoroutine(Animations.SetColor(() => joint1.GetComponent<Renderer>().material.color, c => joint1.GetComponent<Renderer>().material.color = c, indicateColor, duration));

        yield return new WaitForSeconds(laggedStart);

        StartCoroutine(Animations.Scale(joint2, Vector3.one * scaleMultiplier, duration));
		StartCoroutine(Animations.SetColor(() => joint2.GetComponent<Renderer>().material.color, c => joint2.GetComponent<Renderer>().material.color = c, indicateColor, duration));

        yield return new WaitForSeconds(laggedStart);

        StartCoroutine(Animations.Scale(joint3.parent, Vector3.one * scaleMultiplier, duration));
        StartCoroutine(Animations.SetColor(() => joint3.GetComponent<Renderer>().material.color, c => joint3.GetComponent<Renderer>().material.color = c, indicateColor, duration));

        yield return new WaitForSeconds(duration - laggedStart * 2f);

        StartCoroutine(Animations.Scale(joint1, Vector3.one, duration));
        StartCoroutine(Animations.SetColor(() => joint1.GetComponent<Renderer>().material.color, c => joint1.GetComponent<Renderer>().material.color = c, Color.white, duration));

        yield return new WaitForSeconds(laggedStart);

        StartCoroutine(Animations.Scale(joint2, Vector3.one, duration));
        StartCoroutine(Animations.SetColor(() => joint2.GetComponent<Renderer>().material.color, c => joint2.GetComponent<Renderer>().material.color = c, Color.white, duration));

        yield return new WaitForSeconds(laggedStart);

        StartCoroutine(Animations.Scale(joint3.parent, Vector3.one, duration));
        StartCoroutine(Animations.SetColor(() => joint3.GetComponent<Renderer>().material.color, c => joint3.GetComponent<Renderer>().material.color = c, Color.white, duration));

        yield return new WaitForSeconds(duration);

		// joint tracker
		StartCoroutine(Animations.MoveTo(joint1Tracker, new Vector3(-1.5f, 2.21f, -0.1f), duration));
        yield return new WaitForSeconds(jointLaggedStart);
        StartCoroutine(Animations.MoveTo(joint2Tracker, new Vector3(1.5f, 2.21f, -0.1f), duration));
    }
}