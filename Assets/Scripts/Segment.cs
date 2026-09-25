using UnityEngine;

public class Segment : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] private Gate gateLeft;
    [SerializeField] private Gate gateRight;
    [SerializeField] private EnemyWave wave;

    [Header("Gates")]
    [SerializeField] private int baseGoodValue = 3;
    [SerializeField] private int goodValuePerSegment = 1;
    [SerializeField] private int basePoorValue = 1;

    [Header("Enemies")]
    [SerializeField] private int baseEnemyCount = 6;
    [SerializeField] private int enemiesPerSegment = 3;
    [SerializeField] private float speedStepPerSegment = 0.1f;
    [SerializeField] private float maxSpeedMultiplier = 1.8f;

    public void Configure(int index)
    {
        int goodValue = baseGoodValue + goodValuePerSegment * index;
        int poorValue = basePoorValue + index / 2;

        bool goodOnLeft = Random.value < 0.5f;
        gateLeft.SetValue(goodOnLeft ? goodValue : poorValue);
        gateRight.SetValue(goodOnLeft ? poorValue : goodValue);

        int enemyCount = baseEnemyCount + enemiesPerSegment * index;
        int columns = Mathf.Min(3 + index / 2, 6);
        float speedMultiplier = Mathf.Min(1f + speedStepPerSegment * index, maxSpeedMultiplier);
        wave.Configure(enemyCount, columns, speedMultiplier);

        Debug.Log($"Segment {index}: gates +{gateLeft.Value} / +{gateRight.Value}, {enemyCount} enemies at {speedMultiplier:0.0}x speed");
    }
}
