using System.Collections.Generic;
using UnityEngine;

public class TrackSpawner : MonoBehaviour
{
    [SerializeField] private Segment segmentPrefab;
    [SerializeField] private float segmentLength = 90f;
    [SerializeField] private float startZ = 20f;
    [SerializeField] private int segmentsAhead = 3;

    private readonly Queue<Segment> active = new Queue<Segment>();
    private Transform player;
    private int nextIndex;

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
        Vector3 position = new Vector3(0f, 0f, startZ + nextIndex * segmentLength);
        Segment segment = Instantiate(segmentPrefab, position, Quaternion.identity);
        segment.Configure(nextIndex);

        active.Enqueue(segment);
        nextIndex++;
    }
}
