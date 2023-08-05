using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recursive : Maze
{
    public override void Generate()
    {
        Generate(5, 5);
    }

    void Generate(int x, int z)
    {
        if (CountSquareNeighbours(x, z) >= 2) return;
        map[x, z] = 0;

        Directions.Shuffle();

        Generate(x + Directions[0].x, z + Directions[0].z);
        Generate(x + Directions[1].x, z + Directions[1].z);
        Generate(x + Directions[2].x, z + Directions[2].z);
        Generate(x + Directions[3].x, z + Directions[3].z);
    }

}
