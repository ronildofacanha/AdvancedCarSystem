using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera[] cameras; // lista de câmeras na cena

    private int currentCameraIndex = 0; // índice da câmera atual

    void Start()
    {
        // desativa todas as câmeras exceto a primeira
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(false);
        }

        cameras[currentCameraIndex].gameObject.SetActive(true); // ativa a primeira câmera
    }

    void Update()
    {
        // alternar entre as câmeras usando a tecla C
        if (Input.GetKeyDown(KeyCode.C))
        {
            currentCameraIndex++;
            if (currentCameraIndex >= cameras.Length)
            {
                currentCameraIndex = 0;
            }

            // desativa todas as câmeras exceto a câmera atual
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
