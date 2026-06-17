using UnityEngine;
using TMPro;

public class EndEffectorTextScript : MonoBehaviour {
	public Transform endEffector;
	TMP_Text text;

	void Start() {
		text = GetComponent<TMP_Text>();
	}

	void Update() {
		text.SetText($"({endEffector.position.x:F2}, {endEffector.position.y:F2})");
    }
}