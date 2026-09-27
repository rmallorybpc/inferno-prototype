using System.Collections.Generic;
using UnityEngine;

public class TrackSpawner : MonoBehaviour
{
    [SerializeField] private Segment segmentPrefab;
    [SerializeField] private float segmentLength = 90f;
    [SerializeField] private float startZ = 20f;
    [SerializeField] private int segmentsAhead = 3;

    [Header("Bosses")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private float[] bossTimes = { 150f, 300f, 450f };
    [SerializeField] private int bossBaseHealth = 6000;
    [SerializeField] private float bossHealthGrowth = 2f;
    [Tooltip("Must match the Player's forward speed, so boss timing lines up with the clock.")]
    [SerializeField] private float runSpeed = 8f;

    private readonly Queue<Segment> active = new Queue<Segment>();
    private Transform player;
    private int nextIndex;
    private int nextBoss;

    public int SegmentAt(float z)
    {
        return Mathf.Max(0, Mathf.FloorToInt((z - startZ) / segmentLength));
    }

    private void Start()
    {
        Crowd crowd = FindFirstObjectByType<Crowd>();
        if (crowd != null) player = crowd.transform;

        for (int i = 0; i < segmentsAhead; i++)
        {
            SpawnNext();
        }
    }

    private void Update()
    {
        if (player == null) return;

        float frontZ = startZ + nextIndex * segmentLength;
        if (frontZ - player.position.z < segmentsAhead * segmentLength)
        {
            SpawnNext();
        }

        while (active.Count > 0 &&
               player.position.z - active.Peek().transform.position.z > segmentLength * 1.5f)
        {
            Destroy(active.Dequeue().gameObject);
        }
    }

    private void SpawnNext()
    {
        float segmentStart = startZ + nextIndex * segmentLength;
        Vector3 position = new Vector3(0f, 0f, segmentStart);
        Segment segment = Instantiate(segmentPrefab, position, Quaternion.identity);

        float arrivalTime = segmentStart / runSpeed;
        bool isBoss = bossPrefab != null &&
                      nextBoss < bossTimes.Length &&
                      arrivalTime >= bossTimes[nextBoss];

        if (isBoss)
        {
            int health = Mathf.RoundToInt(bossBaseHealth * Mathf.Pow(bossHealthGrowth, nextBoss));
            segment.ConfigureBoss(nextIndex, bossPrefab, health);
            Debug.Log($"Boss {nextBoss + 1} placed at segment {nextIndex + 1} with {health} health");
            nextBoss++;
        }
        else
        {
            segment.Configure(nextIndex);
        }

        active.Enqueue(segment);
        nextIndex++;
    }
}
