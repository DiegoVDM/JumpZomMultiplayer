using Unity.Netcode;
using UnityEngine;

public class NetworkTransformTest2D : NetworkBehaviour
{
    [SerializeField] private float radius = 2f;
    [SerializeField] private float period = 6.28f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (!IsServer) return;

        float angle = (Time.time / period) * Mathf.PI * 2f;

        float x = Mathf.Cos(angle) * radius;
        float y = Mathf.Sin(angle) * radius;

        transform.position = startPosition + new Vector3(x, y, 0f);
    }
}