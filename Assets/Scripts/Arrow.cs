using System.Collections;
using TMPro;
using UnityEngine;

enum DistanceLoc {
    NONE,
    TOP,
    BOTTOM,
	LEFT,
	RIGHT,
};

public class Arrow : MonoBehaviour {

	public Color color;

	Vector2 start = Vector2.zero;
	[SerializeField]
	Vector2 end = Vector2.zero;

	[SerializeField]
	float duration = 1f;

	[SerializeField]
	bool doubleSided = false;

	[SerializeField]
	[Range(-180f, 180f)]
	float arcDegrees = 0f;
	[SerializeField]
	int segments = 10;

	[Header("Arrow Head")]
	[SerializeField]
	bool resizeHead = false;
	[SerializeField]
	[Range(0f, 1f)]
	float resizeFinishPercent = 0.1f;

	[Header("Debug")]
	[SerializeField]
	[Range(0.0001f, 1f)]
	public float percent = 1f;

	bool animFinished = false;
	bool animGoing = false;

	float lineThickness = 0.1f;

	[SerializeField]
	DistanceLoc distanceLoc;
	[SerializeField]
	bool textUpright = false;
    [SerializeField]
	float textOffset = 0.25f;
	[SerializeField]
	[Range(0f, 2f)]
	int decimalCount = 2;

    LineRenderer lineRenderer;
	Transform arrowHead;
	TMP_Text distanceText;

    private void OnValidate() {
		lineRenderer = GetComponent<LineRenderer>();
        arrowHead = transform.GetChild(0);
        distanceText = transform.GetChild(1).GetComponent<TMP_Text>();

        if (lineRenderer) {
			lineRenderer.startColor = color;
			lineRenderer.endColor = color;

			DrawArc();
        }

        if (arrowHead) {
			arrowHead.GetComponent<LineRenderer>().startColor = color;
			arrowHead.GetComponent<LineRenderer>().endColor = color;
		}
    }

    void Start() {
		lineRenderer = GetComponent<LineRenderer>();
		arrowHead = transform.GetChild(0);
        distanceText = transform.GetChild(1).GetComponent<TMP_Text>();

        Restart();
	}

	void Update() {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        if (arrowHead) {
            arrowHead.GetComponent<LineRenderer>().startColor = color;
            arrowHead.GetComponent<LineRenderer>().endColor = color;
        }

        if (percent <= 0.0001f) {
			lineRenderer.startWidth = 0f;
			arrowHead.GetComponent<LineRenderer>().startWidth = 0f;
        } else {
            lineRenderer.startWidth = lineThickness;
            arrowHead.GetComponent<LineRenderer>().startWidth = lineThickness;
        }

        if (!animGoing) {
			// just set arrow to stats, so easy debugging
			//percent = animFinished ? 1f : 0.0001f;
            DrawArc();
		}
    }

	public void Restart() {
		animFinished = false;
		percent = 0.0001f;
    }

	//[ContextMenu("Animate Line")]
	public void StartLineAnim() {
		StartCoroutine(LineAnimation(duration));
	}

	IEnumerator LineAnimation(float duration) {
		animGoing = true;

        float elapsed = 0f;
		while (elapsed < duration) {
			elapsed += Time.deltaTime;
			float percent = elapsed / duration;
			float logistical = Math.Logistical(percent);

			this.percent = logistical;
			DrawArc();

			yield return null;
		}

		percent = 1f;
		DrawArc();

        animGoing = false;
		animFinished = true;

    }
	void DrawArc() {

		Vector2 dir = end - start;
		float dist = dir.magnitude;

		if (dist < 0.0001f)
			return;

		Vector2 dirN = dir.normalized;

		// perpendicular direction (left side)
		Vector2 perp = new Vector2(-dirN.y, dirN.x);

		// split angle into magnitude + sign
		float angleRad = Mathf.Abs(arcDegrees) * Mathf.Deg2Rad;
		float side = Mathf.Sign(arcDegrees);

        lineRenderer.positionCount = segments + 1;

        // straight line fallback
        if (Mathf.Approximately(arcDegrees, 0f)) {
			for (int i = 0; i <= segments; i++) {
				float t = i / (float)segments;
				lineRenderer.SetPosition(i, Vector2.Lerp(start, end, t * percent));
			}

            UpdateArrowHead();
            return;
		}

		// radius from chord + angle
		float radius = dist / (2f * Mathf.Sin(angleRad / 2f));

		// avoid invalid geometry
		radius = Mathf.Abs(radius);

		Vector2 mid = (start + end) * 0.5f;

		float height = Mathf.Sqrt(Mathf.Max(0f, radius * radius - (dist * dist) / 4f));

		Vector2 center = mid + perp * side * height;

		float startAngle = Mathf.Atan2(start.y - center.y, start.x - center.x);

		float totalAngle = angleRad * side;

		for (int i = 0; i <= segments; i++) {
			float t = i / (float)segments;

			float a = startAngle + totalAngle * t * percent;

			Vector2 point = center + new Vector2(
				Mathf.Cos(a),
				Mathf.Sin(a)
			) * radius;

			lineRenderer.SetPosition(i, point);
		}

		UpdateArrowHead();
    }

	void UpdateArrowHead() {
        Vector3 lastPos = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
        Vector3 secondToLastPos = lineRenderer.GetPosition(lineRenderer.positionCount - 2);
        Vector3 arrowHeadDir = lastPos - secondToLastPos;
        float angle = Mathf.Atan2(arrowHeadDir.y, arrowHeadDir.x);
        arrowHead.localEulerAngles = new Vector3(0f, 0f, angle * Mathf.Rad2Deg);

        arrowHead.localPosition = lastPos;

		if (distanceLoc != DistanceLoc.NONE) {
            distanceText.enabled = true;

			if (decimalCount == 0)
				distanceText.SetText($"{Vector2.Distance(start, lastPos):0}");
			if (decimalCount == 1)
                distanceText.SetText($"{Vector2.Distance(start, lastPos):0.0}");
			if (decimalCount == 2)
                distanceText.SetText($"{Vector2.Distance(start, lastPos):0.00}");

            Vector3 dir = lastPos - (Vector3)start;
            float textAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 180f;

            if (distanceLoc == DistanceLoc.TOP) {
				distanceText.transform.localEulerAngles = new Vector3(0f, 0f, textAngle);
				distanceText.transform.localPosition = (start + (Vector2)lastPos) / 2f + new Vector2(0f, textOffset);
			} else if (distanceLoc == DistanceLoc.BOTTOM) {
                distanceText.transform.localPosition = (start + (Vector2)lastPos) / 2f + new Vector2(0f, -textOffset);
				distanceText.transform.localEulerAngles = new Vector3(0f, 0f, textAngle);
            } else if (distanceLoc == DistanceLoc.LEFT) {
                distanceText.transform.localPosition = (start + (Vector2)lastPos) / 2f + new Vector2(-textOffset, 0f);
				distanceText.transform.localEulerAngles = new Vector3(0f, 0f, textAngle + 90);
            } else if (distanceLoc == DistanceLoc.RIGHT) {
                distanceText.transform.localPosition = (start + (Vector2)lastPos) / 2f + new Vector2(textOffset, 0f);
				distanceText.transform.localEulerAngles = new Vector3(0f, 0f, textAngle + 90);
            }

			if (textUpright)
                distanceText.transform.eulerAngles = new Vector3(0f, 0f, 0f);

		} else {
			distanceText.enabled = false;
		}

        if (resizeHead) {
            float headScale = Mathf.Lerp(0.0f, 1f, percent / resizeFinishPercent);
            arrowHead.localScale = new Vector3(headScale, headScale, headScale);

            Color textColor = color;
            textColor.a = headScale;
            distanceText.color = textColor;

            if (headScale == 1f) // do this so I can animate opacity/text color
                distanceText.color = color;
        }
    }
}