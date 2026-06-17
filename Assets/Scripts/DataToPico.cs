using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using UnityEngine.InputSystem;

public class DataToPico : MonoBehaviour {

	[SerializeField]
	float sendDelay = 1f;

	TcpClient client;
	NetworkStream stream;

	void Start() {
		client = new TcpClient("10.0.0.44", 12345);
		stream = client.GetStream();

		StartCoroutine(SendData());
	}

	void Update() {
		
	}

	IEnumerator SendData() {
		while (true) {
			yield return new WaitForSeconds(sendDelay);

            byte[] data = Encoding.UTF8.GetBytes($"{Mouse.current.position.ReadValue().x},{Mouse.current.position.ReadValue().y},{2},{3}\n");
            stream.Write(data, 0, data.Length);
        }
	}
}