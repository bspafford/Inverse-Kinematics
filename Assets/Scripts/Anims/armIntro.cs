using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class armIntro : MonoBehaviour {

    public Transform tipJoint;
    public Vector3 tipGoToJoint;

    public Transform otherJoint;
    public Vector3 otherJointPos;

    public Transform segment1;
    public Vector3 segment1Pos;
    public Vector3 segment1Rot;

    public Transform segment2;
    public Vector3 segment2Pos;
    public Vector3 segment2Rot;

    public Transform target;
    public Vector3 targetEndPos;
    public Vector3 secondEndPos;
    public float arcHeight;

    public SpriteRenderer segment1SpriteRenderer;
    public Sprite segment11;
    public Sprite segment12;

    public SpriteRenderer segment2SpriteRenderer;
    public Sprite segment21;
    public Sprite segment22;

    public SpriteRenderer joint1SpriteRenderer;
    public Sprite joint11;
    public Sprite joint12;

    public SpriteRenderer joint2SpriteRenderer;
    public Sprite joint21;
    public Sprite joint22;

    public SpriteRenderer baseSpriteRenderer;
    public Sprite base1;
    public Sprite base2;

    public SpriteRenderer targetSpriteRenderer;
    public Sprite target1;
    public Sprite target2;

    public float paperDuration = 0.25f;

    bool animFinished = false;

    float duration = 1f;
    
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
        StartCoroutine(Anim());
    }

    void Start() {
        StartCoroutine(PaperAnim());

        target.localScale = Vector3.zero;
    }

    IEnumerator PaperAnim() {
        segment1SpriteRenderer.sprite = segment11;
        segment2SpriteRenderer.sprite = segment21;
        joint1SpriteRenderer.sprite = joint11;
        joint2SpriteRenderer.sprite = joint12;
        baseSpriteRenderer.sprite = base1;
        targetSpriteRenderer.sprite = target1;

        yield return new WaitForSeconds(paperDuration);

        segment1SpriteRenderer.sprite = segment12;
        segment2SpriteRenderer.sprite = segment22;
        joint1SpriteRenderer.sprite = joint12;
        joint2SpriteRenderer.sprite = joint22;
        baseSpriteRenderer.sprite = base2;
        targetSpriteRenderer.sprite = target2;
        yield return new WaitForSeconds(paperDuration);

        StartCoroutine(PaperAnim());
    }

    void Update() {
        if (animFinished) {
            Vector2 screenPosition = Mouse.current.position.ReadValue();
            target.position = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10 - 0.3f));
        }
        segment2.GetComponent<InverseKinematics>().targetLoc = target.position - transform.position;
    }

    IEnumerator Anim() {
        StartCoroutine(Animations.Scale(target, Vector3.one * 0.25f, duration));
        yield return new WaitForSeconds(duration);
        
        yield return new WaitForSeconds(3f);

        StartCoroutine(Animations.MoveTo(tipJoint, tipGoToJoint, duration));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(3f);

        StartCoroutine(Animations.MoveTo(otherJoint, otherJointPos, duration));
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(3f);

        // StartCoroutine(Animations.MoveTo(segment1, segment1Pos, duration));
        StartCoroutine(Animations.Rotate(segment1, segment1Rot, duration));

        // StartCoroutine(Animations.MoveTo(segment2, segment2Pos, duration));
        StartCoroutine(Animations.Rotate(segment2, segment2Rot, duration));
        
        StartCoroutine(Animations.MoveTo(otherJoint, otherJointPos, duration));
        StartCoroutine(Animations.MoveTo(tipJoint, tipGoToJoint, duration));
        
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(3f);

        segment2.GetComponent<InverseKinematics>().enabled = true;
        //StartCoroutine(MoveInArc(targetEndPos, duration, arcHeight));
        Vector3 mousePos = Camera.main.WorldToScreenPoint(target.position);
        Mouse.current.WarpCursorPosition(mousePos);
        animFinished = true;
        //yield return new WaitForSeconds(duration);
        //yield return new WaitForSeconds(1f);
        //StartCoroutine(MoveInArc(secondEndPos, duration, arcHeight));
    }

    public IEnumerator MoveInArc(Vector3 endPosition, float duration, float arcHeight) {
        Vector3 startPosition = target.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = Math.Logistical(t);

            // Linear movement between start and end
            Vector3 pos = Vector3.Lerp(startPosition, endPosition, t);

            // Arc offset (peaks at t = 0.5)
            pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;

            target.position = pos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.position = endPosition;
    }
}