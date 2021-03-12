using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform cam;
    public float speed = 3;
    public Transform swordHinge;
    public float maxSway = 20;
    public GameObject sword;
    public float damage;

    enum State{
        Walking,
        Slamming,
        Dashing,
        Hurting,
        Dying,
    }

    [Header("Slamming Variables")]
    Vector3 Angle;
    float slamProgress = 1;
    public float slamCD = 1.5f;
    float slamRange = 80;
    bool slamLeft = false;
    public Transform swordHitbox;

    [Header("Dashing Variables")]
    Vector3 DashAngle;
    float dashTimer = 0;
    float dashCDTimer = 0;
    public float dashTimerLength = 0.5f;
    public float dashCD = 1;
    public float dashStrength = 7;

    [Header("Hurting Variables")]
    float hurtProgress = 0;
    public float hurtTime = 1f;

    public LayerMask raycastLayerMask;
    List<Vector3> lastPos =  new List<Vector3>();

    State currentState = State.Walking;

    Generation generation;

    IEnumerator Start(){
        yield return new WaitForEndOfFrame();
        var stats = GameObject.FindObjectOfType<Game>().stats;
        dashCD = stats.DashCD;
        slamCD = stats.SlamCD;
        speed = stats.Speed;
        damage = stats.Damage;
        generation = GameObject.FindObjectOfType<Generation>();
    }

    public void onSwordHit(Collider other){
        if(other == null){
            swordHitbox.GetComponentInChildren<OnSwordHit>().colliding.Remove(other);
            return;
        }
        if(currentState == State.Slamming){
            if(!other.transform.IsChildOf(transform)){
                if(other.gameObject.tag == "Crystal"){
                    other.GetComponent<Crystal>().Hit();
                }else if(other.gameObject.GetComponent<Enemy>()){
                    print("HitEnemy");
                    var enemy = other.gameObject.GetComponent<Enemy>();
                    enemy.health -= damage;
                    enemy.switchState(Enemy.stateEnum.Hit);
                }
            }
        }
    }

    public void GetHit(){
        SwitchState(State.Hurting);
    }

    IEnumerator onFall(){
        sword.SetActive(false);
        yield return new WaitForSeconds(.2f);
        transform.position = lastPos[0] - new Vector3(0,lastPos[0].y,0) + new Vector3(0,0.765f,0);
        sword.SetActive(true);
        SwitchState(State.Hurting);
    }

    void FallCheck(){
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(-Vector3.up), out hit, Mathf.Infinity,raycastLayerMask))
        {
            lastPos.Add(hit.point);
            if(lastPos.Count > 30){
                lastPos.Remove(lastPos[0]);
            }
        }
        else
        {
            StartCoroutine(onFall());
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
            case State.Hurting:
                OnHurt();
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

    void OnHurt(){
        hurtProgress = 0;
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
        FallCheck();
    }

    void SlamUpdate(){
        if(slamProgress == 0){
            foreach(Collider other in swordHitbox.GetComponentInChildren<OnSwordHit>().colliding){
                onSwordHit(other);
            }
        }
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        if(!slamLeft && swordHinge.localRotation != Quaternion.LookRotation(Angle) * Quaternion.Euler(0,slamRange,-90)){
            var newq = Quaternion.Lerp(Quaternion.LookRotation(Angle) * Quaternion.Euler(0,-slamRange,-90), Quaternion.LookRotation(Angle) * Quaternion.Euler(0,slamRange,-90),slamProgress);
            swordHinge.localRotation = newq;
        }else if(slamLeft && swordHinge.localRotation != Quaternion.LookRotation(Angle) * Quaternion.Euler(0,-slamRange,-90)){
            var newq = Quaternion.Lerp(Quaternion.LookRotation(Angle) * Quaternion.Euler(0,slamRange,-90), Quaternion.LookRotation(Angle) * Quaternion.Euler(0,-slamRange,-90),slamProgress);
            swordHinge.localRotation = newq;
        }else{
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

    void HurtUpdate(){
        hurtProgress += Time.deltaTime;
        if(hurtProgress % .2f < .1){
            sword.SetActive(false);
        }else{
            sword.SetActive(true);
        }
        if(hurtProgress > hurtTime){
            sword.SetActive(true);
            SwitchState(State.Walking);
        }
        WalkUpdate();
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
            case State.Hurting:
                HurtUpdate();
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
        foreach(Room room in generation.rooms.Values){
            if(room.type == "Spawn" || room.active) continue;
            var obj = room.summoner;
            if(Vector3.Distance(obj.transform.position,transform.position) < 0.75f){
                obj.GetComponentInChildren<Canvas>().enabled = true;
                if(Input.GetKey(KeyCode.E)){
                    obj.GetComponentInChildren<Slider>().value += 0.01f;
                    if(obj.GetComponentInChildren<Slider>().value >= 1){
                        room.ActivateSummoner(GameObject.FindObjectOfType<Game>());
                        obj.GetComponentInChildren<Canvas>().enabled = false;
                    }
                }else{
                    obj.GetComponentInChildren<Slider>().value = 0;
                }
            }else{
                obj.GetComponentInChildren<Canvas>().enabled = false;
                obj.GetComponentInChildren<Slider>().value = 0;
            }
        }
    }
}
