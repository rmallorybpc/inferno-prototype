using System.Collections.Generic;
using UnityEngine;

public class Crowd : MonoBehaviour
{
    public enum Formation { Behind, Ahead }

    [SerializeField] private GameObject followerPrefab;
    [SerializeField] private float spacing = 0.6f;
    [SerializeField] private float followerHeight = -0.5f;
    [SerializeField] private float regroupSpeed = 6f;

    private readonly List<Transform> followers = new List<Transform>();
    private readonly List<Vector3> targets = new List<Vector3>();
    private Formation formation = Formation.Behind;

    public int Count => followers.Count + 1;

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

    public void SetFormation(Formation newFormation)
    {
        if (formation == newFormation) return;

        formation = newFormation;
        Arrange();
        Debug.Log($"Formation: {formation}");
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
