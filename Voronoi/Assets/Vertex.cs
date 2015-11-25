using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class Vertex
{
    public Vertex(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public bool isNan()
    {
        return float.IsNaN(x) || float.IsNaN(y);
    }

    public float x;
    public float y;
}