using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private int damage = 1;

    private bool spent;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += Vector3.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (spent) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;

        spent = true;
        enemy.TakeDamage(damage);
        Destroy(gameObject);
    }
}
