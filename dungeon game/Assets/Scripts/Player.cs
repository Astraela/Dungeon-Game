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
        Dashing,
        Hurting,
        Dying,
    }

    Vector3 Angle;
    float slamProgress = 1;
    public float slamCD = 1.5f;
    float slamRange = 80;
    bool slamLeft = false;
    public Transform swordHitbox;

    Vector3 DashAngle;
    float dashTimer = 0;
    float dashCDTimer = 0;
    public float dashTimerLength = 0.5f;
    public float dashCD = 1;
    public float dashStrength = 7;

    State currentState = State.Walking;

    public void onSwordHit(Collider other){
        if(other == null){
            swordHitbox.GetComponentInChildren<OnSwordHit>().colliding.Remove(other);
            return;
        }
        if(currentState == State.Slamming){
            if(!other.transform.IsChildOf(transform)){
                if(other.gameObject.tag == "Crystal"){
                    other.GetComponent<Crystal>().Hit();
                }
            }
        }
    }

    void SwitchState(State newState){
        switch(newState){
            case State.Slamming:
                OnSlam();
            break;
            case State.Dashing:
                OnDash();
            break;
        }
        currentState = newState;
    }

    void OnSlam(){
        UnityEngine.Camera camera = cam.GetComponent<UnityEngine.Camera>();
        Vector3 plrPos = camera.WorldToScreenPoint(transform.position);
        Angle = Vector3.Normalize(plrPos - Input.mousePosition);
        Angle = new Vector3(Angle.x,0,Angle.y);
        slamLeft = !slamLeft;
        swordHitbox.localRotation = Quaternion.LookRotation(Angle) * Quaternion.Euler(0,180,0);
        if(slamLeft){
            swordHinge.localRotation = Quaternion.LookRotation(Angle) * Quaternion.Euler(0,slamRange,-90);
        }else{
            swordHinge.localRotation = Quaternion.LookRotation(Angle) * Quaternion.Euler(0,-slamRange,-90);
        }
            swordHinge.GetComponentInChildren<TrailRenderer>().emitting = true;

        slamProgress = 0;
    }

    void OnDash(){
        Vector3 Movement = Vector3.zero;

        float xSpeed = Input.GetAxisRaw("Horizontal");
        float zSpeed = Input.GetAxisRaw("Vertical");
        //Movement += new Vector3(xSpeed,0,zSpeed);
        Movement = -transform.forward * xSpeed;
        Movement += transform.right * zSpeed;

        if(Movement == Vector3.zero){
            SwitchState(State.Walking);
            return;
        }

        swordHinge.GetComponentInChildren<TrailRenderer>().emitting = true;
        Movement = Vector3.Normalize(Movement);
        DashAngle = Movement;
    }

    void WalkUpdate(){
        Vector3 Movement = Vector3.zero;

        float xSpeed = Input.GetAxisRaw("Horizontal");
        float zSpeed = Input.GetAxisRaw("Vertical");
        //Movement += new Vector3(xSpeed,0,zSpeed);
        Movement = -transform.forward * xSpeed;
        Movement += transform.right * zSpeed;
        //Movement = Vector3.Normalize(Movement);
        Movement = Vector3.Normalize(Movement);
        Movement *= speed;

        var rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = Movement;
        swordHinge.localRotation = Quaternion.Euler(-Input.GetAxis("Horizontal")*maxSway,0,-Input.GetAxis("Vertical")*maxSway);
    }

    void SlamUpdate(){
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        if(!slamLeft && swordHinge.localRotation != Quaternion.LookRotation(Angle) * Quaternion.Euler(0,slamRange,-90)){
            var newq = Quaternion.Lerp(Quaternion.LookRotation(Angle) * Quaternion.Euler(0,-slamRange,-90), Quaternion.LookRotation(Angle) * Quaternion.Euler(0,slamRange,-90),slamProgress);
            swordHinge.localRotation = newq;
        }else if(slamLeft && swordHinge.localRotation != Quaternion.LookRotation(Angle) * Quaternion.Euler(0,-slamRange,-90)){
            var newq = Quaternion.Lerp(Quaternion.LookRotation(Angle) * Quaternion.Euler(0,slamRange,-90), Quaternion.LookRotation(Angle) * Quaternion.Euler(0,-slamRange,-90),slamProgress);
            swordHinge.localRotation = newq;
        }else{
            foreach(Collider other in swordHitbox.GetComponentInChildren<OnSwordHit>().colliding){
                onSwordHit(other);
            }
            SwitchState(State.Walking);
        }
        slamProgress += Time.deltaTime;
        slamProgress = Mathf.Pow(slamProgress,0.7f);
        if(slamProgress >= 0.95f)
            swordHinge.GetComponentInChildren<TrailRenderer>().emitting = false;
    }

    void DashUpdate(){
        var rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = DashAngle * dashStrength;
        swordHinge.localRotation = Quaternion.Euler(-Input.GetAxis("Horizontal")*(maxSway*3),0,-Input.GetAxis("Vertical")*(maxSway*3));
    
        dashTimer += Time.deltaTime;
        if(dashTimer > dashTimerLength){
            SwitchState(State.Walking);
            dashTimer = 0;
            dashCDTimer = 0;
            swordHinge.GetComponentInChildren<TrailRenderer>().emitting = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch(currentState){
            case State.Walking:
                WalkUpdate();
            break;
            case State.Slamming:
                SlamUpdate();
            break;
            case State.Dashing:
                DashUpdate();
            break;
        }
        if(Input.GetKeyDown(KeyCode.Mouse0) && slamProgress >= slamCD && currentState == State.Walking){
            SwitchState(State.Slamming);
        }else if(slamProgress < slamCD){
            slamProgress += Time.deltaTime;
        }
        if(Input.GetKeyDown(KeyCode.Space) && dashCDTimer >= dashCD && currentState == State.Walking){
            SwitchState(State.Dashing);
        }else if(dashCDTimer < dashCD){
            dashCDTimer += Time.deltaTime;
        }
    }
}
