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
    private Vector3 boxsize = Vector3.zero;

    void Start()
    {
        Current = maxHealth;
        boxsize.x = 2;
        boxsize.z = 2;
        boxsize.y = 2;
    }

    void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.BoxCast(RaycastPosition.position,boxsize,transform.forward,out hit, Quaternion.identity,rayDist, Car)) 
        {
            float force = controller.calculateForce();
          
            hit.collider.GetComponentInParent<CarDamage>().RecieveDamage(force, this.transform);
        }
    }

    public void RecieveDamage(float force,Transform car)
    {
        if (force > 4000)
        {
            Debug.Log(force);
            Current -=  (force) / CarRb.mass;

            CarRb.AddForceAtPosition( new Vector3 (Vector3.Angle(car.position,this.transform.position) * force ,0, Vector3.Angle(car.position, this.transform.position) * force),transform.position, ForceMode.Force);
        }
    }
}
