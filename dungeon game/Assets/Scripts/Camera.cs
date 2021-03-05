using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    public GameObject plr;
    // Start is called before the first frame update
    public Vector3 offset = new Vector3(2.5f, 2.5f ,2.5f);
    Vector3 goal;
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        goal = plr.transform.position + offset;
        transform.position = Vector3.Lerp(transform.position,goal,.1f);
        transform.LookAt(transform.position -offset);
    }
}
