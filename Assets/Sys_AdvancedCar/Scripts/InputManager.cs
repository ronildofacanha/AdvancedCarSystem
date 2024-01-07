using System.Collections;
using UnityEngine;
using TMPro;
public class InputManager : MonoBehaviour
{
    public float vertical;
    public float horizontal;
    public bool handbrake = false;
    public bool nitrous = false;
    public bool driftMode = false;
    public bool powerEngine = false;
    public bool burnout = false;
    public float burnoutTime = 0;
    private CarController car;
    public TextMeshProUGUI controladores;

    private void Start()
    {
        car = GetComponent<CarController>();
    }

    private void FixedUpdate()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        handbrake = (Input.GetAxis("Jump")) !=0? true: false;
        powerEngine = (Input.GetKeyUp(KeyCode.X));
        nitrous = (Input.GetKey(KeyCode.N));
        driftMode = (Input.GetAxis("Fire1")) != 0 ? true : false;
        BurnoutAxis();

        controladores.text = ("[CONTROLADORES]"+
            "\n[W,S] Vertical = "+vertical+
            "\n[A,D] Horizontal = "+horizontal+
            "\n[Space] Handbrake = "+handbrake+
            "\n[N] Nitrous = "+nitrous+
            "\n[Ctrl] DriftMode = "+driftMode+
            "\n[Handbrake+Vertical] Burnout = " + burnout);
    }
    public void BurnoutAxis()
    {
        if (handbrake && car.currentKPM < 1 && vertical !=0)
        {
            burnout = true;
        }
        else
        {
            burnout = false;
        }
    }
}