using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class FKIntro : MonoBehaviour {
	public Transform segment1;
	public Transform segment2;
	public SpriteRenderer floorLine;
	public SpriteRenderer floor;
    public Angle angle1;
    public Angle angle2;

    public Arrow line10;
    public Arrow line11;
    public Arrow line12;
    public Arrow line20;
    public Arrow line21;
    public Arrow line22;

    public DashedLine xLine;
    public DashedLine yLine;

    InputAction startAction;

	void Start() {
		startAction = InputSystem.actions.FindAction("start");
		startAction.started += StartAnim;

    }

	void StartAnim(InputAction.CallbackContext context) {
		StartCoroutine(Anim());
	}


	IEnumerator Anim() {
		// animate arms to be side by side
		float duration = 1f;
        StartCoroutine(Animations.Rotate(segment1.GetChild(1), Vector3.zero, duration));
        StartCoroutine(Animations.Rotate(segment2, Vector3.zero, duration));
		StartCoroutine(Animations.MoveTo(segment1, new Vector2(-2f, -1.5f), duration));
		StartCoroutine(Animations.MoveTo(segment2, new Vector2(2f, -1f), duration));
        StartCoroutine(Animations.Fade(() => floorLine.color, c => floorLine.color = c, true, duration));
        StartCoroutine(Animations.Fade(() => floor.color, c => floor.color = c, true, duration));
        yield return new WaitForSeconds(duration);

		yield return new WaitForSeconds(1f);

        line10.StartLineAnim();
        line11.StartLineAnim();
        line20.StartLineAnim();
        line21.StartLineAnim();
        yield return new WaitForSeconds(0.85f);
        line12.StartLineAnim();
        line22.StartLineAnim();

        yield return new WaitForSeconds(1f);

        // rotate segments to show angle
        StartCoroutine(Animations.Rotate(segment1.GetChild(1), new Vector3(0f, 0f, 15.4f), duration));
        StartCoroutine(Animations.Rotate(segment2, new Vector3(0f, 0f, -248.96f), duration));
        StartCoroutine(Animations.AnimateAngle(angle1, 0f, 105.4f, duration));
        StartCoroutine(Animations.AnimateAngle(angle2, 0f, 201.04f, duration));
        StartCoroutine(Animations.Fade(() => line10.color, c => line10.color = c, true, duration / 2f));
        StartCoroutine(Animations.Fade(() => line11.color, c => line11.color = c, true, duration / 2f));
        StartCoroutine(Animations.Fade(() => line12.color, c => line12.color = c, true, duration / 2f));
        StartCoroutine(Animations.Fade(() => line20.color, c => line20.color = c, true, duration / 2f));
        StartCoroutine(Animations.Fade(() => line21.color, c => line21.color = c, true, duration / 2f));
        StartCoroutine(Animations.Fade(() => line22.color, c => line22.color = c, true, duration / 2f));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        StartCoroutine(Animations.MoveTo(segment1, new Vector2(0f, -3f), duration));
        StartCoroutine(Animations.MoveTo(segment2, new Vector2(-0.82f, -0.1f), duration));
        StartCoroutine(Animations.Fade(() => floorLine.color, c => floorLine.color = c, false, duration));
        StartCoroutine(Animations.Fade(() => floor.color, c => floor.color = c, false, duration));
        StartCoroutine(Animations.Fade(() => angle1.color, c => angle1.color = c, true, duration));
        StartCoroutine(Animations.Fade(() => angle2.color, c => angle2.color = c, true, duration));
        StartCoroutine(Animations.MoveTo(angle1.transform, new Vector2(0f, -3f), duration));
        StartCoroutine(Animations.MoveTo(angle2.transform, new Vector2(-0.82f, -0.1f), duration));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(Animations.AnimateDashedLines(xLine, new Vector2(-2.68666f, 0), duration));
        yield return StartCoroutine(Animations.AnimateDashedLines(yLine, new Vector2(0, 2.181961f), duration));
    }
}