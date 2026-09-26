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
    [SerializeField] private int maxShotsPerSecond = 150;

    private Crowd crowd;
    private float timer;
    private int volleyCount;

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

        FireVolley();
        volleyCount++;
    }

    private void FireVolley()
    {
        int followerCount = crowd != null ? crowd.Followers.Count : 0;
        int shooterCount = 1 + followerCount;

        int volleyBudget = Mathf.Max(1, Mathf.RoundToInt(maxShotsPerSecond * fireInterval));
        int maxShooters = Mathf.Max(1, volleyBudget / shotsPerShooter);

        if (shooterCount <= maxShooters)
        {
            for (int i = 0; i < shooterCount; i++)
            {
                Fire(ShooterAt(i), damage);
            }
            return;
        }

        float step = shooterCount / (float)maxShooters;
        int scaledDamage = Mathf.Max(1, Mathf.RoundToInt(damage * step));
        float phase = (volleyCount * 0.37f) % 1f;

        for (int k = 0; k < maxShooters; k++)
        {
            int index = Mathf.FloorToInt((k + phase) * step) % shooterCount;
            Fire(ShooterAt(index), scaledDamage);
        }
    }

    private Transform ShooterAt(int index)
    {
        if (index == 0 || crowd == null) return transform;
        return crowd.Followers[index - 1];
    }

    private void Fire(Transform shooter, int shotDamage)
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
            if (projectile != null) projectile.SetDamage(shotDamage);
        }
    }
}
