using UnityEngine;

public class WheelTrail : MonoBehaviour
{
    public WheelCollider wheelCollider;

    public TrailRenderer trailRendererPrefab;
    public ParticleSystem smokeParticlesPrefab;
    public float valueMinOfEmission = 0.1f; // Valor limite para considerar que o carro está derrapando

    [Header("Skid Source")]
    public AudioSource slipSource;

    private TrailRenderer trailRenderer;
    private ParticleSystem smokeParticles;
    private ParticleSystem.EmissionModule smokeEmission;

    private void Start()
    {
        trailRenderer = Instantiate(trailRendererPrefab, transform);
        trailRenderer.emitting = false;

        smokeParticles = Instantiate(smokeParticlesPrefab, transform);
        smokeEmission = smokeParticles.emission;
        smokeEmission.enabled = false;
    }
 
    void EmissionTrailsForwardSlip()
    {
        WheelHit hit;
        wheelCollider.GetGroundHit(out hit);

        // Verifica se o pneu está tocando o chão
        if (wheelCollider.isGrounded && Mathf.Abs(hit.forwardSlip) > valueMinOfEmission)
        {
            // Obter posição e rotação do pneu
            Vector3 position;
            Quaternion rotation;
            wheelCollider.GetWorldPose(out position, out rotation);

            // Calcula a posição embaixo da roda
            float wheelHeight = wheelCollider.radius * 2f;
            Vector3 trailPosition = position - transform.up * (wheelHeight / 2f);

            // Atualiza a posição e a rotação do trail renderer
            trailRenderer.transform.position = trailPosition += transform.up * 0.1f;
            trailRenderer.transform.rotation = rotation;

            // Ativa o trail renderer
            trailRenderer.emitting = true;

            //SMOKE
            smokeParticles.transform.position = transform.position;
            smokeEmission.enabled = true;
        }
        else
        {
            // Desativa o trail renderer
            trailRenderer.emitting = false;
            smokeEmission.enabled = false;
        }
    }

    void EmissionTrailsSidewaysSlip()
    {
        WheelHit hit;
        wheelCollider.GetGroundHit(out hit);

        if (wheelCollider.isGrounded && Mathf.Abs(hit.forwardSlip) > valueMinOfEmission)
        {
            // Obter posição e rotação do pneu
            Vector3 position;
            Quaternion rotation;
            wheelCollider.GetWorldPose(out position, out rotation);

            // Calcula a posição embaixo da roda
            float wheelHeight = wheelCollider.radius * 2f;
            Vector3 posWheels = position - transform.up * (wheelHeight / 2f);

            // Atualiza a posição e a rotação do trail renderer
            trailRenderer.transform.position = posWheels += transform.up * 0.01f;
            trailRenderer.transform.rotation = rotation;

            // Ativa o trail renderer
            trailRenderer.emitting = true;

            //SMOKE
            smokeParticles.transform.position = position;
            smokeEmission.enabled = true;
            StartPlay();
        }
        else
        {
            // Desativa o trail renderer
            trailRenderer.emitting = false;
            smokeEmission.enabled = false;
            StopPlay();
        }
    }

    void StartPlay()
    {
        //audio
        if (!slipSource.isPlaying)
        {
            slipSource.volume = 0.5f;
            slipSource.Play();
        }
    }
    void StopPlay()
    {
        // audio
        slipSource.volume = 0;
        slipSource.Pause();
    }
    private void FixedUpdate()
    {
        EmissionTrailsSidewaysSlip();
    }
}
