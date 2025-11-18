using UnityEngine;
using OscJack;

public class OSCReceiver : MonoBehaviour
{
    [Header("OSC Settings")]
    public int listenPort = 9001;
    public string oscAddress = "/source/pos";

    private OscServer server;

    private float receivedX;
    private float receivedY;
    private float receivedZ;
    private bool hasNewData = false;

    void Start()
    {
        server = new OscServer(listenPort);

        server.MessageDispatcher.AddCallback(
            oscAddress,
            (string address, OscDataHandle data) =>
            {
                receivedX = data.GetElementAsFloat(0);
                receivedY = data.GetElementAsFloat(1);
                receivedZ = data.GetElementAsFloat(2);
                hasNewData = true;
            }
        );
    }

    void Update()
    {
        if (hasNewData)
        {
            transform.position = new Vector3(receivedX, receivedY, receivedZ);
            hasNewData = false;
        }
    }

    void OnDestroy()
    {
        server?.Dispose();
    }
}
