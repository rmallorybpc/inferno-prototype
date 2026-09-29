using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Hud : MonoBehaviour
{
    [SerializeField] private string heroName = "Hero";
    [SerializeField] private float fontSize = 48f;
    [SerializeField] private float restartDelay = 1f;
    [SerializeField] private int maxUpgradeLines = 2;

    private struct UpgradeEntry
    {
        public string text;
        public bool isMax;
    }

    private Crowd crowd;
    private Experience experience;
    private Shooter shooter;
    private TrackSpawner spawner;
    private LevelUpScreen levelUpScreen;
    private TextMeshProUGUI statusText;
    private TextMeshProUGUI progressText;
    private TextMeshProUGUI centerText;
    private float runTime;
    private float endedTime;
    private bool won;

    private void OnEnable()
    {
        Boss.GuardianDefeated += HandleGuardianDefeated;
    }

    private void OnDisable()
    {
        Boss.GuardianDefeated -= HandleGuardianDefeated;
    }

    private void HandleGuardianDefeated()
    {
        won = true;
    }

    private void Start()
    {
        crowd = FindFirstObjectByType<Crowd>();
        spawner = FindFirstObjectByType<TrackSpawner>();
        levelUpScreen = FindFirstObjectByType<LevelUpScreen>();

        if (crowd != null)
        {
            experience = crowd.GetComponent<Experience>();
            shooter = crowd.GetComponent<Shooter>();
        }

        BuildCanvas();
    }

    private void Update()
    {
        if (crowd == null) return;

        bool ended = won || crowd.IsDefeated;
        if (!ended) runTime += Time.deltaTime;

        int segment = spawner != null ? spawner.SegmentAt(crowd.transform.position.z) + 1 : 0;
        int minutes = (int)(runTime / 60f);
        int seconds = (int)(runTime % 60f);
        string time = $"{minutes}:{seconds:00}";

        string levelLine = experience != null
            ? $"\nLevel {experience.Level}  XP {experience.Current}/{experience.Required}"
            : "";

        string crowdMax = crowd.IsFull ? "  MAX" : "";
        statusText.text = $"{heroName}  Health {crowd.HeroHealth}\nCrowd {crowd.Count}{crowdMax}";
        progressText.text = $"Segment {segment}\n{time}{levelLine}{BuildUpgradeLines()}";

        if (!ended) return;

        if (!centerText.gameObject.activeSelf)
        {
            centerText.gameObject.SetActive(true);
            centerText.text = won ? WinText(time) : LossText(segment, time);
        }

        endedTime += Time.unscaledDeltaTime;
        if (endedTime < restartDelay) return;

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private string WinText(string time)
    {
        string level = experience != null ? $"  Level {experience.Level}" : "";
        return $"CIRCLE CLEARED\n<size=70%>{time}{level}  Crowd {crowd.Count}</size>\n\nTap to restart";
    }

    private string LossText(int segment, string time)
    {
        string levelReached = experience != null ? $", level {experience.Level}" : "";
        return $"Run over\nSegment {segment}, {time}{levelReached}\n\nTap to restart";
    }

    private string BuildUpgradeLines()
    {
        List<UpgradeEntry> entries = new List<UpgradeEntry>();
        bool fireMaxed = shooter != null && shooter.IsFireRateMaxed;

        AddEntry(entries, "Fire", UpgradeType.FireRate, fireMaxed, true);
        AddEntry(entries, "Volley", UpgradeType.ExtraShot, false, true);
        AddEntry(entries, "Heat", UpgradeType.Damage, false, false);

        bool[] selected = new bool[entries.Count];
        int shown = 0;

        for (int i = 0; i < entries.Count && shown < maxUpgradeLines; i++)
        {
            if (entries[i].isMax) continue;
            selected[i] = true;
            shown++;
        }

        for (int i = 0; i < entries.Count && shown < maxUpgradeLines; i++)
        {
            if (selected[i]) continue;
            selected[i] = true;
            shown++;
        }

        string lines = "";
        for (int i = 0; i < entries.Count; i++)
        {
            if (selected[i]) lines += "\n" + entries[i].text;
        }

        return lines;
    }

    private void AddEntry(List<UpgradeEntry> entries, string label, UpgradeType type,
        bool forceMax, bool alwaysShow)
    {
        if (levelUpScreen == null || !levelUpScreen.TryGetLevel(type, out int level, out int maxLevel))
        {
            if (alwaysShow) entries.Add(new UpgradeEntry { text = $"{label} Lv 1", isMax = false });
            return;
        }

        if (!alwaysShow && level < 2) return;

        bool isMax = forceMax || level >= maxLevel;
        string text = isMax ? $"{label} Lv MAX" : $"{label} Lv {level}/{maxLevel}";
        entries.Add(new UpgradeEntry { text = text, isMax = isMax });
    }

    private void BuildCanvas()
    {
        GameObject canvasObject = new GameObject("HudCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        statusText = CreateText(canvas.transform, "Status",
            new Vector2(0f, 1f), new Vector2(40f, -40f), new Vector2(600f, 200f),
            TextAlignmentOptions.TopLeft);

        progressText = CreateText(canvas.transform, "Progress",
            new Vector2(1f, 1f), new Vector2(-40f, -40f), new Vector2(600f, 380f),
            TextAlignmentOptions.TopRight);

        centerText = CreateText(canvas.transform, "Center",
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 600f),
            TextAlignmentOptions.Center);

        centerText.gameObject.SetActive(false);
    }

    private TextMeshProUGUI CreateText(Transform parent, string name, Vector2 anchor,
        Vector2 offset, Vector2 size, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.outlineWidth = 0.25f;
        text.outlineColor = Color.black;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = offset;
        rect.sizeDelta = size;

        return text;
    }
}