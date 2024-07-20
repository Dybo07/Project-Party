using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEffector : MonoBehaviour
{
    public float speedMultiplier;
    public float speedDuration;
    public float speedAfterBurnDuration;


    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CarController_Force car))
        {
            car.SpeedSlowBoostCar(speedMultiplier, speedDuration, speedAfterBurnDuration);
        }
    }
}
