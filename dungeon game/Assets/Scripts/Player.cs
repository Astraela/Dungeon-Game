using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform cam;
    public float speed = 3;
    public Transform swordHinge;
    public float maxSway = 20;

    enum State{
        Walking,
        Slamming,
        Hurting,
        Dying,
    }

    State currentState = State.Walking;
    void SwitchState(State newState){
        switch(newState){
            case State.Slamming:
                OnSlam();
            break;
        }
        currentState = newState;
    }

    void OnSlam(){
        print("SLAMMING");
    }

    void WalkUpdate(){
        Vector3 Movement = Vector3.zero;

        float xSpeed = Input.GetAxisRaw("Horizontal");
        float zSpeed = Input.GetAxisRaw("Vertical");
        //Movement += new Vector3(xSpeed,0,zSpeed);
        Movement = -transform.forward * xSpeed;
        Movement += transform.right * zSpeed;
        //Movement = Vector3.Normalize(Movement);
        Movement *= speed;

        var rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = Movement;
        swordHinge.localRotation = Quaternion.Euler(-Input.GetAxis("Horizontal")*maxSway,0,-Input.GetAxis("Vertical")*maxSway);
    }

    // Update is called once per frame
    void Update()
    {
        switch(currentState){
            case State.Walking:
                WalkUpdate();
            break;
        }

        if(Input.GetKeyDown(KeyCode.Mouse0)){
            SwitchState(State.Slamming);
        }
    }
}
