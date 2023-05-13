using System.Collections;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BeybladePhysics : MonoBehaviour
{
    [SerializeField]
    public float torque = 100f; // Defina o torque desejado
    public float maxTorque = 200f;

    public float moveForce = 5f;            // For�a de movimento da Beyblade

    // Vari�veis de f�sica
    [SerializeField]
    public float initDecay = 0.5f;
    public float decayRate = 0.1f; // A taxa de decaimento da velocidade de giro da Beyblade
    public float decayRotation = 10f;
    public float attractionForce = 10f; // for�a de atra��o entre as Beyblades
    public float repulsionForce = 50f; // for�a de repuls�o caso as Beyblades fiquem muito pr�ximas
    public float maxRotationX = 0f;
    public float maxRotationZ = 0f;

    // Vari�veis de controle

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
            currentTorque = Mathf.Clamp(currentTorque, torque, maxTorque); // Limite o torque ao valor m�ximo
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
            float rotationAmount = currentTorque * Time.deltaTime; // quantidade de rota��o em graus
            transform.Rotate(Vector3.up, rotationAmount); // rotaciona o objeto em torno do eixo Y
    }

    void EulerBalance()
    {
     
            // Rota��o em X e Z
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

    // Fun��o para imprimir na tela se a Beyblade est� girando
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Beyblade")
        {
            Debug.Log("SPLASH!");
            float num = Random.Range(0.0f, 10.0f);   
            currentTorque -= num * Time.deltaTime;
        }
    }


    // Fun��o para calcular a for�a de atra��o entre duas Beyblades
    public void AttractTo()
    {
        if (currentTorque >= 0)
        {
            // For�a de movimento
            float randomForce = Random.Range((moveForce / 2), moveForce);
            rb.AddRelativeForce(0f, 0f, randomForce);
            // buscar todas as Beyblades no cen�rio
            GameObject[] beyblades = GameObject.FindGameObjectsWithTag("Beyblade");

            // para cada Beyblade, calcular a for�a de atra��o/repu��o e aplic�-la na f�sica
            foreach (GameObject beyblade in beyblades)
            {
                if (beyblade != gameObject) // n�o calcular atra��o consigo mesma
                {
                    Vector3 direction = beyblade.transform.position - transform.position;
                    float distance = direction.magnitude;
                    float forceMagnitude = 0f;

                    // calcular for�a de atra��o se as Beyblades estiverem distantes
                    if (distance > 0.5f)
                    {
                        forceMagnitude = attractionForce * rb.mass * beyblade.GetComponent<BeybladePhysics>().rb.mass / Mathf.Pow(distance, 2f);
                    }
                    // calcular for�a de repuls�o se as Beyblades estiverem pr�ximas
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
