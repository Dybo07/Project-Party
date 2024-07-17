using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public Vector2 input;
    public float jump;
    public Rigidbody rb;
    public bool grounded;
    private bool isCoroutineRunning;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        transform.Translate(new Vector3(input.x, 0, input.y) * speed * Time.deltaTime);
    }

    public void OnMove(InputAction.CallbackContext ctx) => input = ctx.ReadValue<Vector2>();

    public void OnJump()
    {
        if (grounded)
        {
            rb.AddForce(0, jump, 0);
            grounded = false;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        grounded = true;
        //if (collision.collider.CompareTag("Ground"))
        //{
            
        //}
    }
}
