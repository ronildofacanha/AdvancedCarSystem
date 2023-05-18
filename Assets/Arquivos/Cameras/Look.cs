using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Look : MonoBehaviour
{
    private Transform alvo;
    // Start is called before the first frame update
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        alvo = player.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Gatilho_Para_Anim.ativado)
            transform.LookAt(alvo);
    }
}
