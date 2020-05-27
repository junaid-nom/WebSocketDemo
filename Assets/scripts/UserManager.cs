using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserNetworkProcessor
{
    public MessageManager msgMan = new MessageManager();
    public readonly string uid; //userid

    public UserNetworkProcessor(string userid)
    {
        uid = userid;
    }
}

public class UserManager : MonoBehaviour
{
    UserNetworkProcessor usernet;
    GameObject playerCharacter;
    GameObject playerPrefab;
    copyFromStruct playerCopyController;
    Animator playerAnimator;
    private bool startUp;
    InputBuffer inputBuffer = new InputBuffer();

    // Start is called before the first frame update
    void Start()
    {
        // Needs stuff to run so doesn't do anything until you call startup function
    }

    public void addMessage(Message m)
    {
        usernet.msgMan.addMessage(m);
    }

    public void startup(string userid, GameObject playerprefab, Vector3 spawnLocation)
    {
        if (!startUp)
        {
            playerPrefab = playerprefab;

            // Create playerCharacter here, spawn somewhere
            createPlayerCharacter(spawnLocation);

            usernet = new UserNetworkProcessor(userid);
            startUp = true;
        }
        else
        {
            Debug.LogError("TRYING TO STARTUP UserManager THAT IS ALREADY STARTED!");
        }
    }

    void createPlayerCharacter(Vector3 spawnLocation)
    {
        playerCharacter = Instantiate(playerPrefab);
        playerCharacter.transform.localPosition = spawnLocation;
        playerCopyController = playerCharacter.GetComponent<copyFromStruct>();
        playerAnimator = playerCharacter.GetComponent<Animator>();
    }

    void processUserInputs()
    {
        List<UserInput> userInps = usernet.msgMan.popAllMessages<UserInput>();
        if (userInps != null)
        {
            // move playerCharacter around
            foreach (var ui in userInps)
            {
                inputBuffer.receiveInput(ui);
            }
            UserInput finalui = inputBuffer.getInput();
            playerCopyController.setMovement(InputToMovement.inputToMovement(finalui, playerCharacter.transform.localPosition, playerCharacter.transform.localRotation, Constants.charMoveSpeed, playerAnimator, Constants.canMoveState, new List<string>(Constants.charStateNames)));
        }
    }

    // Use LateUpdate so Server has a chance to process all the message that came in this frame
    void LateUpdate()
    {
        if (startUp)
        {
            processUserInputs();

            // Send info about self to Server to broadcast

            // Delete if dced for awhile
            if (System.DateTime.Now.Subtract(usernet.msgMan.LastMessageTime).TotalSeconds > 60)
            {
                deleteSelf();
            }
        }
    }


    // TODO: Call if player DCED for greater than X time.
    void deleteSelf(/*Server server*/)
    {
        // remove from server List of UserManagers
        // Destroy any related objects like playerCharacter
        // destory self (component)
    }
}


