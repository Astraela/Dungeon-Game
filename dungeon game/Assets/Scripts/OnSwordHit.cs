using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSwordHit : MonoBehaviour
{
    Player plr;

    public List<Collider> colliding = new List<Collider>();

    void Start(){
        plr = GetComponentInParent<Player>();
    }

    void OnTriggerEnter(Collider other)
    {
        colliding.Add(other);
    }

    void OnTriggerExit(Collider other){
        colliding.Remove(other);
    }
}
