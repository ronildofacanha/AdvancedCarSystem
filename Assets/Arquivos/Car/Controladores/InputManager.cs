using System.Collections;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public float vertical;
    public float horizontal;
    public bool handbrake = false;
    public bool driftMode = false;
    public bool burnout = false;
    public float burnoutTime = 0;
    private CarController car;

    private void Start()
    {
        car = GetComponent<CarController>();
    }

    private void FixedUpdate()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        handbrake = (Input.GetAxis("Jump")) !=0? true: false;
        driftMode = (Input.GetAxis("Fire1")) != 0 ? true : false;
        BurnoutAxis();
    }
    public void BurnoutAxis()
    {
        if (handbrake && car.currentKPM < 1)
        {
            if (burnoutTime < 3)
            {
                burnoutTime += Time.deltaTime;
                burnout = true;
            }
            else
            {
                burnout = false;
            }
        }
        else
        {
            burnoutTime = 0;
            burnout = false;
        }
    }
        public void Nitrus()
    {
        if (handbrake && vertical !=0 && car.currentKPM<1)
        {
            if (burnoutTime < 3)
            {
                burnoutTime += Time.deltaTime;
                burnout = true;
            }
            else
            {
                burnout = false;
            
            }
        }
        else
        {
            burnoutTime = 0;
            burnout = false;
        }
    }
}