using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkObject))]
public class NetworkRigidBodyHandler : NetworkBehaviour
{
    private Rigidbody rb;

    public float pushForcePower;
    public float pushTorquePower;

    public float fov;
    public float forwardBoostOnHit;

    public float bumpMultplier;




    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();

        if (IsOwner == false)
        {
            rb.isKinematic = true;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.TryGetComponent(out NetworkRigidBodyHandler networkRb))
        {
            Vector3 dir = (networkRb.transform.position - transform.position).normalized;

            Vector3 impactForce = dir * pushForcePower * bumpMultplier;
            Vector3 impactTorque = dir * pushTorquePower * bumpMultplier;

            ApplyCollision_ServerRpc(networkRb.NetworkObjectId, impactForce, impactTorque);

            if (forwardBoostOnHit > 0)
            {
                Vector3 directionToTarget = (networkRb.transform.position - transform.position).normalized;

                float dotProduct = Vector3.Dot(transform.forward, directionToTarget);
                float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

                if (angle <= fov)
                {
                    rb.AddForce(transform.forward * forwardBoostOnHit, ForceMode.Acceleration);
                }
            }
        }
    }

    [ServerRpc]
    private void ApplyCollision_ServerRpc(ulong otherNetworkObjectId, Vector3 forceToApply, Vector3 torqueToApply)
    {
        ApplyCollision_ClientRpc(otherNetworkObjectId, forceToApply, torqueToApply);
    }

    [ClientRpc]
    private void ApplyCollision_ClientRpc(ulong otherNetworkObjectId, Vector3 forceToApply, Vector3 torqueToApply)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects[otherNetworkObjectId].TryGetComponent(out NetworkRigidBodyHandler networkRb) && networkRb.IsOwner)
        {
            networkRb.rb.AddForce(forceToApply, ForceMode.Acceleration);
            networkRb.rb.AddTorque(new Vector3(0, torqueToApply.y, 0), ForceMode.Acceleration);
        }
    }
}
