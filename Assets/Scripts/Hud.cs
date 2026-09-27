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

    private Crowd crowd;
    private Experience experience;
    private Shooter shooter;
    private TrackSpawner spawner;
    private LevelUpScreen levelUpScreen;
    private TextMeshProUGUI statusText;
    private TextMeshProUGUI progressText;
    private TextMeshProUGUI centerText;
    private float runTime;
    private float defeatedTime;

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

        if (!crowd.IsDefeated) runTime += Time.deltaTime;

        int segment = spawner != null ? spawner.SegmentAt(crowd.transform.position.z) + 1 : 0;
        int minutes = (int)(runTime / 60f);
        int seconds = (int)(runTime % 60f);
        string time = $"{minutes}:{seconds:00}";

        string levelLine = experience != null
            ? $"\nLevel {experience.Level}  XP {experience.Current}/{experience.Required}"
            : "";

        bool fireMaxed = shooter != null && shooter.IsFireRateMaxed;
        string fireLine = "\n" + UpgradeLine("Fire", UpgradeType.FireRate, fireMaxed);
        string volleyLine = "\n" + UpgradeLine("Volley", UpgradeType.ExtraShot, false);

        string crowdMax = crowd.IsFull ? "  MAX" : "";
        statusText.text = $"{heroName}  Health {crowd.HeroHealth}\nCrowd {crowd.Followers.Count}{crowdMax}";
        progressText.text = $"Segment {segment}\n{time}{levelLine}{fireLine}{volleyLine}";

        if (!crowd.IsDefeated) return;

        if (!centerText.gameObject.activeSelf)
        {
            string levelReached = experience != null ? $", level {experience.Level}" : "";
            centerText.gameObject.SetActive(true);
            centerText.text = $"Run over\nSegment {segment}, {time}{levelReached}\n\nTap to restart";
        }

        defeatedTime += Time.deltaTime;
        if (defeatedTime < restartDelay) return;

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private string UpgradeLine(string label, UpgradeType type, bool forceMax)
    {
        if (levelUpScreen == null || !levelUpScreen.TryGetLevel(type, out int level, out int maxLevel))
        {
            return $"{label} Lv 1";
        }

        if (forceMax || level >= maxLevel) return $"{label} Lv MAX";
        return $"{label} Lv {level}/{maxLevel}";
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
