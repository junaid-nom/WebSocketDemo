using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationInfo : MonoBehaviour
{
    public List<AnimationClip> animations = new List<AnimationClip>();
    public Dictionary<string, AnimationClip> nameToAnimation = new Dictionary<string, AnimationClip>();
    // Start is called before the first frame update
    void Start()
    {
        animations.ForEach(anim => nameToAnimation.Add(anim.name, anim));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
