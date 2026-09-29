using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private int damage = 1;

    private static readonly Collider[] burstHits = new Collider[32];

    private float burstRadius;
    private int burstDamage;

    public void SetDamage(int amount)
    {
        damage = amount;
    }

    public void SetBurst(float radius, int amount)
    {
        burstRadius = radius;
        burstDamage = amount;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (damage <= 0) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            damage -= enemy.TakeDamage(damage);
        }
        else
        {
            Boss boss = other.GetComponent<Boss>();
            if (boss == null) return;
            damage -= boss.TakeDamage(damage);
        }

        Burst(other);

        if (damage <= 0) Destroy(gameObject);
    }

    private void Burst(Collider struck)
    {
        if (burstRadius <= 0f || burstDamage <= 0) return;

        int count = Physics.OverlapSphereNonAlloc(
            transform.position, burstRadius, burstHits, ~0, QueryTriggerInteraction.Collide);

        for (int i = 0; i < count; i++)
        {
            Collider hit = burstHits[i];
            if (hit == struck) continue;

            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(burstDamage);
                continue;
            }

            Boss boss = hit.GetComponent<Boss>();
            if (boss != null) boss.TakeDamage(burstDamage);
        }
    }
}
