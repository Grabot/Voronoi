using UnityEngine;
using System.Collections;

public class Triangle {

    Vertex[] T = new Vertex[3];

    public Triangle(Vertex a_v1, Vertex a_v2, Vertex a_v3)
    {
        T[0] = a_v1;
        T[1] = a_v2;
        T[2] = a_v3;
    }

    public bool inside( Vertex a_u1 )
    {
        int i = 0;
        int j = 0;
        bool inside = false;

        for (i = 0, j = 2; i < 3; j = i++)
        {
            if (((T[i].y > a_u1.y) != (T[j].y > a_u1.y)) &&
             (a_u1.x < (T[j].x - T[i].x) * (a_u1.y - T[i].y) / (T[j].y - T[i].y) + T[i].x))
                inside = !inside;
        }
        return inside;
    }

    public Vertex getRandomVertex
    {
        get { return T[0]; }
    }

}
