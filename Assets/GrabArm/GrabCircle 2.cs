using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabCircle2 : MonoBehaviour {

	InputAction mouseInput;
	SpriteRenderer spriteRenderer;

	Vector3 ogSize;

	bool mouseDown = false;
	bool prevMouseDown = false;

	Vector3 mouseOffset;

	[SerializeField]
	Color hoveredColor = Color.red;

	[SerializeField]
	Texture2D cursorPoint;

	bool hovering = false;
	bool prevHover = false;

	bully armScript;

	public int jointNum = -1;

	void Start() {
		mouseInput = InputSystem.actions.FindAction("Attack");
		spriteRenderer = GetComponent<SpriteRenderer>();
		ogSize = transform.localScale;

		mouseInput.started += OnMousePressed;
		mouseInput.canceled += OnMouseReleased;

        //armScript = GameObject.Find("Arm").transform.GetChild(1).GetComponent<bully>();

		Transform parent = transform.parent.parent;

		while (parent != null) {
			jointNum++;
			parent = parent.parent;
        }
    }

	void Update() {
		Vector2 mousePos = Mouse.current.position.ReadValue();
		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

		hovering = MouseOverCircle(mouseWorldPos);

		if (mouseInput.ReadValue<float>() == 0f && hovering && !prevHover) {
			StartCoroutine(Hover(true));
			print("hovering");
		} else if ((!hovering && !prevHover && !mouseDown && prevMouseDown) || (!mouseDown && !hovering && prevHover)) {
			StartCoroutine(Hover(false));
			print("stop hovering!");
		}

		if (mouseDown) {
			Transform arm = transform.parent.parent;
			Vector2 dir = Vector3.Normalize(mouseWorldPos - arm.position - mouseOffset);
			float angle = Mathf.Atan2(dir.y, dir.x) * 180f / Mathf.PI - 90f;
			arm.rotation = Quaternion.Euler(0f, 0f, angle);

			print("setting rot");

			if (armScript) {
				armScript.grabbedIndex = jointNum - 1;
			}
		}

		prevHover = hovering;
		prevMouseDown = mouseDown;
    }

	void OnMousePressed(InputAction.CallbackContext context) {
		Vector2 mousePos = Mouse.current.position.ReadValue();
		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
		if (MouseOverCircle(mouseWorldPos)) {
			mouseOffset = mouseWorldPos - transform.position;
			mouseDown = true;
		}
	}

	void OnMouseReleased(InputAction.CallbackContext context) {
		mouseDown = false;
    }

	bool MouseOverCircle(Vector2 mouseWorldPos) {
		return Vector2.Distance(mouseWorldPos, transform.position) < transform.localScale.x / 4f;
	}

	IEnumerator Hover(bool normal) {
		float elapsed = 0f;
		float time = 0.25f;

		Color currColor = spriteRenderer.color;
		Vector3 startScale = transform.localScale;

		Color toColor = normal ? hoveredColor : Color.white;
		Vector3 toSize = normal ? ogSize * 1.1f : ogSize;

		// Set Cursor
		if (normal)
			Cursor.SetCursor(cursorPoint, new Vector2(8, 10), CursorMode.Auto);
        else
            Cursor.SetCursor(null, Vector3.zero, CursorMode.Auto);

        while (elapsed < time) {
			elapsed += Time.deltaTime;
			float percent = elapsed / time;
			float logistic = 1f / (1f + 10 * Mathf.Exp(-10f * (percent - 0.5f)));

			spriteRenderer.color = Color.Lerp(currColor, toColor, logistic);
			transform.localScale = Vector3.Lerp(startScale, toSize, logistic);
			yield return null;
		}

        spriteRenderer.color = toColor;
        transform.localScale = toSize;
    }
}