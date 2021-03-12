using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalRotate : MonoBehaviour
{
    //rotating script for the crystal in the summoner
    bool rotating = false;
    //Moves the crystal up and destroys the crystal holder
    public void StartRotate(){
        transform.position = transform.position + new Vector3(0,.2f,0);
        Destroy(transform.parent.Find("Cube").gameObject);
        rotating = true;
    }

    void Update()
    {
        if(rotating){
            transform.localRotation *= Quaternion.Euler(1,-1,-1);
        }
    }
}
