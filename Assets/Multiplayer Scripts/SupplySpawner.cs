using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class SupplySpawner : NetworkBehaviour
{
    [SerializeField] private SupplyPickup supplyPickupPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float firstSpawnDelay = 1f;
    [SerializeField] private float spawnInterval = 2.5f;
    [SerializeField] private int maxActivePickups = 3;

    private Coroutine spawnRoutine;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public override void OnNetworkDespawn()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(firstSpawnDelay);

        while (true)
        {
            if (CanSpawnAnotherPickup())
            {
                SpawnPickup();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private bool CanSpawnAnotherPickup()
    {
        SupplyPickup[] pickups = FindObjectsByType<SupplyPickup>(FindObjectsSortMode.None);
        return pickups.Length < maxActivePickups;
    }

    private void SpawnPickup()
    {
        if (supplyPickupPrefab == null)
        {
            Debug.LogError("SupplySpawner: supplyPickupPrefab is not assigned.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("SupplySpawner: no spawn points assigned.");
            return;
        }

        Transform chosenPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        SupplyPickup pickupInstance = Instantiate(
            supplyPickupPrefab,
            chosenPoint.position,
            Quaternion.identity
        );

        NetworkObject netObj = pickupInstance.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("SupplySpawner: pickup prefab is missing a NetworkObject.");
            Destroy(pickupInstance.gameObject);
            return;
        }

        netObj.Spawn(true);
    }
}