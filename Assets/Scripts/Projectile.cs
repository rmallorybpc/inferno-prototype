using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private int damage = 1;
    [SerializeField] private GameObject burstFlashPrefab;
    [SerializeField] private GameObject hellfirePatchPrefab;

    private static readonly Collider[] burstHits = new Collider[32];

    private float burstRadius;
    private int burstDamage;
    private float hellfireRadius;
    private int hellfireDamage;
    private float hellfireDuration;

    public void SetDamage(int amount)
    {
        damage = amount;
    }

    public void SetBurst(float radius, int amount)
    {
        burstRadius = radius;
        burstDamage = amount;
    }

    public void SetHellfire(float radius, int amount, float duration)
    {
        hellfireRadius = radius;
        hellfireDamage = amount;
        hellfireDuration = duration;
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
        Ignite();

        if (damage <= 0) Destroy(gameObject);
    }

    private void Burst(Collider struck)
    {
        if (burstRadius <= 0f || burstDamage <= 0) return;

        BurstFlash.Spawn(burstFlashPrefab, transform.position, burstRadius);

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

    private void Ignite()
    {
        if (hellfireRadius <= 0f || hellfireDamage <= 0) return;

        Vector3 ground = new Vector3(transform.position.x, 0.02f, transform.position.z);
        HellfirePatch.Spawn(hellfirePatchPrefab, ground, hellfireRadius, hellfireDamage, hellfireDuration);
    }
}
