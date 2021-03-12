using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// THE GENERATION SCRIPT
public class Generation : MonoBehaviour
{
    public GameObject floor;
    Dictionary<Vector2Int,Floor> grid = new Dictionary<Vector2Int, Floor>(); //existing tiles

    public Dictionary<Vector2Int,Room> rooms = new Dictionary<Vector2Int, Room>(); //existing rooms
    public Vector2Int gridSize; //grid size

    public GameObject summoner; //summoner object

    public Transform floorParent;// object for floor to parent to

    public Material transBrick;//material for transparent wall


    void Start()
    {
        AllocateRooms();
    }

    //check if room overlaps something
    bool RoomOverlapping(Vector2Int X, Vector2Int Y){
        for(int i = X.x; i <= X.y;i++){
            for(int j = Y.x; j <= Y.y;j++){
                if(grid.ContainsKey(new Vector2Int(i,j)))
                    return true;
            }
        }
        return false;
    }

    //if you can place a tile there
    bool CanPlaceTile(Vector2Int tile,Dictionary<Vector2Int,Floor> blacklist){
        for(int i = -1; i <= 1; i++){
            for(int j = -1; j <= 1; j++){
                if(Mathf.Abs(i+j) == 1){
                    if(grid.ContainsKey(new Vector2Int(tile.x + i, tile.y + j)) && !blacklist.ContainsKey(new Vector2Int(tile.x + i, tile.y + j))){
                        return false;
                    }
                }
            }
        }
        return true;
    }

    //get the room by whether or not that tile is in a room
    Room GetRoomByTile(Vector2Int tile){
        foreach(Room room in rooms.Values){
            if(room.roomGrid.ContainsKey(tile))
                return room;
        }
        return null;
    }

    //check if a floor tile is next to a room
    bool IsConnectedToRoom(Vector2Int tile,Room room){
        if(room.roomGrid.ContainsKey(tile))
            return false;
        for(int i = -1; i <= 1; i++){
            for(int j = -1; j <= 1; j++){
                if(Mathf.Abs(i+j) == 1){
                    if(room.roomGrid.ContainsKey(new Vector2Int(tile.x + i, tile.y + j))){
                        return true;
                    }
                }
            }
        }
        return false;
    }

    //place the roomTiles
    void AllocateRoomTiles(Room room){
        for(int i = 0; i < room.size.x;i++){
            for(int j = 0; j < room.size.y;j++){
                RoomFloor newFloor = new RoomFloor(new Vector2Int(room.tile.x + i, room.tile.y + j),Instantiate(floor, new Vector3((room.tile.x + i)*2,0,(room.tile.y + j)*2),Quaternion.identity));
                room.roomGrid.Add(newFloor.tile,newFloor);
                grid.Add(newFloor.tile,newFloor);
            }
        }
    }

    //generate a path
    //I loop through every room to generate a path towards the spawn room
    //First I check what directions it needs to go then select a random direction
    //When placing a tile I check for 2 things
    //1. is it next to a room and was a previous one also?
    // if so I delete the previous one. this way rooms stay more off-path
    //2. is it next to a different tile?
    //if it is, I place the tile then break. it made a path thats connected to a different path
    void GeneratePaths(){
        foreach(Room room in rooms.Values){
            if(!room.hasPath){
                Dictionary<Vector2Int,Floor> placedTiles = new Dictionary<Vector2Int, Floor>();
                foreach(KeyValuePair<Vector2Int,Floor> tile in room.roomGrid){
                    placedTiles.Add(tile.Key,tile.Value);
                }
                Vector2Int closest = room.tile;

                foreach(Room room2 in rooms.Values){
                    if(room2.type == "Spawn")
                        closest = room2.tile;
                }
                   Vector2Int current = room.tile;
                while(current != closest){
                    int xdir = 0;
                    int ydir = 0;
                    int xdis = Mathf.Abs(current.x - closest.x);
                    int ydis = Mathf.Abs(current.y - closest.y);
                    //get the values and choose a random one
                    if((Random.Range(0,2) == 0 && xdis != 0) || ydis == 0){
                        xdir = 1;
                        if(current.x > closest.x){
                            xdir = -1;
                        }
                    }else{
                        ydir = 1;
                        if(current.y > closest.y){
                            ydir = -1;
                        }
                    }
                    //place tile maybe?
                    var newCurrent = current + new Vector2Int(xdir,ydir);
                    if(CanPlaceTile(newCurrent,placedTiles)){
                        if(!grid.ContainsKey(newCurrent)){
                            Floor newFloor = new Floor(newCurrent,Instantiate(floor, new Vector3(newCurrent.x*2,0,newCurrent.y*2),Quaternion.identity));
                            grid.Add(newCurrent,newFloor);
                            placedTiles.Add(newCurrent,newFloor);
                            if(IsConnectedToRoom(current,room) && IsConnectedToRoom(newCurrent,room)){
                                Destroy(grid[current].floor);
                                grid.Remove(current);
                                placedTiles.Remove(current);
                            }
                        }
                        current = newCurrent;
                        //place tile and break;
                    }else if(!grid.ContainsKey(newCurrent)){
                        Floor newFloor = new Floor(newCurrent,Instantiate(floor, new Vector3(newCurrent.x*2,0,newCurrent.y*2),Quaternion.identity));
                        grid.Add(newCurrent,newFloor);
                        break;
                    }else{
                        current = newCurrent;
                    }
                }
                room.hasPath = true;
            }
        }
    }

    //setup decoration, it first makes a bool using Random.Range to see whether or not to do it. then either does it or dont
    void SetupDecorations(){
        foreach(KeyValuePair<Vector2Int,Floor> tile in grid){
            var obj = tile.Value.floor;
            bool broken = Random.Range(0,2) == 0;
            if(broken){
                var tiles = obj.transform.Find("Decorations").Find("Tiles").Find("Normal");
                foreach(Transform small in tiles){
                    bool isBroken = Random.Range(0,7) == 0;
                    if(isBroken){
                        small.localScale = new Vector3(Random.Range(0.018f,.09f),0.05f,Random.Range(0.018f,.09f));
                        float xRange = .09f - small.localScale.x/2;
                        float yRange = .09f - small.localScale.y/2;
                        small.localPosition = small.localPosition + new Vector3(Random.Range(-xRange,xRange),0,Random.Range(-yRange,yRange));
                    }
                }
            }
            bool hasFountain = Random.Range(0,15) == 0;
            var fountain = obj.transform.Find("Decorations").Find("FountainThing");
            if(hasFountain){
                int rotation = Random.Range(0,4)*90;
                fountain.rotation = Quaternion.Euler(0,rotation,0);
            }else{
                Destroy(fountain.gameObject);
            }
            bool hasMoss = Random.Range(0,9) == 0;
            var moss = obj.transform.Find("Decorations").Find("Moss");
            if(hasMoss){
                int rotation = Random.Range(0,4)*90;
                moss.rotation = Quaternion.Euler(0,rotation,0);
            }else{
                Destroy(moss.gameObject);
            }
            var crystal = obj.transform.Find("Decorations").Find("Crystals");
            if(!hasFountain && tile.Value.GetType().ToString() != "RoomFloor" ){
                bool hasCrystal = Random.Range(0,12) == 0;
                if(hasCrystal){
                    int crystalSize = Random.Range(0,100);
                    if(crystalSize < 60){
                        Destroy(crystal.Find("MediumCrystal").gameObject);
                        Destroy(crystal.Find("LargeCrystal").gameObject);
                    }else if(crystalSize >= 60 && crystalSize < 87){
                        Destroy(crystal.Find("SmallCrystal").gameObject);
                        Destroy(crystal.Find("LargeCrystal").gameObject);
                    }else if(crystalSize >= 87){
                        Destroy(crystal.Find("SmallCrystal").gameObject);
                        Destroy(crystal.Find("MediumCrystal").gameObject);
                    }
                    int rotation = Random.Range(0,4)*90;
                    crystal.rotation = Quaternion.Euler(0,rotation,0);
                }else{
                Destroy(crystal.gameObject);
            }
            }else{
                Destroy(crystal.gameObject);
            }

            //Checks if its a roomfloor, if there's no tile connected to it and place a wall
            //if there is a tile connected to it and the tile is a normal floortile, place a door
            var wall = obj.transform.Find("Decorations").Find("Wall");
            var door = obj.transform.Find("Decorations").Find("Door");
            if(tile.Value.GetType().ToString() == "RoomFloor"){
                for(int i = -1; i <= 1; i++){
                    for(int j = -1; j <=1;j++){
                        if(Mathf.Abs(i-j) == 1){
                            if(!grid.ContainsKey(tile.Key + new Vector2Int(i,j))){
                                var newWall = Instantiate(wall.gameObject,obj.transform.Find("Decorations"));
                                newWall.transform.rotation = Quaternion.LookRotation(new Vector3(i,0,j));
                                if(i-j == -1)
                                newWall.GetComponentInChildren<MeshRenderer>().material = transBrick;
                            }else if(grid[tile.Key + new Vector2Int(i,j)].GetType().ToString() == "Floor"){
                                var newDoor = Instantiate(door.gameObject,obj.transform.Find("Decorations"));
                                newDoor.transform.rotation = Quaternion.LookRotation(new Vector3(i,0,j));
                                Room room = GetRoomByTile(tile.Key);
                                if(room != null){
                                    room.doors.Add(newDoor);
                                }
                                if(i-j == -1)
                                foreach(MeshRenderer renderer in newDoor.GetComponentsInChildren<MeshRenderer>()){
                                    renderer.material = transBrick;
                                }
                            }
                        }
                    }
                }
            }
            Destroy(wall.gameObject);
            Destroy(door.gameObject);
            obj.transform.SetParent(floorParent);
        }
    }

    //Loop through all tiles once again once the navmesh is setup to spawn enemies perhaps
    void spawnEnemies(){
        foreach(KeyValuePair<Vector2Int,Floor> tile in grid){
            bool hasEnemy = Random.Range(0,9) == 0;
            if(hasEnemy){
                NavMeshHit closestHit;
                if(NavMesh.SamplePosition( new Vector3(tile.Key.x*2,0.503f,tile.Key.y*2) , out closestHit, 500, 1 ) ){
                    Enemy enemy = GameObject.FindObjectOfType<Game>().GetEnemy();
                    var newEnemy = Instantiate(enemy);
                    newEnemy.transform.position = closestHit.position;
                } 
                print(closestHit.position);
            }
        }
    }

    //try to create and allocate a room
    //it has a max of 1000 tries to make sure it doesn't get in an endless loop
    //although in theory this is bad cause there is an incredible improbable chance that it still cant place a room after 1000 tries
    //but fuck it
    Room CreateRoom(string type,Vector2Int size){
        Vector2Int startPoint = new Vector2Int(Random.Range(0,gridSize.x),Random.Range(0,gridSize.y));
        int counter = 0;
        while(counter < 1000 && RoomOverlapping(new Vector2Int(startPoint.x-1,startPoint.x+size.x),new Vector2Int(startPoint.y-1,startPoint.y+size.y))){
            startPoint = new Vector2Int(Random.Range(0,gridSize.x),Random.Range(0,gridSize.y));
            counter++;
        }
        if(counter>=1000)
            return null;
        Room room = new Room(startPoint,type,size);
        rooms.Add(startPoint,room);
        AllocateRoomTiles(room);

        if(type != "Spawn"){
            var newSummoner = Instantiate(summoner);
            newSummoner.transform.localPosition = new Vector3(startPoint.x*2f -1 + size.x,0.113f,startPoint.y*2 -1 + size.y);
            room.summoner = newSummoner;
        }

        return room;
    }

    //do everything
    //spawn 3 mini's and a spawn, generate path, spawn boss, generate path, setup decorations
    //setup navmesh, spawn player if in dungeon scene, close boss doors, spawn enemies
    void AllocateRooms(){
        for(int i = 0; i < 3;i++){
            CreateRoom("Mini",new Vector2Int(3,3));
        }
        var spawn = CreateRoom("Spawn",new Vector2Int(2,2));
        GeneratePaths();
        var boss = CreateRoom("Final",new Vector2Int(4,4));
        GeneratePaths();
        SetupDecorations();
        floorParent.GetComponentInChildren<NavMeshSurface>().BuildNavMesh();
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != 0){
            transform.GetChild(0).position = new Vector3(spawn.tile.x*2,0.765f,spawn.tile.y*2);
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(0).SetParent(null);
        }
        foreach(GameObject door in boss.doors){
            StartCoroutine(GameObject.FindObjectOfType<Game>().letsgo(door.transform.Find("Cube"),door.transform.Find("DoorGoal").position));
        }
        spawnEnemies();
    }

}

//class for the room
public class Room{
    public string type;
    public Vector2Int size;
    public Vector2Int tile;
    public Dictionary<Vector2Int,Floor> roomGrid = new Dictionary<Vector2Int, Floor>();
    public List<GameObject> doors = new List<GameObject>();
    
    public GameObject summoner;
    public bool active = false;
    public bool bossDefeated = false;

    public bool hasPath = false;
    public Room(Vector2Int _tile,string _type, Vector2Int _size){
        tile = _tile;
        type = _type;
        size = _size;
    }

    public void ActivateSummoner(Game game){
        if(bossDefeated){
            game.End();
        }else{
            active = true;
            game.ActivateSummoner(this);
        }
    }
}

//class for the FLoor
public class Floor{
    public GameObject floor;
    public Vector2Int tile;
    public Floor(Vector2Int _tile, GameObject _floor){
        tile = _tile;
        floor = _floor;
    }
    public Floor(){}
}

//class for the roomfloor to differentiate tiles
public class RoomFloor : Floor{

    public RoomFloor(Vector2Int _tile, GameObject _floor){
        tile = _tile;
        floor = _floor;
    }
}