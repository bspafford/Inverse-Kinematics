using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Arrow))]
public class TestEditor : UnityEditor.Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        Arrow script = (Arrow)target;

        if (GUILayout.Button("Animate Line")) {
            script.StartLineAnim();
        }

        if (GUILayout.Button("Reset Line")) {
            script.Restart();
        }
    }
}