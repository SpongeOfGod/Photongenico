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
          
                hit.collider.GetComponentInParent<CarDamage>().RecieveDamage(force);
            
        }
    }
    


     public void RecieveDamage( float force)
    {        
           Current -= (force * controller.carVelocityRatio) / CarRb.mass;
        Debug.Log(Current);
    }
}
