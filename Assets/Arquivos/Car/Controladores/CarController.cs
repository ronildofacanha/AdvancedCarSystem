using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

/*
 * 1� marcha � 40 km/h.
 * 2� marcha � 60 km/h.
 * 3� marcha � 100 km/h.
 * 4� marcha � 160 km/h.
 * 5� marcha � 180 km/h.
 */

public class CarController : MonoBehaviour
{
    #region Variables
    internal enum DriveType
    {
        frontDrive,
        rearDrive,
        allDrive
    }
    internal enum BreakeType
    {
        frontDrive,
        rearDrive,
        allDrive
    }
    [Header("Engine")]
    public float totalTorque;
    public AnimationCurve enginePower;
    public float wheelsRPM;
    public float engineRPM;
    public float smoothTime;
    public float[] gears;
    public int gearNum = 0;
    public float handBrakeFrictionMultiplier = 2f;

    [Header("Config Wheels")]
    [SerializeField] private DriveType driveType;
    [SerializeField] private BreakeType breakeType;
    public GameObject WM, WC;
    public WheelCollider[] wheels;
    public Transform[] wMeshes;

    [Header("Config Car")]
    public float maxKPM = 180f;
    public float brakeTorque = 3000f;
    public float acceleration = 0.3f;
    public float[] angularDrag;

    [Header("Config Drift")]
    public float addAcceleration = 1.5f;
    public float SDF_F, SDF_R;
    public float FWF_R = 0.1f;
    public float SDF_Default, FWF_Default;
    public float maxDriftAngle = 30, driftForce = 10;
    public float maxStabilizingForce = 200f;

    [Header("Auto")]
    public GameObject centerOfMass; // Altura do centro de massa do carro
    public float downforce = 10.0f;
    public float AntiRoll = 5000.0f;

    [Header("Monitore")]
    public float currentKPM = 0f;
    public float currentRPM = 0.0f;
    public bool isEngineStart = false;
    public bool leftWheelsTouchingGround = false;
    public bool rightWheelsTouchingGround = false;
    public bool allWheelsTouchingGround = false;

    #region ___PRIVATE GAME___
    private Rigidbody _car;
    private InputManager IM;
    private WheelFrictionCurve _SDF_F, _SDF_R, _FWF_R, _SDF_Default, _FWF_Default;
    private WheelFrictionCurve forwardFriction, sidewaysFriction;
    private float driftFactor;
    private float radius = 6;
    private float radiusWheelsController = 6f;
    private float _acceleration = 0.3f;

    #endregion // END PRIVATE
    #endregion // ALL 
    void Start()
    {
        GetGameObject();
        StartVariables();
    }


    void Update()
    {
        if (Mathf.Abs(IM.vertical) != 0 && gearNum == 0)
        {
            StartCoroutine(IE_StartEngine());
        }

        Shifter();
    }
    void FixedUpdate()
    {
        //TwoWheelsDriving();

        if (IM.burnout)
        {
            Burnout();
        }
        else
        {
            Default();
        }

        if (IM.driftMode)
        {
            DriftMode();
        }
        if (currentKPM >= maxKPM * 0.3f)
        {
            AdjustTraction();
        }
        
        TorqueBrake();
        TouchingGround();
        KPH_RPM();
        TorqueTypeForce();
        SteerVehicle();
        CalculateEngineTorque();
        AllForces();
    }
    void StartVariables()
    {
        _SDF_F = wheels[3].sidewaysFriction; _SDF_F.stiffness = SDF_F;

        _SDF_R = wheels[3].sidewaysFriction; _SDF_R.stiffness = SDF_R;

        _FWF_R = wheels[3].forwardFriction; _FWF_R.stiffness = FWF_R;

        _SDF_Default = wheels[3].sidewaysFriction; _SDF_Default.stiffness = SDF_Default;

        _FWF_Default = wheels[3].forwardFriction; _FWF_Default.stiffness = FWF_Default;

        _acceleration = acceleration;
        _car.angularDrag = angularDrag[1];
        gearNum = 0;
        Default();
    }
    void GetGameObject()
    {
        _car = GetComponent<Rigidbody>();
        IM = GetComponent<InputManager>();
        // get wheels in this.gameobject
        WC = GameObject.Find("WC");
        WM = GameObject.Find("WM");

        wheels = new WheelCollider[4];
        wMeshes = new Transform[4];

        wheels[0] = WC.transform.Find("FrontLeftWheel").gameObject.GetComponent<WheelCollider>();
        wMeshes[0] = WM.transform.Find("FrontLeftWheel").gameObject.GetComponent<Transform>();

        wheels[1] = WC.transform.Find("FrontRightWheel").gameObject.GetComponent<WheelCollider>();
        wMeshes[1] = WM.transform.Find("FrontRightWheel").gameObject.GetComponent<Transform>();

        wheels[2] = WC.transform.Find("RearLeftWheel").gameObject.GetComponent<WheelCollider>();
        wMeshes[2] = WM.transform.Find("RearLeftWheel").gameObject.GetComponent<Transform>();

        wheels[3] = WC.transform.Find("RearRightWheel").gameObject.GetComponent<WheelCollider>();
        wMeshes[3] = WM.transform.Find("RearRightWheel").gameObject.GetComponent<Transform>();
    }
    void AllForces()
    {
        if (_car != null)
        {
            //COM
            centerOfMass = gameObject.transform.Find("COM").gameObject;
            _car.centerOfMass = centerOfMass.transform.localPosition;

            //DOWNFORCE
            _car.AddForce(-transform.up * downforce * _car.velocity.magnitude);

            // ANT-ROLL
            WheelHit hit;
            float travelL = 1.0f;
            float travelR = 1.0f;

            bool groundedL = wheels[0].GetGroundHit(out hit);
            if (groundedL)
            {
                travelL = (-wheels[0].transform.InverseTransformPoint(hit.point).y - wheels[0].radius) / wheels[0].suspensionDistance;
            }

            bool groundedR = wheels[1].GetGroundHit(out hit);
            if (groundedR)
            {
                travelR = (-wheels[1].transform.InverseTransformPoint(hit.point).y - wheels[1].radius) / wheels[1].suspensionDistance;
            }

            float antiRollForce = (travelL - travelR) * AntiRoll;

            if (groundedL)
                _car.AddForceAtPosition(wheels[0].transform.up * -antiRollForce, wheels[0].transform.position);

            if (groundedR)
                _car.AddForceAtPosition(wheels[1].transform.up * antiRollForce, wheels[1].transform.position);
        }
    }
    void CalculateEngineTorque()
    {
        wheelRPM();
        totalTorque = enginePower.Evaluate(engineRPM) * ((gears[gearNum]) * acceleration) * IM.vertical;
        float velocity = 0.0f;
        engineRPM = Mathf.SmoothDamp(engineRPM, 1000 + (Mathf.Abs(wheelsRPM) * 3.6f * (gears[gearNum])), ref velocity, smoothTime);
    }
    void wheelRPM()
    {
        float sum = 0;
        int R = 0;
        for (int i = 0; i < 4; i++)
        {
            sum += wheels[i].rpm;
            R++;
        }
        wheelsRPM = (R != 0) ? sum / R : 0;
    }
    void TouchingGround()
    {

        leftWheelsTouchingGround = wheels[0].isGrounded || wheels[2].isGrounded;
        rightWheelsTouchingGround = wheels[1].isGrounded || wheels[3].isGrounded;
        allWheelsTouchingGround = wheels[0].isGrounded || wheels[2].isGrounded || wheels[1].isGrounded || wheels[3].isGrounded;

        if (!allWheelsTouchingGround)
        {
            StartCoroutine(IE_AutoRotationCar(5f));
        }
    }
    void DriftMode()
    {
        if (currentKPM < 20 && IM.horizontal!=0)
        {
            acceleration = addAcceleration;
            _car.angularDrag = angularDrag[0];
        }
        // sidewaysFriction                 |      forwardFriction
        wheels[0].sidewaysFriction = _SDF_F; wheels[0].forwardFriction = _SDF_F;
        wheels[1].sidewaysFriction = _SDF_F; wheels[1].forwardFriction = _SDF_F;
        //
        wheels[2].sidewaysFriction = _SDF_R; wheels[2].forwardFriction = _FWF_R;
        wheels[3].sidewaysFriction = _SDF_R; wheels[3].forwardFriction = _FWF_R;
        // Torque
        wheels[0].brakeTorque = 0;
        wheels[1].brakeTorque = 0;
        wheels[2].brakeTorque = brakeTorque*0.2f;
        wheels[3].brakeTorque = brakeTorque*0.2f;
    }
    void Default()
    {

        // //SDF_Default
        wheels[0].sidewaysFriction = _SDF_Default;
        wheels[1].sidewaysFriction = _SDF_Default;
        wheels[2].sidewaysFriction = _SDF_Default;
        wheels[3].sidewaysFriction = _SDF_Default;
        // FWF_Default
        wheels[0].forwardFriction = _FWF_Default;
        wheels[1].forwardFriction = _FWF_Default;
        wheels[2].forwardFriction = _FWF_Default;
        wheels[3].forwardFriction = _FWF_Default;
        // Brake
        wheels[0].brakeTorque = 0f;
        wheels[1].brakeTorque = 0f;
        wheels[2].brakeTorque = 0f;
        wheels[3].brakeTorque = 0f;
        // AC Default
        acceleration = _acceleration;
        _car.angularDrag = angularDrag[0];
    }
    void SteerVehicle()
    {
        if (IM.horizontal > 0)
        {
            //rear tracks size is set to 1.5f       wheel base has been set to 2.55f
            wheels[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * IM.horizontal;
            wheels[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * IM.horizontal;
        }
        else if (IM.horizontal < 0)
        {
            wheels[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * IM.horizontal;
            wheels[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * IM.horizontal;
            //transform.Rotate(Vector3.up * steerHelping);

        }
        else
        {
            wheels[0].steerAngle = 0;
            wheels[1].steerAngle = 0;
        }


        UpdateWheel(wheels[0], wMeshes[0]);
        UpdateWheel(wheels[1], wMeshes[1]);
        UpdateWheel(wheels[2], wMeshes[2]);
        UpdateWheel(wheels[3], wMeshes[3]);

        // Aplica estabilização às rodas do veículo
        foreach (WheelCollider wheel in wheels)
        {
            // Aplica força de estabilização para manter o veículo na pista
            Vector3 wheelDirection = wheel.transform.up;
            Vector3 stabilizingForce = -wheelDirection * maxStabilizingForce;
            wheel.attachedRigidbody.AddForceAtPosition(stabilizingForce, wheel.transform.position);
        }
    }
    void TorqueBrake()
    {
        if (IM.handbrake && !IM.driftMode && !IM.burnout && ((int)currentKPM) != 0)
        {
            BrakeStart(1);
        }

        if (IM.vertical == 0 && !IM.burnout && ((int)currentKPM) != 0)
        {
            BrakeStart(0.3f); //30% 
        }
    }
    void AdjustTraction()
    {
        //tine it takes to go from normal drive to drift 
        float driftSmothFactor = .7f * Time.deltaTime;

        if (IM.handbrake)
        {
            sidewaysFriction = wheels[0].sidewaysFriction;
            forwardFriction = wheels[0].forwardFriction;

            float velocity = 0;
            sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = forwardFriction.extremumValue = forwardFriction.asymptoteValue =
                Mathf.SmoothDamp(forwardFriction.asymptoteValue, driftFactor * handBrakeFrictionMultiplier, ref velocity, driftSmothFactor);

            for (int i = 0; i < 4; i++)
            {
                wheels[i].sidewaysFriction = sidewaysFriction;
                wheels[i].forwardFriction = forwardFriction;
            }

            sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = forwardFriction.extremumValue = forwardFriction.asymptoteValue = 1.1f;
            //extra grip for the front wheels
            for (int i = 0; i < 2; i++)
            {
                wheels[i].sidewaysFriction = sidewaysFriction;
                wheels[i].forwardFriction = forwardFriction;
            }
            GetComponent<Rigidbody>().AddForce(transform.forward * (currentKPM / 400) * 10000);
        }
        //executed when handbrake is being held
        else
        {
            forwardFriction = wheels[0].forwardFriction;
            sidewaysFriction = wheels[0].sidewaysFriction;

            forwardFriction.extremumValue = forwardFriction.asymptoteValue = sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue =
                ((currentKPM * handBrakeFrictionMultiplier) / 300) + 1;

            for (int i = 0; i < 4; i++)
            {
                wheels[i].forwardFriction = forwardFriction;
                wheels[i].sidewaysFriction = sidewaysFriction;

            }
        }

        //checks the amount of slip to control the drift
        for (int i = 2; i < 4; i++)
        {

            WheelHit wheelHit;

            wheels[i].GetGroundHit(out wheelHit);

            if (wheelHit.sidewaysSlip < 0) driftFactor = (1 + -IM.horizontal) * Mathf.Abs(wheelHit.sidewaysSlip);

            if (wheelHit.sidewaysSlip > 0) driftFactor = (1 + IM.horizontal) * Mathf.Abs(wheelHit.sidewaysSlip);
        }

    }
    void Shifter()
    {
        if (Input.GetKeyUp(KeyCode.E) && gearNum >= 0)
        {
            gearNum += 1;
        }
        else if (Input.GetKeyUp(KeyCode.Q) && gearNum >= 0)
        {
            gearNum -= 1;
        }
    }
    void BrakeStart(float force) // force = 0.5f | 50%
    {
        if (allWheelsTouchingGround)
        {
            if (breakeType == BreakeType.allDrive)
            {
                foreach (WheelCollider wheel in wheels)
                {
                    wheel.brakeTorque = ((brakeTorque * force) / 4);
                }
            }
            else if (driveType == DriveType.frontDrive)
            {
                wheels[0].brakeTorque = ((brakeTorque * force) / 2);
                wheels[1].brakeTorque = ((brakeTorque * force) / 2);
                wheels[2].brakeTorque = 0;
                wheels[3].brakeTorque = 0;
            }
            else if (driveType == DriveType.rearDrive)
            {
                wheels[0].brakeTorque = 0;
                wheels[1].brakeTorque = 0;
                wheels[2].brakeTorque = ((brakeTorque * force) / 2);
                wheels[3].brakeTorque = ((brakeTorque * force) / 2);
            }
        }
    }
    void BrakePause()
    {
        wheels[0].brakeTorque = 0;
        wheels[1].brakeTorque = 0;
        wheels[2].brakeTorque = 0;
        wheels[3].brakeTorque = 0;
    }
    void TorqueTypeForce()
    {
        if (currentKPM < maxKPM && currentKPM > -maxKPM && gearNum != 0)
        {
            if (driveType == DriveType.allDrive)
            {
                foreach (WheelCollider wheel in wheels)
                {
                    wheel.motorTorque = (totalTorque / 4);
                }
            }
            else if (driveType == DriveType.frontDrive)
            {
                wheels[0].motorTorque = (totalTorque / 2);
                wheels[1].motorTorque = (totalTorque / 2);
            }
            else if (driveType == DriveType.rearDrive)
            {
                wheels[2].motorTorque = (totalTorque / 2);
                wheels[3].motorTorque = (totalTorque / 2);
            }
        }
    }
    void KPH_RPM()
    {
        // km/h
        currentKPM = _car.velocity.magnitude * 3.6f;
        currentKPM = ((int)currentKPM);
        // RPM
        currentRPM = 2 * Mathf.PI * wheels[0].radius * wheels[0].rpm * 60 / 1000;
        currentRPM = ((int)currentRPM);

        if (currentKPM > maxKPM)
        {
            float maxSpeedMS = maxKPM / 3.6f;
            _car.velocity = _car.velocity.normalized * maxSpeedMS;
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

    public float tiltForce = 1000;

    private void TwoWheelsDriving()
    {
        // Apply tilt force to simulate lifting onto two wheels
        float tiltAngle = transform.eulerAngles.z;
        float tiltForceMagnitude = Mathf.Abs(tiltAngle) > 90f ? tiltForce : 0f;
        Vector3 tiltForceVector = transform.up * tiltForceMagnitude;
        _car.AddForce(tiltForceVector, ForceMode.Force);
    }
    IEnumerator IE_StartEngine()
    {
        BrakeStart(1);
        yield return new WaitForSeconds(0.5f);
        if (!isEngineStart)
        {
            gearNum = 1;
            isEngineStart = true;
            print("Ligado");
        }

        yield return new WaitForSeconds(0.5f);
        BrakePause();
    }
    IEnumerator IE_AutoRotationCar(float i)
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
                Vector3 currentRotation = _car.transform.rotation.eulerAngles;
                _car.transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0f);
                print("CAPOTOU");
            }
        }
    }
    void Burnout()
    {
        acceleration = addAcceleration;

        wheels[2].forwardFriction = _FWF_R;
        wheels[3].forwardFriction = _FWF_R;

        wheels[0].brakeTorque = brakeTorque;
        wheels[1].brakeTorque = brakeTorque;
    }
}
