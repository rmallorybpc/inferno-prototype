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

    [Header("Hero")]
    [SerializeField] private float heroDamageMultiplier = 3f;
    [SerializeField] private float heroProjectileScale = 1.8f;

    private Crowd crowd;
    private float timer;
    private int volleyCount;

    public float VolleysPerSecond => 1f / fireInterval;
    public bool IsFireRateMaxed => fireInterval <= minFireInterval + 0.0001f;
    public int LastVolleyShots { get; private set; }
    public int LastShotDamage { get; private set; }

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
        LastVolleyShots = shotsPerShooter;
        LastShotDamage = damage;
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
        int heroDamage = Mathf.Max(1, Mathf.RoundToInt(damage * heroDamageMultiplier));
        Fire(transform, heroDamage, heroProjectileScale);

        int followerCount = crowd != null ? crowd.Followers.Count : 0;
        int volleyBudget = Mathf.Max(1, Mathf.RoundToInt(maxShotsPerSecond * fireInterval));
        int followerBudget = Mathf.Max(0, volleyBudget - shotsPerShooter);
        int maxShooters = followerBudget / shotsPerShooter;

        LastVolleyShots = shotsPerShooter;
        LastShotDamage = damage;

        if (followerCount == 0 || maxShooters == 0) return;

        if (followerCount <= maxShooters)
        {
            foreach (Transform follower in crowd.Followers)
            {
                Fire(follower, damage, 1f);
            }

            LastVolleyShots += followerCount * shotsPerShooter;
            return;
        }

        float step = followerCount / (float)maxShooters;
        int scaledDamage = Mathf.Max(1, Mathf.RoundToInt(damage * step));
        float phase = (volleyCount * 0.37f) % 1f;

        for (int k = 0; k < maxShooters; k++)
        {
            int index = Mathf.FloorToInt((k + phase) * step) % followerCount;
            Fire(crowd.Followers[index], scaledDamage, 1f);
        }

        LastVolleyShots += maxShooters * shotsPerShooter;
        LastShotDamage = scaledDamage;
    }

    private void Fire(Transform shooter, int shotDamage, float scale)
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
            if (scale != 1f) shot.transform.localScale *= scale;

            Projectile projectile = shot.GetComponent<Projectile>();
            if (projectile != null) projectile.SetDamage(shotDamage);
        }
    }
}
