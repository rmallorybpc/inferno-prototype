using UnityEngine;

public class EnemyWave : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int count = 6;
    [SerializeField] private int columns = 3;
    [SerializeField] private float spacing = 1.2f;
    [SerializeField] private float activationDistance = 65f;

    private float speedMultiplier = 1f;
    private Transform player;
    private bool spawned;

    public void Configure(int newCount, int newColumns, float newSpeedMultiplier)
    {
        count = newCount;
        columns = newColumns;
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
        Spawn();
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
