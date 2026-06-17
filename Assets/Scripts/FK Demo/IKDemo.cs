using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class IKDemo : MonoBehaviour {
    InputAction startAction;

    public Transform target;

    public Transform segment1;
    public Transform segment2;

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

    void Start() {
        ArmSize2 segment1ArmSize = segment1.GetComponent<ArmSize2>();
        ArmSize2 segment2ArmSize = segment2.GetComponent<ArmSize2>();

        segment1ArmSize.SetSize(-0.27f);
        segment1.eulerAngles = Vector3.zero;
        segment2ArmSize.SetSize(0f);
    }

    void StartAnim(InputAction.CallbackContext context) {
        print("starting!");
        StartCoroutine(Anim());
    }

    IEnumerator Anim() {
        float duration = 1f;

        // animate target position
        SpriteRenderer targetRenderer = target.GetComponent<SpriteRenderer>();
        StartCoroutine(Animations.Fade(() => targetRenderer.color, c => targetRenderer.color = c, false, duration));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        //StartCoroutine(Animations.MoveTo(target, new Vector3(3f, -3, -0.3f), duration));
        //yield return new WaitForSeconds(duration);

        //yield return new WaitForSeconds(1f);

        StartCoroutine(Animations.MoveTo(target, new Vector3(3f, -2, -0.3f), duration));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        // animate segment lengths
        ArmSize2 segment1ArmSize = segment1.GetComponent<ArmSize2>();
        ArmSize2 segment2ArmSize = segment2.GetComponent<ArmSize2>();
        StartCoroutine(Animations.CustomFloat(() => segment1ArmSize.GetSize(), s => segment1ArmSize.SetSize(s), -0.27f, 3f, duration));
        StartCoroutine(Animations.Rotate(segment1, new Vector3(0f, 0f, 16.95f), duration));
        yield return new WaitForSeconds(duration * 0.9f);

        //yield return new WaitForSeconds(1f);

        StartCoroutine(Animations.CustomFloat(() => segment2ArmSize.GetSize(), s => segment2ArmSize.SetSize(s), 0f, 2f, duration));
        StartCoroutine(Animations.Rotate(segment2, new Vector3(0f, 0f, 130.5f), duration));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);
    }

}