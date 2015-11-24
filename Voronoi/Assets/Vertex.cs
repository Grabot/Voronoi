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

    public Vector2 Position;
}
