using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;

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
        if(location.Equals(checkingObj.location))
        {
            return true;
        }
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
        // open.Add(startNode);
        closed.Add(startNode);
        lastPos = startNode;
    }

    //open list is when the values have not yet been traversed, closed list is when they have already been traversed
    void Search(PathMarker thisNode)
    {
        if(thisNode == null)
            return;
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
            
            TextMesh[] values = pathBlock.GetComponentsInChildren<TextMesh>();
            values[0].text = "G: " + G.ToString("0.00");
            values[1].text = "H: " + H.ToString("0.00");
            values[2].text = "F: " + F.ToString("0.00");

            if(!UpdateMarker(neighbor, G, H, F, thisNode))// when this is false, it has to be added as a new node in the open list
                open.Add(new PathMarker(neighbor, G, H ,F, pathBlock, thisNode));
        }
        open = open.OrderBy(p => p.F).ThenBy(n => n.H).ToList<PathMarker>();//sort values by lowest F, then sub-sort by lowest H. The one at the top of the list will have lowest F and H
        PathMarker pm = (PathMarker) open.ElementAt(0);// this is the next point
        closed.Add(pm);
        open.RemoveAt(0);

        pm.marker.GetComponent<Renderer>().material = closedMaterial;

        lastPos = pm;
    }

    //this method is to update the G, H, F values on the values in the open list when they are re-added as neighbors. Returns false when the location is not present in the open list
    bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt)
    {
        foreach(PathMarker p in open)
        {
            if(p.location.Equals(pos))
            {
                p.G = g;
                p.H = h;
                p.F = f;
                p.parent = prt;
                return true;
            }
        }
        return false;
    }
    bool IsClosed(MapLocation marker)
    {
        foreach(PathMarker p in closed)
        {
            // if(p.location.x == marker.x && p.location.z == marker.z)
            if(p.location.Equals(marker))
                return true;
        }
        return false;
    }

    void GetPath()
    {
        removeAllMarkers();
        PathMarker begin = lastPos;

        while(!startNode.Equals(begin) && begin != null)
        {
            Instantiate(pathP, new Vector3(begin.location.x * maze.scale , 0, begin.location.z * maze.scale) , Quaternion.identity);
            begin = begin.parent;
        }

        Instantiate(pathP, new Vector3(startNode.location.x * maze.scale ,0f, startNode.location.z * maze.scale) , Quaternion.identity);
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
            BeginSearch();
        
        if(Input.GetKeyDown(KeyCode.C) && !done)
            Search(lastPos);

        if(Input.GetKeyDown(KeyCode.M))
            GetPath();
    }
}
