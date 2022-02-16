
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;


//this script is basicly a networked object pooling system. it managers and takes care of who owns objects and finds on for the local player to get.

public class controlerdeligator : UdonSharpBehaviour
{
    [HideInInspector]
    public bool Ustatus = false; // status for error checker
    [HideInInspector]
    public string errortext = "init text"; //text before something broke.

    [HideInInspector]
    public VRCPlayerApi localuser; //local playersapi
    [HideInInspector]
    public tagplayercontroler[] controlers; // array of all the controlers available. should have enough for the max amout of users you can have in a instace (2x the set cap)
    [HideInInspector]
    public tagplayercontroler ownedcontroler; //saved reference to the controler they local user owns.
    [HideInInspector]
    public tagplayercontroler mastersowned;

    [UdonSynced(UdonSyncMode.None), HideInInspector]
    public int id = -1;
    [UdonSynced(UdonSyncMode.None), HideInInspector]
    public int oid = -1;

    private int lastid = -1;
    [HideInInspector]
    public int trycount = 0;
    [HideInInspector]
    public int stuckusers = 0;
    [HideInInspector]
    public int seeustuck = 0;
    [HideInInspector]
    public bool needstorestart = false;
    //

    public string objtagname;
    public string failtagname;
    private int lastfired;

    [HideInInspector]
    public VRCPlayerApi[] playerlist;
    public int totalusers;

    public gamecontroller gamecontrol; // saved reference to the gamecontroler
    public buildcheck buildck;
    


    private void Start()
    {
        Ustatus = false; // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error start"; //error message if something boke here

        localuser = Networking.LocalPlayer;
        controlers = GetComponentsInChildren<tagplayercontroler>();
        playerlist = new VRCPlayerApi[totalusers];
        Debug.Log("are you master:" + Networking.IsMaster);
        if (Networking.IsMaster)
        {
            mastercheck();
        }
        else
        {
            Debug.Log("requesting which is master");
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "getmaster");
        }
    }

    public void getmaster()
    {
        Ustatus = false; // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error getmaster"; //error message if something boke here
        Debug.Log("master request received");
        if (Networking.IsMaster && lastfired != Time.frameCount)
        {
            lastfired = Time.frameCount;
            Debug.Log("checking master stuff now");
            mastercheck();
        }
    }

    public void mastercheck()
    {
        Ustatus = false; // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error mastercheck"; //error message if something boke here

        if (ownedcontroler == null)
        {
            for (int b = 0; b < controlers.Length; b++)
            {
                VRCPlayerApi currentowner = Networking.GetOwner(controlers[b].gameObject);

                if (localuser == currentowner)
                {
                    controlers[b].ownerapi = localuser;
                    ownedcontroler = controlers[b];
                    Debug.Log("sending which is master");
                    ownedcontroler.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "setmasteras");
                    break;
                }
            }
        }
        else
        {
            Debug.Log("sending which is master");
            ownedcontroler.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "setmasteras");
        }
    }

    public void checkcapusers()
    {
        for (int i = 0; i < playerlist.Length; i++)
        {
            if (playerlist[i] != null && !playerlist[i].isLocal)
            {
                int objid = -1;

                if (int.TryParse(playerlist[i].GetPlayerTag(objtagname), out objid))
                {
                    Vector3 location = playerlist[i].GetPosition();
                    if (location.x == 0 && location.z == 0)
                    {
                        stuckusers++;
                    }
                }
            }
        }
        Debug.Log("saw these users stuck: " + stuckusers);
        if(stuckusers != 0)
        {
            needstorestart = true;
            buildck.problem += "|stuck users|";
            Debug.Log("user needs to restart. current players you cant see.");
        }
        buildck.updatetext();

    }
    

    public virtual void OnPlayerJoined(VRC.SDKBase.VRCPlayerApi player)
    {
        Ustatus = false; // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error joinp"; //error message if something boke here

        for (int i = 0; i < playerlist.Length; i++)
        {
            if (playerlist[i] == null)
            {
                playerlist[i] = player;
                break;
            }
        }

        player.SetPlayerTag(objtagname, "");
    }

    public virtual void OnPlayerLeft(VRC.SDKBase.VRCPlayerApi player)
    {
        Ustatus = false; // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error playerleft"; //error message if something boke here

        if (Networking.IsMaster && ownedcontroler != mastersowned)
        {
            mastercheck();
        }

        for (int i = 0; i < playerlist.Length; i++)
        {
            if (playerlist[i] == player)
            {
                playerlist[i] = null;
                break;
            }
        }
    }


    public virtual void OnPreSerialization()
    {
        Ustatus = false; // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error preserial"; //error message if something boke here

        id = -1;
        oid = -1;
        bool owned = false;
        VRCPlayerApi currentowner = null;
        for (int i = 0; i < playerlist.Length; i++)
        {
            if (playerlist[i] != null)
            {
                owned = false;
                if (playerlist[i].isLocal)
                {
                    if (ownedcontroler != null && mastersowned == ownedcontroler)
                    {
                        owned = true;
                    }
                    else
                    {
                        mastercheck();
                    }
                }
                else
                {
                    string tag = playerlist[i].GetPlayerTag(objtagname);
                    int objid = -1;

                    if (int.TryParse(tag, out objid))
                    {
                        currentowner = Networking.GetOwner(controlers[int.Parse(playerlist[i].GetPlayerTag(objtagname))].gameObject);
                        if (playerlist[i] == currentowner)
                        {
                            owned = true;
                        }
                    }

                }

                if (!owned)
                {
                    int tempid = playerlist[i].playerId;

                    if (playerlist[i].GetPlayerTag(failtagname) != "failed")
                    {
                        if (lastid == tempid)
                        {
                            trycount++;
                        }
                        else
                        {
                            trycount = 1;
                            lastid = tempid;
                        }

                        if (20 > trycount)
                        {
                            id = tempid;
                        }
                        else
                        {
                            playerlist[i].SetPlayerTag(failtagname, "failed");
                            trycount = 0;
                            lastid = -1;
                        }
                    }

                    for (int b = 0; b < controlers.Length; b++)
                    {
                        currentowner = Networking.GetOwner(controlers[b].gameObject);

                        if (currentowner.isMaster && controlers[b] != mastersowned)
                        {
                            oid = b;
                            break;
                        }
                    }


                    if (id != -1)
                    {
                        break;
                    }
                }
            }

            if (i == playerlist.Length - 1)
            {
                trycount = 0;
                lastid = -1;
                for (int c = 0; c < playerlist.Length; c++)
                {
                    if (playerlist[c] != null)
                    {
                        playerlist[c].SetPlayerTag(failtagname, "");
                    }
                }
            }
        }

    }

    public virtual void OnDeserialization()
    {
        Ustatus = false; // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error deserialization"; //error message if something boke here
        //Debug.Log("can i grab?:" + Networking.IsNetworkSettled + "|" + !needstorestart);
        if (id == localuser.playerId && Networking.IsNetworkSettled && !needstorestart)
        {
            //Debug.Log("do i own and master have one set?");
            if (ownedcontroler == null && mastersowned != null)
            {
                //Debug.Log("yes!!!");
                bool alreadyown = false;

                for (int b = 0; b < controlers.Length; b++)
                {
                    if (Networking.IsObjectReady(controlers[b].gameObject))
                    {
                        VRCPlayerApi currentowner = Networking.GetOwner(controlers[b].gameObject);
                        if (localuser == currentowner)
                        {
                            controlers[b].takeownership();
                            alreadyown = true;
                            break;
                        }
                    }
                }
                //Debug.Log("do i already own one?:" + alreadyown);
                if (!alreadyown)
                {/*
                    for (int b = 0; b < controlers.Length; b++)
                    {
                        if (Networking.IsObjectReady(controlers[b].gameObject))
                        {
                            VRCPlayerApi currentowner = Networking.GetOwner(controlers[b].gameObject);
                            if (currentowner.isMaster && controlers[b] != mastersowned)
                            {
                                controlers[b].takeownership();
                                break;
                            }
                        }
                    }*/
                    Debug.Log("is object ready?:" + Networking.IsObjectReady(controlers[oid].gameObject));
                    if (Networking.IsObjectReady(controlers[oid].gameObject))
                    {
                        VRCPlayerApi currentowner = Networking.GetOwner(controlers[oid].gameObject);
                        if (currentowner.isMaster && controlers[oid] != mastersowned)
                        {
                            Debug.Log("take ownership of it");
                            controlers[oid].takeownership();
                            //break;
                        }
                    }

                }
            }
        }
       
        if (ownedcontroler == null || !Networking.IsOwner(ownedcontroler.gameObject))
        {
            if (buildck.mastersees)
            {
                if (Vector3.Distance(localuser.GetPosition(), gamecontrol.respawnpoint.position) > 7) //if the player is frozen and not near spawn
                {
                    localuser.TeleportTo(gamecontrol.respawnpoint.position, gamecontrol.respawnpoint.rotation, VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, false);
                }
                if (needstorestart == false)
                {
                    needstorestart = true;
                    buildck.problem += "|lose ownership|";
                }
                
            }


            buildck.updatetext();
           
        }/*
        else if (needstorestart)
        {
            needstorestart = false;
            buildck.updatetext();

        }*/
    }
    /*
    public virtual void OnOwnershipTransferred()
    {
       
        
    }
    */
    private void LateUpdate()
    {
        Ustatus = true; // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 16";
    }

}
