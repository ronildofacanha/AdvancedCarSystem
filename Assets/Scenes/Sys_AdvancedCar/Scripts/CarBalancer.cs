using UnityEngine;

public class CarBalancer : MonoBehaviour
{
    public Rigidbody carRigidbody;
    public float balance = 1000f;
    public float maxTiltAngle = 45f;
    public float tiltCorrectionSpeed = 5f;

    private void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // Verifica se o objeto está inclinado além do ângulo máximo
        if (IsObjectTipping())
        {
            // Calcula a força de equilíbrio oposta à inclinação
            Vector3 balanceForce = CalculateBalanceForce();

            // Aplica a força de equilíbrio no Rigidbody do objeto
            carRigidbody.AddForce(balanceForce, ForceMode.Force);
        }
    }

    private bool IsObjectTipping()
    {
        // Verifica se o objeto está inclinado além do ângulo máximo
        float currentTiltAngle = Mathf.Abs(transform.rotation.eulerAngles.z);
        return currentTiltAngle > maxTiltAngle;
    }
    private Vector3 CalculateBalanceForce()
    {
        // Calcula a força de equilíbrio oposta à inclinação
        Vector3 balanceForce = -transform.up * balance;
        return balanceForce;
    }


}
