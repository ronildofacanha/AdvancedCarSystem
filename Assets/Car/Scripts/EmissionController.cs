using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmissionController : MonoBehaviour
{
    public Light[] frontLights;
    public Light[] rearLights;
    public Light[] downLights;
    public ParticleSystem[] backFires;

    private bool isFrontLights = false;
    private bool isRearLights = false;
    private bool isDownLights = false;
    private bool isBackFires = false;

    private void Start()
    {

        foreach (var backFires in backFires)
        {
            OnEmissionParticle(backFires, 0);
        }

        foreach (var frontLight in frontLights)
        {
            OnLight(frontLight, 0);
        }

        foreach (var rearLight in rearLights)
        {
            OnLight(rearLight, 0);
        }

        foreach (var downLight in downLights)
        {
            OnLight(downLight, 0);
        }
    }

    void OnLight(Light light, int num)
    {
        if (num == 1)
        {
            light.enabled = true;
        }
        else
        {
            light.enabled = false;
        }
    }
    void OnEmissionParticle(ParticleSystem backFire, int num)
    {
        var emission = backFire.emission;

        if (num == 1)
        {
            emission.enabled = true;
        }
        else
        {
            emission.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            if (isFrontLights == false)
            {
                foreach (var frontLight in frontLights)
                {
                    OnLight(frontLight, 1);
                }
                isFrontLights = true;
            }
            else
            {
                foreach (var frontLight in frontLights)
                {
                    OnLight(frontLight, 0);
                }
                isFrontLights = false;
            }
        }

        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            if (isRearLights == false) 
            {
                foreach (var rearLight in rearLights)
                {

                    OnLight(rearLight, 1);
                }
                isRearLights = true;
            }
            else
            {
                foreach (var rearLight in rearLights)
                {

                    OnLight(rearLight, 0);
                }
                isRearLights = false;
            }
        }

        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            if (isDownLights == false)
            {
                foreach (var downLight in downLights)
                {
                    OnLight(downLight, 1);
                }
                isDownLights = true;
            }
            else
            {
                foreach (var downLight in downLights)
                {
                    OnLight(downLight, 0);
                }
                isDownLights = false;
            }
        }
    }

    public void NitrousOn()
    {
        foreach (var backFire in backFires)
        {
            OnEmissionParticle(backFire, 1);
        }
    }
    public void NitrousOff()
    {
        foreach (var backFire in backFires)
        {
            OnEmissionParticle(backFire, 0);
        }
    }
}
