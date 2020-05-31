using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    public MeshRenderer mesh;
    Color defColor;
    // Start is called before the first frame update
    void Start()
    {
        defColor = mesh.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setColor(Color color)
    {
        mesh.material.color = color;
    }
    public void resetColor()
    {
        mesh.material.color = defColor;
    }
}
