using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    GameObject fill = null;
    float initialScale;
    // Start is called before the first frame update
    void Start()
    {
        bootup();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void bootup()
    {
        if (fill == null)
        {
            fill = this.gameObject;
            initialScale = fill.transform.localScale.x;
        }
    }

    public void setHPScale(float fraction)
    {
        bootup();
        float newx = initialScale * fraction;
        fill.transform.localScale = new Vector3(newx, fill.transform.localScale.y, fill.transform.localScale.z);
    }
}
