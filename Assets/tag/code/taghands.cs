
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


//follows the hands and sends messages when you touch players.

public class taghands : UdonSharpBehaviour
{
    [HideInInspector]
    public bool Ustatus = false;  // status for error checker
    [HideInInspector]
    public string errortext = "init text";  // status for error checker

    public controlerdeligator controldeligate; //reference to control deligator
    public SphereCollider sphcollider; //colliders of object this scirpt is on
    public bool hand;  //which hand this should be on
    public bool track = true;  //if the object should follow the users hand
    public Renderer visual;  //reference to the visual mesh renderer to change materials.


    private void Start()
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 1";

        sphcollider = GetComponent<SphereCollider>();
        visual.enabled = false;
    }
    /*
    private void FixedUpdate()
    {
        Ustatus = false;

        if (track)
        {
            switch (hand)
            {
                case true:
                    if (controldeligate.localuser.IsUserInVR())
                    {
                        transform.position = controldeligate.localuser.GetBonePosition(HumanBodyBones.LeftHand);
                    }
                    else
                    {
                        Vector3 controler = controldeligate.localuser.GetBonePosition(HumanBodyBones.LeftHand);
                        Vector3 hand = controldeligate.localuser.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
                        if (Vector3.Distance(controler, hand) > 0.3f)
                        {
                            transform.position = hand;
                        }
                        else
                        {
                            transform.position = controler;
                        }
                    }

                    break;

                case false:
                    if (controldeligate.localuser.IsUserInVR())
                    {
                        transform.position = controldeligate.localuser.GetBonePosition(HumanBodyBones.RightHand);
                    }
                    else
                    {
                        Vector3 controler = controldeligate.localuser.GetBonePosition(HumanBodyBones.RightHand);
                        Vector3 hand = controldeligate.localuser.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
                        if (Vector3.Distance(controler, hand) > 0.3f)
                        {
                            transform.position = hand;
                        }
                        else
                        {
                            transform.position = controler;
                        }
                    }
                    break;
            }
        }
    }
    */

    private void OnTriggerEnter(Collider other) //event when you tag someone and what to do
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 2";

        if (other != null)  //if it gets the player it will be null
        {
            errortext = "obj name:" + gameObject.name + "\n" + "Error 3";
            tagplayercontroler target = other.GetComponent<tagplayercontroler>();
            errortext = "obj name:" + gameObject.name + "\n" + "Error 4";
            if (target != null) //if it is a player
            {
                errortext = "obj name:" + gameObject.name + "\n" + "Error 5";
                bool owner = Networking.IsOwner(target.gameObject); //checks if its your or someone else
                errortext = "obj name:" + gameObject.name + "\n" + "Error 6";
                if (controldeligate.ownedcontroler.freezer && !owner) //if anothers and you are a tagger you will send freeze event to them.
                {
                    errortext = "obj name:" + gameObject.name + "\n" + "Error 7";
                    target.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "freezeme");
                    Networking.SetOwner(controldeligate.localuser, target.visual.gameObject);
                }
                else if (target.isfrozen && !owner)  //if not tagger you will send unfreeze event.
                {
                    errortext = "obj name:" + gameObject.name + "\n" + "Error 8";
                    target.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "unfreezeme");
                    controldeligate.ownedcontroler.tagtime[target.myplace] = controldeligate.gamecontrol.gameclock.seconds;
                    //Networking.SetOwner(controldeligate.localuser, target.visual.gameObject);
                }
            }


        }
    }

    private void Update() //sents the object to the players hand
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 9";

        if (track)//if it should track
        {
            switch (hand) //which hand to track
            {
                case true:
                    errortext = "obj name:" + gameObject.name + "\n" + "Error 10";
                    if (controldeligate.localuser != null)
                    {
                        errortext = "obj name:" + gameObject.name + "\n" + "Error 11";
                        if (controldeligate.localuser.IsUserInVR())//if user is in vr
                        {
                            errortext = "obj name:" + gameObject.name + "\n" + "Error 12";
                            Vector3 controler = controldeligate.localuser.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;//get their hand position
                            Vector3 position = controldeligate.localuser.GetPosition();//get their root position
                            Vector2 distance = new Vector2(controler.x - position.x, controler.z - position.z); //sets it as a vector2
                            distance = Vector2.ClampMagnitude(distance, 1.2f);//claps magnatude to 1.2 for how far they should be.
                            transform.position = new Vector3(position.x + distance.x, controler.y, position.z + distance.y); //sets their position to limited reach
                        }
                        else //if not in vr just use tracking data of hand.
                        {
                            errortext = "obj name:" + gameObject.name + "\n" + "Error 13";
                            transform.position = controldeligate.localuser.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
                        }
                    }


                    break;

                case false:
                    errortext = "obj name:" + gameObject.name + "\n" + "Error 14";
                    if (controldeligate.localuser != null)
                    {
                        errortext = "obj name:" + gameObject.name + "\n" + "Error 15";
                        if (controldeligate.localuser.IsUserInVR()) //check above its the same thing just other hand
                        {
                            errortext = "obj name:" + gameObject.name + "\n" + "Error 16";
                            Vector3 controler = controldeligate.localuser.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
                            Vector3 position = controldeligate.localuser.GetPosition();
                            Vector2 distance = new Vector2(controler.x - position.x, controler.z - position.z);
                            distance = Vector2.ClampMagnitude(distance, 1.2f);
                            transform.position = new Vector3(position.x + distance.x, controler.y, position.z + distance.y);
                        }
                        else
                        {
                            errortext = "obj name:" + gameObject.name + "\n" + "Error 17";
                            transform.position = controldeligate.localuser.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
                        }
                    }


                    break;
            }
        }
    }

    private void LateUpdate()
    {
        Ustatus = true;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 18";
    }
}
