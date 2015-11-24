using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Vertex
{
    public Vertex(float x, float y)
    {
        Position = new Vector2(x, y);
    }

    public Vertex(Vector2 vec)
    {
        Position = vec;
    }
   
    public float y
    {
        get { return Position.y; }
    }

    public float x
    {
        get { return Position.x; }
    }

    public Vector2 Position;
}
