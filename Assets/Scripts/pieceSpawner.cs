using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class pieceSpawner : MonoBehaviour {

	public List<Transform> pieces;

    InputAction startAction;

    public float duration = 1f;
    public float spawnIntervals = 0.1f;

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
        float spawnNum = duration / spawnIntervals;
        for (int i = 0; i < spawnNum; ++i) {
            int randPiece = Random.Range(0, pieces.Count);
            Transform spawnedPiece = Instantiate(pieces[randPiece]);
            spawnedPiece.position = transform.position;
            spawnedPiece.eulerAngles = new Vector3(Random.Range(-180f, 180f), Random.Range(-180f, 180f), Random.Range(-180f, 180f));

            yield return new WaitForSeconds(spawnIntervals);
        }

        yield return null;
    }
}