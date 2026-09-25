using TMPro;
using UnityEngine;

public class Gate : MonoBehaviour
{
    [SerializeField] private int value = 3;
    [SerializeField] private float labelHeight = 2.2f;
    [SerializeField] private float labelSize = 10f;

    private TextMeshPro label;
    private bool used;

    public int Value => value;

    private void Awake()
    {
        GameObject labelObject = new GameObject("GateLabel");
        label = labelObject.AddComponent<TextMeshPro>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = labelSize;
        label.rectTransform.sizeDelta = new Vector2(6f, 3f);

        labelObject.transform.SetParent(transform.parent, false);
        labelObject.transform.position = transform.position + Vector3.up * labelHeight;

        UpdateLabel();
    }

    public void SetValue(int newValue)
    {
        value = newValue;
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (label != null) label.text = $"+{value}";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (!other.CompareTag("Player")) return;

        Crowd crowd = other.GetComponent<Crowd>();
        if (crowd == null) return;

        used = true;
        crowd.Add(value);
        Debug.Log($"Gate passed: +{value}, crowd now {crowd.Count}");

        gameObject.SetActive(false);
        if (label != null) label.gameObject.SetActive(false);
    }
}
