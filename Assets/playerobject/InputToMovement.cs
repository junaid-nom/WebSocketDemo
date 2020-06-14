using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InputTimed
{
    public System.DateTime created;
    public UserInput inp;

    public InputTimed(DateTime created, UserInput inp)
    {
        this.created = created;
        this.inp = inp;
    }
}

public class InputBuffer
{
    List<InputTimed> receivedInput; // Right now only need the very last input. if this was more complex game with fighting game style inputs then would need a list

    // Start is called before the first frame update
    public InputBuffer()
    {
        receivedInput = new List<InputTimed>();
    }

    public void receiveInput(UserInput inp)
    {
        receivedInput.Add(new InputTimed(System.DateTime.Now, inp));
        //Debug.Log("Got inp" + inp);
    }

    void clearOld()
    {
        receivedInput = receivedInput.FindAll(inps => Constants.timeDiff(System.DateTime.Now, inps.created).TotalMilliseconds <= Constants.inputLifetimeMS);
    }

    public void clearBuffer()
    {
        receivedInput.Clear();
    }

    public UserInput getInput()
    {
        clearOld();
        UserInput inp;
        if (receivedInput.Count > 0)
        {
            UserInput combined = new UserInput();
            UserInput lastInp = receivedInput[receivedInput.Count - 1].inp;
            combined.x = lastInp.x;
            combined.y = lastInp.y;
            
            List<InputTimed> pressed = receivedInput.FindAll(inps => inps.inp.buttonsDown.Contains(true));
            if (pressed.Count > 0)
            {
                combined.buttonsDown = pressed[pressed.Count - 1].inp.buttonsDown;
                combined.target = pressed[pressed.Count - 1].inp.target;
            }
            else
            {
                combined.buttonsDown = lastInp.buttonsDown;// this is just an empty list
                combined.target = lastInp.target;
            }

            inp = combined;
        }
        else
        {
            inp = new UserInput();
            inp.buttonsDown = new List<bool>();
        }
        
        return inp;
    }
}

public class InputToMovement : MonoBehaviour
{
    public static UserInput getClientInput()
    {
        UserInput newInp = new UserInput();
        newInp.x = Input.GetAxis("Horizontal");
        newInp.y = Input.GetAxis("Vertical");
        newInp.buttonsDown = new List<bool>();
        newInp.buttonsDown.Add(Input.GetMouseButtonDown(0));
        newInp.buttonsDown.Add(Input.GetMouseButtonDown(1));
        newInp.buttonsDown.Add(Input.GetMouseButtonDown(2));
        newInp.buttonsDown.Add(Input.GetButton("dodge"));
        newInp.buttonsDown.Add(Input.GetButton("pickup"));

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            Vector3 target = ray.GetPoint(distance);
            newInp.target = target;
            //Vector3 direction = target - transform.position;
            //float rotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            //transform.rotation = Quaternion.Euler(0, rotation, 0);
        }
        
        return newInp;
    }

    public static Quaternion getRotationFromInput(UserInput inp)
    {
        Vector3 direction = new Vector3(inp.x, 0, inp.y).normalized;
        return Quaternion.Euler(0, Quaternion.LookRotation((direction).normalized).eulerAngles.y, 0);
    }

    public static Quaternion getRotationToLookAt(Vector3 position, Vector3 lookAt)
    {
        position = new Vector3(position.x, 0, position.z);
        lookAt = new Vector3(lookAt.x, 0, lookAt.z);
        Vector3 directionTo = lookAt - position;
        return Quaternion.Euler(0, Quaternion.LookRotation((directionTo).normalized).eulerAngles.y, 0);
    }

    public static CopyMovement inputToMovement(UserInput inp, Vector3 oldPositionLocal, Quaternion oldRotationLocal, float speed, Animator animator, string canChangeState, List<string> stateNames, string uid, float health)
    {
        CopyMovement cp = new CopyMovement();
        // Have to use IsName because you can't check animation state directly. Though you can check animation clip I didn't here.
        bool canChange = animator.GetCurrentAnimatorStateInfo(0).IsName(canChangeState);
        bool canDodge = false;
        foreach (string s in Constants.dodgeFromStates)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName(s))
            {
                canDodge = true;
            }
        }
        inp.x = Mathf.Clamp(inp.x, -1, 1);
        inp.y = Mathf.Clamp(inp.y, -1, 1);

        if (canChange && (inp.x != 0 || inp.y != 0))
        {
            Vector3 direction = new Vector3(inp.x, 0, inp.y).normalized;
            Vector3 newPosition = oldPositionLocal + (direction * (Time.deltaTime * speed));
            cp.localPosition = newPosition;
            cp.localRotation = getRotationFromInput(inp);
        }
        else
        {
            cp.localPosition = oldPositionLocal;
            cp.localRotation = oldRotationLocal;
        }

        int index = inp.buttonsDown.FindIndex(u => u);
        
        if (canDodge && inp.buttonsDown != null && inp.buttonsDown.Count >=3 && inp.buttonsDown[3])
        {
            cp.anim_state = stateNames[3];
            cp.normalizedTime = 0;
        }
        else if (!canChange)
        {
            // this is a little strange cause the animation playing on client will get constantly overriden to a delayed version from the server. So need to ignore normalized time if local normalizedTime is > msg time
            cp.anim_state = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            cp.normalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        }
        else if (canChange && index >= 0)
        {
            // change direction to what is pointed at
            var lookat = getRotationToLookAt(cp.localPosition, inp.target);
            cp.localRotation = lookat;
            cp.anim_state = stateNames[index];
            cp.normalizedTime = 0;
        }
        else
        {
            cp.anim_state = null;
            cp.normalizedTime = -1;
        }

        cp.objectInfo = new NetworkObjectInfo(animator.gameObject.GetInstanceID()+"", NetworkObjectType.playerCharacter, uid);

        cp.health = health;
        return cp;
    }

    //public IEnumerator SendInputAfterTime(float delay, UserInput inp)
    //{
    //    Debug.Log("send inp soon");
    //    yield return new WaitForSeconds(delay);

    //    // Code to execute after the delay
    //    // target.setMovement(inputToMovement(inp, objectToControl.transform.localPosition, objectToControl.transform.localRotation, speed, animator, canMoveState, stateNames));
    //    Debug.Log("send inp now");
    //    inputBuffer.receiveInput(inp);
    //}



    //// Update is called once per frame
    void Update()
    {
        //StartCoroutine(SendInputAfterTime(sendPingSecs, getInput(), copying));

        if (Client.ws != null 
            && Client.ws.GetState() == HybridWebSocket.WebSocketState.Open)
        {
            UserInput nowInput = getClientInput();
            //Send msg to server
            Client.ws.Send(BinarySerializer.Serialize(nowInput));

            // Get my GameObject, update its rotation right away
            if (Client.myobjsByType.ContainsKey(System.Enum.GetName(typeof(NetworkObjectType), NetworkObjectType.playerCharacter)))
            {
                string myObjID = Client.myobjsByType[System.Enum.GetName(typeof(NetworkObjectType), NetworkObjectType.playerCharacter)][0];
                GameObject myChar = Client.objIDToObject[myObjID].gameObject;
                Animator instantFeedbackAnimator = myChar.GetComponent<Animator>();
                bool canChange = instantFeedbackAnimator.GetCurrentAnimatorStateInfo(0).IsName(Constants.canMoveState);
                if (canChange && (nowInput.x != 0 || nowInput.y != 0))
                {
                    myChar.transform.localRotation = getRotationFromInput(nowInput);
                }
            }
            
        }
        


        //StartCoroutine(SendInputAfterTime(sendPingSecs, nowInput)); // all movement is delayed, but locally change the rotation right away


        //bool canChange = instantFeedbackAnimator.GetCurrentAnimatorStateInfo(0).IsName(canMoveState);
        //if (canChange && (nowInput.x != 0 || nowInput.y != 0))
        //    instantFeedbackObject.transform.localRotation = getRotationFromInput(nowInput);

        //UserInput delayedInp = inputBuffer.getInput();
        //copying.setMovement(inputToMovement(delayedInp, objectToControl.transform.localPosition, objectToControl.transform.localRotation, speed, animator, canMoveState, stateNames));
    }
}
