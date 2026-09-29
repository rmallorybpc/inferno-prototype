using UnityEngine;

public class BurstFlash : MonoBehaviour
{
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private int maxPerSecond = 20;

    private static readonly int ColorId = Shader.PropertyToID("_BaseColor");
    private static float windowStart;
    private static int windowCount;

    private Renderer flashRenderer;
    private MaterialPropertyBlock block;
    private Color baseColor;
    private float elapsed;
    private float targetScale = 1f;

    public static void Spawn(GameObject prefab, Vector3 position, float radius)
    {
        if (prefab == null) return;

        BurstFlash template = prefab.GetComponent<BurstFlash>();
        int cap = template != null ? template.maxPerSecond : 20;

        if (Time.time < windowStart || Time.time - windowStart >= 1f)
        {
            windowStart = Time.time;
            windowCount = 0;
        }

        if (windowCount >= cap) return;
        windowCount++;

        GameObject flash = Instantiate(prefab, position, Quaternion.identity);
        BurstFlash effect = flash.GetComponent<BurstFlash>();
        if (effect != null) effect.targetScale = radius * 2f;
    }

    private void Awake()
    {
        flashRenderer = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
        baseColor = flashRenderer.sharedMaterial.GetColor(ColorId);
        transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsed / duration);

        transform.localScale = Vector3.one * targetScale * Mathf.Lerp(0.3f, 1f, progress);

        Color faded = baseColor;
        faded.a = baseColor.a * (1f - progress);
        block.SetColor(ColorId, faded);
        flashRenderer.SetPropertyBlock(block);

        if (progress >= 1f) Destroy(gameObject);
    }
}
