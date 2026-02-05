using UnityEngine;
using UnityEngine.InputSystem;

public class XRVerticalHorizontalMove : MonoBehaviour
{
    public InputActionProperty rightStick;
    public InputActionProperty leftStick;

    public float horizontalSpeed = 2f;
    public float verticalSpeed = 1.5f;

    [SerializeField] Transform cameraTransform; // assign Main Camera here

    private void OnEnable()
    {
        rightStick.action?.Enable();
        leftStick.action?.Enable();
    }

    private void OnDisable()
    {
        rightStick.action?.Disable();
        leftStick.action?.Disable();
    }

    void Update()
    {
        if (cameraTransform == null) return;

        Vector2 right = rightStick.action.ReadValue<Vector2>();
        Vector2 left = leftStick.action.ReadValue<Vector2>();

        Vector3 forward = cameraTransform.forward; forward.y = 0; forward.Normalize();
        Vector3 rightDir = cameraTransform.right; rightDir.y = 0; rightDir.Normalize();

        Vector3 horizontalMove = (forward * right.y + rightDir * right.x) * horizontalSpeed * Time.deltaTime;
        float verticalMove = left.y * verticalSpeed * Time.deltaTime;

        transform.position += horizontalMove + Vector3.up * verticalMove;
    }
}
