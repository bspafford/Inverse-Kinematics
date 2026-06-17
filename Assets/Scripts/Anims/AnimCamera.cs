using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class AnimCamera : MonoBehaviour {
    public float duration = 1f;
    public Vector3 desiredCamPos = Vector3.zero;
    public Vector3 lookAt;

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

    void Update() {
        Camera.main.transform.LookAt(lookAt);
    }

    IEnumerator Anim() {
        StartCoroutine(Animations.MoveTo(Camera.main.transform, desiredCamPos, duration));
        yield return null;
    }
}