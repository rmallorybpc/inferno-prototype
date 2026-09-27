using System.Collections;
using TMPro;
using UnityEngine;

public enum GateOperation { Add, Subtract, Multiply, Divide }

public class Gate : MonoBehaviour
{
    [SerializeField] private GateOperation operation = GateOperation.Add;
    [SerializeField] private int value = 3;
    [SerializeField] private float labelHeight = 2.2f;
    [SerializeField] private float labelSize = 10f;
    [SerializeField] private Color helpfulColor = new Color(0.3f, 0.6f, 1f);
    [SerializeField] private Color harmfulColor = new Color(1f, 0.55f, 0.1f);
    [SerializeField] private float pulseScale = 1.15f;
    [SerializeField] private float pulseTime = 0.25f;
    [SerializeField] private float dimAmount = 0.35f;

    private TextMeshPro label;
    private Renderer gateRenderer;

    public GateOperation Operation => operation;
    public int Value => value;
    public Color DisplayColor => IsHelpful ? helpfulColor : harmfulColor;

    public string Label
    {
        get
        {
            switch (operation)
            {
                case GateOperation.Subtract: return $"-{value}";
                case GateOperation.Multiply: return $"×{value}";
                case GateOperation.Divide: return $"÷{value}";
                default: return $"+{value}";
            }
        }
    }

    private bool IsHelpful => operation == GateOperation.Add || operation == GateOperation.Multiply;

    private void Awake()
    {
        gateRenderer = GetComponent<Renderer>();

        Collider gateCollider = GetComponent<Collider>();
        if (gateCollider != null) gateCollider.enabled = false;

        GameObject labelObject = new GameObject("GateLabel");
        label = labelObject.AddComponent<TextMeshPro>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = labelSize;
        label.rectTransform.sizeDelta = new Vector2(6f, 3f);

        labelObject.transform.SetParent(transform.parent, false);
        labelObject.transform.position = transform.position + Vector3.up * labelHeight;

        Refresh();
    }

    public void SetGate(GateOperation newOperation, int newValue)
    {
        operation = newOperation;
        value = newValue;
        Refresh();
    }

    public void ShowChosen()
    {
        StartCoroutine(PulseAndHide());
    }

    public void ShowRejected()
    {
        Color dimmed = DisplayColor * dimAmount;
        dimmed.a = 1f;

        if (gateRenderer != null) gateRenderer.material.color = dimmed;
        if (label != null) label.color = dimmed;
    }

    private void Refresh()
    {
        if (label != null)
        {
            label.text = Label;
            label.color = DisplayColor;
        }

        if (gateRenderer != null) gateRenderer.material.color = DisplayColor;
    }

    private IEnumerator PulseAndHide()
    {
        Vector3 baseScale = transform.localScale;
        Vector3 bigScale = new Vector3(baseScale.x * pulseScale, baseScale.y * pulseScale, baseScale.z);

        if (gateRenderer != null)
        {
            gateRenderer.material.color = Color.Lerp(DisplayColor, Color.white, 0.5f);
        }

        float elapsed = 0f;
        while (elapsed < pulseTime)
        {
            elapsed += Time.deltaTime;
            float wave = Mathf.Sin(Mathf.Clamp01(elapsed / pulseTime) * Mathf.PI);
            transform.localScale = Vector3.Lerp(baseScale, bigScale, wave);
            yield return null;
        }

        transform.localScale = baseScale;
        gameObject.SetActive(false);
        if (label != null) label.gameObject.SetActive(false);
    }
}
