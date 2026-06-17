using UnityEngine;
using System.Collections.Generic;

public class ArmSize2 : MonoBehaviour {

    public float size = 2f;

    public Transform joint;
    public Transform segment;

    private void OnValidate() {
        Update();
    }

    private void Update() {
        Transform arm = transform;

        segment.localScale = new Vector3(segment.localScale.x, size, segment.localScale.z);

        joint.localPosition = new Vector3(0f, size, -0.1f);
    }

    public void SetSize(float size) {
        this.size = size;
    }

    public float GetSize() {
        return size;
    }

}