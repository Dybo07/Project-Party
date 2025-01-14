using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkObject))]
public class NetworkRigidBodyHandler : NetworkBehaviour
{
    private Rigidbody rb;

    public float pushForcePower;
    public float impactForcePower;
    public float pushTorquePower;

    public float bumpMultplier;

    public Vector3 prevVel;
    public Vector3 currentVel;

    public float maxPowerNerfer;



    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();

        if (IsOwner == false)
        {
            rb.isKinematic = true;
        }
    }

    private void FixedUpdate()
    {
        currentVel = rb.velocity;
        prevVel = currentVel;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.TryGetComponent(out NetworkRigidBodyHandler networkRb))
        {
            Vector3 dir = (networkRb.transform.position - transform.position).normalized;

            Vector3 impactForce = dir * pushForcePower * bumpMultplier * prevVel.magnitude / maxPowerNerfer;
            impactForce += dir * impactForcePower * bumpMultplier;

            Vector3 impactTorque = dir * pushTorquePower * bumpMultplier * prevVel.magnitude / maxPowerNerfer;

            ApplyCollision_ServerRpc(networkRb.NetworkObjectId, impactForce, impactTorque);
        }
    }
    private void OnCollisionStay(Collision other)
    {
        if (other.transform.TryGetComponent(out NetworkRigidBodyHandler networkRb))
        {
            Vector3 dir = (networkRb.transform.position - transform.position).normalized;

            Vector3 impactForce = dir * pushForcePower * bumpMultplier * prevVel.magnitude / maxPowerNerfer;
            impactForce += dir * impactForcePower * bumpMultplier;

            Vector3 impactTorque = dir * pushTorquePower * bumpMultplier * prevVel.magnitude / maxPowerNerfer;

            ApplyCollision_ServerRpc(networkRb.NetworkObjectId, impactForce, impactTorque);
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
