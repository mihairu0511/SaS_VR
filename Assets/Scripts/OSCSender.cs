using UnityEngine;
using OscJack;
using UnityEngine.XR;

public class OSCSender : MonoBehaviour
{
    [Header("References")]
    public Transform audioSourceTransform;
    public Camera vrCamera;

    [Header("OSC Settings")]
    public string host = "127.0.0.1";
    public int port = 9000;

    private OscClient client;

    void Start()
    {
        client = new OscClient(host, port);
    }

    void Update()
    {
        if (!audioSourceTransform || !vrCamera) return;

        Vector3 s = audioSourceTransform.position;
        client.Send("/source/pos", s.x, s.y, s.z);

        Vector3 l = vrCamera.transform.position;
        client.Send("/listener/pos", l.x, l.y, l.z);

        Vector3 f = vrCamera.transform.forward;
        Vector3 u = vrCamera.transform.up;
        client.Send("/listener/forward", f.x, f.y, f.z);
        client.Send("/listener/up", u.x, u.y, u.z);

       Vector3 relPos = vrCamera.transform.InverseTransformPoint(audioSourceTransform.position);
        client.Send("/source/rel", relPos.x, relPos.y, relPos.z);
    }

    void OnDestroy()
    {
        client?.Dispose();
    }
}