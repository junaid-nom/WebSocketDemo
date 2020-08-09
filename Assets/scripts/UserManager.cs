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
    Health playerHealth;
    public PlayerObject playerObject;
    private bool startUp;
    InputBuffer inputBuffer = new InputBuffer();
    public string currentConnID;
    bool closed = false;
    public bool equipedSlot1 = true;
    public string playerName = "-";

    // Start is called before the first frame update
    void Start()
    {
        // Needs stuff to run so doesn't do anything until you call startup function
    }

    public void addMessage(Message m)
    {
        if (usernet.msgMan != null)
        {
            usernet.msgMan.addMessage(m);
        } else
        {
            Debug.Log("Empty usernet " + usernet.uid);
        }
    }

    public int countMessages()
    {
        return usernet.msgMan.countAllMessages();
    }

    public void clearMessages()
    {
        usernet.msgMan.clearAllMessages();
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
        playerObject = playerCharacter.GetComponent<PlayerObject>();
        playerHealth = playerCharacter.GetComponent<Health>();
        playerCharacter.name = "P-" + usernet.uid;
        playerObject.uid = usernet.uid;
        playerObject.playerName = playerName;
        playerObject.isClientObject = false;
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

        equipedSlot1 = finalui.equipedSlot1;

        CopyMovement cp = InputToMovement.inputToMovement(finalui, playerCharacter.transform.localPosition, playerCharacter.transform.localRotation, Constants.charMoveSpeed, playerAnimator, Constants.canMoveState, new List<string>(Constants.charUserControlledStateNames), currentConnID, playerHealth.getHealth(), playerObject.getEquipedWeapon(equipedSlot1), playerObject.score, playerName);
        playerCopyController.setMovement(cp);
        
        if (playerObject.dead && finalui.buttonsDown.Count>=5 &&  finalui.buttonsDown[4])
        {
            playerObject.respawn();
        }
        
        if (cp.anim_state != null && cp.anim_state!="" && cp.anim_state != "canMoveState" && cp.normalizedTime == 0)
        {
            //reset inp buffer
            inputBuffer.clearBuffer();
        }
        
        Server.sendToAll(cp);
        Server.sendToSpecificUser(usernet.uid, playerObject.privateInfo);
    }

    void processOpenMessage()
    {
        if (usernet.msgMan.popAllMessages<OpenMessage>() != null)
        {
            // Send to that player a message telling them their connection/userID
            Server.sendToSpecificUser(usernet.uid, new StringMessage("userid:" + currentConnID));
        }
    }

    void processCloseMessage()
    {
        if (usernet.msgMan.popAllMessages<CloseMessage>() != null)
        {
            deleteSelf();
        }
    }
    
    void processNameMessage()
    {
        List<NameSetMessage> userInps = usernet.msgMan.popAllMessages<NameSetMessage>();
        if (userInps != null)
        {
            playerName = userInps[userInps.Count - 1].name;
        }
    }

    // Use LateUpdate so Server has a chance to process all the message that came in this frame
    public void customUpdate()
    {
        processCloseMessage();
        if (startUp && !closed)
        {
            processOpenMessage();
            processUserInputs();
            processNameMessage();

            // Send info about self to Server to broadcast

            // Delete if dced for awhile
            
            if (System.DateTime.Now.Subtract(usernet.msgMan.LastMessageTime).TotalSeconds > Constants.secondsUntilConsiderDC)
            {
                deleteSelf();
            }
            //Send Info about playerCharacter here. 
            // If we dont client will assume it died
        }
        clearMessages();
    }


    // TODO: Call if player DCED for greater than X time.
    void deleteSelf(/*Server server*/)
    {
        Debug.Log("Closing uid:" + usernet.uid);
        playerObject.die();
        // remove from server List of UserManagers
        Server.removeUserManager(usernet.uid);
        // Destroy any related objects like playerCharacter
        Destroy(playerCharacter);

        // send DestroyObject message to all
        Server.sendToAll(new DeleteMessage(usernet.uid, null));

        closed = true;

        // destory self (component). Because this component lives in Server by itself
        Destroy(this);
    }
}


