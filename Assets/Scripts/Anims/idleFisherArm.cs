using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class idleFisherArm : MonoBehaviour {
    InputAction startAction;
    InputAction nextAction;

    float duration = 1f;

    public TMP_Text text;
    public TMP_Text text2;

    void Awake() {
        startAction = InputSystem.actions.FindAction("start");
        nextAction = InputSystem.actions.FindAction("next");
    }

    private void OnEnable() {
        startAction.started += StartAnim;
        startAction.Enable();
        nextAction.started += StartNext;
        nextAction.Enable();
    }

    private void OnDisable() {
        startAction.started -= StartAnim;
        startAction.Disable();
        nextAction.started -= StartNext;
        nextAction.Disable();
    }

    void StartAnim(InputAction.CallbackContext context) {
        print("starting!");
        StartCoroutine(Anim());
    }

    void StartNext(InputAction.CallbackContext context) {
        print("starting!");
        StartCoroutine(Next());
    }

    void Start() {
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0f);
        text2.color = new Color(text2.color.r, text2.color.g, text2.color.b, 0f);
        transform.GetChild(0).GetComponent<threeJoint>().enabled = false;
    }

    IEnumerator Anim() {
        StartCoroutine(Animations.MoveTo(transform, new Vector3(-2.8900001f, -5.28000021f, 0f), duration));
        yield return new WaitForSeconds(duration * 0.5f);

        StartCoroutine(Animations.Fade(() => text.color, c => text.color = c, false, duration));
    }

    IEnumerator Next() {

        StartCoroutine(Animations.Fade(() => text.color, c => text.color = c, true, duration));
        StartCoroutine(Animations.Fade(() => text2.color, c => text2.color = c, false, duration));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        transform.GetChild(0).GetComponent<threeJoint>().enabled = true;
    }
}