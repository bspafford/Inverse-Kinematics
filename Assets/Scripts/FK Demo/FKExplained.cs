using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class FKExplained : MonoBehaviour {
    public Transform segment1;
    public Transform segment2;
    public SpriteRenderer floorLine;
    public SpriteRenderer floor;

    public SpriteRenderer fakeBackground; // faking background to "fade" segment2

    public Angle angle1;
    public Angle angle2;

    public DashedLine xLine;
    public DashedLine yLine;

    public DashedLine xLine2;
    public DashedLine yLine2;

    public Arrow line10;
    public Arrow line11;
    public Arrow line12;

    public SpriteRenderer smallBase;
    public SpriteRenderer smallTip;

    InputAction startAction;

    // have to do this in Awake, onEnable, and OnDisable to make inputs work correctly without having to do scene/domain reload on play
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


    IEnumerator Anim() {
        float duration = 1f;

        // get segment1 alone
        StartCoroutine(Animations.MoveTo(segment1, new Vector2(-0.8f, -1.5f), duration));
        StartCoroutine(Animations.Fade(() => floorLine.color, c => floorLine.color = c, true, duration));
        StartCoroutine(Animations.Fade(() => floor.color, c => floor.color = c, true, duration));
        yield return new WaitForSeconds(duration * 0.1f);
        StartCoroutine(Animations.Fade(() => fakeBackground.color, c => fakeBackground.color = c, false, duration * 0.9f));
        yield return new WaitForSeconds(duration * 0.9f);

        segment2.gameObject.SetActive(false);
        segment2.GetComponent<DropShadow>().Remove();
        fakeBackground.gameObject.SetActive(false);

        yield return new WaitForSeconds(1f);

        // show segment length
        line10.StartLineAnim();
        line11.StartLineAnim();
        yield return new WaitForSeconds(0.85f);
        line12.StartLineAnim();

        yield return new WaitForSeconds(0.4f);

        // rotate segment to show angle
        StartCoroutine(Animations.AnimateAngle(angle1, 0f, 56.26f, duration));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        StartCoroutine(Animations.Fade(() => line10.color, c => line10.color = c, true, duration));
        StartCoroutine(Animations.Fade(() => line11.color, c => line11.color = c, true, duration));
        StartCoroutine(Animations.Fade(() => line12.color, c => line12.color = c, true, duration));
        StartCoroutine(Animations.Fade(() => angle1.color, c => angle1.color = c, true, duration));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        // turn into small line
        StartCoroutine(Animations.MoveTo(segment1.GetChild(1).GetChild(0).GetChild(0), new Vector2(0, 1.5f), duration, true));
        StartCoroutine(Animations.MoveTo(segment1.GetChild(1).GetChild(0).GetChild(1), new Vector2(0, 1.5f), duration, true));
        StartCoroutine(Animations.Fade(() => smallBase.color, c => smallBase.color = c, false, duration));
        StartCoroutine(Animations.Fade(() => smallTip.color, c => smallTip.color = c, false, duration));
        StartCoroutine(Animations.Scale(smallTip.transform, new Vector2(0.3f, 0.3f), duration));
        StartCoroutine(Animations.Scale(segment1.GetChild(0), new Vector2(0.3f, 0.3f), duration));
        StartCoroutine(Animations.Scale(segment1.GetChild(1).GetChild(0).GetChild(2).GetChild(0), new Vector2(0.3f, 0.3f), duration));
        StartCoroutine(Animations.MoveTo(segment1.GetChild(0), new Vector2(0f, 0f), duration, true));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        StartCoroutine(Animations.AnimateDashedLines(xLine, new Vector2(1.732551f, 0), duration));
        StartCoroutine(Animations.AnimateDashedLines(yLine, new Vector2(0, -2.48f), duration));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        // shrink segment1 to bottom of screen
        StartCoroutine(Animations.Scale(segment1, new Vector2(0.6f, 0.6f), duration));
        StartCoroutine(Animations.MoveTo(segment1, new Vector2(-0.8f, -4f), duration));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        // create segment2
        segment2.gameObject.SetActive(true);
        segment2.transform.position = new Vector3(-1.15f, 0.8f, 0f);
        segment2.GetComponent<DropShadow>().enabled = true;
        segment2.GetComponent<DropShadow>().RecalcList();
        fakeBackground.gameObject.SetActive(true);
        fakeBackground.transform.position = new Vector3(0f, 0f, -0.2f);
        StartCoroutine(Animations.Fade(() => fakeBackground.color, c => fakeBackground.color = c, true, duration));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        // segment2 to thin line
        StartCoroutine(Animations.Scale(segment2.GetChild(0), new Vector2(0.15f, 2f), duration));
        StartCoroutine(Animations.Scale(segment2.GetChild(1), new Vector2(0.3f, 0.3f), duration));
        StartCoroutine(Animations.Scale(segment2.GetChild(2), new Vector2(0.3f, 0.3f), duration));
        StartCoroutine(Animations.Scale(segment2.GetChild(2), new Vector2(0.3f, 0.3f), duration));
        SpriteRenderer segment2Tip = segment2.GetChild(0).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        StartCoroutine(Animations.Fade(() => segment2Tip.color, c => segment2Tip.color = c, true, duration));
        StartCoroutine(Animations.Scale(segment2Tip.transform, new Vector2(3.33333f * 0.3f, 0.3f), duration));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        // animate dashed lines
        StartCoroutine(Animations.AnimateDashedLines(xLine2, new Vector2(-1.33f, 0), duration));
        StartCoroutine(Animations.AnimateDashedLines(yLine2, new Vector2(0, -1.51f), duration));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        // put segments side by side
        StartCoroutine(Animations.Scale(segment1, Vector2.one, duration));
        StartCoroutine(Animations.MoveTo(segment1, new Vector2(-3.7f, -1.5f), duration));
        StartCoroutine(Animations.MoveTo(segment2, new Vector2(2.57f, 0.57f), duration));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        // stack on top
        StartCoroutine(Animations.MoveTo(segment1, new Vector2(-1.036275f, -1.5977f), duration));
        StartCoroutine(Animations.MoveTo(segment2, new Vector2(0.63f, 0.9f), duration));
        StartCoroutine(Animations.Fade(() => xLine.color, c => xLine.color = c, true, duration));
        StartCoroutine(Animations.Fade(() => yLine.color, c => yLine.color = c, true, duration));
        StartCoroutine(Animations.Fade(() => xLine2.color, c => xLine2.color = c, true, duration));
        StartCoroutine(Animations.Fade(() => yLine2.color, c => yLine2.color = c, true, duration));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        // bring joints back to normal size
        StartCoroutine(Animations.MoveTo(segment1.GetChild(1).GetChild(0).GetChild(0), new Vector2(-0.1760001f, 1.5f), duration, true));
        StartCoroutine(Animations.MoveTo(segment1.GetChild(1).GetChild(0).GetChild(1), new Vector2(0.1579999f, 1.5f), duration, true));
        StartCoroutine(Animations.Scale(segment1.GetChild(0), new Vector2(1f, 1f), duration));
        StartCoroutine(Animations.Scale(segment1.GetChild(1).GetChild(0).GetChild(2).GetChild(0), new Vector2(1f, 1f), duration));
        StartCoroutine(Animations.Scale(segment1.GetChild(1).GetChild(0).GetChild(2).GetChild(0), new Vector2(1f, 1f), duration));
        StartCoroutine(Animations.Scale(smallTip.transform, new Vector2(1f, 1f), duration));
        StartCoroutine(Animations.Fade(() => smallTip.color, c => smallTip.color = c, true, duration));
        StartCoroutine(Animations.Fade(() => smallBase.color, c => smallBase.color = c, true, duration));
        StartCoroutine(Animations.MoveTo(segment1.GetChild(0), new Vector2(0f, -0.25f), duration, true));
        // segment2 bring back to normal
        StartCoroutine(Animations.Scale(segment2.GetChild(0), new Vector2(0.5f, 2f), duration));
        StartCoroutine(Animations.Scale(segment2Tip.transform, new Vector2(1f, 1f), duration));
        StartCoroutine(Animations.Scale(segment2.GetChild(1), new Vector2(0.5f, 0.5f), duration));
        StartCoroutine(Animations.Scale(segment2.GetChild(2), new Vector2(0.5f, 0.5f), duration));
        StartCoroutine(Animations.Fade(() => segment2Tip.color, c => segment2Tip.color = c, false, duration));
        // move back to default spot
        StartCoroutine(Animations.MoveTo(segment1, new Vector2(0f, -3f), duration, true));
        StartCoroutine(Animations.MoveTo(segment2, new Vector2(1.666275f, -0.5023003f), duration, true));
        // fade back in floor
        StartCoroutine(Animations.Fade(() => floorLine.color, c => floorLine.color = c, false, duration));
        StartCoroutine(Animations.Fade(() => floor.color, c => floor.color = c, false, duration));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);


        yield return null;
    }
}