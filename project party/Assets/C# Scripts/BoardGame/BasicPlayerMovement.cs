using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class BasicPlayerMovement : NetworkBehaviour
{
    public float speed;

    public float hor;
    public float vert;
    private Vector3 dir;
    public TMP_Text text;

   

    private void Start()
    {

        text = GameObject.FindGameObjectWithTag("Text").GetComponent<TMP_Text>();
    }

    
    private void Update()
    {
        if (!IsOwner) return;
        InputCheck();
        Moving();
    }

    private void InputCheck()
    {
        hor = Input.GetAxisRaw("Horizontal");
        vert = Input.GetAxisRaw("Vertical");
    }

    private void Moving()
    {

        dir = new Vector3(hor, 0, vert);
        transform.Translate(dir * speed * Time.deltaTime);
    }
}
