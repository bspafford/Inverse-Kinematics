using UnityEngine;
using UnityEngine.InputSystem;

public class followCursor : MonoBehaviour {

	public Transform target;
	public float distance = 0f;

	void Start() {
		
	}

	void Update() {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, distance));

		target.position = mouseWorldPos;
    }
}