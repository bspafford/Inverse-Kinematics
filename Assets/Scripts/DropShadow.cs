using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DropShadow : MonoBehaviour {

	[SerializeField]
	Vector3 offset = new Vector3(0.333f, -0.333f, 1f);
	Material dropShadowMat;

	List<GameObject> objects = new List<GameObject>();

	[SerializeField]
	List<GameObject> excludeList = new List<GameObject>();
	HashSet<GameObject> excludes = new HashSet<GameObject>();

	int childCount = 0;
	int prevChildCount = 0;

	void Start() {
		excludes = new HashSet<GameObject>(excludeList);

		dropShadowMat = Resources.Load<Material>("dropShadow");

		RecalcList();
    }

	void LateUpdate() {
		int index = 0;

        if (childCount != prevChildCount) {
            RecalcList();
            //UpdateAllChildren(gameObject, ref index);
        }

        prevChildCount = childCount;

		UpdateAllChildren(gameObject, ref index);
	}

	public void RecalcList() {
        foreach (GameObject obj in objects)
            Destroy(obj);
        objects.Clear();

        GetAllChildren(gameObject, ref objects);

        for (int i = 0; i < objects.Count; i++) {
            if (!objects[i].activeInHierarchy || excludes.Contains(objects[i])) continue;
            
            if (objects[i].GetComponent<SpriteRenderer>()) {
                Vector3 scale = objects[i].transform.lossyScale;
                objects[i] = Instantiate(objects[i]);
                objects[i].transform.localScale = scale;
                objects[i].GetComponent<SpriteRenderer>().material = dropShadowMat;
                foreach (Transform child in objects[i].transform) {
                    Destroy(child.gameObject);
                }
                Destroy(objects[i].GetComponent<GrabCircle>());
                Destroy(objects[i].GetComponent<GrabCircle2>());
                Destroy(objects[i].GetComponent<BoxCollider2D>());
                Destroy(objects[i].GetComponent<DropShadow>());
                Destroy(objects[i].GetComponent<targetTrackhand>());
                Destroy(objects[i].GetComponent<grabObject2D>());
                Destroy(objects[i].GetComponent<GrabObject>());
                Destroy(objects[i].GetComponent<Rigidbody2D>());
                Destroy(objects[i].GetComponent<CircleCollider2D>());

            } else if (objects[i].GetComponent<LineRenderer>()) {
                Vector3 scale = objects[i].transform.lossyScale;
                objects[i] = Instantiate(objects[i]);
                objects[i].transform.localScale = scale;
                objects[i].GetComponent<LineRenderer>().startColor = dropShadowMat.color;
                objects[i].GetComponent<LineRenderer>().endColor = dropShadowMat.color;
                foreach (Transform child in objects[i].transform) {
                    Destroy(child.gameObject);
                }
                Destroy(objects[i].GetComponent<DropShadow>());
                Destroy(objects[i].GetComponent<Arrow>());
                Destroy(objects[i].GetComponent<DashedLine>());
                Destroy(objects[i].GetComponent<Angle>());
            } else if (objects[i].GetComponent<TMP_Text>()) {
                Vector3 scale = objects[i].transform.lossyScale;
                objects[i] = Instantiate(objects[i]);
                objects[i].transform.localScale = scale;
                objects[i].GetComponent<TMP_Text>().color = dropShadowMat.color;
                foreach (Transform child in objects[i].transform) {
                    Destroy(child.gameObject);
                }
                Destroy(objects[i].GetComponent<DropShadow>());
            } else {
                objects[i] = null;
            }
        }
    }

	void GetAllChildren(GameObject parent, ref List<GameObject> objects) {
		objects.Add(parent);
		foreach (Transform child in parent.transform) {
			GetAllChildren(child.gameObject, ref objects);
		}
	}

	void UpdateAllChildren(GameObject parent, ref int index) {
		if (index <= objects.Count - 1 && objects[index]) {
			objects[index].transform.position = parent.transform.position + offset;
			objects[index].transform.rotation = parent.transform.rotation;
			objects[index].transform.localScale = parent.transform.lossyScale;

			LineRenderer lineRenderer = objects[index].GetComponent<LineRenderer>();
            if (lineRenderer) {
				LineRenderer parentLineRenderer = parent.GetComponent<LineRenderer>();
				CopyLineRenderers(parentLineRenderer, lineRenderer);
            }
            TMP_Text text = objects[index].GetComponent<TMP_Text>();
            TMP_Text parentText = parent.GetComponent<TMP_Text>();
            if (text && parentText) {
                text.SetText(parentText.text);
                Color color = dropShadowMat.color;
                text.color = new Color(color.r, color.g, color.b, parentText.color.a);
            }
		}
        ++index;
        foreach (Transform child in parent.transform) {
            UpdateAllChildren(child.gameObject, ref index);
        }

		childCount = index + 1;
    }

	void CopyLineRenderers(LineRenderer source, LineRenderer target) {
        if (!source)
            return;

        target.startWidth = source.startWidth;
        target.endWidth = source.endWidth;
        target.widthMultiplier = source.widthMultiplier;

		//Color color = dropShadowMat.GetColor("_Color");
		//color.a = source.startColor.a;
        //target.startColor = color;
        //target.endColor = color;

        target.material = dropShadowMat;
		Color color = dropShadowMat.color;
		color.a = source.startColor.a;
		target.material.SetColor("_Color", color);
        target.alignment = source.alignment;
        target.textureMode = source.textureMode;
        target.numCapVertices = source.numCapVertices;
        target.numCornerVertices = source.numCornerVertices;

        target.shadowCastingMode = source.shadowCastingMode;
        target.receiveShadows = source.receiveShadows;

        target.loop = source.loop;
        target.useWorldSpace = source.useWorldSpace;

		target.positionCount = source.positionCount;
		for (int i = 0; i < source.positionCount; ++i) {
			target.SetPosition(i, source.GetPosition(i));
		}
    }

    public void Remove() {
        foreach (GameObject obj in objects)
            Destroy(obj);
        objects.Clear();

        this.enabled = false;
    }
}