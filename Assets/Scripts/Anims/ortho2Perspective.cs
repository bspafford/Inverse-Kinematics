using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

public class ortho2Perspective : MonoBehaviour {
    public float farSize = 1f;
    public float closeSize = 60f;

    public Vector3 farPos;
    public Vector3 closePos;

    InputAction startAction;

    public float k = 10f;
    public float duration = 1f;

    bool animGoing = true;

    [Range(0f, 1f)]
    public float percent = 0f;

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
        //Vector3 pos = Vector3.Lerp(farPos, closePos, percent);
        //float size = Mathf.Lerp(farSize, closeSize, percent);
        //Camera.main.transform.position = pos;
        //Camera.main.fieldOfView = size;
        if (animGoing) {
            float distance = Vector3.Distance(Camera.main.transform.position, Vector3.zero);
            float fov = 2f * Mathf.Atan(Camera.main.orthographicSize / distance) * Mathf.Rad2Deg;
            Camera.main.transform.position = Vector3.Lerp(farPos, closePos, percent);
            Camera.main.fieldOfView = fov;
        }
    }

    IEnumerator Anim() {

        StartCoroutine(Animations.CustomFloat(() => percent, f => percent = f, 0f, 1f, duration, k));
        //StartCoroutine(Animations.CustomFloat(() => Camera.main.fieldOfView, f => Camera.main.fieldOfView = f, farSize, closeSize, duration));
        //StartCoroutine(Animations.CustomFloat(() => Camera.main.transform.position.z, f => Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, f), farPos.z, closePos.z, duration));
        yield return new WaitForSeconds(duration);

        animGoing = false;

        yield return new WaitForSeconds(1f);

        StartCoroutine(Animations.CustomFloat(() => Camera.main.fieldOfView, f => Camera.main.fieldOfView = f, 68.97565f, 60f, duration));

        yield return new WaitForSeconds(1f);

    }
}