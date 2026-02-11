using UnityEngine;

public class FishWander : MonoBehaviour
{
    public Transform center; // set this (e.g., an empty GameObject)

    public float radius = 3.0f;
    private float baseAngularSpeedDeg = 35f; // degrees/sec (base)

    public float verticalStep = 3.0f; // distance to move down/up each level
    private int circlesPerLevel = 1;   // how many circles before changing height
    private float transitionAngleDeg = 0f;       // the point on the circle where we change height (0deg = +X)
    private float transitionAngleWindowDeg = 12f; // how close (in degrees) we must be to start the vertical move
    private float verticalTransitionDuration = 1.0f; // seconds to swim between levels

    private float speedVariation = 0.4f;      // 0.0 = none, 0.4 = +/- 40% approx
    private float speedVariationRate = 0.15f; // how fast the speed changes (Hz-ish)

    [Tooltip("Overall speed scale for the orbit (use < 1 to slow down).")]
    public float speedScale = 0.75f;

    [Tooltip("Clamp the speed multiplier so it never gets too fast/slow.")]
    private float minSpeedMul = 0.6f;
    private float maxSpeedMul = 1.2f;

    [Header("Turning")]
    private float turnSpeed = 4.0f;

    public enum ModelForwardAxis { Z_Plus, Z_Minus, X_Plus, X_Minus }
    public ModelForwardAxis modelForwardAxis = ModelForwardAxis.Z_Minus;

    float _angleRad;
    float _noiseT;
    int _currentCircleCount = 0;
    int _verticalDirection = 0; // 0 = middle, -1 = down, +1 = up
    float _baseY;

    bool _transitionArmed = false;
    bool _isTransitioning = false;
    int _nextVerticalDirection = 0;
    float _yFrom;
    float _yTo;
    float _yT;
    float _frozenSpeedMul = 1f;

    void Start()
    {
        if (!center)
        {
            // If center not set, create one at current position
            GameObject c = new GameObject("FishOrbitCenter");
            c.transform.position = transform.position;
            center = c.transform;
        }

        // Initialize angle based on current position around center
        Vector3 flat = transform.position - center.position;
        flat.y = 0f;
        if (flat.sqrMagnitude < 0.0001f) flat = Vector3.forward * radius;

        _angleRad = Mathf.Atan2(flat.z, flat.x);
        _noiseT = Random.value * 100f;

        _baseY = transform.position.y;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        float speedMul;

        // During vertical transitions, freeze the speed multiplier so the change of level feels seamless
        if (_isTransitioning)
        {
            speedMul = _frozenSpeedMul;
        }
        else
        {
            _noiseT += dt * speedVariationRate;
            float n = Mathf.PerlinNoise(_noiseT, 0.123f) * 2f - 1f; // [-1, 1]
            speedMul = 1f + n * speedVariation;              // around 1.0
            speedMul = Mathf.Clamp(speedMul, minSpeedMul, maxSpeedMul);
        }

        // Advance around circle
        float angSpeedRad = Mathf.Deg2Rad * baseAngularSpeedDeg * speedMul * speedScale;
        _angleRad += angSpeedRad * dt;

        // Detect completed circle
        if (_angleRad >= Mathf.PI * 2f)
        {
            _angleRad -= Mathf.PI * 2f;
            _currentCircleCount++;

            if (_currentCircleCount >= circlesPerLevel)
            {
                _currentCircleCount = 0;

                // Cycle: middle -> down -> up -> repeat (arm the change; don't teleport)
                if (_verticalDirection == 0) _nextVerticalDirection = -1;
                else if (_verticalDirection == -1) _nextVerticalDirection = 1;
                else _nextVerticalDirection = 0;

                _transitionArmed = true;
            }
        }

        float x = Mathf.Cos(_angleRad) * radius;
        float z = Mathf.Sin(_angleRad) * radius;

        // Start vertical transition only when we reach the chosen point on the circle
        if (_transitionArmed && !_isTransitioning)
        {
            float targetAng = Mathf.Deg2Rad * transitionAngleDeg;
            float angDelta = Mathf.Abs(Mathf.DeltaAngle(_angleRad * Mathf.Rad2Deg, transitionAngleDeg));

            if (angDelta <= transitionAngleWindowDeg)
            {
                _transitionArmed = false;
                _isTransitioning = true;
                _yFrom = transform.position.y;
                _yTo = _baseY + (_nextVerticalDirection * verticalStep);
                _yT = 0f;

                // Freeze current speed multiplier for the duration of the transition
                _frozenSpeedMul = speedMul;
                _frozenSpeedMul = Mathf.Clamp(_frozenSpeedMul, minSpeedMul, maxSpeedMul);
            }
        }

        float levelY = _baseY + (_verticalDirection * verticalStep);
        float y = levelY;

        if (_isTransitioning)
        {
            _yT += dt / Mathf.Max(0.0001f, verticalTransitionDuration);
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_yT));
            y = Mathf.Lerp(_yFrom, _yTo, t);

            if (_yT >= 1f)
            {
                _isTransitioning = false;
                _verticalDirection = _nextVerticalDirection;
                y = _yTo;
            }
        }

        Vector3 targetPos = new Vector3(center.position.x + x, y, center.position.z + z);

        // Move toward target at the correct physical speed (no teleport)
        float tangentialSpeed = Mathf.Abs(angSpeedRad) * radius; // meters/sec
        float moveSpeed = Mathf.Max(0.01f, tangentialSpeed);
        Vector3 nextPos = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * dt);

        // Look where we're going (direction to next position)
        Vector3 moveDir = (nextPos - transform.position);
        if (moveDir.sqrMagnitude > 0.000001f)
        {
            Quaternion face = Quaternion.LookRotation(moveDir.normalized, Vector3.up);
            Quaternion correction = Quaternion.FromToRotation(GetModelForwardLocal(modelForwardAxis), Vector3.forward);
            Quaternion targetRot = face * correction;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * dt);
        }

        transform.position = nextPos;
    }

    Vector3 GetModelForwardLocal(ModelForwardAxis axis)
    {
        switch (axis)
        {
            case ModelForwardAxis.Z_Plus:  return Vector3.forward;
            case ModelForwardAxis.Z_Minus: return Vector3.back;
            case ModelForwardAxis.X_Plus:  return Vector3.right;
            case ModelForwardAxis.X_Minus: return Vector3.left;
            default: return Vector3.forward;
        }
    }
}