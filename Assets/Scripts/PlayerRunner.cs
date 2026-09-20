using UnityEngine;

public class PlayerRunner : MonoBehaviour
{
    [SerializeField] private float forwardSpeed = 8f;

    private void Update()
    {
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.World);
    }
}