
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;


//controler each player will get for game.

public class tagplayercontroler : UdonSharpBehaviour
{
    [HideInInspector]
    public bool Ustatus = false;
    [HideInInspector]
    public string errortext = "init text";  // status for error checker

    [HideInInspector]
    public bool ingame = false; //if you are player is in game
    [HideInInspector]
    public VRCPlayerApi ownerapi; //api of the user who owns this object
    [HideInInspector]
    public int arraynum;
    [HideInInspector]
    public bool freezer = false; //if they are the tagger
    private Transform transform; //reference to this transform
    public taghands[] hands;  //reference to hands
    public CapsuleCollider lcollider;  //reference to this capsule collider
    [HideInInspector]
    public controlerdeligator controldeliget; // reference to control deligator
    public Renderer visual; //reference to this renderer to change colors
    [HideInInspector]
    public bool isfrozen = false;  //variable if user is frozen

    public float pingcalc = 0;
    public float ping;
    private bool pingfired = false;
    private bool gotsent = false;
    private int firesent = 0;
    private int firegot = 0;
    private int firelook = 0;
    //public Text visout;

    public Material attackermat; //material for taggers
    public Material targetsmat;  //material for runners
    public Material frozenmat;  //material for when you are frozzen

    [HideInInspector]
    public Vector3 frozepoint;  //where the player was when frozen
    [HideInInspector]
    public Vector3 lastpos;  //where they user was last frame
    private int checkcount = 0;  //how many frames the user was going too fast
    private float topspeed = 4;  //local variable of how fast the player should be going.

    [HideInInspector]
    public float[] tagtime;
    private bool gottagger;
    [HideInInspector]
    public int myplace;
    



    void Start()
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 1";

        controldeliget = GetComponentInParent<controlerdeligator>();
        transform = GetComponent<Transform>();
        arraynum = transform.GetSiblingIndex();
        nonuseset();
        VRCPlayerApi currentowner = Networking.GetOwner(gameObject);

        tagtime = new float[controldeliget.totalusers];
        for (int i = 0; i < tagtime.Length; i++)
        {
            tagtime[i] = 0;
        }
        gottagger = true;

        if (ownerapi != currentowner)
        {
            if (!currentowner.isMaster || controldeliget.mastersowned == this)
            {
                ownerapi = currentowner;
                currentowner.SetPlayerTag(controldeliget.objtagname, arraynum.ToString());
            }
            else
            {
                ownerapi = null;
            }

        }

    }

    public void takeownership()
    {
        Networking.SetOwner(controldeliget.localuser, gameObject);
        if (Networking.IsOwner(gameObject))
        {
            controldeliget.ownedcontroler = this;
            ownerapi = controldeliget.localuser;
            Debug.Log(ownerapi.displayName + "is now taking this object");
            controldeliget.checkcapusers();
            if (!Networking.IsMaster)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "seestuck");
            }
            
        }
        
    }

    public void seestuck()
    {
        Debug.Log("we are checking if users see these changes.");
        
        if(controldeliget.ownedcontroler != null && !Networking.GetOwner(this.gameObject).isMaster)
        {
            Vector3 location = ownerapi.GetPosition();
            if (Networking.IsMaster)
            {
                if (ownerapi != null)
                {

                    if (location.x == 0 && location.z == 0)
                    {
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "masterfail");
                    }
                    else
                    {
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "mastersuccess");
                    }
                }
                else
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "masterfail");
                }
            }
            else
            {
                if (location.x == 0 && location.z == 0)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "usersfail");
                }
            }
        }
        
    }

    public void mastersuccess()
    {
        controldeliget.buildck.gotmastermessage = true;
        controldeliget.buildck.mastersees = true;
        controldeliget.buildck.updatetext();
    }

    public void masterfail()
    {
        controldeliget.buildck.gotmastermessage = true;
        controldeliget.buildck.mastersees = false;
        controldeliget.needstorestart = true;
        controldeliget.buildck.problem += "|master fail|";
        controldeliget.buildck.updatetext();
    }

    public void usersfail()
    {
        controldeliget.seeustuck++;
        controldeliget.needstorestart = true;
        controldeliget.buildck.problem += "|users fail|";
        controldeliget.buildck.updatetext();
    }

    public void masterretake()
    {
        if (Networking.IsMaster)
        {
            takeownership();
        }
    }

    public void setmasteras()
    {
        Debug.Log("setting master");
        VRCPlayerApi currentowner = Networking.GetOwner(gameObject);
        if (ownerapi != currentowner)
        {
            ownerapi = currentowner;
        }

        controldeliget.mastersowned = this;
    }

    public virtual void OnOwnershipTransferred()
    {
        VRCPlayerApi currentowner = Networking.GetOwner(gameObject);

        if (ownerapi != currentowner)
        {
            if ((!currentowner.isMaster || controldeliget.mastersowned == this))
            {
                ownerapi = currentowner;
                currentowner.SetPlayerTag(controldeliget.objtagname, arraynum.ToString());
                
            }
            else
            {
                ownerapi = null;
            }
        }
    }

    public virtual void OnPlayerLeft(VRC.SDKBase.VRCPlayerApi player)//checks if the player leaving is owner
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 12";

        if (ownerapi == player) //if the player is the same as owner
        {
            errortext = "obj name:" + gameObject.name + "\n" + "Error 13";
            ownerapi = null;//set the owner to null
            ingame = false;
            transform.localPosition = Vector3.zero; //move it out of the way.
            errortext = "obj name:" + gameObject.name + "\n" + "Error 14";
            nonuseset();
        }
    }
    
    public void netping()
    {
        if (ownerapi != null && !ownerapi.isLocal)
        {
            firegot++;

            if (firegot == firelook)
            {
                pingfired = false;
                ping = pingcalc;
                //Debug.Log(ownerapi.displayName + "ping:" + ping);
                
            }
        }
        
    }
    
    private void Update() //mainly used for player movement
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 15";

        if (ownerapi != null)//if its owned set its position to the owners.
        {
            errortext = "obj name:" + gameObject.name + "\n" + "Error 16";
            transform.position = ownerapi.GetPosition();

            
            if (ingame)
            {
                if (pingfired)
                {
                    pingcalc += Time.deltaTime;
                }

                if (!gotsent && Networking.GetNetworkDateTime().Second % 2 == 0)
                {
                    if (ownerapi.isLocal)
                    {
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "netping");
                    }
                    else
                    {
                        firesent++;

                        if (!pingfired)
                        {
                            pingcalc = 0f;
                            pingfired = true;
                            firelook = firesent;
                        }
                    }
                    gotsent = true;
                }
                else if (Networking.GetNetworkDateTime().Second % 2 != 0)
                {
                    gotsent = false;
                }
                //visout.text = (ping + "\n" + pingcalc);
            }
            else
            {
                pingfired = false;
                firesent = 0;
                firelook = 0;
                firegot = 0;
                pingcalc = 0;
                ping = 0;
            }
            

            if (ownerapi.isLocal) //if its the local players check things
            {
                if (ingame) //if you are in game do these checks.
                {
                    errortext = "obj name:" + gameObject.name + "\n" + "Error 18";
                    if (isfrozen && Vector3.Distance(ownerapi.GetPosition(), controldeliget.gamecontrol.respawnpoint.position) > 2) //if the player is frozen and not near spawn
                    {
                        errortext = "obj name:" + gameObject.name + "\n" + "Error 19";
                        Vector3 headpos = ownerapi.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position; //checks head position (incase they open their settings and try and playspace)
                        headpos.y = frozepoint.y; //sets y so i can just check horrizontal movement
                        errortext = "obj name:" + gameObject.name + "\n" + "Error 20";
                        if (Vector3.Distance(frozepoint, ownerapi.GetPosition()) > .4f) //if they are too far away from their frozen spot teleport them back.
                        {
                            errortext = "obj name:" + gameObject.name + "\n" + "Error 21";
                            ownerapi.TeleportTo(frozepoint, ownerapi.GetRotation(), VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, true);
                        }
                    }
                    /*
                    //this section checks for if the player is going to fast.
                    errortext = "obj name:" + gameObject.name + "\n" + "Error 22";
                    Vector3 movedirection = (ownerapi.GetPosition() - lastpos) / Time.deltaTime;//calculates players velocity direction
                    movedirection.y = 0;
                    //Debug.Log("direction" + movedirection);
                    float velocity = Mathf.Round(Vector3.Magnitude(movedirection) * 100f) / 100f; //converts and rounds to a float value
                                                                                                  //Debug.Log(velocity);
                    errortext = "obj name:" + gameObject.name + "\n" + "Error 13";
                    if (velocity > topspeed) //if their velocity if too fast then what they should be going.
                    {
                        errortext = "obj name:" + gameObject.name + "\n" + "Error 14";
                        checkcount++; //add to counter how many times this is true.
                        Vector3 newpos = lastpos + (Vector3.ClampMagnitude(movedirection, topspeed) * Time.deltaTime); //calculate where they should be if they were going correct speed.
                                                                                                                       //Debug.Log("clamped direction" + Vector3.ClampMagnitude(movedirection, topspeed));
                        errortext = "obj name:" + gameObject.name + "\n" + "Error 15";
                        if (checkcount > 10) //if counter is above 10 times 
                        {
                            //Debug.Log("player going too fast: " + velocity);

                            //ownerapi.SetVelocity(Vector3.zero);
                            if (Time.frameCount % 3 == 0) //teleport them on every third frame so its not every frame.
                            {
                                errortext = "obj name:" + gameObject.name + "\n" + "Error 16";
                                ownerapi.TeleportTo(newpos, ownerapi.GetRotation(), VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, true);
                            }



                        }
                        errortext = "obj name:" + gameObject.name + "\n" + "Error 17";
                        lastpos = newpos; //sets their last position to where they should be.


                    }
                    else //if they are not speeding or has stopped speeding
                    {
                        errortext = "obj name:" + gameObject.name + "\n" + "Error 18";
                        checkcount = 0; //reset counter
                        lastpos = ownerapi.GetPosition(); //save position for next frame.
                    }
                    errortext = "obj name:" + gameObject.name + "\n" + "Error 19";
                    */
                    if (!gottagger)
                    {
                        VRCPlayerApi taggedby = Networking.GetOwner(visual.gameObject);
                        if (!taggedby.isLocal)
                        {
                            gottagger = true;
                            for (int i = 0; i < controldeliget.controlers.Length; i++)
                            {
                                if (taggedby == controldeliget.controlers[i].ownerapi)
                                {
                                    if (controldeliget.controlers[i].freezer)
                                    {
                                        tagtime[myplace] += controldeliget.controlers[i].ping;
                                        //Debug.Log("tagged by:" + controldeliget.controlers[i].ownerapi.displayName);

                                        for (int e = 0; e < tagtime.Length; e++)
                                        {
                                            if(controldeliget.controlers[e].ownerapi != null)
                                            {
                                                //Debug.Log(controldeliget.controlers[e].ownerapi.displayName + "|" + tagtime[e] + "|| < ||" + ownerapi.displayName + "|" + tagtime[myplace]);
                                                if (tagtime[e] < tagtime[myplace] && e != myplace)
                                                {
                                                    //Debug.Log(true);
                                                    controldeliget.controlers[e].SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "refreezeme");
                                                    Networking.SetOwner(controldeliget.localuser, controldeliget.controlers[e].visual.gameObject);
                                                }
                                                else
                                                {
                                                    //Debug.Log(false);
                                                }
                                            }
                                            

                                        }
                                    }
                                    else
                                    {
                                        for (int e = 0; e < tagtime.Length; e++)
                                        {
                                            if (tagtime[e] < tagtime[myplace] && e != myplace)
                                            {
                                                controldeliget.controlers[e].SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "refreezeme");
                                                Networking.SetOwner(controldeliget.localuser, controldeliget.controlers[e].visual.gameObject);
                                            }

                                        }
                                    }
                                    Networking.SetOwner(controldeliget.localuser, visual.gameObject);
                                    break;
                                }
                            }
                        }
                    }

                    
                }
                else //save players position for future testing
                {
                    errortext = "obj name:" + gameObject.name + "\n" + "Error 20";
                    lastpos = ownerapi.GetPosition();
                }
                errortext = "obj name:" + gameObject.name + "\n" + "Error 21";
            }
        }
        
        errortext = "obj name:" + gameObject.name + "\n" + "Error 17";
        

    }

    private void LateUpdate()
    {
        Ustatus = true;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 22";
    }

    public void targetprep() //event if they are a runner.
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 23";

        freezer = false;
        setcolors(targetsmat); //set their color
        visual.enabled = true; //make their capsules visable
        errortext = "obj name:" + gameObject.name + "\n" + "Error 24";
        if (ownerapi != null && ownerapi.isLocal) //things to only do localy for the user getting set.
        {
            errortext = "obj name:" + gameObject.name + "\n" + "Error 25";
            ownerapi.SetRunSpeed(4f);
            ownerapi.SetJumpImpulse(0f);
            topspeed = 4f;
            visual.enabled = false;
            errortext = "obj name:" + gameObject.name + "\n" + "Error 28";
            controldeliget.gamecontrol.teleportgame(); //comunicates to gamecontroler they are all setup and to teleport them.
            errortext = "obj name:" + gameObject.name + "\n" + "Error 26";/*
            foreach (taghands hand in hands) //sets up hand colliders.
            {
                errortext = "obj name:" + gameObject.name + "\n" + "Error 27";
                hand.visual.enabled = true;
                hand.sphcollider.enabled = true;
            }*/
            controldeliget.gamecontrol.hud.yourunner();
        }
        errortext = "obj name:" + gameObject.name + "\n" + "Error 29";


    }

    public void attackerprep() //event if they are a tagger
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 30";

        freezer = true;
        setcolors(attackermat);
        visual.enabled = true;
        errortext = "obj name:" + gameObject.name + "\n" + "Error 31";
        if (ownerapi != null && ownerapi.isLocal)
        {
            errortext = "obj name:" + gameObject.name + "\n" + "Error 32";
            ownerapi.SetRunSpeed(4.4f);
            ownerapi.SetJumpImpulse(0f);
            topspeed = 4.4f;
            visual.enabled = false;
            errortext = "obj name:" + gameObject.name + "\n" + "Error 35";
            controldeliget.gamecontrol.teleportgame(); //comunicates to gamecontroler they are all setup and to teleport them.
            errortext = "obj name:" + gameObject.name + "\n" + "Error 33";/*
            foreach (taghands hand in hands)//sets up hand colliders.
            {
                errortext = "obj name:" + gameObject.name + "\n" + "Error 34";
                hand.visual.enabled = true;
                hand.sphcollider.enabled = true;
            }*/
            controldeliget.gamecontrol.hud.youtagger();
            
        }
        errortext = "obj name:" + gameObject.name + "\n" + "Error 36";

    }

    public void nonuseset() //event to revert user to default settings.
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 37";

        freezer = false;
        setcolors(attackermat);
        visual.enabled = false;
        ingame = false;
        isfrozen = false;
        errortext = "obj name:" + gameObject.name + "\n" + "Error 38";
        if (ownerapi != null && ownerapi.isLocal)
        {
            errortext = "obj name:" + gameObject.name + "\n" + "Error 39";
            ownerapi.SetRunSpeed(4f);
            ownerapi.SetJumpImpulse(3f);
            topspeed = 4f;
            ownerapi.Immobilize(false);
            errortext = "obj name:" + gameObject.name + "\n" + "Error 40";
            if (Vector3.Distance(ownerapi.GetPosition(), controldeliget.gamecontrol.respawnpoint.position) > 2) //if the player is frozen and not near spawn
            {
                ownerapi.TeleportTo(controldeliget.gamecontrol.respawnpoint.position, controldeliget.gamecontrol.respawnpoint.rotation, VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, false);
            }

            gottagger = true;
                
            foreach (taghands hand in hands)
            {
                errortext = "obj name:" + gameObject.name + "\n" + "Error 41";
                hand.visual.enabled = false;
                hand.sphcollider.enabled = false;
            }
            //Networking.SetOwner(controldeliget.localuser, visual.gameObject);
            errortext = "obj name:" + gameObject.name + "\n" + "Error 42";
        }
        errortext = "obj name:" + gameObject.name + "\n" + "Error 43";
    }
    /*
    public void setplayerup() //event to revert user to default settings.
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 37";

        freezer = false;
        visual.enabled = false;
        ingame = false;
        isfrozen = false;

        if (ownerapi != null && ownerapi.isLocal)
        {

            ownerapi.SetRunSpeed(4f);
            ownerapi.SetJumpImpulse(3f);
            topspeed = 4f;
            ownerapi.Immobilize(false);

            if (Vector3.Distance(ownerapi.GetPosition(), controldeliget.gamecontrol.respawnpoint.position) > 2) //if the player is frozen and not near spawn
            {
                ownerapi.TeleportTo(controldeliget.gamecontrol.respawnpoint.position, controldeliget.gamecontrol.respawnpoint.rotation, VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, false);
            }

            gottagger = true;

            foreach (taghands hand in hands)
            {
  
                hand.visual.enabled = false;
                hand.sphcollider.enabled = false;
            }
            controldeliget.gamecontrol.teleportgame();
        }
    }*/

    public void freezeme() //event to freeze the user.
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 44";

        if (!freezer && ownerapi != null && !isfrozen && ingame)
        {
            errortext = "obj name:" + gameObject.name + "\n" + "Error 54";
            isfrozen = true;
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "frozencolor"); //sends event for everyone to change their color.
            errortext = "obj name:" + gameObject.name + "\n" + "Error 55";
            ownerapi.Immobilize(true);
            errortext = "obj name:" + gameObject.name + "\n" + "Error 56";
            frozepoint = ownerapi.GetPosition();
            errortext = "obj name:" + gameObject.name + "\n" + "Error 57";
            foreach (taghands hand in hands)
            {
                errortext = "obj name:" + gameObject.name + "\n" + "Error 58";
                hand.sphcollider.enabled = false;
            }
            controldeliget.gamecontrol.hud.frozen();
            tagtime[myplace] = controldeliget.gamecontrol.gameclock.seconds;
            //  Debug.Log(ownerapi.displayName + " was tagged at: " + tagtime[myplace]);
            gottagger = false;
            
        }

    }

    public void refreezeme() //event to freeze the user.
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 44";

        if (!freezer && ownerapi != null && !isfrozen && ingame)
        {
            errortext = "obj name:" + gameObject.name + "\n" + "Error 54";
            isfrozen = true;
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "frozencolor"); //sends event for everyone to change their color.
            errortext = "obj name:" + gameObject.name + "\n" + "Error 55";
            ownerapi.Immobilize(true);
            errortext = "obj name:" + gameObject.name + "\n" + "Error 56";
            frozepoint = ownerapi.GetPosition();
            errortext = "obj name:" + gameObject.name + "\n" + "Error 57";
            foreach (taghands hand in hands)
            {
                errortext = "obj name:" + gameObject.name + "\n" + "Error 58";
                hand.sphcollider.enabled = false;
            }
            controldeliget.gamecontrol.hud.refrozen();
            gottagger = false;
        }

    }
    
    public void unfreezeme() //event to unfreeze the user.
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 59";

        if (!freezer && ownerapi != null && isfrozen)
        {
            errortext = "obj name:" + gameObject.name + "\n" + "Error 60";
            isfrozen = false;
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "targetcolor");//sends event for everyone to change their color.
            errortext = "obj name:" + gameObject.name + "\n" + "Error 61";
            ownerapi.Immobilize(false);
            errortext = "obj name:" + gameObject.name + "\n" + "Error 62";
            foreach (taghands hand in hands)
            {
                hand.sphcollider.enabled = true;
            }
            controldeliget.gamecontrol.hud.unfrozen();

            if (!Networking.IsOwner(visual.gameObject))
            {
                Networking.SetOwner(controldeliget.localuser, visual.gameObject);
            }
        }
    }
    
    public void frozencolor() //setting color and if they should be frozen
    {
        errortext = "obj name:" + gameObject.name + "\n" + "Error 63";
        setcolors(frozenmat);
        isfrozen = true;
    }

    public void targetcolor()//setting color and if they should be frozen
    {
        errortext = "obj name:" + gameObject.name + "\n" + "Error 64";
        setcolors(targetsmat);
        isfrozen = false;
    }

    public void attackercolor()//setting color and if they should be frozen
    {
        errortext = "obj name:" + gameObject.name + "\n" + "Error 65";
        setcolors(attackermat);
        isfrozen = false;
    }

    public void setcolors(Material colors) //event that sets the color.
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 66";

        visual.material = colors;
        errortext = "obj name:" + gameObject.name + "\n" + "Error 67";
        if (ownerapi != null && ownerapi.isLocal)
        {
            errortext = "obj name:" + gameObject.name + "\n" + "Error 68";
            hands[0].visual.material = colors;
            hands[1].visual.material = colors;
        }

    }
}
