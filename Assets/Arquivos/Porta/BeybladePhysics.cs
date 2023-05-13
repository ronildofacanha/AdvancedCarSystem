using System.Collections;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BeybladePhysics : MonoBehaviour
{
    [SerializeField]
    public float torque = 100f; // Defina o torque desejado
    public float maxTorque = 200f;

    public float moveForce = 5f;            // Força de movimento da Beyblade

    // Variáveis de física
    [SerializeField]
    public float initDecay = 0.5f;
    public float decayRate = 0.1f; // A taxa de decaimento da velocidade de giro da Beyblade
    public float decayRotation = 10f;
    public float attractionForce = 10f; // força de atração entre as Beyblades
    public float repulsionForce = 50f; // força de repulsão caso as Beyblades fiquem muito próximas
    public float maxRotationX = 0f;
    public float maxRotationZ = 0f;

    // Variáveis de controle

    private Rigidbody rb;
    private float currentTorque;
    private bool isMaxTorque = false;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        RotationResistance();
    }

    void RotationResistance()
    {

        if (currentTorque >= maxTorque)
        {
            isMaxTorque = true;
        }

        if (!isMaxTorque)
        {

            currentTorque += decayRate * Time.deltaTime; // Aumente o torque a cada quadro
            currentTorque = Mathf.Clamp(currentTorque, torque, maxTorque); // Limite o torque ao valor máximo
        }
        else
        {
            if (currentTorque <= 0)
            {
                currentTorque = 0;
            }
            else
            {
                currentTorque -= decayRate * Time.deltaTime;
            }
        }
            //rb.angularVelocity = transform.up * currentTorque;
            rb.AddTorque(0f, currentTorque, 0f);
            float rotationAmount = currentTorque * Time.deltaTime; // quantidade de rotação em graus
            transform.Rotate(Vector3.up, rotationAmount); // rotaciona o objeto em torno do eixo Y
    }

    void EulerBalance()
    {
     
            // Rotação em X e Z
            float currentRotationX = transform.rotation.eulerAngles.x;
            float currentRotationZ = transform.rotation.eulerAngles.z;
            float newRotationX = currentRotationX + transform.rotation.x * Time.deltaTime;
            float newRotationZ = currentRotationZ + transform.rotation.z * Time.deltaTime;

            newRotationX = Mathf.Clamp(newRotationX, -maxRotationX, maxRotationX);
            newRotationZ = Mathf.Clamp(newRotationZ, -maxRotationZ, maxRotationZ);

            transform.rotation = Quaternion.Euler(newRotationX, transform.rotation.eulerAngles.y, newRotationZ);


        if (currentTorque >= (torque * initDecay))
        {
            return;
        }
        else if (currentTorque < (torque * initDecay))
        {
            maxRotationX += decayRotation * Time.deltaTime;
            maxRotationZ += decayRotation * Time.deltaTime;
        }
    }
    void FixedUpdate()
    {
     
        if (Input.GetKeyDown(KeyCode.Space))
        {
        }

            if (currentTorque >= torque * 0.3f)
            {
                RotationResistance();
                EulerBalance();
                AttractTo();
            }
    }

    // Função para imprimir na tela se a Beyblade está girando
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Beyblade")
        {
            Debug.Log("SPLASH!");
            float num = Random.Range(0.0f, 10.0f);   
            currentTorque -= num * Time.deltaTime;
        }
    }


    // Função para calcular a força de atração entre duas Beyblades
    public void AttractTo()
    {
        if (currentTorque >= 0)
        {
            // Força de movimento
            float randomForce = Random.Range((moveForce / 2), moveForce);
            rb.AddRelativeForce(0f, 0f, randomForce);
            // buscar todas as Beyblades no cenário
            GameObject[] beyblades = GameObject.FindGameObjectsWithTag("Beyblade");

            // para cada Beyblade, calcular a força de atração/repução e aplicá-la na física
            foreach (GameObject beyblade in beyblades)
            {
                if (beyblade != gameObject) // não calcular atração consigo mesma
                {
                    Vector3 direction = beyblade.transform.position - transform.position;
                    float distance = direction.magnitude;
                    float forceMagnitude = 0f;

                    // calcular força de atração se as Beyblades estiverem distantes
                    if (distance > 0.5f)
                    {
                        forceMagnitude = attractionForce * rb.mass * beyblade.GetComponent<BeybladePhysics>().rb.mass / Mathf.Pow(distance, 2f);
                    }
                    // calcular força de repulsão se as Beyblades estiverem próximas
                    else
                    {
                        forceMagnitude = -repulsionForce;
                    }

                    Vector3 force = direction.normalized * forceMagnitude;
                    rb.AddForce(force);
                }
            }
        }
    }

}
