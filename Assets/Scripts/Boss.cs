using TMPro;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [SerializeField] private string displayName = "BOSS";
    [SerializeField] private float approachSpeed = 4f;
    [SerializeField] private float trackingSpeed = 16f;
    [SerializeField] private float holdDistance = 25f;
    [SerializeField] private float grindInterval = 0.5f;
    [SerializeField] private int hitsPerGrind = 3;
    [SerializeField] private int experienceValue = 20;
    [SerializeField] private float fiveChestChance = 0.1f;
    [SerializeField] private float threeChestChance = 0.3f;
    [SerializeField] private float labelHeight = 1.5f;
    [SerializeField] private float labelSize = 14f;

    private Crowd crowd;
    private PlayerRunner runner;
    private Experience experience;
    private LevelUpScreen levelUpScreen;
    private BoxCollider box;
    private TextMeshPro label;
    private int maxHealth = 1;
    private int health = 1;
    private float speedMultiplier = 1f;
    private float touchingUntil;
    private float grindTimer;
    private bool holding;
    private bool defeated;

    public void Configure(int newHealth, float newSpeedMultiplier)
    {
        maxHealth = Mathf.Max(1, newHealth);
        health = maxHealth;
        speedMultiplier = newSpeedMultiplier;
        UpdateLabel();
    }

    public int TakeDamage(int amount)
    {
        if (defeated || amount <= 0) return 0;

        int used = Mathf.Min(amount, health);
        health -= used;
        UpdateLabel();

        if (health <= 0) Defeat();
        return used;
    }

    private void Awake()
    {
        box = GetComponent<BoxCollider>();

        GameObject labelObject = new GameObject("BossLabel");
        label = labelObject.AddComponent<TextMeshPro>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = labelSize;
        label.color = Color.white;
        label.outlineWidth = 0.2f;
        label.outlineColor = Color.black;
        label.rectTransform.sizeDelta = new Vector2(10f, 4f);
        UpdateLabel();
    }

    private void Start()
    {
        crowd = FindFirstObjectByType<Crowd>();
        levelUpScreen = FindFirstObjectByType<LevelUpScreen>();

        if (crowd != null)
        {
            runner = crowd.GetComponent<PlayerRunner>();
            experience = crowd.GetComponent<Experience>();
        }

        Vector3 position = transform.position;
        position.y = 0f;
        transform.position = position;
    }

    private void Update()
    {
        if (defeated || crowd == null) return;

        Vector3 position = transform.position;
        Vector3 goal = crowd.transform.position;
        float step = approachSpeed * speedMultiplier * Time.deltaTime;

        if (crowd.IsDefeated)
        {
            position.x = Mathf.MoveTowards(position.x, goal.x, step);
            position.z = Mathf.MoveTowards(position.z, goal.z, step);
            transform.position = position;
            return;
        }

        crowd.SetFormation(Crowd.Formation.Ahead);

        if (!holding && position.z - goal.z <= holdDistance)
        {
            holding = true;
            if (runner != null) runner.SetHeld(true);
        }

        position.x = Mathf.MoveTowards(position.x, goal.x, trackingSpeed * speedMultiplier * Time.deltaTime);

        bool touching = Time.time < touchingUntil;

        if (touching)
        {
            grindTimer += Time.deltaTime;
            while (grindTimer >= grindInterval)
            {
                grindTimer -= grindInterval;
                for (int i = 0; i < hitsPerGrind; i++)
                {
                    crowd.TakeHit(null);
                }
            }
        }
        else
        {
            position.z = Mathf.MoveTowards(position.z, goal.z, step);
        }

        transform.position = position;
    }

    private void LateUpdate()
    {
        if (label == null) return;

        float top = box != null ? box.center.y + box.size.y * 0.5f : 4f;
        label.transform.position = transform.position + Vector3.up * (top + labelHeight);
    }

    private void OnTriggerStay(Collider other)
    {
        if (crowd == null || other.attachedRigidbody == null) return;
        if (other.attachedRigidbody.transform != crowd.transform) return;

        touchingUntil = Time.time + 0.15f;
    }

    private void OnDestroy()
    {
        if (runner != null) runner.SetHeld(false);
        if (label != null) Destroy(label.gameObject);
    }

    private void UpdateLabel()
    {
        if (label != null) label.text = $"<size=60%>{displayName}</size>\n{health}";
    }

    private void Defeat()
    {
        defeated = true;

        float roll = Random.value;
        int picks = roll < fiveChestChance ? 5 : roll < fiveChestChance + threeChestChance ? 3 : 1;

        if (levelUpScreen != null) levelUpScreen.GrantTreasure(picks);
        if (experience != null) experience.Add(experienceValue);
        if (crowd != null) crowd.SetFormation(Crowd.Formation.Behind);

        Debug.Log($"Boss defeated, treasure x{picks}");
        Destroy(gameObject);
    }
}
