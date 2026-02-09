using UnityEngine;

public class FishWander : MonoBehaviour
{
    [Header("Roam Area (Trigger Collider)")]
    public BoxCollider roamBox;

    [Header("Movement")]
    public float speed = 1.5f;
    public float turnSpeed = 2.0f;
    public float waypointReachDist = 0.5f;

    [Header("Behavior")]
    public float minWaitAtPoint = 0.0f;
    public float maxWaitAtPoint = 0.5f;

    public enum ModelForwardAxis { Z_Plus, Z_Minus, X_Plus, X_Minus }
    [Header("Model Orientation")]
    [Tooltip("Pick which LOCAL axis points out of the fish's NOSE (front).")]
    public ModelForwardAxis modelForwardAxis = ModelForwardAxis.Z_Minus;

    Vector3 _target;
    float _waitTimer;

    void Start()
    {
        PickNewTarget();
    }

    void Update()
    {
        if (!roamBox) return;

        if (_waitTimer > 0f)
        {
            _waitTimer -= Time.deltaTime;
            return;
        }

        Vector3 toTarget = _target - transform.position;

        if (toTarget.magnitude <= waypointReachDist)
        {
            _waitTimer = Random.Range(minWaitAtPoint, maxWaitAtPoint);
            PickNewTarget();
            return;
        }

        Vector3 dir = toTarget.normalized;
        if (dir.sqrMagnitude < 0.0001f) return;

        // Rotation that looks toward the travel direction (Unity assumes local +Z is forward)
        Quaternion faceDir = Quaternion.LookRotation(dir, Vector3.up);

        // Convert your model's "nose axis" to Unity's +Z forward
        Vector3 modelForwardLocal = GetModelForwardLocal(modelForwardAxis);
        Quaternion correction = Quaternion.FromToRotation(modelForwardLocal, Vector3.forward);

        Quaternion targetRot = faceDir * correction;

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);

        // Move along travel direction (stable + doesn't depend on model forward)
        transform.position += dir * speed * Time.deltaTime;
    }

    Vector3 GetModelForwardLocal(ModelForwardAxis axis)
    {
        switch (axis)
        {
            case ModelForwardAxis.Z_Plus: return Vector3.forward;  // +Z
            case ModelForwardAxis.Z_Minus: return Vector3.back;     // -Z
            case ModelForwardAxis.X_Plus: return Vector3.right;    // +X
            case ModelForwardAxis.X_Minus: return Vector3.left;     // -X
            default: return Vector3.forward;
        }
    }

    void PickNewTarget()
    {
        Bounds b = roamBox.bounds;
        _target = new Vector3(
            Random.Range(b.min.x, b.max.x),
            Random.Range(b.min.y, b.max.y),
            Random.Range(b.min.z, b.max.z)
        );
    }


    void OnDrawGizmosSelected()
    {
        if (!roamBox) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(roamBox.bounds.center, roamBox.bounds.size);
    }
}
