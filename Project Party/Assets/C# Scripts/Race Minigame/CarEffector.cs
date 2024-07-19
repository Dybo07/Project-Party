using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEffector : MonoBehaviour
{
    public List<CarController_Force> cars;

    public float speedMultiplier;
    public float speedDuration;
    public float speedAfterBurnDuration;


    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CarController_Force car) && cars.Contains(car) == false)
        {
            car.SpeedSlowBoostCar(speedMultiplier, speedDuration, speedAfterBurnDuration);
            cars.Add(car);
        }
    }
}
