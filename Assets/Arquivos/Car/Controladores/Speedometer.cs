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
    public Image nitrus;

    private CarController car;
    private float initFillAmount;

    private void Start()
    {
        car = GetComponent<CarController>();
        initFillAmount = nitrus.fillAmount;
        statusLabel.text = "N";
    }
    private void Update()
    {
        speedLabel.text = ((int)car.currentKPM) +"";

        arrow.localEulerAngles =
                new Vector3(0, 0, Mathf.Lerp(minSpeedArrowAngle, maxSpeedArrowAngle, car.currentKPM / maxSpeed));

        NitrusUI();
        
        if (car.gearNum == 0 && car.currentKPM == 0)
        {
            statusLabel.text = "N";
            return;
        }
    }
    public void UpdateGear()
    {
        if (car.gearNum == 0)
        {
            statusLabel.text = "N";
            return;
        }
        else if (car.currentRPM < 0)
        {
            statusLabel.text = "R";
            return;
        }
        else if (car.gearNum > 0)
        {
            statusLabel.text = car.gearNum + "";
        }
    }
    public void NitrusUI()
    {
        float fillAmount = car.currentNitroDuration / car.nitroDuration;
        nitrus.fillAmount = fillAmount * initFillAmount;
    }
}
