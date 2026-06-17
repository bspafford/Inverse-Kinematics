using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Angle : MonoBehaviour {

	LineRenderer lineRenderer;
	LineRenderer circleLineRenderer;

    public float startAngle = 0f;
    public float endAngle = 90f;

    [SerializeField]
    float edgeLength = 1f;

    [SerializeField]
    float thickness = 0.1f;
    public Color color = Color.white;

    [SerializeField]
    float radius = 0.5f;
    [SerializeField]
    int segmentCount = 10;

    [SerializeField]
    bool showAngleText;
    [SerializeField]
    float fadeAngle = 10f;

    TMP_Text angleText;

    float prevStartAngle;
    float prevEndAngle;

    [SerializeField]
    float textDist = 0.3f;

    private void OnValidate() {
        UpdateAngle();
    }

    void Update() {
        UpdateAngle();
    }

    void UpdateAngle() {
        angleText = transform.GetChild(1).GetComponent<TMP_Text>();
        lineRenderer = GetComponent<LineRenderer>();
        circleLineRenderer = transform.GetChild(0).GetComponent<LineRenderer>();

        lineRenderer.SetPosition(0, new Vector3(Mathf.Cos(startAngle * Mathf.Deg2Rad), Mathf.Sin(startAngle * Mathf.Deg2Rad)) * edgeLength);
        lineRenderer.SetPosition(1, Vector3.zero);
        lineRenderer.SetPosition(2, new Vector3(Mathf.Cos(endAngle * Mathf.Deg2Rad), Mathf.Sin(endAngle * Mathf.Deg2Rad)) * edgeLength);

        circleLineRenderer.positionCount = segmentCount;
        for (int i = 0; i < segmentCount; i++) {
            float t = i / (float)(segmentCount - 1);
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            Vector3 point = AngleToPoint(angle, radius);
            circleLineRenderer.SetPosition(i, point);
        }

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        circleLineRenderer.startColor = color;
        circleLineRenderer.endColor = color;
        lineRenderer.startWidth = thickness;
        lineRenderer.endWidth = thickness;
        circleLineRenderer.startWidth = thickness;
        circleLineRenderer.endWidth = thickness;

        float angleDist = Mathf.Abs(endAngle - startAngle);
        if (showAngleText) {
            angleText.enabled = true;
            angleText.SetText($"{angleDist:0}");

            float avgAngle = (startAngle + endAngle) / 2f;
            angleText.transform.localPosition = new Vector3(Mathf.Cos(avgAngle * Mathf.Deg2Rad), Mathf.Sin(avgAngle * Mathf.Deg2Rad), 0f) * (radius + textDist);

            if (prevStartAngle != startAngle || prevEndAngle != endAngle) {
                if (fadeAngle != 0)
                    color.a = angleDist / fadeAngle;
            }
            angleText.color = color;
        } else {
            angleText.enabled = false;
        }

        prevStartAngle = startAngle;
        prevEndAngle = endAngle;
    }

    Vector3 AngleToPoint(float angleDeg, float radius) {
        float rad = angleDeg * Mathf.Deg2Rad;

        return new Vector3(
            Mathf.Cos(rad) * radius,
            Mathf.Sin(rad) * radius,
            0f
        );
    }
}