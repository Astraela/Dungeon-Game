using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


//script for everythign player
public class Player : MonoBehaviour
{
    public Transform cam;
    public float speed = 3;
    public Transform swordHinge;
    public float maxSway = 20;
    public GameObject sword;
    public float damage;

    public enum State{
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

    public State currentState = State.Walking;

    Generation generation;

    //wait a frame for unneeded Game object to delete itself
    //load and assigin all stats.
    IEnumerator Start(){
        yield return new WaitForEndOfFrame();
        var stats = GameObject.FindObjectOfType<Game>().stats;
        dashCD = stats.DashCD;
        slamCD = stats.SlamCD;
        speed = stats.Speed;
        damage = stats.Damage;
        generation = GameObject.FindObjectOfType<Generation>();
    }

    //when the sword hit something, check if it still exists and send over that it Hit that;
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
                    var enemy = other.gameObject.GetComponent<Enemy>();
                    enemy.health -= damage;
                    enemy.switchState(Enemy.stateEnum.Hit);
                }
            }
        }
    }

    //when the enemy hits me :(
    public void GetHit(){
        SwitchState(State.Hurting);
    }

    //when you fall into le water
    IEnumerator onFall(){
        sword.SetActive(false);
        yield return new WaitForSeconds(.2f);
        transform.position = lastPos[0] - new Vector3(0,lastPos[0].y,0) + new Vector3(0,0.765f,0);
        sword.SetActive(true);
        SwitchState(State.Hurting);
    }

    //raycast down every frame to store the position in a list, and remove the last one if its long enough.
    //checks if ur position is on floor
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

    //normal generic switchstate function
    public void SwitchState(State newState){
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
            case State.Dying:
                OnDeath();
            break;
        }
        currentState = newState;
    }

    //When state is switched to slam.
    //calculate and store angle.
    //differentiate what side you slash and sets rotation.
    //enabled trail
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

    //checks and stores the angle to dash
    void OnDash(){
        Vector3 Movement = Vector3.zero;

        float xSpeed = Input.GetAxisRaw("Horizontal");
        float zSpeed = Input.GetAxisRaw("Vertical");
        //Movement += new Vector3(xSpeed,0,zSpeed);
        Movement = -transform.forward * xSpeed;
        Movement += transform.right * zSpeed;

        if(xSpeed == 0 && zSpeed == 0){
            SwitchState(State.Walking);
            return;
        }

        swordHinge.GetComponentInChildren<TrailRenderer>().emitting = true;
        Movement = Vector3.Normalize(Movement);
        DashAngle = Movement;
    }

    //On Hurt :(
    void OnHurt(){
        hurtProgress = 0;
    }

    //On DEATH, freeze rigidbody so I dont get pushed, make sword fall in pieces
    void OnDeath(){
        foreach(Transform child in sword.transform){
            child.gameObject.AddComponent<BoxCollider>();
            child.gameObject.AddComponent<Rigidbody>();
        }
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
    }

    //on walk, check for falling.
    // apply velocity and make sword sway depending on angle.
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

    //when you be slamming, rotate the sword and stop when far enough
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

    //when you be dashing, sets velocity to desired, stops when dashed for long enough
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

    //flickers sword to signify invincibility frames and that ur hurt
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
        //does desired state function
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

        //if ur clicking, and you can slam, slam
        if(Input.GetKeyDown(KeyCode.Mouse0) && slamProgress >= slamCD && currentState == State.Walking){
            SwitchState(State.Slamming);
        }else if(slamProgress < slamCD){
            slamProgress += Time.deltaTime;
        }

        //if ur holding space, and u can dash, dash
        if(Input.GetKey(KeyCode.Space) && dashCDTimer >= dashCD && currentState == State.Walking){
            SwitchState(State.Dashing);
        }else if(dashCDTimer < dashCD){
            dashCDTimer += Time.deltaTime;
        }
        
        //if u pressed escape, pause game
        if(Input.GetKeyDown(KeyCode.Escape)){
            GameObject.FindObjectOfType<EscMenu>().OnEsc();
        }

        //Check voor summoning
        //there is 200% a better way to do this
        foreach(Room room in generation.rooms.Values){
            if(room.type == "Spawn" || (room.active && room.bossDefeated == false)) continue;
            var obj = room.summoner;
            var canvas = obj.GetComponentInChildren<Canvas>();
            var slider = obj.GetComponentInChildren<Slider>();
            if(Vector3.Distance(obj.transform.position,transform.position) < 0.9f){
                canvas.enabled = true;
                if(Input.GetKey(KeyCode.E)){
                    slider.value += 0.01f;
                    if(slider.value >= 1){
                        room.ActivateSummoner(GameObject.FindObjectOfType<Game>());
                        canvas.enabled = false;
                    }
                }else{
                    slider.value = 0;
                }
            }else{
                canvas.enabled = false;
                slider.value = 0;
            }
        }
    }
}
