using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    public GameObject plr;

    Vector3 offset = new Vector3(-2.5f, 2.5f ,2.5f);//offset rom player
    Vector3 goal;

    //Fixed update so it doesnt jitter
    //Makes camera look at player at same angle
    void FixedUpdate()
    {
        goal = plr.transform.position + offset;
        transform.position = Vector3.Lerp(transform.position,goal,.1f);
        transform.LookAt(transform.position -offset);
    }
}
