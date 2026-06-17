using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class growLimb : MonoBehaviour {

    public float duration = 1f;

    public Transform joint2;
    public Transform square;
    public Transform circle;

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

    IEnumerator Anim() {
        StartCoroutine(Animations.MoveTo(joint2, new Vector3(0f, 2f, 0f), 1f, true));

        StartCoroutine(Animations.MoveTo(circle, new Vector3(0f, 2f, -0.1f), 1f, true));
        StartCoroutine(Animations.Scale(circle, new Vector3(1f, 1f, 1f), 1f));

        StartCoroutine(Animations.Scale(square, new Vector3(0.5f, 2f, 1f), 1f));
        StartCoroutine(Animations.MoveTo(square, new Vector3(0f, 1f, 0f), 1f, true));



        yield return null;
    }
}