using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleVehicleController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxVelocity;
    [SerializeField] private float speed;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float jumpForce;

    [Header("Components")]
    private Rigidbody rb;

    [Header("Other")]
    [SerializeField] private float groundCheckDistance;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos;

    //Input Variables
    private bool acelerationInput;
    private bool reverseInput;
    private bool breakInput;
    private bool steerLeft;
    private bool steerRight;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        HandleInputs();
    }

    private void HandleInputs()
    {
        acelerationInput = Input.GetKey(KeyCode.W);
        reverseInput = Input.GetKey(KeyCode.S);
        steerRight = Input.GetKey(KeyCode.D);
        steerLeft = Input.GetKey(KeyCode.A);
        breakInput = Input.GetKey(KeyCode.Q);

        if (Input.GetKeyDown(KeyCode.Space) && GroundCheck())
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        Move();
        Turn();
    }

    private void Move()
    {
        if(!GroundCheck()) return;

        if (breakInput)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.01f);
            return;
        }

        if (acelerationInput)
        {
            rb.AddForce(transform.forward * speed, ForceMode.Force);
        }

        if (reverseInput)
        {
            rb.AddForce(-transform.forward * speed, ForceMode.Force);
        }

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);

        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        localVelocity.x = 0;
        rb.velocity = transform.TransformDirection(localVelocity);
    }

    private void Turn()
    {
        if (steerRight)
        {
            rb.AddTorque(Vector3.up * turnSpeed);
        }
        else if (steerLeft)
        {
            rb.AddTorque(-Vector3.up * turnSpeed);
        }
    }

    private void Jump()
    {
        rb.AddForce(transform.up * jumpForce);
    }

    public bool GroundCheck()
    {
        RaycastHit hit;
        bool grounded;
        Color rayColor;

        if (Physics.Raycast(transform.position, -transform.up, out hit, groundCheckDistance))
        {
            grounded = true;
            rayColor = Color.green;
        }
        else 
        {
            grounded = false;
            rayColor = Color.red;
        }

        if (drawGizmos) Debug.DrawRay(transform.position, -transform.up.normalized * groundCheckDistance, rayColor);
        return grounded;
    }

    //private void OnDrawGizmos()
    //{
    //    if (!drawGizmos) return;

    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawRay(transform.position, -transform.up.normalized * groundCheckDistance);
    //}
}
