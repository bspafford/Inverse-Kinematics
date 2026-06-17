using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class rotDemo : MonoBehaviour {
    InputAction startAction;

    public Transform segment1;
    public Arrow arrow1;

    public Transform segment2;
    public Arrow arrow2;

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
        StartCoroutine(Animations.Rotate(segment1, new Vector3(0f, 35.77f + 360f, 0f), duration, true));
        StartCoroutine(Animations.Rotate(arrow1.transform.parent, new Vector3(0f, 360f, 0f), duration));
        arrow1.StartLineAnim();
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        arrow2.gameObject.SetActive(true);
        StartCoroutine(Animations.Fade(() => arrow1.color, c => arrow1.color = c, true, duration));
        StartCoroutine(Animations.Rotate(segment2, new Vector3(0f, 0f, -50.154f - 360f), duration, true));
        //StartCoroutine(Animations.Rotate(arrow2.transform.parent, new Vector3(0f, 0f, -360f), duration));
        arrow2.StartLineAnim();
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        StartCoroutine(Animations.Fade(() => arrow2.color, c => arrow2.color = c, true, duration));
    }
}