using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player1;
    public Transform player2;
    public float distance = 10.0f;
    public float height = 5.0f;
    public float damping = 2.0f;
    public float rotationDamping = 3.0f;


    private Vector3 velocity;

    private void LateUpdate()
    {
        if (!player1 || !player2) return;

        // calcula a posi��o e a rota��o desejadas da c�mera com base na posi��o m�dia dos jogadores
        Vector3 targetPosition = (player1.position + player2.position) / 2f;
        targetPosition += (player1.up + player2.up) / 2f * height;
        Vector3 cameraOffset = transform.forward * -distance;
        Vector3 desiredPosition = targetPosition + cameraOffset;
        Quaternion desiredRotation = Quaternion.LookRotation(targetPosition - transform.position, Vector3.up);

        // movimenta a c�mera gradualmente em dire��o � posi��o e rota��o desejadas
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * damping);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationDamping);

    }
}