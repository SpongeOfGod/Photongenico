using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCarController : MonoBehaviour
{
    #region Player Input

    private bool breakInput;
    private bool jumpInput;

    private float moveInput;
    private float steerInput;

    #endregion

    #region Components

    [Header("Rigidbodies")]
    [SerializeField] private Rigidbody sphereRB;
    [SerializeField] private Rigidbody vehicleRB;

    [Header("Other Components")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance;

    #endregion

    #region Stats

    [Header("Car Stats")]
    [SerializeField] private float forwardSpeed;
    [SerializeField] private float backwardSpeed;
    [SerializeField] private float turnSpeed;

    [Header("Aerial Stats")]
    [SerializeField] private float fallSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float airDrag;
    [SerializeField] private float alignToGroundTime;

    private float normalDrag;

    #endregion

    #region Other Variables

    private bool isGrounded;

    #endregion

    private void OnEnable()
    {
        //Detach rigidbodies from vehicle.
        sphereRB.transform.parent = null;
        sphereRB.transform.position = transform.position;

        vehicleRB.transform.parent = null;
        vehicleRB.transform.position = transform.position;
    }

    private void Start()
    {
        normalDrag = sphereRB.drag;
    }

    private void Update()
    {
        HandleInputs();

        //Set vehicles's position to the sphere position to simulate it moving.
        transform.position = sphereRB.transform.position;

        //Set vehicle's rotation.
        if (isGrounded)
        {
            float newRotation = steerInput * turnSpeed * Time.deltaTime * Input.GetAxisRaw("Vertical"); //Multiplied by move input to negate rotation when not moving and to reverse rotation when going backwards. 
            transform.Rotate(0, newRotation, 0, Space.World);
        }
    }

    private void HandleInputs()
    {
        breakInput = Input.GetKey(KeyCode.Q);
        jumpInput = Input.GetKey(KeyCode.Space);

        steerInput = Input.GetAxisRaw("Horizontal");
        moveInput = Input.GetAxisRaw("Vertical");

        moveInput *= moveInput > 0 ? forwardSpeed : backwardSpeed;
    }

    private void FixedUpdate()
    {
        GroundCheck();
        ChangeVehicleDrag();

        if (isGrounded)
        {
            //Move the vehicle.
            sphereRB.AddForce(transform.forward * moveInput, ForceMode.Acceleration);
        }
        else
        {
            //Add extra gravity.
            sphereRB.AddForce(transform.up * -fallSpeed);
        }

        //Make the vehicle's collider rotation match the vehicle's rotation.
        vehicleRB.MoveRotation(transform.rotation);
    }

    private void GroundCheck()
    {
        RaycastHit hit;

        isGrounded = Physics.Raycast(transform.position, -transform.up, out hit, groundCheckDistance,groundLayer);
        Debug.DrawRay(transform.position, -transform.up.normalized * groundCheckDistance, Color.blue);

        //Rotate vehicle to be parallel to ground
        Quaternion toRotateTo = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, toRotateTo, alignToGroundTime * Time.deltaTime);
    }

    private void ChangeVehicleDrag()
    {
        if (isGrounded) sphereRB.drag = normalDrag;
        else sphereRB.drag = airDrag;
    }
}
