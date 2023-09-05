using Unity.VisualScripting;
using UnityEngine;

public class Bola : MonoBehaviour
{
    public Vector3 impulse;
    public float speed = 5.0f;
    Rigidbody rb;

    float speedX;
    float speedY;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        speedX = Random.Range(0, 2) == 0 ? -1 : 1;
        speedY = Random.Range(0, 2) == 0 ? -1 : 1;
        rb.velocity = new Vector3(Random.Range(5, 10) * speedX, Random.Range(5, 10) * speedY,0);
        rb.AddForce(impulse, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Muda a direção após a colisão
        Vector3 newDirection = Vector3.Reflect(rb.velocity.normalized, collision.contacts[0].normal);
    }
}
