using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    GameObject fill;
    float initialScale;
    // Start is called before the first frame update
    void Start()
    {
        fill = this.gameObject;
        initialScale = fill.transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setHPScale(float fraction)
    {
        float newx = initialScale * fraction;
        fill.transform.localScale = new Vector3(newx, fill.transform.localScale.y, fill.transform.localScale.z);
    }
}
