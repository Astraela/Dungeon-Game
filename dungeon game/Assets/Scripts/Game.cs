using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using System.IO;


//script for most of the gameUI, gameStats and gameLoop
public class Game : MonoBehaviour
{
    public GameObject canvas;
    public Slider health;
    public Text crystals;
    public Stats stats;

    int clearedRooms = 0;

    public List<Enemy> enemyList = new List<Enemy>();
    public List<Enemy> bossList = new List<Enemy>();

    //Destroy itself if there's already one
    //this is purely for testing so I dont have to start in the main scene
    void Awake()
    {
        if(GameObject.FindObjectsOfType<Game>().Length > 1){
            Destroy(gameObject);
        }else if(SceneManager.GetActiveScene().buildIndex == 2){
            canvas.SetActive(true);
        }
    }

    //returns a random enemy
    public Enemy GetEnemy(){
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

    //returns a random boss
    Enemy GetBoss(){
        int rnd = Random.Range(0,100);
        if(rnd < 90)
            return bossList[0];
        return bossList[1];
    }

    //lerps the object to its goal
    public IEnumerator letsgo(Transform door,Vector3 goal){
        Vector3 start = door.position;
        float max = 60;
        for(int i = 0; i <= max; i++){
            door.position = Vector3.Lerp(start,goal,i/max);
            yield return new WaitForEndOfFrame();
        }
    }

    //opens the boss room when 3 summoners are activated
    void OpenBossRoom(){
        foreach(Room room in GameObject.FindObjectOfType<Generation>().rooms.Values){
            if(room.type == "Final"){
                foreach(GameObject door in room.doors){
                    StartCoroutine(letsgo(door.transform.Find("Cube"),door.transform.Find("DoorGoal").position- new Vector3(0,1.09f,0)));
                }
            }
        }
    }

    //Summoner loop
    IEnumerator spawnEnemies(Vector3 center, Room room){
        var summoner = room.summoner.transform;

            //Lowers the summoner
            Transform summonerBlock = summoner.Find("Summoner");
            Vector3 goal = summonerBlock.position - new Vector3(0,1,0);
            StartCoroutine(letsgo(summonerBlock,goal));
            yield return new WaitForSeconds(1);

        //if its a mini room, spawn 10 enemies
        if(room.type == "Mini"){
            for(int i = 0; i < 10; i++){

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
            //if its a boss room, spawn 5 enemies and a boss
        }else{
            for(int i = 0; i < 5; i++){

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
            yield return new WaitForSeconds(5);
            var boss = GetBoss();
            GameObject newBoss = null;
            if(true){//otherwise it'll cry about how closestHit already exists blablabla; boo hoo, if(true){} goes brrrr
                NavMeshHit closestHit;
                Vector3 angle = new Vector3(Random.Range(-1,1),0,Random.Range(-1,1));
                angle = Vector3.Normalize(angle) * .9f;
                Vector3 pos = center + angle;
                if(NavMesh.SamplePosition( pos, out closestHit, 500, 1 ) ){
                    newBoss = Instantiate(boss.gameObject);
                    newBoss.transform.position = closestHit.position;
                }
            }
            //wait till boss is defeated
            while(true){
                if(newBoss == null){
                    break;
                }
                yield return new WaitForEndOfFrame();
            }
            room.bossDefeated = true;
        }
        //start rotating of summoner crystal
        summoner.GetComponentInChildren<CrystalRotate>().StartRotate();

        clearedRooms++;
        if(clearedRooms >= 3)
            OpenBossRoom();
        //open doors
        foreach(GameObject door in room.doors){
            StartCoroutine(letsgo(door.transform.Find("Cube"),door.transform.Find("DoorGoal").position - new Vector3(0,1.09f,0)));
        }
    }

    //When summoner is activated close door and spawn enemies
    public void ActivateSummoner(Room room){
        Vector3 center = new Vector3(room.tile.x*2 + room.size.x/2,0.503f,room.tile.y*2+ room.size.y/2);
        foreach(GameObject door in room.doors){
            StartCoroutine(letsgo(door.transform.Find("Cube"),door.transform.Find("DoorGoal").position));
        }
        StartCoroutine(spawnEnemies(center,room));
    }

    //save for when you died or won
    void Save(){
        string fullPath = "";
        if(Application.isEditor){
            fullPath = Application.dataPath + "/Save" + stats.selectedSave + ".txt";
        }
        else
        {
            fullPath = Application.persistentDataPath + "/Save" + stats.selectedSave + ".txt";
        }
        string content = JsonUtility.ToJson(stats);
        FileStream file = File.Open(fullPath,FileMode.Create);
        StreamWriter writer = new StreamWriter(file);
        writer.Write(content);
        writer.Close();
        file.Close();
    }

    //death effect
    IEnumerator Death(){
        GameObject.FindObjectOfType<Player>().SwitchState(Player.State.Dying);
        float max = 60;
        for(int i = 0; i < max; i++){
            UnityEngine.Camera.main.orthographicSize = 1.6f - i/max;
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(2);
        End();
    }

    //ends the game and returns to upgrade scene
    public void End(){
        SetHealth(100);
        canvas.SetActive(false);
        Save();
        SceneManager.LoadScene(1);
    }

    //on damaged
    public void Damage(float h){
        stats.Health -= h;
        health.value = stats.Health/100;
        if(stats.Health <= 0){
            StartCoroutine(Death());
        }
    }

    //to set the health
    public void SetHealth(float h){
        stats.Health = h;
        health.value = h/100;
    }

    //to add crystals
    public void AddCrystals(int c){
        stats.Crystals += c;
        crystals.text = stats.Crystals.ToString();
    }

    //to set crystals
    public void SetCrystals(int c){
        stats.Crystals = c;
        crystals.text = c.ToString();
    }
}
