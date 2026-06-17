using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class targetTrackhand : MonoBehaviour {

    [SerializeField]
    Vector3 desiredLoc = new Vector3(0, -1, 0);

    TcpClient client;
    StreamReader reader;

    public float x, y, dist, w, h;

    [SerializeField]
    Vector3 offset;
    [SerializeField]
    bool flip = false;

    void Start() {
        print("starting!");
        try {
            client = new TcpClient("127.0.0.1", 5052);
            reader = new StreamReader(client.GetStream());
            print("Connected!");
        } catch {
            print("Could Not Connect to Server");
        }
    }

    void Update() {
        print(reader);
        if (reader == null) return;

        string line = reader.ReadLine();
        print(line);
        if (line == null) return;

        string[] p = line.Split(',');

        x = float.Parse(p[0]);
        y = float.Parse(p[1]);
        // depth = p[2]
        dist = float.Parse(p[3]);
        w = float.Parse(p[4]);
        h = float.Parse(p[5]);

        Vector2 percent = new Vector2(x / w, y / h);
        if (flip) {
            percent.y = 1f - percent.y;
        }

        // Camera Size in world units
        float height = 2f * Camera.main.orthographicSize;
        float width = height * Camera.main.aspect;

        desiredLoc = new Vector3(percent.x * width - width / 2f, percent.y * height - height / 2f, 0f);

        if (float.IsNaN(desiredLoc.x) || float.IsNaN(desiredLoc.y))
            return;

        transform.position = desiredLoc + offset;
    }
}