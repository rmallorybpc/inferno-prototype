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
    [SerializeField] private Color cardColor = new Color(0.25f, 0.12f, 0.08f, 0.95f);
    [SerializeField] private Color bossCardColor = new Color(0.5f, 0.35f, 0.05f, 0.95f);

    private readonly List<UpgradeData> pool = new List<UpgradeData>();
    private readonly List<UpgradeData> fallbacks = new List<UpgradeData>();
    private readonly List<UpgradeData> offered = new List<UpgradeData>();
    private readonly Dictionary<UpgradeData, int> picks = new Dictionary<UpgradeData, int>();
    private readonly List<Button> cardButtons = new List<Button>();
    private readonly List<TextMeshProUGUI> cardTexts = new List<TextMeshProUGUI>();
    private readonly List<Vector2> cardPositions = new List<Vector2>();

    private Experience experience;
    private Crowd crowd;
    private Shooter shooter;
    private GameObject panel;
    private TextMeshProUGUI headingText;
    private int levelPending;
    private int bossRewardPending;
    private int bossesDefeated;

    public bool TryGetLevel(UpgradeType type, out int level, out int maxLevel)
    {
        foreach (UpgradeData upgrade in pool)
        {
            if (upgrade.type != type || upgrade.maxPicks <= 0) continue;

            level = PicksOf(upgrade);
            maxLevel = upgrade.maxPicks;
            return true;
        }

        level = 1;
        maxLevel = 0;
        return false;
    }

    public void GrantBossReward()
    {
        bossesDefeated++;
        bossRewardPending++;
        if (!panel.activeSelf) ShowCards();
    }

    private void Start()
    {
        foreach (UpgradeData upgrade in Resources.LoadAll<UpgradeData>("Upgrades"))
        {
            if (upgrade.isFallback) fallbacks.Add(upgrade);
            else pool.Add(upgrade);
        }

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
        levelPending++;
        if (!panel.activeSelf) ShowCards();
    }

    private int PicksOf(UpgradeData upgrade)
    {
        return picks.TryGetValue(upgrade, out int count) ? count : 0;
    }

    private bool IsAvailable(UpgradeData upgrade)
    {
        if (upgrade.unlockAfterBoss > bossesDefeated) return false;
        if (upgrade.maxPicks > 0 && PicksOf(upgrade) >= upgrade.maxPicks) return false;

        switch (upgrade.type)
        {
            case UpgradeType.FireRate:
                return shooter == null || !shooter.IsFireRateMaxed;
            case UpgradeType.Recruit:
                return crowd == null || !crowd.IsFull;
            default:
                return true;
        }
    }

    private void ShowCards()
    {
        bool bossReward = bossRewardPending > 0;
        List<UpgradeData> candidates = new List<UpgradeData>();

        if (bossReward)
        {
            foreach (UpgradeData upgrade in pool)
            {
                if (upgrade.unlockAfterBoss == bossesDefeated && IsAvailable(upgrade)) candidates.Add(upgrade);
            }
        }

        bool newUnlock = candidates.Count > 0;

        if (candidates.Count == 0)
        {
            foreach (UpgradeData upgrade in pool)
            {
                if (IsAvailable(upgrade)) candidates.Add(upgrade);
            }
        }

        if (candidates.Count == 0) candidates.AddRange(fallbacks);

        if (candidates.Count == 0)
        {
            levelPending = 0;
            bossRewardPending = 0;
            panel.SetActive(false);
            Time.timeScale = 1f;
            return;
        }

        if (bossReward)
        {
            string unlocked = newUnlock ? "\n<size=55%>New upgrade</size>" : "";
            headingText.text = $"Boss defeated{unlocked}";
        }
        else
        {
            headingText.text = "Level up";
        }

        offered.Clear();
        int count = bossReward ? 1 : Mathf.Min(choices, candidates.Count);

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
            if (!visible) continue;

            cardTexts[i].text = CardText(offered[i]);

            RectTransform rect = (RectTransform)cardButtons[i].transform;
            bool centered = offered.Count == 1 && cardPositions.Count > 1;
            rect.anchoredPosition = centered ? cardPositions[1] : cardPositions[i];

            Image background = (Image)cardButtons[i].targetGraphic;
            background.color = bossReward ? bossCardColor : cardColor;
        }

        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    private string CardText(UpgradeData upgrade)
    {
        string rank = upgrade.maxPicks > 0
            ? $"  Lv {PicksOf(upgrade) + 2}/{upgrade.maxPicks}"
            : "";

        return $"<b>{upgrade.title}</b>{rank}\n<size=70%>{upgrade.description}</size>";
    }

    private void Choose(int index)
    {
        if (index >= offered.Count) return;

        UpgradeData chosen = offered[index];
        picks[chosen] = PicksOf(chosen) + 1;
        Apply(chosen);

        if (bossRewardPending > 0) bossRewardPending--;
        else levelPending--;

        if (bossRewardPending + levelPending > 0)
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
            case UpgradeType.Burst:
                if (shooter != null) shooter.AddBurst(whole);
                break;
            case UpgradeType.Hellfire:
                if (shooter != null) shooter.AddHellfire(whole);
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

        headingText = CreateLabel(panel.transform, "Level up", new Vector2(0f, 600f), new Vector2(900f, 220f), fontSize * 1.4f);

        for (int i = 0; i < choices; i++)
        {
            int index = i;
            Vector2 position = new Vector2(0f, 250f - i * 330f);

            Button button = CreateCard(panel.transform, position, out TextMeshProUGUI text);
            button.onClick.AddListener(() => Choose(index));

            cardButtons.Add(button);
            cardTexts.Add(text);
            cardPositions.Add(position);
        }

        panel.SetActive(false);
    }

    private Button CreateCard(Transform parent, Vector2 position, out TextMeshProUGUI text)
    {
        GameObject cardObject = new GameObject("Card");
        cardObject.transform.SetParent(parent, false);

        Image background = cardObject.AddComponent<Image>();
        background.color = cardColor;

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
