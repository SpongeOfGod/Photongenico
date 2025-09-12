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

    void Start()
    {
        Current = maxHealth;
    }

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
            Current -=  (force) / CarRb.mass;
            Debug.Log(Current);

            CarRb.AddForce(transform.position * force, ForceMode.Impulse);
        }
    }
}
