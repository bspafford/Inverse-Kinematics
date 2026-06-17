using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TMPro;

public class DashedLine : MonoBehaviour {

    [SerializeField]
    GameObject dashedSegment;

    Vector2 start = Vector2.zero;
	public Vector2 end = new Vector2(1, 0);

    [SerializeField]
    float thickness = 0.1f;
    [SerializeField]
    float dashedLength = 0.2f;
    [SerializeField]
    float dashSpacing = 0.1f;

    public Color color = Color.white;

    [SerializeField]
    DistanceLoc distanceLoc;
    [SerializeField]
    bool textUpright = false;
    [SerializeField]
    float fadeTextDist = 1f; // how long line has to be for text to be fully visible

    TMP_Text distanceText;

    List<LineRenderer> lineSegments = new List<LineRenderer>();

    /*
    private void OnValidate() {
#if UNITY_EDITOR
        EditorApplication.delayCall += DelayedUpdate;
#endif
    }

#if UNITY_EDITOR
    void DelayedUpdate() {
        // object may have been deleted
        if (this == null) return;

        UpdateLine();
    }
#endif
    */

    void Start() {
        distanceText = transform.GetChild(0).GetComponent<TMP_Text>();
    }

    void Update() {
        UpdateLine();
    }

    void UpdateLine() {
        // destroy all old instances
        foreach (LineRenderer segment in lineSegments)
            DestroyImmediate(segment.gameObject);
        lineSegments.Clear();

        // calc line distance
        float dist = Vector2.Distance(start, end);
        float segmentNum = dist / (dashedLength + dashSpacing);

        lineSegments.Capacity = Mathf.CeilToInt(segmentNum);
        for (int i = 0; i < segmentNum; ++i) {
            GameObject spawnedSegment = Instantiate(dashedSegment, transform);

            LineRenderer line = spawnedSegment.GetComponent<LineRenderer>();
            InitLine(line, i);
            lineSegments.Add(line);
        }

        if (lineSegments.Count > 0) {
            Vector2 pos = lineSegments[lineSegments.Count - 1].transform.localPosition;

            Vector2 dir1 = (end - start).normalized * dashedLength;
            Vector2 toEnd = end - pos;

            lineSegments[lineSegments.Count - 1].SetPosition(1, dir1.magnitude < toEnd.magnitude ? dir1 : toEnd);
        }

        if (distanceLoc != DistanceLoc.NONE) {
            distanceText.enabled = true;
            distanceText.SetText($"{Vector2.Distance(start, end):0.00}");

            if (distanceLoc == DistanceLoc.TOP) {
                distanceText.transform.localPosition = (Vector3)(start + end) / 2f + new Vector3(0f, 0.25f, -1f);
            } else if (distanceLoc == DistanceLoc.BOTTOM) {
                distanceText.transform.localPosition = (Vector3)(start + end) / 2f + new Vector3(0f, -0.25f, -1f);
            }

            Vector3 dir = end - start;
            float textAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 180f;
            distanceText.transform.localEulerAngles = new Vector3(0f, 0f, textAngle);

            if (textUpright)
                distanceText.transform.eulerAngles = new Vector3(0f, 0f, 0f);

            Color distanceTextColor = color;
            if (fadeTextDist != 0)
                color.a = dist / fadeTextDist;
            distanceText.color = distanceTextColor;
        } else {
            distanceText.enabled = false;
        }
    }

    void InitLine(LineRenderer line, int index) {
        line.startWidth = thickness;
        line.endWidth = thickness;

        line.startColor = color;
        line.endColor = color;

        line.SetPosition(1, new Vector2(dashedLength, 0f));

        line.transform.localPosition = new Vector2((dashedLength + dashSpacing) * index, 0f);
    }
}