using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Speedometer : MonoBehaviour
{
    public float maxSpeed = 180f; // The maximum speed of the target ** IN KM/H **

    public float minSpeedArrowAngle;
    public float maxSpeedArrowAngle;

    [Header("UI")]
    public TextMeshProUGUI speedLabel; // The label that displays the speed;
    public TextMeshProUGUI statusLabel; // The label that displays the speed;
    public RectTransform arrow; // The arrow in the speedometer

    private CarController car;

    private void Start()
    {
        car = GetComponent<CarController>();
    }
    private void Update()
    {
        speedLabel.text = ((int)car.currentSpeed) + " km/h";

        if (car.currentRPM > 0)
        {
            statusLabel.text = ((int)car.statusEngine) + "";
        }
        else if(car.currentSpeed == 0)
        {
            statusLabel.text = "N";
        }
        else
        {
            statusLabel.text = "R";
        }

        arrow.localEulerAngles =
                new Vector3(0, 0, Mathf.Lerp(minSpeedArrowAngle, maxSpeedArrowAngle, car.currentSpeed / maxSpeed));
    }
}
