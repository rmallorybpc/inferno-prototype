using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class LevelUpScreen : MonoBehaviour
{
    [SerializeField] private int choices = 3;
    [SerializeField] private float fontSize = 44f;

    private readonly List<UpgradeData> pool = new List<UpgradeData>();
    private readonly List<UpgradeData> offered = new List<UpgradeData>();
    private readonly List<Button> cardButtons = new List<Button>();
    private readonly List<TextMeshProUGUI> cardTexts = new List<TextMeshProUGUI>();

    private Experience experience;
    private Crowd crowd;
    private Shooter shooter;
    private GameObject panel;
    private int pending;

    private void Start()
    {
        pool.AddRange(Resources.LoadAll<UpgradeData>("Upgrades"));
        if (pool.Count == 0) Debug.LogWarning("No upgrades found in Resources/Upgrades.");

        crowd = FindFirstObjectByType<Crowd>();
        if (crowd != null)
        {
            experience = crowd.GetComponent<Experience>();
            shooter = crowd.GetComponent<Shooter>();
        }

        if (experience != null) experience.LeveledUp += OnLeveledUp;

        EnsureEventSystem();
        BuildPanel();
    }

    private void OnDestroy()
    {
        if (experience != null) experience.LeveledUp -= OnLeveledUp;
        Time.timeScale = 1f;
    }

    private void OnLeveledUp(int level)
    {
        if (pool.Count == 0) return;

        pending++;
        if (!panel.activeSelf) ShowCards();
    }

    private void ShowCards()
    {
        offered.Clear();
        List<UpgradeData> candidates = new List<UpgradeData>(pool);
        int count = Mathf.Min(choices, candidates.Count);

        for (int i = 0; i < count; i++)
        {
            int pick = Random.Range(0, candidates.Count);
            offered.Add(candidates[pick]);
            candidates.RemoveAt(pick);
        }

        for (int i = 0; i < cardButtons.Count; i++)
        {
            bool visible = i < offered.Count;
            cardButtons[i].gameObject.SetActive(visible);

            if (visible)
            {
                cardTexts[i].text = $"<b>{offered[i].title}</b>\n<size=70%>{offered[i].description}</size>";
            }
        }

        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void Choose(int index)
    {
        if (index >= offered.Count) return;

        Apply(offered[index]);
        pending--;

        if (pending > 0)
        {
            ShowCards();
            return;
        }

        panel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void Apply(UpgradeData upgrade)
    {
        int whole = Mathf.RoundToInt(upgrade.amount);

        switch (upgrade.type)
        {
            case UpgradeType.FireRate:
                if (shooter != null) shooter.ImproveFireRate(upgrade.amount);
                break;
            case UpgradeType.Damage:
                if (shooter != null) shooter.AddDamage(whole);
                break;
            case UpgradeType.ExtraShot:
                if (shooter != null) shooter.AddShots(whole);
                break;
            case UpgradeType.HeroHealth:
                if (crowd != null) crowd.Heal(whole);
                break;
            case UpgradeType.Recruit:
                if (crowd != null) crowd.Add(whole);
                break;
        }

        Debug.Log($"Upgrade chosen: {upgrade.title}");
    }

    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();
    }

    private void BuildPanel()
    {
        GameObject canvasObject = new GameObject("LevelUpCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        panel = new GameObject("Panel");
        panel.transform.SetParent(canvasObject.transform, false);

        Image dim = panel.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.7f);
        RectTransform panelRect = dim.rectTransform;
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        CreateLabel(panel.transform, "Level up", new Vector2(0f, 600f), new Vector2(900f, 150f), fontSize * 1.4f);

        for (int i = 0; i < choices; i++)
        {
            int index = i;
            float y = 250f - i * 330f;

            Button button = CreateCard(panel.transform, new Vector2(0f, y), out TextMeshProUGUI text);
            button.onClick.AddListener(() => Choose(index));

            cardButtons.Add(button);
            cardTexts.Add(text);
        }

        panel.SetActive(false);
    }

    private Button CreateCard(Transform parent, Vector2 position, out TextMeshProUGUI text)
    {
        GameObject cardObject = new GameObject("Card");
        cardObject.transform.SetParent(parent, false);

        Image background = cardObject.AddComponent<Image>();
        background.color = new Color(0.25f, 0.12f, 0.08f, 0.95f);

        RectTransform rect = background.rectTransform;
        rect.sizeDelta = new Vector2(860f, 280f);
        rect.anchoredPosition = position;

        Button button = cardObject.AddComponent<Button>();
        button.targetGraphic = background;

        text = CreateLabel(cardObject.transform, "", Vector2.zero, new Vector2(800f, 240f), fontSize);
        return button;
    }

    private TextMeshProUGUI CreateLabel(Transform parent, string content, Vector2 position,
        Vector2 size, float labelSize)
    {
        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(parent, false);

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = content;
        label.fontSize = labelSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;

        RectTransform rect = label.rectTransform;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        return label;
    }
}
