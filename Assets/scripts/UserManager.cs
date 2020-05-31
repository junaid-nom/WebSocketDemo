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
    public string currentConnID;

    // Start is called before the first frame update
    void Start()
    {
        // Needs stuff to run so doesn't do anything until you call startup function
    }

    public void addMessage(Message m)
    {
        usernet.msgMan.addMessage(m);
    }

    public void startup(string userid, string connID, GameObject playerprefab, Vector3 spawnLocation)
    {
        if (!startUp)
        {
            playerPrefab = playerprefab;

            usernet = new UserNetworkProcessor(userid);
            startUp = true;
            currentConnID = connID;

            // Create playerCharacter here, spawn somewhere
            createPlayerCharacter(spawnLocation);
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
        PlayerObject po = playerCharacter.GetComponent<PlayerObject>();
        po.uid = usernet.uid;
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
            
        }
        UserInput finalui = inputBuffer.getInput();
        CopyMovement cp = InputToMovement.inputToMovement(finalui, playerCharacter.transform.localPosition, playerCharacter.transform.localRotation, Constants.charMoveSpeed, playerAnimator, Constants.canMoveState, new List<string>(Constants.charStateNames), currentConnID);
        playerCopyController.setMovement(cp);
        Server.sendToAll(cp);
    }

    void processOpenMessage()
    {
        if (usernet.msgMan.popAllMessages<OpenMessage>() != null)
        {
            // Send to that player a message telling them their connection/userID
            Server.sendToSpecificUser(currentConnID, new StringMessage("userid:" + currentConnID));
        }
    }

    void processCloseMessage()
    {
        if (usernet.msgMan.popAllMessages<CloseMessage>() != null)
        {
            deleteSelf();
        }
        
    }

    // Use LateUpdate so Server has a chance to process all the message that came in this frame
    public void customUpdate()
    {
        if (startUp)
        {
            processOpenMessage();
            processUserInputs();

            // Send info about self to Server to broadcast

            // Delete if dced for awhile
            if (System.DateTime.Now.Subtract(usernet.msgMan.LastMessageTime).TotalSeconds > 60)
            {
                deleteSelf();
            }
            //Send Info about playerCharacter here. 
            // If we dont client will assume it died
        }
    }


    // TODO: Call if player DCED for greater than X time.
    void deleteSelf(/*Server server*/)
    {
        // remove from server List of UserManagers
        // Destroy any related objects like playerCharacter
        // destory self (component)
        // send DestroyObject message to all
    }
}


