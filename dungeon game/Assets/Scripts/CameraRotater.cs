using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotater : MonoBehaviour
{
    //rotates the camera in the menu scene
    void Update()
    {
        transform.rotation *= Quaternion.Euler(0,.05f,0);
    }
}
