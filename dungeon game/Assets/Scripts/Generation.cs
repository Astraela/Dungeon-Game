using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Generation : MonoBehaviour
{
    public GameObject floor;
    Dictionary<Vector2Int,Floor> grid = new Dictionary<Vector2Int, Floor>();

    Dictionary<Vector2Int,Room> rooms = new Dictionary<Vector2Int, Room>();
    public Vector2Int gridSize;
    void Start()
    {
        AllocateRooms();
    }

    bool RoomOverlapping(Vector2Int X, Vector2Int Y){
        for(int i = X.x; i <= X.y;i++){
            for(int j = Y.x; j <= Y.y;j++){
                if(grid.ContainsKey(new Vector2Int(i,j)))
                    return true;
            }
        }
        return false;
    }

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

    Room GetRoomByTile(Vector2Int tile){
        foreach(Room room in rooms.Values){
            if(room.roomGrid.ContainsKey(tile))
                return room;
        }
        return null;
    }

    bool IsConnectedToRoom(Vector2Int tile,Room room){
        if(room.roomGrid.ContainsKey(tile))
            return false;
        for(int i = -1; i <= 1; i++){
            for(int j = -1; j <= 1; j++){
                if(Mathf.Abs(i+j) == 1){
                    if(room.roomGrid.ContainsKey(new Vector2Int(tile.x + i, tile.y + j)))
                    return true;
                }
            }
        }
        return false;
    }

    void AllocateRoomTiles(Room room){
        for(int i = 0; i < room.size.x;i++){
            for(int j = 0; j < room.size.y;j++){
                RoomFloor newFloor = new RoomFloor(new Vector2Int(room.tile.x + i, room.tile.y + j),Instantiate(floor, new Vector3((room.tile.x + i)*2,0,(room.tile.y + j)*2),Quaternion.identity));
                room.roomGrid.Add(newFloor.tile,newFloor);
                grid.Add(newFloor.tile,newFloor);
                //newFloor.floor.transform.localScale += new Vector3(0,1,0);
            }
        }
    }

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

    void SetupPaths(){
        foreach(KeyValuePair<Vector2Int,Floor> tile in grid){
            if(tile.Value.GetType().ToString() == "RoomFloor"){
                foreach(Transform child in tile.Value.floor.transform){
                    child.GetComponent<MeshRenderer>().enabled = true;
                    child.GetComponent<BoxCollider>().enabled = true;
                }
                continue;
            }
            for(int i = -1; i <= 1; i++){
                for(int j = -1; j <=1;j++){
                    if(Mathf.Abs(i-j) == 1){
                        if(grid.ContainsKey(tile.Key + new Vector2Int(i,j))){
                            var side = tile.Value.floor.transform.Find(i + "," + j);
                            side.GetComponent<MeshRenderer>().enabled = true;
                            side.GetComponent<BoxCollider>().enabled = true;
                        }
                        
                    }
                }
            }//1,1
            if(grid.ContainsKey(tile.Key + new Vector2Int(1,1)) && grid.ContainsKey(tile.Key + new Vector2Int(0,1)) && grid.ContainsKey(tile.Key + new Vector2Int(1,0))){
                var side = tile.Value.floor.transform.Find(1 + "," + 1);
                side.GetComponent<MeshRenderer>().enabled = true;
                side.GetComponent<BoxCollider>().enabled = true;
            }//-1,-1
            if(grid.ContainsKey(tile.Key + new Vector2Int(-1,-1)) && grid.ContainsKey(tile.Key + new Vector2Int(0,-1)) && grid.ContainsKey(tile.Key + new Vector2Int(-1,0))){
                var side = tile.Value.floor.transform.Find(-1 + "," + -1);
                side.GetComponent<MeshRenderer>().enabled = true;
                side.GetComponent<BoxCollider>().enabled = true;
            }//1,-1
            if(grid.ContainsKey(tile.Key + new Vector2Int(1,-1)) && grid.ContainsKey(tile.Key + new Vector2Int(0,-1)) && grid.ContainsKey(tile.Key + new Vector2Int(1,0))){
                var side = tile.Value.floor.transform.Find(1 + "," + -1);
                side.GetComponent<MeshRenderer>().enabled = true;
                side.GetComponent<BoxCollider>().enabled = true;
            }//-1,1
            if(grid.ContainsKey(tile.Key + new Vector2Int(-1,1)) && grid.ContainsKey(tile.Key + new Vector2Int(0,1)) && grid.ContainsKey(tile.Key + new Vector2Int(-1,0))){
                var side = tile.Value.floor.transform.Find(-1 + "," + 1);
                side.GetComponent<MeshRenderer>().enabled = true;
                side.GetComponent<BoxCollider>().enabled = true;
            }
            foreach(Transform child in tile.Value.floor.transform){
                if(child.GetComponent<MeshRenderer>().enabled == false)
                    Destroy(child.gameObject);
            }
        }
    }

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

            var wall = obj.transform.Find("Decorations").Find("Wall");
            var door = obj.transform.Find("Decorations").Find("Door");
            if(tile.Value.GetType().ToString() == "RoomFloor"){
                for(int i = -1; i <= 1; i++){
                    for(int j = -1; j <=1;j++){
                        if(Mathf.Abs(i-j) == 1){
                            if(!grid.ContainsKey(tile.Key + new Vector2Int(i,j))){
                                var newWall = Instantiate(wall.gameObject,obj.transform.Find("Decorations"));
                                newWall.transform.rotation = Quaternion.LookRotation(new Vector3(i,0,j));
                            }else if(grid[tile.Key + new Vector2Int(i,j)].GetType().ToString() == "Floor"){
                                var newDoor = Instantiate(door.gameObject,obj.transform.Find("Decorations"));
                                newDoor.transform.rotation = Quaternion.LookRotation(new Vector3(i,0,j));
                                Room room = GetRoomByTile(tile.Key + new Vector2Int(i,j));
                                if(room != null){
                                    room.doors.Add(newDoor);
                                }
                            }
                        }
                    }
                }
            }
            Destroy(wall.gameObject);
            Destroy(door.gameObject);

            obj.GetComponentInChildren<NavMeshSurface>().BuildNavMesh();
        }
    }

    void CreateRoom(string type,Vector2Int size){
        Vector2Int startPoint = new Vector2Int(Random.Range(0,gridSize.x),Random.Range(0,gridSize.y));
        int counter = 0;
        while(counter < 1000 && RoomOverlapping(new Vector2Int(startPoint.x-1,startPoint.x+size.x),new Vector2Int(startPoint.y-1,startPoint.y+size.y))){
            startPoint = new Vector2Int(Random.Range(0,gridSize.x),Random.Range(0,gridSize.y));
            counter++;
        }
        if(counter>=1000)
            return;
        Room room = new Room(startPoint,type,size);
        rooms.Add(startPoint,room);
        AllocateRoomTiles(room);
    }

    void AllocateRooms(){
        for(int i = 0; i < 3;i++){
            CreateRoom("Mini",new Vector2Int(3,3));
        }
        CreateRoom("Spawn",new Vector2Int(2,2));
        GeneratePaths();
        CreateRoom("Final",new Vector2Int(4,4));
        GeneratePaths();
        //SetupPaths();
        SetupDecorations();

        NavMeshHit myNavHit;
        if(NavMesh.SamplePosition(new Vector3(0,0,0), out myNavHit,100, -1)){
            transform.position = myNavHit.position;
        }
    }

}

public class Room{
    public string type;
    public Vector2Int size;
    public Vector2Int tile;
    public Dictionary<Vector2Int,Floor> roomGrid = new Dictionary<Vector2Int, Floor>();

    public List<GameObject> doors = new List<GameObject>();

    public bool hasPath = false;
    public Room(Vector2Int _tile,string _type, Vector2Int _size){
        tile = _tile;
        type = _type;
        size = _size;
    }
}

public class Floor{
    public GameObject floor;
    public Vector2Int tile;
    public Floor(Vector2Int _tile, GameObject _floor){
        tile = _tile;
        floor = _floor;
    }
    public Floor(){}
}

public class RoomFloor : Floor{

    public RoomFloor(Vector2Int _tile, GameObject _floor){
        tile = _tile;
        floor = _floor;
    }
}