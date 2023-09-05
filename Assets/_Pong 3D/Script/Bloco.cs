using UnityEngine;

public class Bloco : MonoBehaviour
{
    Rigidbody rb;
    public float destroyDelay = 2.0f;

    private BoxCollider boxCollider;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Pega a referência para o BoxCollider e desativa o Rigidbody no início
        boxCollider = GetComponent<BoxCollider>();
        rb.isKinematic = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Ativa o Rigidbody, desativa o BoxCollider e destrói o bloco após 2 segundos
        rb.isKinematic = false;
        boxCollider.enabled = false;
        rb.useGravity = true;
        Destroy(gameObject, destroyDelay);
    }
}
