using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReduceErrorAnim : MonoBehaviour {

    public float duration = 1f;

    public Arrow arrow;

    public Transform target;
    public Vector3 startLoc;
    public Vector3 endLoc;

    InputAction startAction;

    void Start() {
        target.position = startLoc;
    }

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
        arrow.StartLineAnim();
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        StartCoroutine(Animations.MoveTo(target, endLoc, duration));
        StartCoroutine(Animations.MoveTo(arrow.transform, new Vector3(0.954f, 1.81f, -1.125f), duration));
        StartCoroutine(Animations.CustomFloat(() => arrow.percent, f => arrow.percent = f, 1f, 0f, duration));

        yield return new WaitForSeconds(duration * 0.5f);

        StartCoroutine(Animations.Fade(() => arrow.color, f => arrow.color = f, true, duration * 0.5f));

    }
}