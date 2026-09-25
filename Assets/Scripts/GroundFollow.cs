using UnityEngine;

public class GroundFollow : MonoBehaviour
{
    [SerializeField] private float leadDistance = 400f;

    private Transform player;

    private void Start()
    {
        Crowd crowd = FindFirstObjectByType<Crowd>();
        if (crowd != null) player = crowd.transform;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 position = transform.position;
        position.z = player.position.z + leadDistance;
        transform.position = position;
    }
}
