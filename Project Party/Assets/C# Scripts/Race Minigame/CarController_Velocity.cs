using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController_Velocity : NetworkBehaviour
{
    private Rigidbody rb;

    public float accelSpeed;
    public float accelDecaySpeed;
    public Vector3 velocityClamp;

    [Header("Multiplier on accelerationSpeed when driving backwards")]
    public float reverseSpeedPercentage;

    public float cSpeed;
    public float targetSpeed;

    public float steerSpeed;
    [Header("How much extra can you steer when car is barely moving")]
    public float lowVelocitySteerMultiplier;

    public float speedMultiplier = 1;
    public bool canMove = true;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    public float accelInput;
    public void OnAccelarate(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !IsOwner)
        {
            accelInput = ctx.ReadValue<float>();
        }
        if (ctx.canceled && !IsOwner)
        {
            accelInput = 0;
        }
    }

    public float steerInput;
    public void OnSteer(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !IsOwner)
        {
            steerInput = ctx.ReadValue<float>();
        }
        if (ctx.canceled && !IsOwner)
        {
            steerInput = 0;
        }
    }

    private void Update()
    {
        if (canMove == false)
        {
            return;
        }

        //multiply speed with reverseSpeedPercentage when reversing and multiply by speedMultiplier
        float _speedMultiplier = ((accelInput < 0) ? reverseSpeedPercentage : 1) * speedMultiplier;

        //accel (move towards) currentSpeed to targetSpeed, according to accelSpeed
        cSpeed = Mathf.MoveTowards(cSpeed, targetSpeed, accelSpeed * _speedMultiplier * Time.deltaTime);

        if (accelInput != 0)
        {
            //if there is input, add input to targetSpeed, according to accelSpeed
            targetSpeed += accelInput * accelSpeed * _speedMultiplier * Time.deltaTime;
        }
        else
        {
            //if there is no input, move targetSpeed back to 0, according to accelDecaySpeed
            targetSpeed = Mathf.MoveTowards(targetSpeed, 0, accelDecaySpeed * _speedMultiplier * Time.deltaTime);
        }

        //clamp speed to maxSpeed (velocityClamp)
        targetSpeed = Mathf.Clamp(targetSpeed, -velocityClamp.x * _speedMultiplier, velocityClamp.x * _speedMultiplier);

        //apply velocity
        rb.velocity = transform.forward * cSpeed;



        //if there is input
        if (steerInput != 0)
        {
            //check how much car can steer based off currentSpeed, and calculates extra steer power on low car velocity
            float steerPowerMultiplier = Mathf.Clamp(Mathf.Abs(cSpeed * lowVelocitySteerMultiplier) / velocityClamp.x, 0, 1);


            Quaternion newRot = rb.rotation;
            newRot *= Quaternion.Euler(0, steerInput * steerSpeed * steerPowerMultiplier * Time.deltaTime, 0);

            //apply rotation
            rb.rotation = newRot;
        }

        //if anything changed, sync car position and rotation to th server
        if (cSpeed != 0 && steerInput != 0)
        {
            SyncCarPosition_ServerRPC(transform.position, transform.rotation.y);
        }
    }    


    //Clients CANT call ClientRpc's so first, a ServerRpc (so the server calls) then a ClientRpc
    [ServerRpc(RequireOwnership = false)]
    private void SyncCarPosition_ServerRPC(Vector3 pos, float rotationY)
    {
        SyncCarPosition_ClientRPC(pos, rotationY);
    }

    //ClientRpc's are called on all clients
    [ClientRpc(RequireOwnership = false)]
    private void SyncCarPosition_ClientRPC(Vector3 pos, float rotationY)
    {
        if (IsOwner == false)
        {
            transform.position = pos;

            Quaternion rot = transform.rotation;
            rot.y = rotationY;
            transform.rotation = rot;
        }
    }
}
