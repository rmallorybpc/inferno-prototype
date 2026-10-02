using UnityEngine;

public class HellfirePatch : MonoBehaviour
{
    [SerializeField] private float tickInterval = 0.5f;
    [SerializeField] private float fadeTime = 0.4f;
    [SerializeField] private int maxPerSecond = 6;
    [SerializeField] private int maxAlive = 24;

    private static readonly int ColorId = Shader.PropertyToID("_BaseColor");
    private static readonly Collider[] hits = new Collider[64];
    private static float windowStart;
    private static int windowCount;
    private static int alive;

    private Renderer patchRenderer;
    private MaterialPropertyBlock block;
    private Color baseColor;
    private float radius = 1f;
    private int damage = 1;
    private float duration = 2f;
    private float elapsed;
    private float tickTimer;

    public static void Spawn(GameObject prefab, Vector3 position, float radius, int damage, float duration)
    {
        if (prefab == null) return;

        HellfirePatch template = prefab.GetComponent<HellfirePatch>();
        if (template == null) return;

        if (alive < 0) alive = 0;

        if (Time.time < windowStart || Time.time - windowStart >= 1f)
        {
            windowStart = Time.time;
            windowCount = 0;
        }

        if (windowCount >= template.maxPerSecond || alive >= template.maxAlive) return;
        windowCount++;

        GameObject patchObject = Instantiate(prefab, position, Quaternion.identity);
        patchObject.transform.localScale = new Vector3(radius * 2f, prefab.transform.localScale.y, radius * 2f);

        HellfirePatch patch = patchObject.GetComponent<HellfirePatch>();
        patch.radius = radius;
        patch.damage = damage;
        patch.duration = duration;
    }

    private void Awake()
    {
        alive++;
        patchRenderer = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
        baseColor = patchRenderer.sharedMaterial.GetColor(ColorId);
    }

    private void OnDestroy()
    {
        alive--;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        tickTimer += Time.deltaTime;

        while (tickTimer >= tickInterval)
        {
            tickTimer -= tickInterval;
            Burn();
        }

        float fadeStart = duration - fadeTime;
        float strength = elapsed < fadeStart ? 1f : Mathf.Clamp01((duration - elapsed) / fadeTime);

        Color faded = baseColor;
        faded.a = baseColor.a * strength;
        block.SetColor(ColorId, faded);
        patchRenderer.SetPropertyBlock(block);

        if (elapsed >= duration) Destroy(gameObject);
    }

    private void Burn()
    {
        Vector3 center = transform.position + Vector3.up * 0.5f;
        int count = Physics.OverlapSphereNonAlloc(center, radius, hits, ~0, QueryTriggerInteraction.Collide);

        for (int i = 0; i < count; i++)
        {
            Enemy enemy = hits[i].GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                continue;
            }

            Boss boss = hits[i].GetComponent<Boss>();
            if (boss != null) boss.TakeDamage(damage);
        }
    }
}
