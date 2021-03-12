using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//simple script to keep track of what the sword hit
public class OnSwordHit : MonoBehaviour
{
    public List<Collider> colliding = new List<Collider>();
    
    void OnTriggerEnter(Collider other)
    {
        colliding.Add(other);
    }

    void OnTriggerExit(Collider other){
        colliding.Remove(other);
    }
}
