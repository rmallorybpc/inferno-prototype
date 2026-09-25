using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 4f, -8f);
    [SerializeField, Range(0f, 1f)] private float lateralFollow = 0f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 position = target.position + offset;
        position.x = target.position.x * lateralFollow + offset.x;
        transform.position = position;
    }
}
