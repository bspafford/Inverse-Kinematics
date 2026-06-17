using System.IO;
using System.Net.Sockets;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class handTracking3DTarget : MonoBehaviour {

    TcpClient client;
    StreamReader reader;

    [Header("Pos")]
    public Vector3 scale = Vector3.one;
    public Vector3 offset = Vector3.zero;

    [Header("Claw")]
    public Transform rightClaw;
    public Transform leftClaw;
    public float minDist = 0.03f;
    public float maxDist = 0.12f;
    public float minRot = 25f;
    public float maxRot = 60f;

    [Header("Debug")]
    public float x;
    public float y;
    public float depth;
    public float dist;
    public float w;
    public float h;

    void Start() {
        try {
            client = new TcpClient("127.0.0.1", 5052);
            reader = new StreamReader(client.GetStream());
        } catch {
            print("Could Not Connect to Server");
        }
    }

    void Update() {
        if (client != null && client.Available > 0 && reader != null) { // only reads if there is something to be read (doesn't wait)
            string line = reader.ReadLine();
            if (line == null) return;

            string[] p = line.Split(',');

            y = float.Parse(p[0]);
            x = float.Parse(p[1]);
            depth = float.Parse(p[2]);
            dist = float.Parse(p[3]);
            w = float.Parse(p[4]);
            h = float.Parse(p[5]);

            if (w == 0 || h == 0)
                return;

            print("pos: (" + x + ", " + y + ")");

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10));

            Vector3 percent = new Vector3(1f - x / w, depth / h, y / h);
            // Vector3 percent = new Vector3(y / h, mouseWorldPos.y, 1f - x / w);
            if (math.isnan(percent.x) || math.isnan(percent.y) || math.isnan(percent.z))
                percent = new Vector3(0f, 0f, 0f);

            // Camera Size in world units
            float height = 2f * Camera.main.orthographicSize;
            float width = height * Camera.main.aspect;


            float desiredHeight = Mouse.current.position.ReadValue().y / Screen.height * 10f;

            Vector3 desiredLoc = new Vector3(percent.x * width - width / 2f, percent.y * height - height / 2f, percent.z * height - height / 2f);
            transform.position = Vector3.Scale(desiredLoc, scale) + offset;


            float t = Mathf.InverseLerp(minDist, maxDist, dist);
            float rightAngle = Mathf.Lerp(minRot, maxRot, t);
            float leftAngle = -rightAngle;

            rightClaw.localEulerAngles = new Vector3(0f, 0f, rightAngle);
            leftClaw.localEulerAngles = new Vector3(0f, 0f, leftAngle);
        }
    }
}