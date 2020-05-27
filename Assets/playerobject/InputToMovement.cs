using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InputBuffer
{
    List<UserInput> receivedInput; // Right now only need the very last input. if this was more complex game with fighting game style inputs then would need a list

    // Start is called before the first frame update
    public InputBuffer()
    {
        receivedInput = new List<UserInput>();
    }

    public void receiveInput(UserInput inp)
    {
        receivedInput.Add(inp);
        Debug.Log("Got inp" + inp);
    }

    public UserInput getInput()
    {
        UserInput inp;
        if (receivedInput.Count > 0)
        {
            UserInput combined = new UserInput();
            UserInput lastInp = receivedInput[receivedInput.Count - 1];
            combined.x = lastInp.x;
            combined.y = lastInp.y;
            
            List<UserInput> pressed = receivedInput.FindAll(inps => inps.buttonsDown.Contains(true));
            if (pressed.Count > 0)
            {
                combined.buttonsDown = pressed[pressed.Count - 1].buttonsDown;
            }
            else
            {
                combined.buttonsDown = lastInp.buttonsDown;// this is just an empty list
            }

            inp = combined;
            receivedInput.RemoveRange(0, receivedInput.Count - 2); // remove all but the last one
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
    //public GameObject objectToControl;
    //public GameObject instantFeedbackObject;
    //public List<string> stateNames;
    //public string canMoveState;
    //public float speed;

    //Animator animator;
    //Animator instantFeedbackAnimator;
    //copyFromStruct copying;

    //float sendPingSecs = .1f;

    //InputBuffer inputBuffer = new InputBuffer();

    // Start is called before the first frame update
    //void Start()
    //{
    //    animator = objectToControl.GetComponent<Animator>();
    //    copying = objectToControl.GetComponent<copyFromStruct>();
    //    instantFeedbackAnimator = instantFeedbackObject.GetComponent<Animator>();
    //}

    public static UserInput getInput()
    {
        UserInput newInp = new UserInput();
        newInp.x = Input.GetAxis("Horizontal");
        newInp.y = Input.GetAxis("Vertical");
        newInp.buttonsDown = new List<bool>();
        newInp.buttonsDown.Add(Input.GetMouseButtonDown(0));
        newInp.buttonsDown.Add(Input.GetMouseButtonDown(1));
        newInp.buttonsDown.Add(Input.GetMouseButtonDown(2));

        return newInp;
    }

    public static Quaternion getRotationFromInput(UserInput inp)
    {
        Vector3 direction = new Vector3(inp.x, 0, inp.y).normalized;
        return Quaternion.Euler(0, Quaternion.LookRotation((direction).normalized).eulerAngles.y, 0);
    }

    public static CopyMovement inputToMovement(UserInput inp, Vector3 oldPositionLocal, Quaternion oldRotationLocal, float speed, Animator animator, string canChangeState, List<string> stateNames)
    {
        CopyMovement cp = new CopyMovement();
        bool canChange = animator.GetCurrentAnimatorStateInfo(0).IsName(canChangeState);
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
        if (canChange && index >= 0)
        {
            cp.anim_state = stateNames[index];
            cp.normalizedTime = 0;
        }
        else
        {
            cp.anim_state = null;
            cp.normalizedTime = -1;
        }

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
    //void Update()
    //{
    //    //StartCoroutine(SendInputAfterTime(sendPingSecs, getInput(), copying));
    //    UserInput nowInput = getInput();

    //    StartCoroutine(SendInputAfterTime(sendPingSecs, nowInput)); // all movement is delayed, but locally change the rotation right away


    //    bool canChange = instantFeedbackAnimator.GetCurrentAnimatorStateInfo(0).IsName(canMoveState);
    //    if (canChange && (nowInput.x != 0 || nowInput.y != 0))
    //        instantFeedbackObject.transform.localRotation = getRotationFromInput(nowInput);

    //    UserInput delayedInp = inputBuffer.getInput();
    //    copying.setMovement(inputToMovement(delayedInp, objectToControl.transform.localPosition, objectToControl.transform.localRotation, speed, animator, canMoveState, stateNames));
    //}
}
