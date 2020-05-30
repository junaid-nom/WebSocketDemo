using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void printTest()
    {
        Debug.Log("GOT YO PRINT");
    }

    public void printTestString(string testy)
    {
        Debug.Log("GOT YO PRINT:" + testy);
    }
}
