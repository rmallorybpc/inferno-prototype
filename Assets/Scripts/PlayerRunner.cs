using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRunner : MonoBehaviour
{
    [SerializeField] private float forwardSpeed = 8f;
    [SerializeField] private float strafeSpeed = 12f;
    [SerializeField] private float laneLimit = 4f;

    private float targetX;
    private bool isDragging;
    private float lastPointerX;

    private void Start()
    {
        targetX = transform.position.x;
    }

    private void Update()
    {
        ReadDrag();
        Move();
    }

    private void ReadDrag()
    {
        if (Pointer.current == null) return;

        bool pressed = Pointer.current.press.isPressed;
        float pointerX = Pointer.current.position.ReadValue().x;

        if (pressed && !isDragging)
        {
            isDragging = true;
            lastPointerX = pointerX;
            return;
        }

        if (!pressed)
        {
            isDragging = false;
            return;
        }

        float deltaPixels = pointerX - lastPointerX;
        lastPointerX = pointerX;

        float deltaWorld = deltaPixels / Screen.width * laneLimit * 2f;
        targetX = Mathf.Clamp(targetX + deltaWorld, -laneLimit, laneLimit);
    }

    private void Move()
    {
        Vector3 position = transform.position;
        position.z += forwardSpeed * Time.deltaTime;
        position.x = Mathf.MoveTowards(position.x, targetX, strafeSpeed * Time.deltaTime);
        transform.position = position;
    }
}