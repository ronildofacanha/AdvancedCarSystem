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
        // Verifica se o objeto est� inclinado al�m do �ngulo m�ximo
        if (IsObjectTipping())
        {
            // Calcula a for�a de equil�brio oposta � inclina��o
            Vector3 balanceForce = CalculateBalanceForce();

            // Aplica a for�a de equil�brio no Rigidbody do objeto
            carRigidbody.AddForce(balanceForce, ForceMode.Force);
        }
    }

    private bool IsObjectTipping()
    {
        // Verifica se o objeto est� inclinado al�m do �ngulo m�ximo
        float currentTiltAngle = Mathf.Abs(transform.rotation.eulerAngles.z);
        return currentTiltAngle > maxTiltAngle;
    }
    private Vector3 CalculateBalanceForce()
    {
        // Calcula a for�a de equil�brio oposta � inclina��o
        Vector3 balanceForce = -transform.up * balance;
        return balanceForce;
    }


}
