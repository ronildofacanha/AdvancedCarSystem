using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera[] cameras; // lista de c�meras na cena

    private int currentCameraIndex = 0; // �ndice da c�mera atual

    void Start()
    {
        // desativa todas as c�meras exceto a primeira
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(false);
        }

        cameras[currentCameraIndex].gameObject.SetActive(true); // ativa a primeira c�mera
    }

    void Update()
    {
        // alternar entre as c�meras usando a tecla C
        if (Input.GetKeyDown(KeyCode.C))
        {
            currentCameraIndex++;
            if (currentCameraIndex >= cameras.Length)
            {
                currentCameraIndex = 0;
            }

            // desativa todas as c�meras exceto a c�mera atual
            for (int i = 0; i < cameras.Length; i++)
            {
                if (i == currentCameraIndex)
                {
                    cameras[i].gameObject.SetActive(true);
                }
                else
                {
                    cameras[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
