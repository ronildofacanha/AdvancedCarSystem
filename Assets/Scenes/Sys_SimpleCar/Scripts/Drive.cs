using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drive : MonoBehaviour
{
    public WheelCollider WC;
    public GameObject WM;
    public float torque = 200;

    void Start()
    {
        WC = this.GetComponent<WheelCollider>();
    }

    void Go(float accel)
    {
        accel = Mathf.Clamp(accel,-1,1);
        float thrustTorque = accel * torque;
        WC.motorTorque = thrustTorque;

        Quaternion quat;
        Vector3 pos;
        WC.GetWorldPose(out pos,out quat);
        WM.transform.position = pos;
        WM.transform.localRotation = quat;

    }
    void Update()
    {
        float force = Input.GetAxis("Vertical");
        Go(force);
    }
}
