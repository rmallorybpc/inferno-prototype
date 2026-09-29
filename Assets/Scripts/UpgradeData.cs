using UnityEngine;

public enum UpgradeType { FireRate, Damage, ExtraShot, HeroHealth, Recruit, Burst }

[CreateAssetMenu(fileName = "Upgrade", menuName = "Inferno/Upgrade")]
public class UpgradeData : ScriptableObject
{
    public string title = "Upgrade";
    [TextArea] public string description = "";
    public UpgradeType type;
    public float amount = 1f;

    [Tooltip("How many times this card can be taken in one run. 0 means no limit.")]
    public int maxPicks = 0;

    [Tooltip("Only offered when every other card is maxed out.")]
    public bool isFallback = false;

    [Tooltip("Hidden until this many bosses are defeated. 0 means available from the start.")]
    public int unlockAfterBoss = 0;
}
