using System.Collections;
using UnityEngine;
/*
 * 1� marcha � 40 km/h.
 * 2� marcha � 80 km/h.
 * 3� marcha � 120 km/h.
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
    internal enum GearType
    {
        Manual,
        Auto
    }

    [Header("TypeMode")]
    [SerializeField] private DriveType driveType;
    [SerializeField] private BreakeType breakeType;
    [SerializeField] private GearType gearType;

    [Header("Acc")]
    public float totalTorque;
    public AnimationCurve enginePower;
    public float wheelsRPM;
    public float engineRPM;
    public float smoothTime;
    public float[] gears;
    public float[] gearChangeSpeed;
    public int gearNum = 0;
    public float handBrakeFrictionMultiplier = 2f;

    [Header("Config Wheels")]
    public GameObject WM, WC;
    public WheelCollider[] wheels;
    public Transform[] wMeshes;

    [Header("Controller")]
    public float maxKPM = 180f;
    public float maxRPM = 5600;
    public float minRPM = 3000;
    public float brakeTorque = 3000f;
    public float[] angularDrag;

    [Header("Config Drift")]
    public float SDF_F, SDF_R;
    public float FWF_R = 0.1f;
    public float SDF_Default, FWF_Default;
    public float maxDriftAngle = 30, driftForce = 10;
    public float maxStabilizingForce = 200f;

    [Header("NITROUS")]
    public float nitroForce = 500f;
    public float nitroDuration = 5f;
    public float nitroCooldown = 10f;
    public float currentNitroDuration;

    [Header("FORCES")]
    public GameObject centerOfMass; // Altura do centro de massa do carro
    public float downforce = 10.0f;
    public float AntiRoll = 5000.0f;

    [Header("Monitore")]
    public float currentKPM = 0f;
    public float currentRPM = 0.0f;
    public bool isPowerEngine;
    public bool leftWheelsTouchingGround = false;
    public bool rightWheelsTouchingGround = false;
    public bool allWheelsTouchingGround = false;

    #region ___PRIVATE GAME___
    private Rigidbody _RG;
    private InputManager _IM;
    private EmissionController _EM;
    private Speedometer _SP;
    private WheelFrictionCurve _SDF_F, _SDF_R, _FWF_R, _SDF_Default, _FWF_Default;
    private WheelFrictionCurve forwardFriction, sidewaysFriction;
    private float driftFactor;
    private float radius = 6;

    #endregion // END PRIVATE
    #endregion // ALL 
    void Start()
    {
        GetGameObjects();
        StartVariables();
    }
    private void Update()
    {
        if (_IM.powerEngine)
        {
            StartCoroutine(IE_PowerEngine());
        }
    }
    void FixedUpdate()
    {
        TouchingGround();
        AllPhysics();
        KPH_RPM();
        TorqueBrake();

        if (isPowerEngine)
        {
            TorqueType();
            GearTypeShift();
            SteerVehicle();
            CalculateEngineTorque();
            calculateEnginePower();
            Nitrous();

            if (_IM.burnout)
            {
                if (gearType == GearType.Manual)
                {
                    Burnout();
                }
            }
            else
            {
                DefaultFriction();
            }

            if (_IM.driftMode)
            {
                DriftMode();
            }
            if (currentKPM >= maxKPM * 0.3f)
            {
                AdjustTraction();
            }
        }
    }
    void StartVariables()
    {
        _SDF_F = wheels[3].sidewaysFriction; _SDF_F.stiffness = SDF_F;

        _SDF_R = wheels[3].sidewaysFriction; _SDF_R.stiffness = SDF_R;

        _FWF_R = wheels[3].forwardFriction; _FWF_R.stiffness = FWF_R;

        _SDF_Default = wheels[3].sidewaysFriction; _SDF_Default.stiffness = SDF_Default;

        _FWF_Default = wheels[3].forwardFriction; _FWF_Default.stiffness = FWF_Default;

        _RG.angularDrag = angularDrag[1];
        gearNum = 0;
        isPowerEngine = false;
        DefaultFriction();
    }
    void GetGameObjects()
    {
        _RG = GetComponent<Rigidbody>();
        _IM = GetComponent<InputManager>();
        _EM = GetComponent<EmissionController>();
        _SP = GetComponent<Speedometer>();
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
    void AllPhysics()
    {
        if (_RG != null)
        {
            //COM
            centerOfMass = gameObject.transform.Find("COM").gameObject;
            _RG.centerOfMass = centerOfMass.transform.localPosition;

            //DOWNFORCE
            _RG.AddForce(-transform.up * downforce * _RG.velocity.magnitude);

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
                _RG.AddForceAtPosition(wheels[0].transform.up * -antiRollForce, wheels[0].transform.position);

            if (groundedR)
                _RG.AddForceAtPosition(wheels[1].transform.up * antiRollForce, wheels[1].transform.position);
        }
    }
    void CalculateEngineTorque()
    {
        wheelRPM();
        totalTorque = enginePower.Evaluate(engineRPM) * (gears[gearNum]) * _IM.vertical;
        totalTorque = totalTorque * 0.32f;
        float velocity = 0.0f;
        engineRPM = Mathf.SmoothDamp(engineRPM, 1000 + (Mathf.Abs(wheelsRPM) * 3.6f * (gears[gearNum])), ref velocity, smoothTime);
    }

    float vertical, totalPower,lastValue;
    private bool flag = false;
    [HideInInspector] public bool test; //engine sound boolean
    private void calculateEnginePower()
    {
        lastValue = engineRPM;

        wheelRPM();

        if (vertical != 0)
        {
            _RG.drag = 0.005f;
        }
        if (vertical == 0)
        {
            _RG.drag = 0.1f;
        }
        totalPower = 3.6f * enginePower.Evaluate(engineRPM) * (vertical);

        float velocity = 0.0f;
        if (engineRPM >= maxRPM || flag)
        {
            engineRPM = Mathf.SmoothDamp(engineRPM, maxRPM - 500, ref velocity, 0.05f);

            flag = (engineRPM >= maxRPM - 450) ? true : false;
            test = (lastValue > engineRPM) ? true : false;
        }
        else
        {
            engineRPM = Mathf.SmoothDamp(engineRPM, 1000 + (Mathf.Abs(wheelsRPM) * 3.6f * (gears[gearNum])), ref velocity, smoothTime);
            test = false;
        }
        if (engineRPM >= maxRPM + 1000) engineRPM = maxRPM + 1000; // clamp at max
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
        if (currentKPM < 20 && _IM.horizontal != 0)
        {
            _RG.angularDrag = angularDrag[0];
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
        wheels[2].brakeTorque = brakeTorque * 0.2f;
        wheels[3].brakeTorque = brakeTorque * 0.2f;
    }
    void DefaultFriction()
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
        // AC DefaultFriction
        _RG.angularDrag = angularDrag[0];
    }
    void SteerVehicle()
    {
        if (_IM.horizontal > 0)
        {
            //rear tracks size is set to 1.5f       wheel base has been set to 2.55f
            wheels[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * _IM.horizontal;
            wheels[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * _IM.horizontal;
        }
        else if (_IM.horizontal < 0)
        {
            wheels[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * _IM.horizontal;
            wheels[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * _IM.horizontal;
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
        if (_IM.handbrake && !_IM.driftMode && !_IM.burnout && ((int)currentKPM) != 0)
        {
            BrakeStart(1);
        }

        if (_IM.vertical == 0 && !_IM.burnout && ((int)currentKPM) != 0)
        {
            BrakeStart(0.2f); //20% 
        }
    }
    void AdjustTraction()
    {
        //tine it takes to go from normal drive to drift 
        float driftSmothFactor = .7f * Time.deltaTime;

        if (_IM.handbrake)
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

            if (wheelHit.sidewaysSlip < 0) driftFactor = (1 + -_IM.horizontal) * Mathf.Abs(wheelHit.sidewaysSlip);

            if (wheelHit.sidewaysSlip > 0) driftFactor = (1 + _IM.horizontal) * Mathf.Abs(wheelHit.sidewaysSlip);
        }

    }
    bool checkGears()
    {
        if (currentKPM >= gearChangeSpeed[gearNum]) return true;
        else return false;
    }
    void GearTypeShift()
    {
        if (gearType == GearType.Manual)
        {
            if (Input.GetKeyUp(KeyCode.E))
            {
                gearNum = 1;
                _SP.UpdateGear();
                return;
            }
            else if (Input.GetKeyUp(KeyCode.Q))
            {
                gearNum = 0;
                _SP.UpdateGear();
                return;
            }
        }
        else if (gearType == GearType.Auto)
        {
            if (engineRPM > minRPM && gearNum < gears.Length - 1 && gearNum >= 0 && currentRPM >= 0 && checkGears())
            {
                gearNum++;
                _SP.UpdateGear();
                return;
            }
            else if (engineRPM < minRPM && gearNum > 0)
            {
                if (gearNum>1)
                {
                    gearNum--;
                }
                else if (currentKPM ==0)
                {
                    gearNum = 0;
                }
                _SP.UpdateGear();
                return;
            }

            if (_IM.vertical != 0 && gearNum == 0)
            {
                gearNum = 1;
                _SP.UpdateGear();
                return;
            }
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
    void TorqueType()
    {
        if (currentKPM < maxKPM && currentKPM > -maxKPM)
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
        currentKPM = _RG.velocity.magnitude * 3.6f;
        currentKPM = ((int)currentKPM);
        // RPM
        currentRPM = 2 * Mathf.PI * wheels[0].radius * wheels[0].rpm * 60 / 1000;
        currentRPM = ((int)currentRPM);

        if (currentKPM > maxKPM)
        {
            float maxSpeedMS = maxKPM / 3.6f;
            _RG.velocity = _RG.velocity.normalized * maxSpeedMS;
        }
    }
    void Burnout()
    {
        wheels[2].forwardFriction = _FWF_R;
        wheels[3].forwardFriction = _FWF_R;

        wheels[0].brakeTorque = brakeTorque;
        wheels[1].brakeTorque = brakeTorque;
    }
    void UpdateWheel(WheelCollider coll, Transform mesh)
    {
        Quaternion quat;
        Vector3 pos;
        coll.GetWorldPose(out pos, out quat);
        mesh.transform.position = pos;
        mesh.transform.rotation = quat;
    }
    void Nitrous()
    {
        if (!_IM.nitrous && currentNitroDuration <= nitroDuration)
        {
            currentNitroDuration += Time.deltaTime/2;
        }
        else if(currentNitroDuration > 0 && _IM.nitrous)
        {
            currentNitroDuration -= Time.deltaTime;
        }

        if (_IM.nitrous)
        {
            if (currentNitroDuration > 0)
            {
                _EM.NitrousOn();
                _RG.AddForce(transform.forward * nitroForce);
            }
            else
            {
                _EM.NitrousOff();
            }
        }
        else
        {
            _EM.NitrousOff();
        }
    }

    IEnumerator IE_PowerEngine()
    {
        BrakeStart(1);
        yield return new WaitForSeconds(0.5f);
        if (!isPowerEngine)
        {
            isPowerEngine = true;
            print("Ligado!");
            _SP.UpdateGear();
            yield return null;
        }
        else
        {
            isPowerEngine = false;
            print("Desligado!");
            _SP.UpdateGear();
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
                Vector3 currentRotation = _RG.transform.rotation.eulerAngles;
                _RG.transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0f);
                print("CAPOTOU");
            }
        }
    }
}
