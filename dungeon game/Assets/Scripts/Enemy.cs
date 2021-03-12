using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//de enemy class, now inclulding Finate State Machines
public class Enemy : MonoBehaviour
{
    public stateEnum state = stateEnum.Walk; //State voor de FSM
    public enum stateEnum{
        Idle,
        Walk,
        Follow,
        Attack,
        Hit
    }
    public float walkRange = 10; //afstand die die kan lopen in de walk state
    public float attackRange = 10; //afstand tussen de enemy en player om je te achtervolgen
    public float loseRange = 12; //afstand tussen de enemy en player om je kwijt te raken
    public float dmgRange = 3; //afstand tussen de enemy en player om je de damagen
    public float hitTime = .2f; //tijd dat de knockback effect duur
    public float idleTime = 3; //hoe lang het idle is na het lopen en (raken van de player/3)
    //Levens met een andere setter voor het destroyen van de enemy en droppen van iron bij dood
    [SerializeField]
    float _health = 10;
    public float health {
        get{return _health;}
        set{
            _health = value;
            if(value <= 0){
                GameObject.FindObjectOfType<Game>().AddCrystals(crystals);
                Destroy(gameObject);
            }
        }
    }
    public int damage = 10; //damage aan de player
    public int crystals = 4;
    public float speed = 2;

    Vector3 startPoint; //startpoint voor de walkdrange

    float currentTime = 0;    //timer voor Idle state
    float currenthitTime = 0; //timer voor hit state
    //benodigde components
    NavMeshAgent agent;
    Rigidbody rb;
    
    Vector3 closest = Vector3.positiveInfinity;
    float closestDistance = int.MaxValue;
    Player closestPlayer;
    Vector3 goal; //goal for Walk state

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        agent = gameObject.AddComponent<NavMeshAgent>();
        agent.radius = 2.39f;
        agent.height = 4.03f;
        agent.speed = speed;
        startPoint = transform.position;
    }


    void GetClosest(){
        var players = GameObject.FindObjectsOfType<Player>();
        Vector3 newClosest = transform.position;
        float newClosestDistance = Mathf.Infinity;
        foreach(Player player in players){
            float distance = Vector3.Distance(transform.position,player.transform.position);
            if(distance < newClosestDistance){
                newClosestDistance = distance;
                newClosest = player.transform.position;
                closestPlayer = player;
            }
        }
        closest = newClosest;
        closestDistance = newClosestDistance;
    }
    //functie om de state te veranderen en een mogelijke functie aan te roepen op verandering
    public void switchState(stateEnum newState){
        switch(newState){
            case stateEnum.Idle:
                currentTime = idleTime;
                break;
            case stateEnum.Walk:
                onWalk();
                break;
            case stateEnum.Follow:
                break;
            case stateEnum.Attack:
                onAttack();
                break;
            case stateEnum.Hit:
                onHit();
                break;
        }
        state = newState;
    }

    //wanneer walk aangeroepen wordt, een nieuw goal maken en de agent laten bewegen
    void onWalk(){
            goal = startPoint + new Vector3(Random.Range(-walkRange,walkRange),0,Random.Range(-walkRange,walkRange));
            agent.SetDestination(goal);
    }
    //wanneer de state walk is kijken of de enemy dichtbij genoeg is bij de player of bij het goal aangekomen is
    void Walk(){
        if(closestDistance < attackRange)
            switchState(stateEnum.Follow);
        
        if(agent.remainingDistance <= agent.stoppingDistance)
            switchState(stateEnum.Idle);

    }

    //wanneer de state idle is kijken of de enemy dichtbij genoeg is bij de player of de timer omlaag halen en veranderen naar walk
    void Idle(){
        if(closestDistance < attackRange)
            switchState(stateEnum.Follow);

        currentTime -= Time.deltaTime;
        if(currentTime <= 0)
            switchState(stateEnum.Walk);
    }
    
    //wanneer follow de state is het pad veranderen naar de player positie. kijken of de enemy te ver weg is en veranderen naar idle of bij dichtbij genoeg veranderen naar attack
    void Follow(){
            agent.SetDestination(closest);
            
        if(closestDistance > loseRange)
            switchState(stateEnum.Idle);

        if(closestPlayer != null && closestDistance < dmgRange && closestPlayer.currentState == Player.State.Walking)
            switchState(stateEnum.Attack);
    }

    //wanneer state verandert wordt naar attack, health naar beneden vertical velocity en hitVector veranderen.
    //agent en pad stoppen en timer resetten.
    void onAttack(){
        closestPlayer.GetHit();
        GameObject.FindObjectOfType<Game>().Damage(damage);
        agent.isStopped = true;
        agent.ResetPath();
        currentTime = idleTime/3;
    }

    //wanneer de state attack is, wachten tot de timer naar beneden is
    void Attack(){
        currentTime -= Time.deltaTime;
        if(currentTime <= 0){
            switchState(stateEnum.Follow);
            agent.isStopped = false;
        }
    }
    
    //wanneer de state naar hit gaat, rigidbody constraints uitzetten, agent uitzetten, velocity toepassen en hitTime resetten;
    void onHit(){
        if(!gameObject) return;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        agent.isStopped = true;
        currenthitTime = hitTime;
    }

    //wanneer de state hit is, wachten tot de timer naar beneden is en alles weer aan te zetten, agen.Warp zorgt tegen teleportatie.
    void Hit(){
        currenthitTime -= Time.deltaTime;
        if(currenthitTime <= 0){
            rb.constraints = RigidbodyConstraints.FreezeAll;
            agent.isStopped = false;
            switchState(stateEnum.Follow);
        }
    }
    //de benodigde state functies aanroepen
    void Update()
    {
        GetClosest();
        switch(state){
            case stateEnum.Walk:
                Walk();
                break;
            case stateEnum.Idle:
                Idle();
                break;
            case stateEnum.Follow:
                Follow();
                break;
            case stateEnum.Attack:
                Attack();
                break;
            case stateEnum.Hit:
                Hit();
                break;
        }
    }
}
