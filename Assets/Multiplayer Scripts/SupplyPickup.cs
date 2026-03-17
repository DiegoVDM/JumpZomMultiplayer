using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(Collider2D))]
public class SupplyPickup : NetworkBehaviour
{
    [SerializeField] private float lifetime = 8f;

    private bool collected = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(DespawnAfterTime());
        }
    }

    private IEnumerator DespawnAfterTime()
    {
        yield return new WaitForSeconds(lifetime);

        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer || collected) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
        {
            player = other.GetComponentInParent<PlayerController>();
        }

        if (player == null) return;

        collected = true;

        if (TeamScoreManager.Instance != null)
        {
            TeamScoreManager.Instance.AddPoint(1);
        }

        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }
}