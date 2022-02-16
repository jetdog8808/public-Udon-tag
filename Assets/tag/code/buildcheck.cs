
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;


//this script is for checking if people are on the correct build of the world. 


public class buildcheck : UdonSharpBehaviour
{
    [UdonSynced, HideInInspector]
    public int mastersbuild = -1;  //synced variable the master sets for the build they are on. 
    public int build; //local build the user is on.
    public Text visual; //ui text to display information.
    public GameObject visualblock;  // visual object to block player and warn player the master is not in the correct version.

    private bool correctbuild = false;
    [HideInInspector]
    public bool gotmastermessage = false;
    [HideInInspector]
    public bool mastersees = false;
    //private bool playerseesacceptable = false;
    public string problem = "";

    public controlerdeligator pool;

    void Start()
    {
        if (!visualblock.activeSelf)
        {
            visualblock.SetActive(true);
        }

        if (Networking.IsMaster) //if the person joining is master
        {
            mastersbuild = build; //set the masters synced build to local build
            visual.text = "master is on build: " + build;
            visualblock.SetActive(false);   //since the master will allways be in the correct build for himself this is always true
        }
        else
        {
            updatetext();
        }

    }

   

    public void updatetext()
    {
        if (!Networking.IsMaster)
        {
            string newtext = "";
            if (correctbuild)
            {
                newtext += "> You and master are on build: " + build;
                if (gotmastermessage)
                {
                    newtext += "\n> master got your message:";
                    if (mastersees)
                    {
                        newtext += "user can see you correctly.";
                        if (pool.stuckusers == 0 && pool.seeustuck == 0)
                        {
                            newtext += "\n> Other users are not stuck for you.\n> Have Fun!!!";
                            visualblock.SetActive(false);
                        }
                        else
                        {
                            tagplayercontroler users = pool.ownedcontroler;
                            if (users != null)
                            {
                                users.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "masterretake");
                                pool.ownedcontroler = null;
                            }
                        }
                    }
                    else
                    {
                        newtext += "user can't see you, you will need to rejoin";
                        tagplayercontroler users = pool.ownedcontroler;
                        if (users != null)
                        {
                            users.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "masterretake");
                            pool.ownedcontroler = null;
                        }
                    }

                }
                else
                {
                    newtext += "\n> Waiting for your capsule, and master to check you are ready.\nCurrently checking: " + pool.id + " || you are: " + pool.localuser.playerId;
                }
            }
            else
            {
                newtext += "Checking if master is on build: " + build + " || If this doesnt update after a second then master is on a different build, or udon broke for you.";
            }

            if (pool.needstorestart)
            {
                newtext += "\n•<color=Red>Something bad broke you need to restart</color>" + problem;
                visualblock.SetActive(true);

            }



            if (visual.text != newtext)
            {
                visual.text = newtext;
                
            }

        }

    }


    public virtual void OnDeserialization()
    {
        if (mastersbuild == build && !correctbuild)
        {
            correctbuild = true;
            updatetext();
        }
        
    }

}
