using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
