using UnityEngine;

public class WheelTrail : MonoBehaviour
{
    public WheelCollider wheelCollider;

    public TrailRenderer trailRendererPrefab;
    public ParticleSystem smokeParticlesPrefab;
    public float valueMinOfEmission = 0.1f; // Valor limite para considerar que o carro est� derrapando

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

    void EmissionTrails()
    {
        WheelHit hit;
        wheelCollider.GetGroundHit(out hit);

        // Verifica se o pneu est� tocando o ch�o
        if (wheelCollider.isGrounded && Mathf.Abs(hit.sidewaysSlip) > valueMinOfEmission)
        {
            // Obter posi��o e rota��o do pneu
            Vector3 position;
            Quaternion rotation;
            wheelCollider.GetWorldPose(out position, out rotation);

            // Calcula a posi��o embaixo da roda
            float wheelHeight = wheelCollider.radius * 2f;
            Vector3 trailPosition = position - transform.up * (wheelHeight / 2f);

            // Atualiza a posi��o e a rota��o do trail renderer
            trailRenderer.transform.position = trailPosition+=transform.up * 0.1f;
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
    private void FixedUpdate()
    {
        EmissionTrails();
    }
}
