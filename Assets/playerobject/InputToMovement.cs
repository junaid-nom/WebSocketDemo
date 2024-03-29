﻿using System;
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
        receivedInput.Add(new InputTimed(System.DateTime.Now, inp)); // last is most recent
        if (receivedInput.Count > Constants.maxInputBuffer)
        {
            receivedInput.RemoveRange(0, receivedInput.Count - (Constants.maxInputBuffer/2));
        }
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
        // THIS INPUT QUEUE IS CLEARED EVERY TIME THERE IS A NEW ANIMATION (get hit, dodge, attack etc)
        UserInput inp;
        if (receivedInput.Count > 0)
        {
            UserInput combined = new UserInput();
            UserInput lastInp = receivedInput[receivedInput.Count - 1].inp;
            
            
            List<InputTimed> pressed = receivedInput.FindAll(inps => inps.inp.buttonsDown.Contains(true));
            if (pressed.Count > 0)
            {
                combined.buttonsDown = pressed[pressed.Count - 1].inp.buttonsDown;
                combined.target = pressed[pressed.Count - 1].inp.target;
                combined.equipedSlot1 = pressed[pressed.Count - 1].inp.equipedSlot1;
                combined.x = pressed[pressed.Count - 1].inp.x;
                combined.y = pressed[pressed.Count - 1].inp.y;
            }
            else
            {
                combined.buttonsDown = lastInp.buttonsDown;// this is just an empty list
                combined.target = lastInp.target;
                combined.equipedSlot1 = lastInp.equipedSlot1;
                combined.x = lastInp.x;
                combined.y = lastInp.y;
            }

            inp = combined;
        }
        else
        {
            inp = new UserInput();
            inp.buttonsDown = new List<bool>();
            inp.equipedSlot1 = true;
        }
        
        return inp;
    }
}

public class InputToMovement : MonoBehaviour
{
    public static UserInput getClientInput(bool equipedSlot1)
    {
        UserInput newInp = new UserInput();
        newInp.x = Input.GetAxis("Horizontal");
        newInp.y = Input.GetAxis("Vertical");
        newInp.buttonsDown = new List<bool>();
        newInp.buttonsDown.Add(Input.GetMouseButtonDown(0));
        newInp.buttonsDown.Add(Input.GetMouseButtonDown(1));
        newInp.buttonsDown.Add(Input.GetMouseButtonDown(2));
        newInp.buttonsDown.Add(Input.GetButton("dodge"));
        newInp.buttonsDown.Add((Client.canPickup || Client.dead) ? Input.GetButton("pickup") : false);
        newInp.equipedSlot1 = equipedSlot1;

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
        return Quaternion.Euler(0, lookRotationYWrapper(direction), 0);
    }

    public static float lookRotationYWrapper(Vector3 directionTo)
    {
        if (directionTo.x == 0 && directionTo.y == 0 && directionTo.z == 0)
        {
            return 0;
        } else
        {
            return Quaternion.LookRotation((directionTo).normalized).eulerAngles.y;
        }
        
    }

    public static Quaternion getRotationToLookAt(Vector3 position, Vector3 lookAt)
    {
        position = new Vector3(position.x, 0, position.z);
        lookAt = new Vector3(lookAt.x, 0, lookAt.z);
        Vector3 directionTo = lookAt - position;
        return Quaternion.Euler(0, lookRotationYWrapper(directionTo), 0);
    }

    public static CopyMovement inputToMovement(UserInput inp, Vector3 oldPositionLocal, Quaternion oldRotationLocal, float speed, Animator animator, string canChangeState, List<string> stateNames, string uid, float health, WeaponType weapon, int score, string playerName)
    {
        CopyMovement cp = new CopyMovement();
        // Have to use IsName because you can't check animation state directly. Though you can check animation clip I didn't here.
        cp.weapon = weapon;
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

        // Note could do some rollback style lag compensation by adding half their ping time to their input messages.
        // then add that to the starting normalizedTime. However would be very extremely easy to abuse.
        // Need a way for server to get their own ping of the client I guess, but that could also be abused.
        
        if (canDodge && inp.buttonsDown != null && inp.buttonsDown.Count > 3 && inp.buttonsDown[3])
        {
            cp.localRotation = getRotationFromInput(inp);
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
        cp.score = score;
        cp.playerName = playerName;
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
            if (Input.GetButtonUp("swapweapon"))
            {
                Client.swapWeapon();
            }

            UserInput nowInput = getClientInput(Client.equipedSlot1);
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
