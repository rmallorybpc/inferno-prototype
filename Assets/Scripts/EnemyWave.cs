using UnityEngine;

public class EnemyWave : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int count = 6;
    [SerializeField] private int columns = 3;
    [SerializeField] private float spacing = 1.2f;
    [SerializeField] private float activationDistance = 65f;

    private float speedMultiplier = 1f;
    private GameObject bossPrefab;
    private int bossHealth;
    private Transform player;
    private bool spawned;

    public void Configure(int newCount, int newColumns, float newSpeedMultiplier)
    {
        count = newCount;
        columns = newColumns;
        speedMultiplier = newSpeedMultiplier;
    }

    public void ConfigureBoss(GameObject newBossPrefab, int health, float newSpeedMultiplier)
    {
        bossPrefab = newBossPrefab;
        bossHealth = health;
        speedMultiplier = newSpeedMultiplier;
    }

    private void Start()
    {
        Crowd crowd = FindFirstObjectByType<Crowd>();
        if (crowd != null) player = crowd.transform;
    }

    private void Update()
    {
        if (spawned || player == null) return;
        if (transform.position.z - player.position.z > activationDistance) return;

        spawned = true;

        if (bossPrefab != null) SpawnBoss();
        else Spawn();
    }

    private void SpawnBoss()
    {
        GameObject spawnedBoss = Instantiate(bossPrefab, transform.position, Quaternion.identity);
        Boss boss = spawnedBoss.GetComponent<Boss>();
        if (boss != null) boss.Configure(bossHealth, speedMultiplier);
    }

    private void Spawn()
    {
        for (int i = 0; i < count; i++)
        {
            int row = i / columns;
            int column = i % columns;
            float x = (column - (columns - 1) * 0.5f) * spacing;

            Vector3 position = transform.position + new Vector3(x, 0f, row * spacing);
            GameObject spawnedEnemy = Instantiate(enemyPrefab, position, Quaternion.identity);

            Enemy enemy = spawnedEnemy.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.SetSpread(x);
                enemy.SetSpeedMultiplier(speedMultiplier);
            }
        }
    }
}
