using UnityEngine;

public class Gate : MonoBehaviour
{
    [SerializeField] private int value = 3;

    private bool used;

    private void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (!other.CompareTag("Player")) return;

        Crowd crowd = other.GetComponent<Crowd>();
        if (crowd == null) return;

        used = true;
        crowd.Add(value);
        Debug.Log($"Gate passed: +{value}, crowd now {crowd.Count}");
        gameObject.SetActive(false);
    }
}
