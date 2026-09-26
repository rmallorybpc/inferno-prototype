using System.Collections.Generic;
using UnityEngine;

public class Segment : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] private Gate gateLeft;
    [SerializeField] private Gate gateRight;
    [SerializeField] private EnemyWave wave;

    [Header("Gates")]
    [SerializeField] private int baseAddValue = 3;
    [SerializeField] private int addValuePerSegment = 1;
    [SerializeField] private int tripleMultiplyFrom = 12;

    [Header("Enemies")]
    [SerializeField] private int baseEnemyCount = 6;
    [SerializeField] private int enemiesPerSegment = 3;
    [SerializeField] private float speedStepPerSegment = 0.1f;
    [SerializeField] private float maxSpeedMultiplier = 1.8f;

    public void Configure(int index)
    {
        ConfigureGates(index);

        int enemyCount = baseEnemyCount + enemiesPerSegment * index;
        int columns = Mathf.Min(3 + index / 2, 6);
        float speedMultiplier = Mathf.Min(1f + speedStepPerSegment * index, maxSpeedMultiplier);
        wave.Configure(enemyCount, columns, speedMultiplier);

        Debug.Log($"Segment {index}: gates {gateLeft.Label} / {gateRight.Label}, {enemyCount} enemies at {speedMultiplier:0.0}x speed");
    }

    private void ConfigureGates(int index)
    {
        int addValue = baseAddValue + addValuePerSegment * index;
        int smallAdd = Mathf.Max(1, addValue / 3);
        int subtractValue = 2 + index / 2;
        int multiplyValue = index >= tripleMultiplyFrom ? 3 : 2;

        List<int> patterns = new List<int> { 0 };
        if (index >= 2) patterns.Add(1);
        if (index >= 3)
        {
            patterns.Add(2);
            patterns.Add(3);
        }
        if (index >= 6) patterns.Add(4);

        int pattern = patterns[Random.Range(0, patterns.Count)];

        GateOperation operationA;
        GateOperation operationB;
        int valueA;
        int valueB;

        switch (pattern)
        {
            case 1:
                operationA = GateOperation.Add; valueA = addValue;
                operationB = GateOperation.Multiply; valueB = multiplyValue;
                break;
            case 2:
                operationA = GateOperation.Multiply; valueA = multiplyValue;
                operationB = GateOperation.Subtract; valueB = subtractValue;
                break;
            case 3:
                operationA = GateOperation.Add; valueA = addValue;
                operationB = GateOperation.Subtract; valueB = subtractValue;
                break;
            case 4:
                operationA = GateOperation.Subtract; valueA = subtractValue;
                operationB = GateOperation.Divide; valueB = 2;
                break;
            default:
                operationA = GateOperation.Add; valueA = addValue;
                operationB = GateOperation.Add; valueB = smallAdd;
                break;
        }

        bool swap = Random.value < 0.5f;

        if (swap)
        {
            gateLeft.SetGate(operationB, valueB);
            gateRight.SetGate(operationA, valueA);
        }
        else
        {
            gateLeft.SetGate(operationA, valueA);
            gateRight.SetGate(operationB, valueB);
        }
    }
}
