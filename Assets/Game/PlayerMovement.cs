using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 0f; //Overridden in Inspector

    Vector3 movement;
    Rigidbody rb;

    public bool allowInput = true;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (allowInput)
        {
            rb.AddForce(movement * moveSpeed * Time.deltaTime);
        }        
    }

    public void OnMove(InputValue value) 
    {
        Vector2 input = value.Get<Vector2>();

        movement = new Vector3(input.x, 0, input.y);
    }
}
