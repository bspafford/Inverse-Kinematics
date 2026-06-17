using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class errorDefinedAs : MonoBehaviour {

    public float duration = 1f;
    public Arrow arrow;
    public TMP_Text text;
    public Transform endEffector;

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
        StartCoroutine(Animations.MoveTo(text.transform, new Vector3(0f, 2.81f, 0f), duration));
        yield return new WaitForSeconds(duration);
        
        yield return new WaitForSeconds(1f);

        arrow.StartLineAnim();
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(1f);

        StartCoroutine(Animations.MoveTo(transform, new Vector3(3, -1, -0.3f), duration));
        StartCoroutine(Animations.MoveTo(arrow.transform, new Vector3(3, -1, -0.4f), duration));
		StartCoroutine(Animations.CustomFloat(() => arrow.percent, c => arrow.percent = c, 1f, 0f, duration));
        yield return new WaitForSeconds(duration);
    }

    void Update() {
        float dist = Vector2.Distance(new Vector2(3f, -1f), new Vector2(endEffector.position.x, endEffector.position.y));
        text.SetText($"e = {dist:F2}");
    }
}