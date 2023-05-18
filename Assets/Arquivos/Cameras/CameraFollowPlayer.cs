using UnityEngine;
using Cinemachine;

public class CameraFollowPlayer : MonoBehaviour
{
    public string playerTag = "Player";
    private GameObject player;

    private void Start()
    {
        // Encontre o objeto do jogador pela tag
        player = GameObject.FindGameObjectWithTag(playerTag);

        // Defina o alvo de acompanhamento da câmera para o jogador
        if (player != null)
        {
            CinemachineFreeLook CM = GetComponent<CinemachineFreeLook>();
            CM.Follow = player.transform;
            CM.LookAt = player.transform;

        }
        else
        {
            Debug.LogWarning("Objeto do jogador não encontrado com a tag: " + playerTag);
        }
    }
}
