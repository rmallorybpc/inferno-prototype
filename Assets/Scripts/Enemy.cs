using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float rushSpeed = 10f;
    [SerializeField] private float trackingSpeed = 16f;
    [SerializeField] private float engageRange = 15f;

    private Transform target;
    private float spreadOffset;
    private bool spent;

    public void SetSpread(float offset)
    {
        spreadOffset = offset;
    }

    private void Start()
    {
        Crowd crowd = FindFirstObjectByType<Crowd>();
        if (crowd != null) target = crowd.transform;
    }

    private void Update()
    {
        if (target == null) return;

        Vector3 position = transform.position;
        Vector3 goal = target.position;

        float distance = Vector2.Distance(
            new Vector2(position.x, position.z),
            new Vector2(goal.x, goal.z));

        bool engaged = distance < engageRange;

        float spread = spreadOffset * Mathf.Clamp01((distance - 2f) / engageRange);
        float aimX = goal.x + spread;

        float lateralSpeed = engaged ? trackingSpeed : speed;
        float forwardSpeed = engaged ? rushSpeed : speed;

        position.x = Mathf.MoveTowards(position.x, aimX, lateralSpeed * Time.deltaTime);
        position.z = Mathf.MoveTowards(position.z, goal.z, forwardSpeed * Time.deltaTime);
        transform.position = position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (spent) return;
        if (other.attachedRigidbody == null) return;

        Crowd crowd = other.attachedRigidbody.GetComponent<Crowd>();
        if (crowd == null) return;

        spent = true;
        crowd.TakeHit(other.transform);
        Destroy(gameObject);
    }
}
