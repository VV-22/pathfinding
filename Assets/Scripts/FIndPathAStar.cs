using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;
using Unity.Burst.Intrinsics;

public class PathMarker
{
    public MapLocation location;
    public float G;
    public float H;
    public float F;
    public GameObject marker;
    public PathMarker parent;

    public PathMarker(MapLocation l,float g,float h, float f, GameObject marker , PathMarker p)
    {
        location = l;
        G = g;
        H = h;
        F = f;
        this.marker = marker;
        parent = p;
    }
    public override bool Equals(object obj)
    {
        if(obj == null || this.GetType() != obj.GetType())
            return false;
        PathMarker checkingObj = obj as PathMarker;
        if(Vector2.Equals(location.ToVector() , checkingObj.location.ToVector())) 
            return true;
        return false;
    }

    public override int GetHashCode()
    {
        return 0;
    }
}



public class FIndPathAStar : MonoBehaviour
{

    public Maze maze;
    public Material closedMaterial;
    public Material openMaterial;

    List<PathMarker> open = new List<PathMarker>();
    List<PathMarker> closed = new List<PathMarker>();

    public GameObject start;
    public GameObject end;
    public GameObject pathP;
    
    PathMarker goalNode;
    PathMarker startNode;

    PathMarker lastPos;

    bool done = false;

    void removeAllMarkers()
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");
        foreach(GameObject marker in markers)
            Destroy(marker);
    }


    void BeginSearch()
    {
        done = false;
        removeAllMarkers();

        List<MapLocation> locations = new List<MapLocation>();
        for(int z = 1; z < maze.depth - 1 ; z++)
            for(int x = 1; x < maze.width - 1 ; x++)
            {
                if(maze.map[x,z] != 1)
                    locations.Add(new MapLocation(x, z));
            }
        locations.Shuffle();

        Vector3 startLocation = new Vector3(locations[0].x * maze.scale, 0f, locations[0].z * maze.scale);
        startNode = new PathMarker(new MapLocation(locations[0].x, locations[0].z) , 0 , 0 , 0  ,Instantiate(start, startLocation, quaternion.identity), null);
        
        Vector3 goalLocation = new Vector3(locations[1].x * maze.scale, 0f, locations[1].z * maze.scale);
        goalNode = new PathMarker(new MapLocation(locations[1].x, locations[1].z) , 0 , 0 , 0  ,Instantiate(end, goalLocation, quaternion.identity), null);

        open.Clear();
        closed.Clear();
        open.Add(startNode);
        lastPos = startNode;
    }


    void Search(PathMarker thisNode)
    {
        if(thisNode.Equals(goalNode))
        {
            //reached final node
            done = true;
            return;
        }

        foreach(MapLocation dir in maze.Directions)
        {
            MapLocation neighbor = dir + thisNode.location;
            if(maze.map[neighbor.x, neighbor.z] == 1)//wall
                continue;
            if(neighbor.x > maze.width - 1 || neighbor.z > maze.depth - 1 || neighbor.x < 1 || neighbor.z < 1) // out of bounds
                continue;
            if(IsClosed(neighbor)) //already in closedList
                continue;

            float G = Vector2.Distance(thisNode.location.ToVector(), neighbor.ToVector()) + thisNode.G;//should be 1 for square and 1.414 for diagonal neighbors (futureproofing for diagonal moves)
            float H = Vector2.Distance(neighbor.ToVector(), goalNode.location.ToVector());
            float F = G + H;
            GameObject pathBlock = Instantiate(pathP , new Vector3(neighbor.x * maze.scale , 0f, neighbor.z * maze.scale), Quaternion.identity);
            
        }
    }

    bool IsClosed(MapLocation marker)
    {
        foreach(PathMarker p in closed)
        {
            if(p.location.Equals(marker))
                return true;
        }
        return false;
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
            BeginSearch();
        
    }
}
