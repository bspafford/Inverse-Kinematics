using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class fingerTracking : MonoBehaviour {

	TcpClient client;
	StreamReader reader;

	[SerializeField]
	Transform thumb1;
    [SerializeField]
    float offset1;
    [SerializeField]
	Transform thumb2;
    [SerializeField]
    float offset2;
    [SerializeField]
    Transform thumb3;
    [SerializeField]
    float offset3;

    [SerializeField]
	Transform index1;
    [SerializeField]
    float offset4;
    [SerializeField]
	Transform index2;
    [SerializeField]
    float offset5;
    [SerializeField]
    Transform index3;
    [SerializeField]
    float offset6;

    [SerializeField]
	Transform middle1;
    [SerializeField]
    float offset7;
    [SerializeField]
	Transform middle2;
    [SerializeField]
    float offset8;
    [SerializeField]
    Transform middle3;
    [SerializeField]
    float offset9;

    [SerializeField]
	Transform ring1;
    [SerializeField]
    float offset10;
    [SerializeField]
	Transform ring2;
    [SerializeField]
    float offset11;
    [SerializeField]
    Transform ring3;
    [SerializeField]
    float offset12;

    [SerializeField]
	Transform pinky1;
    [SerializeField]
    float offset13;
    [SerializeField]
	Transform pinky2;
    [SerializeField]
    float offset14;
    [SerializeField]
    Transform pinky3;
    [SerializeField]
    float offset15;

    [SerializeField]
	int size = 5;

	List<float> thumb1Rot = new List<float>();
	List<float> thumb2Rot = new List<float>();
	List<float> thumb3Rot = new List<float>();

    List<float> index1Rot = new List<float>();
	List<float> index2Rot = new List<float>();
	List<float> index3Rot = new List<float>();

    List<float> middle1Rot = new List<float>();
	List<float> middle2Rot = new List<float>();
	List<float> middle3Rot = new List<float>();

    List<float> ring1Rot = new List<float>();
	List<float> ring2Rot = new List<float>();
	List<float> ring3Rot = new List<float>();

    List<float> pinky1Rot = new List<float>();
	List<float> pinky2Rot = new List<float>();
	List<float> pinky3Rot = new List<float>();

    void Start() {
		try {
			client = new TcpClient("127.0.0.1", 5052);
			reader = new StreamReader(client.GetStream());
			print("Connected!");
		} catch {
			print("Could Not Connect to Server");
		}
	}

	void Update() {
		if (client != null && client.Available > 0 && reader != null) {
			string line = reader.ReadLine();
			if (line == null) return;

			string[] p = line.Split(',');

			// Expecting 15 values (5 fingers × 3 joints)
			if (p.Length != 15) return;

			float[] v = new float[15];
			for (int i = 0; i < 15; i++)
				v[i] = float.Parse(p[i]);

			// Example mapping (you MUST match your rig)
			float t1Rot = Add(ref thumb1Rot, v[0]);
			float t2Rot = Add(ref thumb2Rot, v[1]);
			float t3Rot = Add(ref thumb3Rot, v[2]);

			float i1Rot = Add(ref index1Rot, v[3]);
			float i2Rot = Add(ref index2Rot, v[4]);
			float i3Rot = Add(ref index3Rot, v[5]);

			float m1Rot = Add(ref middle1Rot, v[6]);
			float m2Rot = Add(ref middle2Rot, v[7]);
			float m3Rot = Add(ref middle3Rot, v[8]);

			float r1Rot = Add(ref ring1Rot, v[9]);
			float r2Rot = Add(ref ring2Rot, v[10]);
			float r3Rot = Add(ref ring3Rot, v[11]);

			float p1Rot = Add(ref pinky1Rot, v[12]);
			float p2Rot = Add(ref pinky2Rot, v[13]);
			float p3Rot = Add(ref pinky3Rot, v[14]);


			// Apply rotations (example axis!)
			thumb1.localRotation = Quaternion.Euler(0, 0, t1Rot + offset1);
			thumb2.localRotation = Quaternion.Euler(0, 0, t2Rot + offset2);
			thumb3.localRotation = Quaternion.Euler(0, 0, t3Rot + offset3);

			index1.localRotation = Quaternion.Euler(0, -i1Rot + offset4, 0);
			index2.localRotation = Quaternion.Euler(0, -i2Rot + offset5, 0);
			index3.localRotation = Quaternion.Euler(0, -i3Rot + offset6, 0);

			middle1.localRotation = Quaternion.Euler(0, -m1Rot + offset7, 0);
			middle2.localRotation = Quaternion.Euler(0, -m2Rot + offset8, 0);
			middle3.localRotation = Quaternion.Euler(0, -m3Rot + offset9, 0);

			ring1.localRotation = Quaternion.Euler(0, -r1Rot + offset10, 0);
			ring2.localRotation = Quaternion.Euler(0, -r2Rot + offset11, 0);
			ring3.localRotation = Quaternion.Euler(0, -r3Rot + offset12, 0);

			pinky1.localRotation = Quaternion.Euler(0, -p1Rot + offset13, 0);
			pinky2.localRotation = Quaternion.Euler(0, -p2Rot + offset14, 0);
			pinky3.localRotation = Quaternion.Euler(0, -p3Rot + offset15, 0);
		}
	}

	float Add(ref List<float> list, float value) {
		if (list.Count < size) {
			list.Add(value);
		} else {
			for (int i = 0; i < list.Count - 1; ++i) {
				list[i] = list[i + 1];
			}
			list[list.Count - 1] = value;
		}

		float avg = 0f;
		for (int i = 0; i < list.Count; ++i) {
			avg += list[i];
		}
		return avg / list.Count;
	}
}