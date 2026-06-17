using Unity.Burst.Intrinsics;
using UnityEngine;

public class ClawScript : MonoBehaviour {

    Transform leftClaw;
    Transform rightClaw;

    [SerializeField]
    [Range(0f, 1f)]
    float percent = 0f;
    
    void Start() {
        Transform arm3 = transform.GetChild(0).GetChild(0);
        rightClaw = arm3.GetChild(0);
        leftClaw = arm3.GetChild(1);
    }

	void Update() {
        float maxOpened = 60f;
        float maxCloseed = 25f;
        float rightAngle = Mathf.Lerp(maxCloseed, maxOpened, percent);
        float leftAngle = -rightAngle;

        rightClaw.localEulerAngles = new Vector3(0f, 0f, rightAngle);
        leftClaw.localEulerAngles = new Vector3(0f, 0f, leftAngle);
    }
}