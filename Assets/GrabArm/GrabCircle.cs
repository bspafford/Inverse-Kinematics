using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class GrabCircle : MonoBehaviour {

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
	[SerializeField]
	Texture2D cursorGrab;

	[SerializeField]
	Transform armSegment;

	bool hovering = false;
	bool prevHover = false;

	threeJoint armScript;

    Bully2 bullyScript;

    public int jointNum = -1;

	public bool audio = true;

	// Audio
	AudioSource audioSource;
	float stopDelay = 0.1f;
	float stopTimer = 0f;
	bool shouldContinue = false; // if should of played audio while it was counting down, continue next time
	float avgDir = 0f;
	int count = 0;

	void Start() {
		audioSource = GameObject.Find("Arm").GetComponent<AudioSource>();

		mouseInput = InputSystem.actions.FindAction("Attack");
		spriteRenderer = GetComponent<SpriteRenderer>();
		ogSize = transform.localScale;

		mouseInput.started += OnMousePressed;
		mouseInput.canceled += OnMouseReleased;

		armScript = GameObject.Find("Arm").transform.GetChild(1).GetComponent<threeJoint>();
		bullyScript = GameObject.Find("Arm").transform.GetChild(1).GetComponent<Bully2>();

		Transform parent = transform.parent.parent;

		while (parent != null) {
			//jointNum++;
			parent = parent.parent;
		}
	}

	void Update() {
		Vector2 mousePos = Mouse.current.position.ReadValue();
		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

		hovering = MouseOverCircle(mouseWorldPos);

		if (mouseInput.ReadValue<float>() == 0f && hovering && !prevHover) {
			StartCoroutine(Hover(true));
		} else if ((!hovering && !prevHover && !mouseDown && prevMouseDown) || (!mouseDown && !hovering && prevHover)) {
			StartCoroutine(Hover(false));
		}

		if (mouseDown) {
			/*
			float prevRot = armSegment.eulerAngles.z;

			Vector2 dir = Vector3.Normalize(mouseWorldPos - armSegment.position - mouseOffset);
			float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
			armSegment.rotation = Quaternion.Euler(0f, 0f, angle);

			if (angle < 0f) angle += 360f;
			bool sameSpot = Mathf.Approximately(prevRot, angle);
			//print(prevRot + " == " + (angle) + " = " + sameSpot);
			if (!sameSpot)
				shouldContinue = true;

			if ((prevRot > 350f && angle < 10f) || (prevRot < 10f && angle > 350f)) {
				// skip
			} else {
				float dist = Mathf.Abs(prevRot - angle) / Time.deltaTime;
				// 0 -> 0.002?
				avgDir += dist;
				++count;
			}

			stopTimer += Time.deltaTime;
			if (stopTimer >= stopDelay) {
				stopTimer = 0f;

				float percent = avgDir / count / 300f;
				if (audio) {
					audioSource.pitch = Mathf.Lerp(0.7f, 0.8f, percent);
					audioSource.volume = Mathf.Lerp(0f, 1f, percent);
					//print(avgDir / count);

					if (!audioSource.isPlaying && shouldContinue) {
						audioSource.Play();
					} else if (!shouldContinue) {
						audioSource.Stop();
					}
				}

				shouldContinue = false;
				avgDir = 0f;
				count = 0;
			}
			*/

			/*
			if (armScript) {
				float rot = armSegment.eulerAngles.z * Mathf.Deg2Rad;
				switch (jointNum) {
					case 1:
						armScript.t1 = rot;
						break;
					case 2:
                        armScript.t2 = rot;

						Vector2 mPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - GameObject.Find("Arm").transform.position;
						Vector2 desired = new Vector3(3, 2, 0);
						Vector2 d = (desired - mPos).normalized;
						float segmentLength = 2;
                        armScript.desiredLoc = d * segmentLength + mPos;

						break;
					case 3:
						//armScript.t3 = rot;
						armScript.desiredLoc = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - GameObject.Find("Arm").transform.position;
						break;
				}
			}
			*/

			if (bullyScript) {
				bullyScript.jointNum = jointNum;
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

			Cursor.SetCursor(cursorGrab, new Vector2(8, 10), CursorMode.Auto);
		}
	}

	void OnMouseReleased(InputAction.CallbackContext context) {
		mouseDown = false;
		audioSource.Stop();

		if (bullyScript && bullyScript.jointNum == jointNum) {
            bullyScript.jointNum = -1;
        }

		if (!hovering)
			return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
		print(mouseWorldPos + ", " + transform.position + ", " + Vector2.Distance(mouseWorldPos, transform.position) + " = " + MouseOverCircle(mouseWorldPos));
		if (MouseOverCircle(mouseWorldPos))
			Cursor.SetCursor(cursorPoint, new Vector2(8, 10), CursorMode.Auto);
		else {
			Cursor.SetCursor(null, Vector3.zero, CursorMode.Auto);
		}

		if (armScript) {
			armScript.desiredLoc = new Vector3(3, 2, 0);
		}
    }

	bool MouseOverCircle(Vector2 mouseWorldPos) {
		return Vector2.Distance(mouseWorldPos, transform.position) < 0.5f;// transform.localScale.x / 4f;
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