using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doRandomStuff : MonoBehaviour
{
    public Animator animator;
    public float speedPerSecond;
    public float maxDistance;

    int maxAnimations = 3;
    string animationVariable = "anim";
    Vector3 targetPosition;
    Vector3 startPosition;
    bool movingToTarget = false;
    // Start is called before the first frame update
    void Start()
    {
        startPosition = gameObject.transform.position;
        targetPosition = startPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("defAnim") && animator.GetInteger("anim") == 0 && moveToTargetPoint())
        {
            // pick new point or animation
            float newRandom = Random.Range(0f, 1f);
            if (newRandom < .35f)
            {
                targetPosition = startPosition + (Random.insideUnitSphere * maxDistance);
                targetPosition.y = startPosition.y;
                transform.rotation = Quaternion.Euler(0, Quaternion.LookRotation((targetPosition - transform.position).normalized).eulerAngles.y, 0);
                movingToTarget = true;
            }
            else
            {
                int newAnim = Random.Range(0, maxAnimations) + 1;
                animator.SetInteger(animationVariable, newAnim);
                movingToTarget = false;
            }
        }
    }
    
    bool moveToTargetPoint()
    {
        float accuracy = .01f;
        float distanceTravel = speedPerSecond * Time.deltaTime;
        if ((gameObject.transform.position - targetPosition).magnitude < accuracy || !movingToTarget)
        {
            movingToTarget = false;
            return true;
        } else
        {
            if ((targetPosition - transform.position).magnitude <= distanceTravel)
            {
                transform.position = targetPosition;
            }
            else
            {
                transform.position += (targetPosition - transform.position).normalized * distanceTravel;
            }
            return false;
        }
    }
}
