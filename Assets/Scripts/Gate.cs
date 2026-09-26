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

    private TextMeshPro label;
    private Renderer gateRenderer;
    private bool used;

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

    private void Refresh()
    {
        Color color = IsHelpful ? helpfulColor : harmfulColor;

        if (label != null)
        {
            label.text = Label;
            label.color = color;
        }

        if (gateRenderer != null)
        {
            gateRenderer.material.color = color;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (!other.CompareTag("Player")) return;

        Crowd crowd = other.GetComponent<Crowd>();
        if (crowd == null) return;

        used = true;
        crowd.ApplyGate(operation, value);
        Debug.Log($"Gate passed: {Label}, crowd now {crowd.Count}");

        gameObject.SetActive(false);
        if (label != null) label.gameObject.SetActive(false);
    }
}
