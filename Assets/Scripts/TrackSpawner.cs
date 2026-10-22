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
    [Tooltip("Health for each boss in order. If there are more bosses than entries, the last entry is reused.")]
    [SerializeField] private int[] bossHealths = { 6000, 28000, 60000 };
    [Tooltip("Must match the Player's forward speed, so boss timing lines up with the clock.")]
    [SerializeField] private float runSpeed = 8f;

    [Header("Guardian")]
    [SerializeField] private GameObject guardianPrefab;
    [SerializeField] private float guardianTime = 600f;
    [SerializeField] private int guardianHealth = 500000;

    private readonly Queue<Segment> active = new Queue<Segment>();
    private Transform player;
    private int nextIndex;
    private int nextBoss;
    private bool guardianPlaced;

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
        if (!guardianPlaced && frontZ - player.position.z < segmentsAhead * segmentLength)
        {
            SpawnNext();
        }

        while (active.Count > 0 &&
               player.position.z - active.Peek().transform.position.z > segmentLength * 1.5f)
        {
            Destroy(active.Dequeue().gameObject);
        }
    }

    private int BossHealth(int bossIndex)
    {
        if (bossHealths == null || bossHealths.Length == 0) return 6000;
        return bossHealths[Mathf.Min(bossIndex, bossHealths.Length - 1)];
    }

    private void SpawnNext()
    {
        if (guardianPlaced) return;

        float segmentStart = startZ + nextIndex * segmentLength;
        Vector3 position = new Vector3(0f, 0f, segmentStart);
        Segment segment = Instantiate(segmentPrefab, position, Quaternion.identity);

        float playerZ = player != null ? player.position.z : 0f;
        float arrivalTime = Time.timeSinceLevelLoad + (segmentStart - playerZ) / runSpeed;

        bool isGuardian = guardianPrefab != null && arrivalTime >= guardianTime;

        bool isBoss = !isGuardian &&
                      bossPrefab != null &&
                      nextBoss < bossTimes.Length &&
                      arrivalTime >= bossTimes[nextBoss];

        if (isGuardian)
        {
            segment.ConfigureBoss(nextIndex, guardianPrefab, guardianHealth);
            Debug.Log($"Guardian placed at segment {nextIndex + 1} with {guardianHealth} health");
            guardianPlaced = true;
        }
        else if (isBoss)
        {
            int health = BossHealth(nextBoss);
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
