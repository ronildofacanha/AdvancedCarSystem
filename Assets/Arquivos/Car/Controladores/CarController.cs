using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

/*
 * 1ª marcha – 20 km/h.
 * 2ª marcha – 40 km/h.
 * 3ª marcha – 60 km/h.
 * 4ª marcha – 80 km/h.
 * 5ª marcha – 100 km/h.
 */

public class CarController : MonoBehaviour
{
    #region
    internal enum DriveType
    {
        frontDrive,
        rearDrive,
        allDrive
    }

    [Header("Config Wheels")]
    public WheelCollider[] wheels;
    public Transform[] meshes;

    [Header("Config Basic")]
    public float acceleration = 500f;
    public float maxSpeed = 60f;
    public float steeringAngle = 30f;
    public float brakeTorque = 1000f;

    [Header("Config Drift Mode")]
    [SerializeField] private DriveType driveType;
    public float driftAcceleration = 1000f;
    public float driftMaxSpeed = 40.0f;
    public float maxInclineAngle = 45.0f;
    public float wheelFrontSideStiffness = 0.1f;
    public float wheelRearSideStiffness = 0.1f;

    [Header("Config Others")]
    public float changeForce = 0.3f;
    public int statusEngine = 0;
    public float currentSpeed = 0f;
    public float currentRPM = 0.0f;
    public float radiusWheelsController = 6f;
    public bool isEngineStart = false;
    public bool isBreake = false;
    public bool isChange = false;
    public bool leftWheelsTouchingGround = false;
    public bool rightWheelsTouchingGround = false;
    public bool allWheelsTouchingGround = false;

    [Header("Physics")]
    public float centerOfMassOffset = 0.5f;
    public float twoWheelsCenter = 0.85f;
    public float downforce = 100.0f;

    #region ___PRIVATE GAME___
    private float auxAcceleration = 0f;
    private float auxMaxSpeed = 0f;
    private int previousStatusEngine = 0;
    private Rigidbody car;
    private float horizontalInput = 0f;
    private float verticalInput = 0f;
    private float torque = 0f;
    private InputManager IM;

    #endregion // END
    #endregion
    void Start()
    {
        GetComponents();
        auxAcceleration = acceleration;
        auxMaxSpeed = maxSpeed;
    }

    void GetComponents()
    {
        car = GetComponent<Rigidbody>();
        IM = GetComponent<InputManager>();
    }

    IEnumerator AutoRotationCar(float i)
    {
        if (allWheelsTouchingGround)
        {
            yield return null;
        }
        else
        {
            yield return new WaitForSeconds(i);

            if (allWheelsTouchingGround)
            {
                yield return null;
            }
            else
            {
                car.transform.rotation = Quaternion.identity;
                print("CAPOTOU");
            }
        }
    }

    public void TouchingGround()
    {
        // wheels[0] = FL;
        // wheels[2] = RL;
        // wheels[3] = RR;
        // wheels[1] = FR;
        leftWheelsTouchingGround = wheels[0].isGrounded || wheels[2].isGrounded;
        rightWheelsTouchingGround = wheels[1].isGrounded || wheels[3].isGrounded;
        allWheelsTouchingGround = wheels[0].isGrounded || wheels[2].isGrounded || wheels[1].isGrounded || wheels[3].isGrounded;
    }
    void TwoWheelCar()
    {
        if (leftWheelsTouchingGround && !rightWheelsTouchingGround && transform.eulerAngles.z >= maxInclineAngle)
        {
            //print("RODAS ESQUERDAS");
            car.centerOfMass = new Vector3(-twoWheelsCenter, 0, 0);
            print(car.centerOfMass);
            return;
        }
        else if (!leftWheelsTouchingGround && rightWheelsTouchingGround && transform.eulerAngles.z >= -maxInclineAngle)
        {
            //print("RODAS DIREITAS");
            car.centerOfMass = new Vector3(twoWheelsCenter, 0f, 0);
            print(car.centerOfMass);
            return;
        }
        else
        {
            if (allWheelsTouchingGround)
            {
                car.centerOfMass = new Vector3(0f, centerOfMassOffset, 0);
            }
            else
            {
                StartCoroutine(AutoRotationCar(5f));
            }
        }
    }

    void Burnout()
    {
        /*
         * "Burnout"
         * 5.000 RPM
        */
        WheelFrictionCurve wfcUp = wheels[3].sidewaysFriction;
        WheelFrictionCurve wfcDown = wheels[3].sidewaysFriction;

        if (IM.driftMode)
        {
            acceleration = driftAcceleration;
            maxSpeed = driftMaxSpeed;

            wfcUp.stiffness = wheelFrontSideStiffness;
            wfcDown.stiffness = wheelRearSideStiffness;

            wheels[0].sidewaysFriction = wfcUp;
            wheels[1].sidewaysFriction = wfcUp;
            //
            wheels[2].sidewaysFriction = wfcDown;
            wheels[3].sidewaysFriction = wfcDown;
        }
        else
        {
            acceleration = auxAcceleration;
            maxSpeed = auxMaxSpeed;
            wfcUp.stiffness = 1f;
            wfcDown.stiffness = 1f;

            wheels[0].sidewaysFriction = wfcUp;
            wheels[1].sidewaysFriction = wfcUp;
            //
            wheels[2].sidewaysFriction = wfcDown;
            wheels[3].sidewaysFriction = wfcDown;
        }
    }

    void FrontWheelsController()
    {
        if (IM.horizontal > 0)
        {
            wheels[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radiusWheelsController + (1.5f / 2))) * IM.horizontal;
            wheels[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radiusWheelsController - (1.5f / 2))) * IM.horizontal;
        }
        else if(IM.horizontal < 0)
        {
            wheels[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radiusWheelsController - (1.5f / 2))) * IM.horizontal;
            wheels[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radiusWheelsController + (1.5f / 2))) * IM.horizontal;
        }
        else
        {
            wheels[0].steerAngle = 0;
            wheels[1].steerAngle = 0;
        }

        UpdateWheel(wheels[0], meshes[0]);
        UpdateWheel(wheels[1], meshes[1]);
        UpdateWheel(wheels[2], meshes[2]);
        UpdateWheel(wheels[3], meshes[3]);

    

    }
    void Update()
    {

        if (Mathf.Abs(torque) > 0 && !isEngineStart)
        {
            StartCoroutine(IEStartEngine());
        }

    }
    void FixedUpdate()
    {
        if (IM.handbrake)
        {
            BreakeStart(0);
        }
        else
        {
            BreakePause();
        }

        torque = IM.vertical * acceleration;
        car.AddForce(-transform.up * downforce * car.velocity.magnitude);

        TouchingGround();
        TwoWheelCar();
        Burnout();
        SpeedCarKMH_RPM();
        ChangeTorque();
        FrontWheelsController();
    }
    void BreakeStart(int option)
    {
        isBreake = true;

        if (option == 1)
        {
            foreach (WheelCollider wheel in wheels)
            {
                wheel.brakeTorque = brakeTorque * changeForce;
            }

            return;
        }
        else if(option == 0)
        {
            foreach (WheelCollider wheel in wheels)
            {
                wheel.brakeTorque = brakeTorque;
            }
        }
    }
    void BreakePause()
    {
        isBreake = false;

        wheels[0].brakeTorque = 0;
        wheels[1].brakeTorque = 0;
        wheels[2].brakeTorque = 0;
        wheels[3].brakeTorque = 0;
    }

    void ChangeTorque()
    {
        // Add TORQUE
        if (currentSpeed < maxSpeed && currentSpeed > -maxSpeed && !isBreake)
        {
            if (driveType == DriveType.allDrive)
            {
                foreach (WheelCollider wheel in wheels)
                {
                    wheel.motorTorque = torque;
                }
            }
        }

        // Change
        if (currentSpeed == 0 && isEngineStart)
        {
            statusEngine = 0;
            return;
        }
        else if (currentSpeed > 0 && currentSpeed < 20)
        {
            statusEngine = 1;
            return;
        }
        else if (currentSpeed > 20 && currentSpeed < 40)
        {
            statusEngine = 2;
            return;
        }
        else if (currentSpeed > 40 && currentSpeed < 60)
        {
            statusEngine = 3;
            return;
        }
        else if (currentSpeed > 60 && currentSpeed < 80)
        {
            statusEngine = 4;
            return;
        }
        else if (currentSpeed > 80 && currentSpeed < 100)
        {
            statusEngine = 5;
            return;
        }
        else if (currentSpeed > 100 && currentSpeed < 120)
        {
            statusEngine = 6;
            return;
        }

        AutoGear();
    }

    private void AutoGear()
    {
        if (statusEngine >1 && statusEngine != previousStatusEngine)
        {
            previousStatusEngine = statusEngine;
            StartCoroutine(IEAutoCharge());
        }
    }

    void SpeedCarKMH_RPM()
    {
        // km/h
        currentSpeed = car.velocity.magnitude * 3.6f;
        currentSpeed = ((int)currentSpeed);
        // RPM
        currentRPM = 2 * Mathf.PI * wheels[0].radius * wheels[0].rpm * 60 / 1000;
        currentRPM = ((int)currentRPM);

        if (currentSpeed > maxSpeed)
        {
            float maxSpeedMS = maxSpeed / 3.6f;
            car.velocity = car.velocity.normalized * maxSpeedMS;
            StartCoroutine(IEAutoCharge());
            // MAX_RPM
        }
    }
    void UpdateWheel(WheelCollider coll, Transform mesh)
    {
        Quaternion quat;
        Vector3 pos;
        coll.GetWorldPose(out pos, out quat);
        mesh.transform.position = pos;
        mesh.transform.rotation = quat;
    }
    public IEnumerator IEStartEngine()
    {
        statusEngine = 1;
        BreakeStart(0);
        yield return new WaitForSeconds(0.5f);
        isEngineStart = true;
        yield return new WaitForSeconds(0.5f);
        BreakePause();
    }

    public IEnumerator IEAutoCharge()
    {
        BreakeStart(1);
        yield return new WaitForSeconds(changeForce);
        BreakePause();
    }
}
