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

    private readonly List<Transform> followers = new List<Transform>();
    private readonly List<Vector3> targets = new List<Vector3>();
    private Formation formation = Formation.Behind;
    private PlayerRunner runner;
    private MeshRenderer heroRenderer;

    public int Count => followers.Count + 1;
    public int HeroHealth => heroHealth;
    public bool IsDefeated => heroHealth <= 0;
    public IReadOnlyList<Transform> Followers => followers;

    private void Awake()
    {
        runner = GetComponent<PlayerRunner>();
        heroRenderer = GetComponent<MeshRenderer>();
    }

    public void Add(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject follower = Instantiate(followerPrefab, transform);
            followers.Add(follower.transform);
            targets.Add(Vector3.zero);
        }

        Arrange();

        for (int i = followers.Count - amount; i < followers.Count; i++)
        {
            followers[i].localPosition = targets[i];
        }
    }

    public void Heal(int amount)
    {
        if (IsDefeated) return;

        heroHealth += amount;
        Debug.Log($"Hero healed, health {heroHealth}");
    }

    public void SetFormation(Formation newFormation)
    {
        if (formation == newFormation) return;

        formation = newFormation;
        Arrange();
        Debug.Log($"Formation: {formation}");
    }

    public void TakeHit(Transform struck)
    {
        if (IsDefeated) return;

        if (followers.Count > 0)
        {
            int index = followers.IndexOf(struck);
            if (index < 0) index = followers.Count - 1;

            Destroy(followers[index].gameObject);
            followers.RemoveAt(index);
            targets.RemoveAt(index);
            Arrange();

            Debug.Log($"Follower lost, crowd now {Count}");
            return;
        }

        heroHealth--;
        Debug.Log($"Hero hit, health {heroHealth}");

        if (IsDefeated)
        {
            Debug.Log("Run over");
            if (runner != null) runner.enabled = false;
            if (heroRenderer != null) heroRenderer.enabled = false;
        }
    }

    private void Update()
    {
        float step = regroupSpeed * Time.deltaTime;

        for (int i = 0; i < followers.Count; i++)
        {
            followers[i].localPosition = Vector3.MoveTowards(
                followers[i].localPosition,
                targets[i],
                step);
        }
    }

    private void Arrange()
    {
        const float goldenAngle = 137.5f * Mathf.Deg2Rad;
        float side = formation == Formation.Ahead ? 1f : -1f;

        for (int i = 0; i < followers.Count; i++)
        {
            int n = i + 1;
            float angle = (n * goldenAngle) % (2f * Mathf.PI) * 0.5f;
            float radius = spacing * Mathf.Sqrt(n + 1) * 1.41f;

            targets[i] = new Vector3(
                Mathf.Cos(angle) * radius,
                followerHeight,
                Mathf.Sin(angle) * radius * side);
        }
    }
}
