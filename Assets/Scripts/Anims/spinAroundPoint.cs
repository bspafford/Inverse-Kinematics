using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class spinAroundPoint : MonoBehaviour {
    InputAction startAction;

    public Vector3 center;
    Vector3 startLoc;

    // have to do this in Awake, onEnable, and OnDisable to make inputs work correctly without having to do scene/domain reload on play
    void Awake() {
        startAction = InputSystem.actions.FindAction("Next");
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
        startLoc = transform.position;
    }

    IEnumerator Anim() {
        yield return Animations.Rotate(transform, new Vector3(0f, 360f, 0f), 2f);
    }
}