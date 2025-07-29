using UnityEngine;


public class SimplifiedSoundController : MonoBehaviour // This system plays tire and engine sounds.
{
    [Header("References")]
    [SerializeField] bool useSounds = false;
    [SerializeField] SimplifiedCarController ezerealCarController;
    [SerializeField] AudioSource tireAudio;
    [SerializeField] AudioSource engineAudio;

    [Header("Settings")]
    
    [Tooltip("Max volume for tire sound. If you need to modify engine volume, you can do it from the object 'Engine Sound' inside 'Electric Truck' object")]
    public float maxVolume = 0.5f; // Maximum volume for high speeds

    [Header("Debug")]
    [SerializeField] bool alreadyPlaying;

    void Start()
    {
        if (useSounds)
        {
            alreadyPlaying = false;

            if (ezerealCarController == null || ezerealCarController.vehicleRB == null || tireAudio == null || engineAudio == null)
            {
                Debug.LogWarning("ezerealSoundController is missing some references. Ignore or attach them if you want to have sound controls.");


            }

            if (tireAudio != null)
            {
                tireAudio.volume = 0f; // Start with zero volume
                tireAudio.Stop();
            }
        }
    }

    public void TurnOnEngineSound()
    {
        Debug.Log("Turning on engine sound");
        if (useSounds)
        {
            if (engineAudio != null)
            {
                engineAudio.Play();
            }
        }
    }

    public void TurnOffEngineSound()
    {
        Debug.Log("Turning off engine sound");

        if (useSounds)
        {
            if (engineAudio != null)
            {
                engineAudio.Stop();
            }
        }
    }

    void Update()
    {
        if (useSounds)
        {
#if UNITY_6000_0_OR_NEWER
                if (ezerealCarController != null && ezerealCarController.vehicleRB != null && tireAudio != null && engineAudio != null)
                {
                    if (!ezerealCarController.stationary && !alreadyPlaying && !ezerealCarController.InAir())
                    {
                        tireAudio.Play();
                        alreadyPlaying = true;
                    }
                    else if (ezerealCarController.stationary || ezerealCarController.InAir())
                    {
                        tireAudio.Stop();
                        alreadyPlaying = false;
                    }

                    // Get the car's current speed
                    float speed = ezerealCarController.vehicleRB.linearVelocity.magnitude;

                    // Calculate the volume based on speed
                    float targetVolume = Mathf.Clamp01(speed / 15) * maxVolume;


                    tireAudio.volume = targetVolume;

                    //Tire Pitch

                    float tireSoundPitch = 0.8f + (Mathf.Abs(ezerealCarController.vehicleRB.linearVelocity.magnitude) / 50f);
                    tireAudio.pitch = tireSoundPitch;

                    //Engine Pitch

                    float engineSoundPitch = 0.8f + (Mathf.Abs(ezerealCarController.vehicleRB.linearVelocity.magnitude) / 25f);
                    engineAudio.pitch = engineSoundPitch;
#else
            if (ezerealCarController != null && ezerealCarController.vehicleRB != null && tireAudio != null && engineAudio != null)
            {
                if (!ezerealCarController.stationary && !alreadyPlaying && !ezerealCarController.InAir())
                {
                    tireAudio.Play();
                    alreadyPlaying = true;
                }
                else if (ezerealCarController.stationary || ezerealCarController.InAir())
                {
                    tireAudio.Stop();
                    alreadyPlaying = false;
                }

                // Get the car's current speed
                float speed = ezerealCarController.vehicleRB.velocity.magnitude;

                // Calculate the volume based on speed
                float targetVolume = Mathf.Clamp01(speed / 15) * maxVolume;


                tireAudio.volume = targetVolume;

                //Tire Pitch

                float tireSoundPitch = 0.8f + (Mathf.Abs(ezerealCarController.vehicleRB.velocity.magnitude) / 50f);
                tireAudio.pitch = tireSoundPitch;

                //Engine Pitch

                float engineSoundPitch = 0.8f + (Mathf.Abs(ezerealCarController.vehicleRB.velocity.magnitude) / 25f);
                engineAudio.pitch = engineSoundPitch;
#endif
            }
        }
    }
}
