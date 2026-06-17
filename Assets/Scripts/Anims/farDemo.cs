using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class farDemo : MonoBehaviour {
	
	public float duration = 1f;
	public Vector3 endCamLoc;
	public Vector3 endCamRot;

	public Vector3 secondCamLoc;
	public Vector3 secondCamRot;

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
		StartCoroutine(Animations.MoveTo(Camera.main.transform, endCamLoc, duration));
		StartCoroutine(Animations.Rotate(Camera.main.transform, endCamRot, duration));
		yield return new WaitForSeconds(duration);
		
		yield return new WaitForSeconds(1f);

		StartCoroutine(Animations.MoveTo(Camera.main.transform, secondCamLoc, duration * 2.5f));
		StartCoroutine(Animations.Rotate(Camera.main.transform, secondCamRot, duration * 2.5f));
		yield return new WaitForSeconds(duration);
	}
}