using System;
using UnityEngine;

public class Experience : MonoBehaviour
{
    [SerializeField] private int baseRequirement = 5;
    [SerializeField] private int requirementPerLevel = 3;

    public int Level { get; private set; } = 1;
    public int Current { get; private set; }
    public int Required => baseRequirement + requirementPerLevel * (Level - 1);

    public event Action<int> LeveledUp;

    public void Add(int amount)
    {
        Current += amount;

        while (Current >= Required)
        {
            Current -= Required;
            Level++;
            Debug.Log($"Level up: {Level}");
            LeveledUp?.Invoke(Level);
        }
    }
}
