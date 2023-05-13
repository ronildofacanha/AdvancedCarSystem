using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 1ª marcha – 20 km/h.
 * 2ª marcha – 40 km/h.
 * 3ª marcha – 40 km/h.
 * 4ª marcha – 50 km/h.
 * 5ª marcha – 60 km/h.
 */

public class EngineAudio : MonoBehaviour
{
    public AudioSource startSound;
    public AudioSource engineSound;
    public AudioSource lowSound;

    public float volumeMax = 1f;
    public float volumeMin = 0.3f;
    public float volumeSpeed = 1f;
    public float pitchMax = 2f;
    public float pitchMin = 0.5f;
    public float minSpeed = 0f; // km/h
    public float maxSpeed = 0f; //km/h
    private float targetPitch;
    
    private float targetVolume;
    private CarController carController;

    // Start is called before the first frame update
    void Start()
    {
        carController = GetComponent<CarController>();
        engineSound.volume = 0;
        lowSound.volume = 0f;
        maxSpeed = carController.maxSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        float currentSpeed = carController.currentSpeed;

        if (carController.statusEngine == 1)
        {
            if (!startSound.isPlaying && !carController.isEngineStart)
            {
                startSound.Play();
                engineSound.Play();
                lowSound.Play();
                engineSound.pitch = pitchMin;
                engineSound.volume = volumeMin;
            }
        }

        if (carController.isEngineStart)
        {
            float pitch = Mathf.InverseLerp(minSpeed, maxSpeed, currentSpeed);
            targetPitch = Mathf.Lerp(pitchMin, pitchMax, pitch);

            float currentVolume = Mathf.InverseLerp(minSpeed, maxSpeed, currentSpeed);
            targetVolume = Mathf.Lerp(volumeMin, volumeMax, currentVolume);

            engineSound.pitch = Mathf.Lerp(engineSound.pitch, targetPitch, Time.deltaTime);
            engineSound.volume = Mathf.Lerp(engineSound.volume, targetVolume, Time.deltaTime);
        }


       /* if (Input.GetAxis("Vertical") > 0)
        {
            print(Input.GetAxis("Vertical")+ "AXIS | VOLUME "+ lowSound);
            AumentarVolume(lowSound);
        }
        else
        {
            DimunirVolume(lowSound);
        }
       */
    }

    private float currentVolume=0;
    public void AumentarVolume(AudioSource sound)
    {
        currentVolume= Mathf.Clamp01(currentVolume);
        float volume = Mathf.Lerp(0, volumeMax, currentVolume);
        sound.volume = volume;
        currentVolume += volumeSpeed * Time.deltaTime;
    }
    public void DimunirVolume(AudioSource sound)
    {
        float volume = Mathf.Lerp(0, volumeMax, currentVolume);
        currentVolume = Mathf.Clamp01(currentVolume);
        currentVolume -= volumeSpeed * Time.deltaTime;
        sound.volume = volume;
    }
}
