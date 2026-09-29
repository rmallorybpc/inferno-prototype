using System.Collections.Generic;
using UnityEngine;

public class Crowd : MonoBehaviour
{
    public enum Formation { Behind, Ahead }

    [SerializeField] private GameObject followerPrefab;
    [SerializeField] private float spacing = 0.6f;
    [SerializeField] private float followerHeight = -0.5f;
    [SerializeField] private float regroupSpeed = 6f;
    [SerializeField] private int heroHealth = 5;
    [SerializeField] private int maxFollowers = 150;
    [SerializeField, Range(0.2f, 1f)] private float behindDepth = 0.45f;

    private readonly List<Transform> followers = new List<Transform>();
    private Formation formation = Formation.Behind;
    private PlayerRunner runner;

    public int Count => followers.Count + 1;
    public int HeroHealth => heroHealth;
    public bool IsDefeated { get; private set; }
    public bool IsFull => followers.Count >= maxFollowers;
    public IReadOnlyList<Transform> Followers => followers;

    private void Awake()
    {
        runner = GetComponent<PlayerRunner>();
    }

    private void Update()
    {
        if (IsDefeated) return;
        Arrange();
    }

    public int PreviewGate(GateOperation operation, int value)
    {
        int total = Count;
        int result;

        switch (operation)
        {
            case GateOperation.Add: result = total + value; break;
            case GateOperation.Subtract: result = total - value; break;
            case GateOperation.Multiply: result = total * value; break;
            case GateOperation.Divide: result = value > 0 ? total / value : total; break;
            default: result = total; break;
        }

        return Mathf.Clamp(result, 1, maxFollowers + 1);
    }

    public void ApplyGate(GateOperation operation, int value)
    {
        int change = PreviewGate(operation, value) - Count;

        if (change > 0) Add(change);
        else if (change < 0) Remove(-change);
    }

    public void Add(int amount)
    {
        if (IsDefeated || followerPrefab == null) return;

        for (int i = 0; i < amount && !IsFull; i++)
        {
            GameObject follower = Instantiate(followerPrefab, transform);
            follower.transform.localPosition = new Vector3(0f, followerHeight, 0f);
            followers.Add(follower.transform);
        }
    }

    public void Remove(int amount)
    {
        for (int i = 0; i < amount && followers.Count > 0; i++)
        {
            int last = followers.Count - 1;
            Transform follower = followers[last];
            followers.RemoveAt(last);
            if (follower != null) Destroy(follower.gameObject);
        }
    }

    public void Heal(int amount)
    {
        if (IsDefeated) return;
        heroHealth += amount;
    }

    public void SetFormation(Formation newFormation)
    {
        formation = newFormation;
    }

    public void TakeHit(Transform struck)
    {
        if (IsDefeated) return;

        if (struck != null)
        {
            int index = followers.IndexOf(struck);
            if (index >= 0)
            {
                followers.RemoveAt(index);
                Destroy(struck.gameObject);
                return;
            }
        }

        if (followers.Count > 0)
        {
            Remove(1);
            return;
        }

        heroHealth--;
        if (heroHealth <= 0) Defeat();
    }

    private void Defeat()
    {
        IsDefeated = true;
        heroHealth = 0;

        if (runner != null) runner.enabled = false;

        foreach (Renderer part in GetComponentsInChildren<Renderer>())
        {
            part.enabled = false;
        }
    }

    private void Arrange()
    {
        float golden = Mathf.PI * (3f - Mathf.Sqrt(5f));
        float side = formation == Formation.Ahead ? 1f : -1f;
        float depth = formation == Formation.Ahead ? 1f : behindDepth;
        float step = regroupSpeed * Time.deltaTime;

        for (int n = 0; n < followers.Count; n++)
        {
            Transform follower = followers[n];
            if (follower == null) continue;

            float angle = (n * golden) % (2f * Mathf.PI) * 0.5f;
            float radius = spacing * Mathf.Sqrt(n + 1) * 1.41f;

            Vector3 target = new Vector3(
                Mathf.Cos(angle) * radius,
                followerHeight,
                Mathf.Sin(angle) * radius * side * depth);

            follower.localPosition = Vector3.MoveTowards(follower.localPosition, target, step);
        }
    }
}
