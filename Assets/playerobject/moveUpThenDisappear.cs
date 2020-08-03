using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveUpThenDisappear : MonoBehaviour
{
    float liveTimeSecs = 2;
    float startTime = 0;
    float speed = 10;
    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + (Time.deltaTime * speed), transform.localPosition.z);
        if (Time.time - startTime > liveTimeSecs)
        {
            Destroy(gameObject);
        }
    }
}
