using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : NetworkBehaviour
{
    private Rigidbody rb;

    public float accelSpeed;
    public float accelDecaySpeed;
    public Vector3 velocityClamp;

    public float speed;
    public float targetSpeed;

    public float steerSpeed;


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
        speed = Mathf.MoveTowards(speed, targetSpeed, accelSpeed * Time.deltaTime);

        if (accelInput != 0)
        {
            targetSpeed += accelInput * accelSpeed * Time.deltaTime;
        }
        else
        {
            targetSpeed = Mathf.MoveTowards(targetSpeed, 0, accelDecaySpeed * Time.deltaTime);
        }

        targetSpeed = Mathf.Clamp(targetSpeed, -velocityClamp.x, velocityClamp.x);

        rb.velocity = transform.forward * speed;



        if (steerInput != 0)
        {
            Quaternion newRot = rb.rotation;
            newRot *= Quaternion.Euler(0, steerInput * steerSpeed * Time.deltaTime, 0);
            rb.rotation = newRot;
        }

        SyncCarPosition_ServerRPC(transform.position, transform.rotation.y);
    }



    [ServerRpc(RequireOwnership = false)]
    private void SyncCarPosition_ServerRPC(Vector3 pos, float rotationY)
    {
        SyncCarPosition_ClientRPC(pos, rotationY);
    }

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
