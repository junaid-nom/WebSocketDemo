using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StringPair
{
    public string s1;

    [TextArea(15, 20)]
    public string s2;
    public StringPair(string s1, string s2)
    {
        this.s1 = s1;
        this.s2 = s2;
    }

    public override bool Equals(object obj)
    {
        var pair = obj as StringPair;
        return pair != null &&
               s1 == pair.s1;
    }
}

public class InspectorDebugger : MonoBehaviour
{
    public List<StringPair> pairs;

    public void addPair(StringPair s)
    {
        int exists = pairs.FindIndex(sin => sin.s1 == s.s1);
        if (exists >= 0)
        {
            pairs[exists] = s;
        }
        else
        {
            pairs.Add(s);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
