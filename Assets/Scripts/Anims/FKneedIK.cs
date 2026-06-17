using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class FKneedIK : MonoBehaviour {
    InputAction startAction;

    public Transform otherSegment1;
    public Transform otherSegment1Tip;

    public Transform otherSegment2;
    public Transform otherSegment2Tip;

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

    void Start() {
        otherSegment1.localScale = new Vector3(1f, 0f, 1f);
        otherSegment1Tip.localPosition = Vector3.zero;

        otherSegment2.localScale = new Vector3(1f, 0f, 1f);
        otherSegment2Tip.localPosition = Vector3.zero;
        otherSegment2.parent.gameObject.SetActive(false);
    }

    IEnumerator Anim() {
        float duration = 1f;

        StartCoroutine(Animations.Scale(otherSegment1, new Vector3(1f, 3f, 1f), duration));
        StartCoroutine(Animations.MoveTo(otherSegment1Tip, new Vector3(0f, 3f, otherSegment1Tip.localPosition.z), duration, true));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        StartCoroutine(Animations.Rotate(otherSegment1.parent, new Vector3(0f, 0f, -33.74f), duration));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        otherSegment2.parent.gameObject.SetActive(true);
        StartCoroutine(Animations.Scale(otherSegment2, new Vector3(1f, 2f, 1f), duration));
        StartCoroutine(Animations.MoveTo(otherSegment2Tip, new Vector3(0f, 2f, otherSegment2Tip.localPosition.z), duration, true));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        StartCoroutine(Animations.Rotate(otherSegment2.parent, new Vector3(0f, 0f, -104.9f - 33.74f), duration));

        yield return new WaitForSeconds(1f);
    }
}