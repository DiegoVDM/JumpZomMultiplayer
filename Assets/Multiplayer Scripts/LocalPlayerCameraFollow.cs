using Unity.Netcode;
using UnityEngine;

public class LocalPlayerCameraFollow : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        MonoBehaviour[] allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (MonoBehaviour behaviour in allBehaviours)
        {
            if (behaviour.GetType().Name == "CinemachineCamera")
            {
                var so = new UnityEditor.SerializedObject(behaviour);
                var trackingTargetProp = so.FindProperty("TrackingTarget");
                if (trackingTargetProp != null)
                {
                    trackingTargetProp.objectReferenceValue = transform;
                    so.ApplyModifiedProperties();
                    Debug.Log("Assigned local player to CinemachineCamera tracking target.");
                    return;
                }
            }
        }

        Debug.LogWarning("Could not find camera tracking target!!! OH nooo");
    }
}