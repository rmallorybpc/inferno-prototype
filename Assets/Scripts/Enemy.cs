using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float rushSpeed = 10f;
    [SerializeField] private float engageRange = 15f;

    private Transform target;
    private bool spent;

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) target = player.transform;
    }

    private void Update()
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;
        if (distance < 0.1f) return;

        float currentSpeed = distance < engageRange ? rushSpeed : speed;
        transform.position += direction / distance * currentSpeed * Time.deltaTime;
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
