using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float fireInterval = 0.5f;
    [SerializeField] private float fireHeight = 0.5f;
    [SerializeField] private float muzzleForward = 0.6f;

    private Crowd crowd;
    private float timer;

    private void Awake()
    {
        crowd = GetComponent<Crowd>();
    }

    private void Update()
    {
        if (crowd != null && crowd.IsDefeated) return;

        timer += Time.deltaTime;
        if (timer < fireInterval) return;
        timer -= fireInterval;

        Fire(transform);

        if (crowd == null) return;

        foreach (Transform follower in crowd.Followers)
        {
            Fire(follower);
        }
    }

    private void Fire(Transform shooter)
    {
        Vector3 origin = new Vector3(
            shooter.position.x,
            fireHeight,
            shooter.position.z + muzzleForward);

        Instantiate(projectilePrefab, origin, Quaternion.identity);
    }
}
