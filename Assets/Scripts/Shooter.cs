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

    [Header("Burst")]
    [SerializeField] private float burstBaseRadius = 1.5f;
    [SerializeField] private float burstRadiusPerLevel = 0.5f;
    [SerializeField] private int burstDamagePerLevel = 1;

    [Header("Hellfire")]
    [SerializeField] private float hellfireBaseRadius = 1.5f;
    [SerializeField] private float hellfireRadiusPerLevel = 0.25f;
    [SerializeField] private int hellfireDamagePerLevel = 1;
    [SerializeField] private float hellfireBaseDuration = 2f;
    [SerializeField] private float hellfireDurationPerLevel = 0.5f;

    private Crowd crowd;
    private float timer;
    private int volleyCount;
    private int burstLevel;
    private int hellfireLevel;

    public float VolleysPerSecond => 1f / fireInterval;
    public bool IsFireRateMaxed => fireInterval <= minFireInterval + 0.0001f;
    public int LastVolleyShots { get; private set; }
    public int LastShotDamage { get; private set; }

    private float BurstRadius => burstLevel > 0 ? burstBaseRadius + burstRadiusPerLevel * (burstLevel - 1) : 0f;
    private int BurstDamage => burstDamagePerLevel * burstLevel;

    private float HellfireRadius => hellfireLevel > 0 ? hellfireBaseRadius + hellfireRadiusPerLevel * (hellfireLevel - 1) : 0f;
    private int HellfireDamage => hellfireDamagePerLevel * hellfireLevel;
    private float HellfireDuration => hellfireLevel > 0 ? hellfireBaseDuration + hellfireDurationPerLevel * (hellfireLevel - 1) : 0f;

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

    public void AddBurst(int levels)
    {
        burstLevel += levels;
        Debug.Log($"Burst level {burstLevel}: radius {BurstRadius:0.0}, damage {BurstDamage}");
    }

    public void AddHellfire(int levels)
    {
        hellfireLevel += levels;
        Debug.Log($"Hellfire level {hellfireLevel}: radius {HellfireRadius:0.00}, damage {HellfireDamage}, lasts {HellfireDuration:0.0}s");
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
        int heroBurst = Mathf.RoundToInt(BurstDamage * heroDamageMultiplier);
        Fire(transform, heroDamage, heroBurst, heroProjectileScale);

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
                Fire(follower, damage, BurstDamage, 1f);
            }

            LastVolleyShots += followerCount * shotsPerShooter;
            return;
        }

        float step = followerCount / (float)maxShooters;
        int scaledDamage = Mathf.Max(1, Mathf.RoundToInt(damage * step));
        int scaledBurst = Mathf.RoundToInt(BurstDamage * step);
        float phase = (volleyCount * 0.37f) % 1f;

        for (int k = 0; k < maxShooters; k++)
        {
            int index = Mathf.FloorToInt((k + phase) * step) % followerCount;
            Fire(crowd.Followers[index], scaledDamage, scaledBurst, 1f);
        }

        LastVolleyShots += maxShooters * shotsPerShooter;
        LastShotDamage = scaledDamage;
    }

    private void Fire(Transform shooter, int shotDamage, int burstDamage, float scale)
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
            if (projectile == null) continue;

            projectile.SetDamage(shotDamage);
            projectile.SetBurst(BurstRadius, burstDamage);
            projectile.SetHellfire(HellfireRadius, HellfireDamage, HellfireDuration);
        }
    }
}
