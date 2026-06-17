
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class LiveGraph : MonoBehaviour {

	[SerializeField]
    int maxPoints = 200;

    [SerializeField]
    float xSpacing = 0.1f;

    LineRenderer line;

	List<float> errorList;

	void Awake() {
		errorList = new List<float>(maxPoints);
        line = GetComponent<LineRenderer>();
    }

	void DrawGraph() {
        if (line)
            line.positionCount = errorList.Count;

        RectTransform rect = GetComponent<RectTransform>();

        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

        Vector3 imgStartLoc = Camera.main.ScreenToWorldPoint(corners[0]);
        Vector3 imgEndLoc = Camera.main.ScreenToWorldPoint(corners[2]);

        xSpacing = (imgEndLoc.x - imgStartLoc.x) / maxPoints;
        //float yMax = imgEndLoc.y - imgStartLoc.y;

        for (int i = 0; i < errorList.Count; i++) {
            float x = i * xSpacing;
            float y = errorList[i] / 2f;

            if (line)
                line.SetPosition(i, imgStartLoc + new Vector3(x, y, 10));
        }
    }

	public void Add(float2 error) {
        if (errorList == null)
            return;

        errorList.Add(new Vector2(error.x, error.y).magnitude);

        if (errorList.Count > maxPoints)
            errorList.RemoveAt(0); // scroll left

        DrawGraph();
    }
}