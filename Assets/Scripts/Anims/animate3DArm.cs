using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.HableCurve;

public class animate3DArm : MonoBehaviour {

    public float duration;
    [Range(0f, 1f)]
    public float t = 0.5f;
    public List<Transform> segments;

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

    private void Start() {
        foreach (Transform segment in segments) {
            segment.localScale = Vector3.zero;
        }
    }

    IEnumerator Anim() {
        float totalDuration = duration;

        float[] weights = new float[segments.Count];
        float totalWeight = 0f;

        for (int i = 0; i < segments.Count; i++) {
            float p0 = (float)i / segments.Count;
            float p1 = (float)(i + 1) / segments.Count;

            float weight = 1f / Mathf.Max(
                Math.Logistical(p1) - Math.Logistical(p0),
                0.0001f
            );

            weights[i] = weight;
            totalWeight += weight;
        }

        for (int i = 0; i < segments.Count; i++) {
            float duration = totalDuration * (weights[i] / totalWeight);
            float adjusted = Mathf.Lerp(totalDuration / segments.Count, duration, t); // between linear and logistical

            yield return StartCoroutine(
                Animations.Scale(
                    segments[i],
                    Vector3.one,
                    adjusted
                )
            );
        }
    }
}