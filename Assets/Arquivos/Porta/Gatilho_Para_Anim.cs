using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Gatilho_Para_Anim : MonoBehaviour
{
    public Animator animator;
    public static bool ativado=false;


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            animator.SetBool("Ativar", true);
            ativado = true;

            print("Enter");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            animator.SetBool("Ativar", false);
            ativado = false;
            print("Exit");
        }
    }
}
