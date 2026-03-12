using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class MultiPScript : NetworkBehaviour
    {
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

        public override void OnNetworkSpawn()
        {
            Position.OnValueChanged += OnStateChanged;

            if (IsOwner)
            {
                transform.position = Position.Value;
            }
        }

        public override void OnNetworkDespawn()
        {
            Position.OnValueChanged -= OnStateChanged;
        }

        private void OnStateChanged(Vector3 previous, Vector3 current)
        {
            transform.position = current;
        }

        public void Move()
        {
            if (IsServer)
            {
                ApplyRandomPosition();
            }
            else
            {
                SubmitPositionRequestServerRpc();
            }
        }

        [Rpc(SendTo.Server)]
        private void SubmitPositionRequestServerRpc(RpcParams rpcParams = default)
        {
            ApplyRandomPosition();
        }

        private void ApplyRandomPosition()
        {
            Vector3 randomPosition = GetRandomPositionOnPlane();
            transform.position = randomPosition;
            Position.Value = randomPosition;
        }
        //ok
        private static Vector3 GetRandomPositionOnPlane()
        {
            return new Vector3(Random.Range(-3f, 3f), Random.Range(-2f, 2f), 0f);
        }
    }
}