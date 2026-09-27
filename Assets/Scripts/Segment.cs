using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Segment : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] private Gate gateLeft;
    [SerializeField] private Gate gateRight;
    [SerializeField] private EnemyWave wave;

    [Header("Gates")]
    [SerializeField] private int baseAddValue = 3;
    [SerializeField] private int addValuePerSegment = 1;
    [SerializeField] private int tripleMultiplyFrom = 12;

    [Header("Enemies")]
    [SerializeField] private int baseEnemyCount = 6;
    [SerializeField] private int enemiesPerSegment = 3;
    [SerializeField] private float speedStepPerSegment = 0.1f;
    [SerializeField] private float maxSpeedMultiplier = 1.8f;

    [Header("Resolution")]
    [SerializeField] private float undecidedHalfWidth = 0.5f;
    [SerializeField] private float popupAhead = 4f;
    [SerializeField] private float popupHeight = 3f;
    [SerializeField] private float popupRise = 2.5f;
    [SerializeField] private float popupTime = 1.2f;
    [SerializeField] private float popupSize = 9f;

    private Crowd crowd;
    private bool resolved;

    public void Configure(int index)
    {
        ConfigureGates(index);

        int enemyCount = baseEnemyCount + enemiesPerSegment * index;
        int columns = Mathf.Min(3 + index / 2, 6);
        float speedMultiplier = Mathf.Min(1f + speedStepPerSegment * index, maxSpeedMultiplier);
        wave.Configure(enemyCount, columns, speedMultiplier);
    }

    private void Start()
    {
        crowd = FindFirstObjectByType<Crowd>();
    }

    private void Update()
    {
        if (resolved || crowd == null || crowd.IsDefeated) return;
        if (crowd.transform.position.z < gateLeft.transform.position.z) return;

        resolved = true;
        ResolveGates();
    }

    private void ResolveGates()
    {
        float heroX = crowd.transform.position.x;
        bool undecided = Mathf.Abs(heroX) < undecidedHalfWidth;

        Gate chosen;
        if (undecided)
        {
            int leftResult = crowd.PreviewGate(gateLeft.Operation, gateLeft.Value);
            int rightResult = crowd.PreviewGate(gateRight.Operation, gateRight.Value);
            chosen = leftResult <= rightResult ? gateLeft : gateRight;
        }
        else
        {
            chosen = heroX < 0f ? gateLeft : gateRight;
        }

        Gate rejected = chosen == gateLeft ? gateRight : gateLeft;

        crowd.ApplyGate(chosen.Operation, chosen.Value);
        chosen.ShowChosen();
        rejected.ShowRejected();

        string heading = undecided ? "<b>UNDECIDED</b>\n" : "";
        string crowdMax = crowd.IsFull ? " MAX" : "";
        string text = $"{heading}{chosen.Label}\n<size=60%>Crowd {crowd.Followers.Count}{crowdMax}</size>";

        StartCoroutine(Popup(text, chosen.DisplayColor, chosen.transform.position.x));

        string side = undecided ? "undecided" : (chosen == gateLeft ? "left" : "right");
        Debug.Log($"Gate {chosen.Label} ({side}), crowd now {crowd.Followers.Count}");
    }

    private IEnumerator Popup(string text, Color color, float x)
    {
        GameObject popupObject = new GameObject("GatePopup");
        TextMeshPro popup = popupObject.AddComponent<TextMeshPro>();
        popup.text = text;
        popup.alignment = TextAlignmentOptions.Center;
        popup.fontSize = popupSize;
        popup.color = color;
        popup.outlineWidth = 0.2f;
        popup.outlineColor = Color.black;
        popup.rectTransform.sizeDelta = new Vector2(8f, 5f);

        float elapsed = 0f;
        while (elapsed < popupTime)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / popupTime);

            if (crowd != null)
            {
                popupObject.transform.position = new Vector3(
                    x,
                    popupHeight + popupRise * progress,
                    crowd.transform.position.z + popupAhead);
            }

            Color faded = color;
            faded.a = 1f - progress * progress;
            popup.color = faded;

            yield return null;
        }

        Destroy(popupObject);
    }

    private void ConfigureGates(int index)
    {
        int addValue = baseAddValue + addValuePerSegment * index;
        int smallAdd = Mathf.Max(1, addValue / 3);
        int subtractValue = 2 + index / 2;
        int multiplyValue = index >= tripleMultiplyFrom ? 3 : 2;

        List<int> patterns = new List<int> { 0 };
        if (index >= 2) patterns.Add(1);
        if (index >= 3)
        {
            patterns.Add(2);
            patterns.Add(3);
        }
        if (index >= 6) patterns.Add(4);

        int pattern = patterns[Random.Range(0, patterns.Count)];

        GateOperation operationA;
        GateOperation operationB;
        int valueA;
        int valueB;

        switch (pattern)
        {
            case 1:
                operationA = GateOperation.Add; valueA = addValue;
                operationB = GateOperation.Multiply; valueB = multiplyValue;
                break;
            case 2:
                operationA = GateOperation.Multiply; valueA = multiplyValue;
                operationB = GateOperation.Subtract; valueB = subtractValue;
                break;
            case 3:
                operationA = GateOperation.Add; valueA = addValue;
                operationB = GateOperation.Subtract; valueB = subtractValue;
                break;
            case 4:
                operationA = GateOperation.Subtract; valueA = subtractValue;
                operationB = GateOperation.Divide; valueB = 2;
                break;
            default:
                operationA = GateOperation.Add; valueA = addValue;
                operationB = GateOperation.Add; valueB = smallAdd;
                break;
        }

        bool swap = Random.value < 0.5f;

        if (swap)
        {
            gateLeft.SetGate(operationB, valueB);
            gateRight.SetGate(operationA, valueA);
        }
        else
        {
            gateLeft.SetGate(operationA, valueA);
            gateRight.SetGate(operationB, valueB);
        }
    }
}
