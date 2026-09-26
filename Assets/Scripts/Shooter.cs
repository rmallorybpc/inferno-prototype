using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float fireInterval = 0.5f;
    [SerializeField] private float minFireInterval = 0.1f;
    [SerializeField] private int damage = 1;
    [SerializeField] private int shotsPerShooter = 1;
    [SerializeField] private float spreadAngle = 8f;
    [SerializeField] private float fireHeight = 0.5f;
    [SerializeField] private float muzzleForward = 0.6f;

    private Crowd crowd;
    private float timer;

    public void ImproveFireRate(float fraction)
    {
        fireInterval = Mathf.Max(minFireInterval, fireInterval * (1f - fraction));
        Debug.Log($"Fire interval now {fireInterval:0.00}s");
    }

    public void AddDamage(int amount)
    {
        damage += amount;
    }

    public void AddShots(int amount)
    {
        shotsPerShooter += amount;
    }

    private void Awake()
    {
        crowd = GetComponent<Crowd>();
    }

    private void Update()
    {
        if (crowd != null && crowd.IsDefeated) return;

        timer += Time.deltaTime;
        if (timer < fireInterval) return;
        timer -= fireInterval;

        Fire(transform);

        if (crowd == null) return;

        foreach (Transform follower in crowd.Followers)
        {
            Fire(follower);
        }
    }

    private void Fire(Transform shooter)
    {
        Vector3 origin = new Vector3(
            shooter.position.x,
            fireHeight,
            shooter.position.z + muzzleForward);

        for (int i = 0; i < shotsPerShooter; i++)
        {
            float angle = (i - (shotsPerShooter - 1) * 0.5f) * spreadAngle;
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);

            GameObject shot = Instantiate(projectilePrefab, origin, rotation);
            Projectile projectile = shot.GetComponent<Projectile>();
            if (projectile != null) projectile.SetDamage(damage);
        }
    }
}
