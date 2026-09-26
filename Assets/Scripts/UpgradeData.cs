using UnityEngine;

public enum UpgradeType { FireRate, Damage, ExtraShot, HeroHealth, Recruit }

[CreateAssetMenu(fileName = "Upgrade", menuName = "Inferno/Upgrade")]
public class UpgradeData : ScriptableObject
{
    public string title = "Upgrade";
    [TextArea] public string description = "";
    public UpgradeType type;
    public float amount = 1f;
}
