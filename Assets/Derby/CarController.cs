using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("References")]

    [SerializeField] private Rigidbody carRB;
    [SerializeField] private Transform[] Raypoints;
    [SerializeField] private LayerMask drivable;
    [SerializeField] private Transform AcelerationTrPoint;

    [Header("Suspension settings")]

    [SerializeField] private float springStiffness; //fuerza maxima que puede soportar cuando esta copletamente comprimido
    [SerializeField] private float restLenght;
    [SerializeField] private float springTravel; // maxima distancia la cual se puede comprimir o extender el resorte de su posicion normal.
    [SerializeField] private float wheelRadius;

    [SerializeField] private float damperStiffness;

    private int[] wheelsOnGround = new int[4];
    private bool IsGrounded;
    [Header("Inputs")]

    private float moveInput = 0;
    private float steerInput = 0;


    [Header("Car Settings")]
    [SerializeField] private float aceleration;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float deceleration;
    [SerializeField] private float steerStrength;
    [SerializeField] private AnimationCurve turningCurve;
    [SerializeField] private float draggcof;
    private Vector3 currentCarLocalVelocity = Vector3.zero;
    private float carVelocityRatio = 0;
    // Start is called before the first frame update
    void Start()
    {
        carRB = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        GetplayerInputs();
    }

    void FixedUpdate()
    {
        suspension();
        CheckGround();
        CalculateVel();
        Movement();
    }

    void suspension()
    {
        for (int i = 0; i < Raypoints.Length; i++)
        {

            RaycastHit hit;
            float maxlenght = restLenght + springTravel;

            if (Physics.Raycast(Raypoints[i].position, -Raypoints[i].up, out hit, maxlenght + wheelRadius, drivable))
            {
                wheelsOnGround[i] = 1;
                float currentSpringLenght = hit.distance - wheelRadius;

                float springCompression = (restLenght - currentSpringLenght) / springTravel;

                float springVelocity = Vector3.Dot(carRB.GetPointVelocity(Raypoints[i].position), Raypoints[i].up);
                float dampForce = damperStiffness * springVelocity;
                float springForce = springStiffness * springCompression;
                float netForce = springForce - dampForce;
                carRB.AddForceAtPosition(netForce * Raypoints[i].up, Raypoints[i].position);

                Debug.DrawLine(Raypoints[i].position, hit.point, Color.cyan);


            }
            else wheelsOnGround[i] = 0;
        }


    }
    void CheckGround()
    {
        int tempgroundwheels = 0;

        for (int i = 0; i < wheelsOnGround.Length; i++)
        {
            tempgroundwheels += wheelsOnGround[i];

        }

        if (tempgroundwheels > 1)
        {
            IsGrounded = true;
        }
        else IsGrounded = false;

    }

    private void Movement()
    {
        if (IsGrounded)
        {
            Acceleration();
            Deceleration();
            turn();
            SidewayDrag();
         }
    }
    private void Acceleration()
    {
        carRB.AddForceAtPosition(aceleration * moveInput * transform.forward, AcelerationTrPoint.position, ForceMode.Acceleration);
    }
     private void Deceleration()
    {
        carRB.AddForceAtPosition(deceleration * moveInput * -transform.forward, AcelerationTrPoint.position, ForceMode.Acceleration);
    }
    private void turn()
    {
        carRB.AddRelativeTorque (steerStrength * steerInput * turningCurve.Evaluate(Mathf.Abs(carVelocityRatio)) * Mathf.Sign(carVelocityRatio) * carRB.transform.up, ForceMode.Acceleration);
    }

    void SidewayDrag()
    {
        float currentSidewaysSpeed = currentCarLocalVelocity.x;

        float dragMag = -currentSidewaysSpeed * draggcof;
        Vector3 dragforce = transform.right * dragMag;

        carRB.AddForceAtPosition(dragforce, carRB.worldCenterOfMass, ForceMode.Acceleration);
    }
    private void CalculateVel()
    {
        currentCarLocalVelocity = transform.InverseTransformDirection(carRB.velocity);
        carVelocityRatio = currentCarLocalVelocity.z / maxSpeed;
    }

    private void GetplayerInputs()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }
}
