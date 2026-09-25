using UnityEngine;

public class CombatZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        SetCombat(other.gameObject, true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        SetCombat(other.gameObject, false);
    }

    private void SetCombat(GameObject player, bool active)
    {
        Crowd crowd = player.GetComponent<Crowd>();
        if (crowd != null)
        {
            crowd.SetFormation(active ? Crowd.Formation.Ahead : Crowd.Formation.Behind);
        }

        PlayerRunner runner = player.GetComponent<PlayerRunner>();
        if (runner != null)
        {
            runner.SetSteeringLocked(active);
        }

        Debug.Log(active ? "Combat started, steering locked" : "Combat ended, steering unlocked");
    }
}
