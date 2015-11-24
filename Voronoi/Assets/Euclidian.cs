using UnityEngine;
using System.Collections;
using System;

public class Euclidian : Distance {


    public double calculate(Vector2 u, Vector2 v)
    {
        return Math.Sqrt((v.x-u.x)*(v.x-u.x) + (v.y-u.y)*(v.y-u.y));
    }

}
