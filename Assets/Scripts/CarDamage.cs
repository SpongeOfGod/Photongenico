using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarDamage : MonoBehaviour
{
    private float maxHealth = 100;
    private float Current = 0;
    public Transform RaycastPosition;
    public Rigidbody CarRb;
    public LayerMask Car;
    [SerializeField] CarController controller;
    float rayDist = 2f;

    // Start is called before the first frame update
    void Start()
    {
        Current = maxHealth;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(RaycastPosition.position,RaycastPosition.forward, out hit,rayDist, Car)) 
        {
            float force = controller.calculateForce();
          
                hit.collider.GetComponentInParent<CarDamage>().RecieveDamage(force, transform);
            
        }
    }



    public void RecieveDamage(float force,Transform car)
    {
        if (force > 1000)
        {
            Debug.Log(force);
            Current -=  (force/10) / CarRb.mass;
            Debug.Log(Current);

            CarRb.AddRelativeForce(transform.position * MathF.Cos(car.transform.eulerAngles.magnitude) * force, ForceMode.Force);
        }
          
    }
}
