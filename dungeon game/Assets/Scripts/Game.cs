using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class Game : MonoBehaviour
{
    public GameObject canvas;
    public Slider health;
    public Text crystals;
    public Stats stats;

    int clearedRooms = 0;

    public List<Enemy> enemyList = new List<Enemy>();
    // Start is called before the first frame update
    void Awake()
    {
        if(GameObject.FindObjectsOfType<Game>().Length > 1){
            Destroy(gameObject);
        }else if(SceneManager.GetActiveScene().buildIndex == 2){
            canvas.SetActive(true);
        }
    }

    Enemy GetEnemy(){
        int rnd = Random.Range(0,100);
        Enemy enemy;
        if(rnd < 48){
            enemy = enemyList[0];
        }else if(rnd >= 48 && rnd < 96){
            enemy = enemyList[1];
        }else{
            enemy = enemyList[2];
        }
        return enemy;
    }

    public IEnumerator letsgo(Transform door,Vector3 goal){
        Vector3 start = door.position;
        int max = 30;
        for(int i = 0; i <= max; i++){
            door.position = Vector3.Lerp(start,goal,i/max);
            yield return new WaitForEndOfFrame();
        }
    }

    void OpenBossRoom(){
        foreach(Room room in GameObject.FindObjectOfType<Generation>().rooms.Values){
            if(room.type == "Final"){
                foreach(GameObject door in room.doors){
                    StartCoroutine(letsgo(door.transform.Find("Cube"),door.transform.Find("DoorGoal").position- new Vector3(0,2,0)));
                }
            }
        }
    }

    IEnumerator spawnEnemies(Vector3 center, Room room){
        var summoner = room.summoner.transform;
        if(room.type == "Mini"){
            Vector3 goal = summoner.position - new Vector3(0,1,0);
            int max = 30;
            for(int i = 0; i <= max; i++){
                summoner.position = Vector3.Lerp(summoner.position,goal,i/max);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(1);
            for(int i = 0; i <= 10; i++){

                Vector3 angle = new Vector3(Random.Range(-1,1),0,Random.Range(-1,1));
                angle = Vector3.Normalize(angle) * .9f;
                Vector3 pos = center + angle;
                NavMeshHit closestHit;
                if(NavMesh.SamplePosition( pos, out closestHit, 500, 1 ) ){
                    var enemy = GetEnemy();
                    var newEnemy = Instantiate(enemy);
                    newEnemy.transform.position = closestHit.position;
                }
                yield return new WaitForSeconds(4);
            }
            foreach(GameObject door in room.doors){
                StartCoroutine(letsgo(door.transform.Find("Cube"),door.transform.Find("DoorGoal").position - new Vector3(0,2,0)));
            }
            clearedRooms++;
            if(clearedRooms >= 3)
                OpenBossRoom();
        }else{
            Vector3 goal = summoner.position - new Vector3(0,1,0);
            int max = 30;
            for(int i = 0; i <= max; i++){
                summoner.position = Vector3.Lerp(summoner.position,goal,i/max);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(1);
            for(int i = 0; i <= 10; i++){

                Vector3 angle = new Vector3(Random.Range(-1,1),0,Random.Range(-1,1));
                angle = Vector3.Normalize(angle) * .9f;
                Vector3 pos = center + angle;
                NavMeshHit closestHit;
                if(NavMesh.SamplePosition( pos, out closestHit, 500, 1 ) ){
                    var enemy = GetEnemy();
                    var newEnemy = Instantiate(enemy);
                    newEnemy.transform.position = closestHit.position;
                }
                yield return new WaitForSeconds(4);
            }
            //SPAWN BOSS
            //SPAWN BOSS
            //SPAWN BOSS
            foreach(GameObject door in room.doors){
                StartCoroutine(letsgo(door.transform.Find("Cube"),door.transform.Find("DoorGoal").position - new Vector3(0,2,0)));
            }
        }
    }

    public void ActivateSummoner(Room room){
        Vector3 center = new Vector3(room.tile.x*2 + room.size.x/2,0.503f,room.tile.y*2+ room.size.y/2);
        foreach(GameObject door in room.doors){
            StartCoroutine(letsgo(door.transform.Find("Cube"),door.transform.Find("DoorGoal").position));
        }
        StartCoroutine(spawnEnemies(center,room));
    }

    public void Damage(float h){
        stats.Health -= h;
        health.value = h/100;
    }

    public void SetHealth(float h){
        stats.Health = h;
        health.value = h/100;
    }

    public void AddCrystals(int c){
        stats.Crystals += c;
        crystals.text = stats.Crystals.ToString();
    }
}
