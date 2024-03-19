using System;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;
public class AudioController : MonoBehaviour
{
    public enum EngineAudioOptions // Options for the engine audio
    {
        Simple, // Simple style audio
        FourChannel // four Channel audio
    }

    public EngineAudioOptions engineSoundStyle = EngineAudioOptions.FourChannel;// Set the default audio options to be four channel
    public AudioClip lowAccelClip;                                              // Audio clip for low acceleration
    public AudioClip lowDecelClip;                                              // Audio clip for low deceleration
    public AudioClip highAccelClip;                                             // Audio clip for high acceleration
    public AudioClip highDecelClip;                                             // Audio clip for high deceleration
    public float pitchMultiplier = 1f;                                          // Used for altering the pitch of audio clips
    public float lowPitchMin = 1f;                                              // The lowest possible pitch for the low sounds
    public float lowPitchMax = 6f;                                              // The highest possible pitch for the low sounds
    public float highPitchMultiplier = 0.25f;                                   // Used for altering the pitch of high sounds
    public float maxRolloffDistance = 500;                                      // The maximum distance where rollof starts to take place
    public float dopplerLevel = 1;                                              // The mount of doppler effect used in the audio
    public bool useDoppler = true;                                              // Toggle for using doppler

    private AudioSource m_LowAccel; // Source for the low acceleration sounds
    private AudioSource m_LowDecel; // Source for the low deceleration sounds
    private AudioSource m_HighAccel; // Source for the high acceleration sounds
    private AudioSource m_HighDecel; // Source for the high deceleration sounds
    private bool m_StartedSound; // flag for knowing if we have started sounds
    private CarController _Car; // Reference to car we are controlling
    private InputManager _IM; // Reference to car we are InputManager

    [Header("VFX")]
    public AudioClip nitrousClip;
    private AudioSource _nitrousSource;

    public AudioClip on;
    private AudioSource _on;
    public AudioClip off;
    private AudioSource _off;

    private void Start()
    {
        //nitrous
        _on = SetUpAudioSource(on);
        _off = SetUpAudioSource(off);

        // get the carcontroller ( this will not be null as we have require component)
        _Car = GetComponent<CarController>();
        _IM = GetComponent<InputManager>();
    }
    private void StartSound()
    {
        // setup the simple audio source
        m_HighAccel = SetUpEngineAudioSource(highAccelClip);
     
        // if we have four channel audio setup the four audio sources
        if (engineSoundStyle == EngineAudioOptions.FourChannel)
        {
            m_LowAccel = SetUpEngineAudioSource(lowAccelClip);
            m_LowDecel = SetUpEngineAudioSource(lowDecelClip);
            m_HighDecel = SetUpEngineAudioSource(highDecelClip);
        }
        // flag that we have started the sounds playing
        m_StartedSound = true;
    }
    private void StopEngine()
    {
        //Destroy all audio sources on this object:
        foreach (var source in GetComponents<AudioSource>())
        {
            if (source.clip.name != on.name && source.clip.name != off.name)
            {
                Destroy(source);
            }
        }

        m_StartedSound = false;
    }
    private void Update()
    {
        if (_Car.isPowerEngine)
        {
            // stop sound if the object is beyond the maximum roll off distance
            if (m_StartedSound)
            {
                StopEngine();
            }

            // start the sound if not playing and it is nearer than the maximum distance
            if (!m_StartedSound)
            {
                StartSound();
            }

            if (m_StartedSound)
            {
                float revs = _Car.engineRPM / _Car.maxRPM;
                // The pitch is interpolated between the min and max values, according to the car's revs.
                float pitch = ULerp(lowPitchMin, lowPitchMax, revs);

                // clamp to minimum pitch (note, not clamped to max for high revs while burning out)
                pitch = Mathf.Min(lowPitchMax, pitch);

                if (engineSoundStyle == EngineAudioOptions.Simple)
                {
                    // for 1 channel engine sound, it's oh so simple:
                    m_HighAccel.pitch = pitch * pitchMultiplier * highPitchMultiplier;
                    m_HighAccel.dopplerLevel = useDoppler ? dopplerLevel : 0;
                    m_HighAccel.volume = 1;
                }
                else
                {
                    // for 4 channel engine sound, it's a little more complex:

                    // adjust the pitches based on the multipliers
                    m_LowAccel.pitch = pitch * pitchMultiplier;
                    m_LowDecel.pitch = pitch * pitchMultiplier;
                    m_HighAccel.pitch = pitch * highPitchMultiplier * pitchMultiplier;
                    m_HighDecel.pitch = pitch * highPitchMultiplier * pitchMultiplier;

                    // get values for fading the sounds based on the acceleration
                    float accFade = Mathf.Abs((_IM.vertical > 0) ? _IM.vertical : 0);
                    float decFade = 1 - accFade;

                    // get the high fade value based on the cars revs
                    float highFade = Mathf.InverseLerp(0.2f, 0.8f, revs);
                    float lowFade = 1 - highFade;

                    // adjust the values to be more realistic
                    highFade = 1 - ((1 - highFade) * (1 - highFade));
                    lowFade = 1 - ((1 - lowFade) * (1 - lowFade));
                    accFade = 1 - ((1 - accFade) * (1 - accFade));
                    decFade = 1 - ((1 - decFade) * (1 - decFade));

                    // adjust the source volumes based on the fade values
                    m_LowAccel.volume = lowFade * accFade;
                    m_LowDecel.volume = lowFade * decFade;
                    m_HighAccel.volume = highFade * accFade;
                    m_HighDecel.volume = highFade * decFade;

                    // adjust the doppler levels
                    m_HighAccel.dopplerLevel = useDoppler ? dopplerLevel : 0;
                    m_LowAccel.dopplerLevel = useDoppler ? dopplerLevel : 0;
                    m_HighDecel.dopplerLevel = useDoppler ? dopplerLevel : 0;
                    m_LowDecel.dopplerLevel = useDoppler ? dopplerLevel : 0;
                }
            }
            // nitour
            Nitrous();
        }
        else
        {
            StopEngine();
        }
    }
    private AudioSource SetUpEngineAudioSource(AudioClip clip)
    {
        // create the new audio source component on the game object and set up its properties
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = 0;
        source.loop = true;

        // start the clip from a random point
        source.time = Random.Range(0f, clip.length);
        source.Play();
        source.minDistance = 5;
        source.maxDistance = maxRolloffDistance;
        source.dopplerLevel = 0;
        return source;
    }
    private AudioSource SetUpAudioSource(AudioClip clip)
    {
        // create the new audio source component on the game object and set up its properties
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = 0;
        //source.loop = true;

        // start the clip from a random point
        source.minDistance = 5;
        source.maxDistance = maxRolloffDistance;
        source.dopplerLevel = 0;
        return source;
    }
    private static float ULerp(float from, float to, float value)
    {
        return (1.0f - value) * from + value * to;
    }
    //
    public void Nitrous()
    {
        if (_IM.nitrous && _Car.currentNitroDuration > 0)
        {
            _nitrousSource.volume = 1;

            if (!_nitrousSource.isPlaying)
            {
                _nitrousSource.Play();
            }
            if (_Car.currentNitroDuration < 1.5f)
            {
                _nitrousSource.volume = Mathf.Lerp(0f, 1f, _Car.currentNitroDuration / 1.5f);
            }
        }
        else
        {
            _nitrousSource.volume = 0;
            _nitrousSource.Pause();
        }
    }
    public void OnEngine()
    {
        if (_on != null)
        {
            if (!_on.isPlaying)
            {
                _on.volume = 1;
                _on.Play();
            }
        }
    }
    public void OffEngine()
    {
        if (_off != null)
        {
            if (!_off.isPlaying)
            {
                _off.volume = 1;
                _off.Play();
            }
        }
    }
}
