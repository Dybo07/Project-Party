using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController_Force : NetworkBehaviour
{
    private Rigidbody rb;


    public Vector3 camOffset;
    private Camera cam;
    
    public Color TeamColorMaterialColor
    {
        set
        {
            teamColorMaterialRenderer.material.color = value;
        }
        get
        {
            return teamColorMaterialRenderer.material.color;
        }
    }

    [SerializeField]
    private Renderer teamColorMaterialRenderer;


    public Transform[] wheels;
    public float wheelTurnSpeed;
    public float frontWheelMaxYRot;


    public float accelSpeed;
    public float accelDecaySpeed;
    public float accelDecaySpeedNoGas;
    public Vector3 velocityClamp;

    public float backwardsAccelMultiplier;

    public float steerSpeed;
    public float maxSteerSpeed;

    public float speedMultiplier = 1;
    public bool canMove;
    public bool gameStarted;


    private bool IsMovingBackwards()
    {
        Vector3 forward = transform.forward;
        Vector3 velocity = rb.velocity;

        float dotProduct = Vector3.Dot(forward, velocity);

        return dotProduct < 0;
    }


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
    }


    public float accelInput;
    public void OnAccelarate(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && IsOwner)
        {
            accelInput = ctx.ReadValue<float>();
        }
        if (ctx.canceled && IsOwner)
        {
            accelInput = 0;
        }
    }

    public float steerInput;
    public void OnSteer(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && IsOwner)
        {
            steerInput = ctx.ReadValue<float>();
        }
        if (ctx.canceled && IsOwner)
        {
            steerInput = 0;
        }
    }



    private void Update()
    {
        if (!IsOwner || !gameStarted)
        {
            return;
        }



        if (canMove)
        {
            cam.transform.position = transform.position + camOffset;


            if (accelInput != 0)
            {
                float _speedMultiplier = (IsMovingBackwards() ? backwardsAccelMultiplier : 1) * speedMultiplier;

                rb.AddForce(transform.forward * accelInput * accelSpeed * _speedMultiplier * Time.deltaTime, ForceMode.Acceleration);
            }
        }

        
        if (steerInput != 0)
        {
            float accelDir = accelInput == -1 ? -backwardsAccelMultiplier : 1;

            rb.AddTorque(transform.up * steerInput * accelDir * steerSpeed * Mathf.Clamp(speedMultiplier * 0.75f, 1, int.MaxValue) * Time.deltaTime, ForceMode.Acceleration);
            rb.maxAngularVelocity = maxSteerSpeed * Mathf.Clamp(speedMultiplier * 0.75f, 1, int.MaxValue);
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner || !gameStarted)
        {
            return;
        }

        float _speedMultiplier = IsMovingBackwards() ? backwardsAccelMultiplier : 1;

        Vector3 output = VectorLogic.Clamp(rb.velocity, -velocityClamp * speedMultiplier, velocityClamp * speedMultiplier);
        rb.velocity = new Vector3(output.x, rb.velocity.y, output.z);

        output = VectorLogic.InstantMoveTowards(rb.velocity, Vector3.zero, accelDecaySpeed * _speedMultiplier * .02f);
        rb.velocity = new Vector3(output.x, rb.velocity.y, output.z);

        if (accelInput == 0)
        {
            output = VectorLogic.InstantMoveTowards(rb.velocity, Vector3.zero, accelDecaySpeedNoGas * _speedMultiplier * .02f);
            rb.velocity = new Vector3(output.x, rb.velocity.y, output.z);
        }

        float rotPercentage = Mathf.Clamp(1.1f / maxSteerSpeed * rb.angularVelocity.y, -1, 1) * (accelInput == -1 ? -backwardsAccelMultiplier : 1);


        float wheelSpeed = (accelInput == -1 ? -backwardsAccelMultiplier : 1) * rb.velocity.magnitude * wheelTurnSpeed;

        wheels[0].localRotation = Quaternion.Euler(wheels[0].localEulerAngles.x + wheelSpeed, 30 * rotPercentage, 0);
        wheels[1].localRotation = Quaternion.Euler(wheels[1].localEulerAngles.x + wheelSpeed, 30 * rotPercentage, 0);
        wheels[2].localRotation = Quaternion.Euler(wheels[2].localEulerAngles.x + wheelSpeed, 0, 0);
        wheels[3].localRotation = Quaternion.Euler(wheels[3].localEulerAngles.x + wheelSpeed, 0, 0);

        //sync car position and rotation to the server
        SyncCarPosition_ServerRPC(transform.position, transform.rotation.y);
    }




    public void SpeedSlowBoostCar(float speedMultiplier, float duration, float afterBurnDuration)
    {
        StopAllCoroutines();
        StartCoroutine(SpeedBoostCarTimer(speedMultiplier, duration, afterBurnDuration));
    }
    private IEnumerator SpeedBoostCarTimer(float multiplier, float duration, float afterBurnDuration)
    {
        speedMultiplier = multiplier;

        yield return new WaitForSeconds(duration);

        float afterBurnTimeLeft = afterBurnDuration;
        float speedBuffDifference = multiplier - 1;
        while (true)
        {
            yield return null;
            afterBurnTimeLeft -= Time.deltaTime;

            speedMultiplier = 1 + speedBuffDifference / afterBurnDuration * afterBurnTimeLeft;

            if (afterBurnTimeLeft <= 0)
            {
                speedMultiplier = 1;
                yield break;
            }
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
