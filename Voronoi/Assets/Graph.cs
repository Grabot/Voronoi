using UnityEngine;
using System.Collections.Generic;

public class Graph : MonoBehaviour
{
    public float m_Radius = 5f;
    List<Vector2> vertices = new List<Vector2>();

    void Awake()
    {
        createVertices();
    }

    private void createVertices()
    {
        vertices.Add(new Vector2(3, 3));
        GameObject gob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gob.transform.position = vertices[0];
        gob.transform.localScale = new Vector3(m_Radius, m_Radius, m_Radius);
            
    }

    void OnMouseDown()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        if (hits.Length > 0)
        {
            Vector3 newPos = hits[0].point;
            vertices.Add(newPos);

            GameObject gob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gob.transform.position = newPos;
            gob.transform.localScale = new Vector3(m_Radius, m_Radius, m_Radius);
        }
    }
}
