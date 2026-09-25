using UnityEngine;

public class CombatZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Crowd crowd = other.GetComponent<Crowd>();
        if (crowd != null) crowd.SetFormation(Crowd.Formation.Ahead);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Crowd crowd = other.GetComponent<Crowd>();
        if (crowd != null) crowd.SetFormation(Crowd.Formation.Behind);
    }
}
