using UnityEngine;

public class Gate : MonoBehaviour
{
    [SerializeField] private int value = 3;

    private bool used;

    private void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (!other.CompareTag("Player")) return;

        used = true;
        Debug.Log($"Gate passed: +{value}");
        gameObject.SetActive(false);
    }
}
