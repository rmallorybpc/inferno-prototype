using UnityEngine;

public class EnemyWave : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int count = 6;
    [SerializeField] private int columns = 3;
    [SerializeField] private float spacing = 1.2f;

    private void Start()
    {
        for (int i = 0; i < count; i++)
        {
            int row = i / columns;
            int column = i % columns;
            float x = (column - (columns - 1) * 0.5f) * spacing;

            Vector3 position = transform.position + new Vector3(x, 0f, row * spacing);
            GameObject spawned = Instantiate(enemyPrefab, position, Quaternion.identity);

            Enemy enemy = spawned.GetComponent<Enemy>();
            if (enemy != null) enemy.SetSpread(x);
        }
    }
}
