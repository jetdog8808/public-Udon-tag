
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;


//this is the logic behind the game.

public class gamecontroller : UdonSharpBehaviour
{
    [HideInInspector]
    public bool Ustatus = false; // status for error checker
    [HideInInspector]
    public string errortext = "init text";  // status for error checker

    public controlerdeligator controllist; //reference to controler deligator
    public gametimer gameclock; //reference to game clock
    public hudmanager hud;
    private tagplayercontroler[] usingame; //array of controlers in game
    public tagmap[] maps; //array of mapts to choose from
    public GameObject[] disonstart; //objects to dissable on game start
    public int mapchoice = 0; //int of which map to play on in maps array (not implimented)
    public VRCPlayerApi localuser;  //api of local user
    public int gamemode = 1;  //int of gamemode to choose from (not implimented)
    public BoxCollider enterzone;  //boxcollider for this object that gets people who want to play
    public BoxCollider fixlaggers;  //box collider of spawn reset object to fix anyone still at spawn
    public float timer = 12f;  //count down timer before game starts
    private bool startimer = false;  //bool to start timer
    public Transform respawnpoint;  //where to respawn players after game
    public Text countdown;  //visual of countdown timer
    public Text masteris; //displays who the master is
    public Toggle masterlock; //toggle to dictate if master has locked start button
    [UdonSynced]
    public bool masterlocked; //synced variabel to set if master has locked start
    [UdonSynced]
    public bool gamestartednet = false; //synced bool of when game has started. mainly for late joiners.
    private bool gamestarted = false;  //local if someone has started the game. required for frame timing specific events.
    [UdonSynced]
    public bool canclick = true;  //dissables start button so you cant spam click start.

    //public GameObject runnwin;  //visual when runners win
    //public GameObject tagwin;  //visual when taggers win
    public GameObject inprogress;  //visual when game is in progress
    public GameObject exitprevention;  //prevent people from going back to spawn and stay in waiting room.

   


    private void Start()
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 1";

        localuser = Networking.LocalPlayer;
        errortext = "obj name:" + gameObject.name + "\n" + "Error 2";
        usingame = new tagplayercontroler[25];  //creates array of how many can be in game. +1 cause im lazy and dont want to fix some code for a end of list check.
        startimer = false;
        errortext = "obj name:" + gameObject.name + "\n" + "Error 3";
        masteris.text = "master is: " + Networking.GetOwner(gameObject).displayName; //master is always owner of any non synced objects. 
        errortext = "obj name:" + gameObject.name + "\n" + "Error 4";
        if (localuser.isMaster)
        {
            masterlock.interactable = true;  //makes the toggle interactable for the master
        }
    }

    public virtual void OnPlayerLeft(VRC.SDKBase.VRCPlayerApi player)  //makes sure if the master leaves that master set objects are correct.
    {
        errortext = "obj name:" + gameObject.name + "\n" + "Error 5";
        masteris.text = "master is: " + Networking.GetOwner(gameObject).displayName;
        errortext = "obj name:" + gameObject.name + "\n" + "Error 6";
        if (localuser.isMaster)
        {
            masterlock.interactable = true; //makes the toggle interactable for the master
        }
    }

    public void updatemasterlock() // message sent from toggle when changed. to set variable of its new state.
    {
        masterlocked = masterlock.isOn;
    }

    private void OnTriggerEnter(Collider other)  //collects all the users in room for game about to start.
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 7";

        if (other != null) //if it gets the player it will be null
        {
            errortext = "obj name:" + gameObject.name + "\n" + "Error 8";
            tagplayercontroler player = other.GetComponent<tagplayercontroler>();
            errortext = "obj name:" + gameObject.name + "\n" + "Error 9";
            if (player != null) //makes sure it gets a object with the correct component.
            {
                errortext = "obj name:" + gameObject.name + "\n" + "Error 10";
                player.ingame = true;
                errortext = "obj name:" + gameObject.name + "\n" + "Error 11";
                for (int i = 0; i < usingame.Length; i++) //adds each player into array for who is in game
                {
                    if (usingame[i] == null)
                    {
                        usingame[i] = player;
                        break;
                    }
                    else if (usingame[i] == player)
                    {
                        break;
                    }
                }
            }


        }
    }
    /*
    private void OnTriggerExit(Collider other)
    {

        Debug.Log("exit");
        if (other != null && !gamestarted)
        {
            Debug.Log("not null");
            tagplayercontroler player = other.GetComponent<tagplayercontroler>();
            Debug.Log("is player null");
            if (player != null)
            {
                Debug.Log("they going out of game");
                player.ingame = false;
                Debug.Log("are they in game");
                for (int i = 0; i < usingame.Length; i++)
                {
                    Debug.Log("checkinglist");
                    if (usingame[i] == player)
                    {
                        Debug.Log("they in game");
                        for (int u = (usingame.Length - 1); u >= 0; u--)
                        {
                            Debug.Log("looking for replacement");
                            if (usingame[u] == null)
                            {
                                Debug.Log("is null");
                            }
                            else
                            {
                                Debug.Log("found one and fill");
                                usingame[i] = usingame[u];
                                Debug.Log("old now null");
                                usingame[u] = null;
                                Debug.Log("replace break");
                                break;
                            }

                        }
                        Debug.Log("found break");
                        break;
                    }
                    else if (usingame[i] == null)
                    {
                        Debug.Log("end of list");
                        break;
                    }

                }

            }


        }

    }
    */

    public void eventstart() //event sent by start button to send a networked event.
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 12";
        if (canclick && !masterlocked || localuser.isMaster) //checks if they can send networked event.
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "prestartgame");
        }


    }

    public void prestartgame() //preps the game variables
    {
        Ustatus = false;  // status for error checker 
        errortext = "obj name:" + gameObject.name + "\n" + "Error 13";
        //Debug.Log("prestartedgame");

        maps[mapchoice].removerespawn(); //makes sure the maps respawn collider is off.
        //tagwin.SetActive(false);  //dissables winning visuals
        //runnwin.SetActive(false);

        canclick = false;  //makes sure you cant click the button again.
                           //controllist.ownedcontroler.lcollider.enabled = true;

        enterzone.enabled = true;  //enables collider to collect users who want to join game
        gamestarted = false;
        startimer = true;
        exitprevention.SetActive(true);

    }

    private void Update() //used for timer and updating synced objects.
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 14";

        if (startimer)  //countdown timer for game preparing things.
        {
            if (5 > timer)
            {

            }
            if (0 > timer && !gamestarted)
            {
                gamestartednet = true;
                errortext = "obj name:" + gameObject.name + "\n" + "Error 15";
                startgame(); //starting game event
            }
            if (-4 > timer)
            {
                fixlaggers.enabled = true;  //resets users who lagged behind.
            }
            if (-5 > timer)
            {
                fixlaggers.enabled = false;  //resets timer
                timer = 12f;
                startimer = false;
                countdown.text = "12";
            }
            timer -= Time.deltaTime;
            countdown.text = timer.ToString("0.0");

        }


        
        //Debug.Log(gamestarted);

        if (inprogress.activeSelf != gamestartednet)  //if synced variable is not the same state as object fix.
        {
            inprogress.SetActive(gamestartednet);
        }

        if (masterlock.isOn != masterlocked)  //if synced variable is not the same state as object fix.
        {
            masterlock.isOn = masterlocked;
        }
    }

    private void LateUpdate()
    {
        Ustatus = true; // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 16";
    }

    public void startgame()
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 17";

        //controllist.ownedcontroler.lcollider.enabled = false;
        gamestarted = true;
        enterzone.enabled = false;

        for (int i = 0; i < disonstart.Length; i++)  //disables everything in array
        {
            disonstart[i].SetActive(false);
        }
        errortext = "obj name:" + gameObject.name + "\n" + "Error 18";
        gameclock.starttimer(300f);  //sets time for game
        errortext = "obj name:" + gameObject.name + "\n" + "Error 19";

        for (int i = 0; i < controllist.controlers.Length; i++)
        {
            controllist.ownedcontroler.tagtime[i] = gameclock.seconds + 50f;
        }

        if (localuser.isMaster)  //if you are master set other players up.
        {
            int num = 0; //number of players in game
            for (int i = 0; i < usingame.Length; i++)  //calculates the user count
            {
                if (usingame[i] != null)
                {
                    num++;

                }
                else
                {
                    break;
                }
                //Debug.Log("");
            }
            //Debug.Log("num = " + num);
            errortext = "obj name:" + gameObject.name + "\n" + "Error 20";
            if (num != 0) //if there is more then 0 users.
            {
                //Debug.Log("get attacker count");
                int attackerscount = Mathf.FloorToInt(num / 4); // calculates how many taggers there should be.
                if (attackerscount == 0) //if there is less then 4 make sure ther is 1
                {
                    attackerscount = 1;
                }
                else if (attackerscount > 4) //limit max taggers
                {
                    attackerscount = 4;
                }
                //Debug.Log("attackers = " + attackerscount);

                int[] attackerlist = new int[6] { -1, -1, -1, -1, -1, -1 }; //array of taggers
                errortext = "obj name:" + gameObject.name + "\n" + "Error 21";
                while (attackerlist[(attackerscount - 1)] == -1) //selects random users to be taggers and make sure there are no repeats.
                {
                    int chosen = Random.Range(0, num); //random number from how many users there are.
                                                       //Debug.Log("chosen numbers = " + chosen);
                    for (int i = 0; i < attackerlist.Length; i++) //loop to make sure that user is not a attacker already.
                    {
                        //Debug.Log("check if valid");
                        if (chosen != attackerlist[i] && attackerlist[i] == -1)
                        {
                            //Debug.Log("attacker chosen");
                            attackerlist[i] = chosen;
                            break;
                        }
                        else if (chosen == attackerlist[i])
                        {
                            //Debug.Log("already chosen");
                            break;
                        }
                        //Debug.Log("attacker spot already taken");
                    }

                }
                //Debug.Log("array index =");
                errortext = "obj name:" + gameObject.name + "\n" + "Error 22";
                //Debug.Log("compare ingame array");
                for (int i = 0; i < num; i++) //loops through player list to setup everyone who is not a tagger.
                {
                    //Debug.Log("compare array");
                    if (usingame[i] != null && i != attackerlist[0] && i != attackerlist[1] && i != attackerlist[2] && i != attackerlist[3] && i != attackerlist[4] && i != attackerlist[5])
                    {
                        //Debug.Log("targetprep sent");
                        usingame[i].SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "targetprep");
                    }

                }
                errortext = "obj name:" + gameObject.name + "\n" + "Error 23";
                for (int i = 0; i < attackerscount; i++)  //loops through player list to setup everyone who is a tagger.
                {
                    //Debug.Log("attacker prep send");
                    usingame[attackerlist[i]].SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "attackerprep");
                    //Debug.Log("attacker prep sent");
                }

            }

        }
    }

    public void teleportgame() //method to teleport everyone who is listed as in game to where they need to be.
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 24";

        if (controllist.ownedcontroler != null && controllist.ownedcontroler.ingame) //make sure the user is ingame and not null
        {
            errortext = "obj name:" + gameObject.name + "\n" + "Error 25";
            if (controllist.ownedcontroler.freezer) //if tagger send to their spawn
            {
                errortext = "obj name:" + gameObject.name + "\n" + "Error 26";
                int num = Random.Range(0, maps[mapchoice].taggerspawns.Length);
                controllist.ownedcontroler.lastpos = maps[mapchoice].taggerspawns[num].position;
                localuser.TeleportTo(maps[mapchoice].taggerspawns[num].position, maps[mapchoice].taggerspawns[num].rotation, VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, false);
            }
            else if (!controllist.ownedcontroler.freezer) //if runner sent to their locations
            {
                errortext = "obj name:" + gameObject.name + "\n" + "Error 27";
                int num = Random.Range(0, maps[mapchoice].runnerspawns.Length);
                controllist.ownedcontroler.lastpos = maps[mapchoice].runnerspawns[num].position;
                localuser.TeleportTo(maps[mapchoice].runnerspawns[num].position, maps[mapchoice].runnerspawns[num].rotation, VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, false);
            }
            else  //redundency check for errors.
            {
                errortext = "obj name:" + gameObject.name + "\n" + "Error 28";
                localuser.TeleportTo(respawnpoint.position, respawnpoint.rotation, VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, false);
                controllist.ownedcontroler.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "nonuseset");
            }
        }
    }

    public void endingconditions() //checks if ending conditions are met.
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 29";

        if (localuser.isMaster) //master is the only one who checks.
        {
            errortext = "obj name:" + gameObject.name + "\n" + "Error 30";
            //Debug.Log("checking condition list");
            for (int i = 0; i < (usingame.Length); i++) //makes sure there are taggers in game
            {
                errortext = "obj name:" + gameObject.name + "\n" + "Error 31";
                if (usingame[i] != null && usingame[i].ingame && usingame[i].freezer)
                {

                    break;
                }
                else if (usingame[i] == null)
                {
                    errortext = "obj name:" + gameObject.name + "\n" + "Error 32";
                    Debug.Log("no attackers");
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "endgame");
                    break;
                }



            }
            errortext = "obj name:" + gameObject.name + "\n" + "Error 33";
            for (int i = 0; i < usingame.Length; i++) //checks if taggers have tagged everyon.
            {
                errortext = "obj name:" + gameObject.name + "\n" + "Error 34";
                if (usingame[i] != null && usingame[i].ingame && !usingame[i].freezer && !usingame[i].isfrozen)
                {
                    //person isnt frozen
                    //Debug.Log("person isnt frozen");
                    break;
                }
                else if (usingame[i] == null)
                {
                    errortext = "obj name:" + gameObject.name + "\n" + "Error 35";
                    //Debug.Log("ending conditions met");
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "tagwon");
                    return;
                }

            }

        }

    }

    public void timeup() //gets called if timer has counted down.
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 36";

        if (localuser.isMaster)
        {
            errortext = "obj name:" + gameObject.name + "\n" + "Error 37";
            //Debug.Log("time is up");
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "runwon");
        }
    }
    
    public void runwon() 
    {
        endgame();
        hud.runnerswin();
    }

    public void tagwon()  //sets visuals when taggers win
    {
        endgame();
        hud.taggerswin();
    }
    

    public void endgame()  //cleans up game and players.
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 38";

        if (startimer) //makes sure timer is off.
        {
            fixlaggers.enabled = false;
            timer = 12f;
            startimer = false;
            countdown.text = "12";
        }
        errortext = "obj name:" + gameObject.name + "\n" + "Error 39";
        maps[mapchoice].respawnplayers(); //enables map respawn collider for any stragglers
                                          //Debug.Log("master reset players");
        errortext = "obj name:" + gameObject.name + "\n" + "Error 40";
        if (localuser.isMaster) //set everyone to default state
        {
            errortext = "obj name:" + gameObject.name + "\n" + "Error 41";
            for (int i = 0; i < usingame.Length; i++)
            {
                if (usingame[i] != null)
                {
                    errortext = "obj name:" + gameObject.name + "\n" + "Error 42";
                    //Debug.Log("reset player");
                    usingame[i].SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "nonuseset");
                }
                else
                {
                    //Debug.Log("reset players list over");
                    break;
                }

            }
        }
        //Debug.Log("local end set");
        canclick = true;
        errortext = "obj name:" + gameObject.name + "\n" + "Error 43";
        gameclock.timeon = false;
        gamestartednet = false;
        exitprevention.SetActive(false);
        hud.fadetextclear();
        //Debug.Log("remove from list");
        errortext = "obj name:" + gameObject.name + "\n" + "Error 44";
        for (int i = 0; i < usingame.Length; i++) //makes in game array null
        {
            usingame[i] = null;
        }
    }

}
