/*
*===========================================================*
*       _      _   ____              _          _           *
*      | | ___| |_|  _ \  ___   __ _| |    __ _| |__  ___   *
*   _  | |/ _ \ __| | | |/ _ \ / _` | |   / _` | '_ \/ __|  *
*  | |_| |  __/ |_| |_| | (_) | (_| | |__| (_| | |_) \__ \  *
*   \___/ \___|\__|____/ \___/ \__, |_____\__,_|_.__/|___/  *
*                              |___/                        *
*===========================================================*
*                                                           *
*                  Auther: Jetdog8808                       *
*                                                           *
*===========================================================*
*/

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace JetDog
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class RequestOwnerEvent : UdonSharpBehaviour
    {
        public UdonBehaviour ownedObj;
        public string eventName;
        public VRCPlayerApi requestingP;
        public VRCPlayerApi requestedO;

        public virtual void Interact() 
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        public virtual bool OnOwnershipRequest(VRC.SDKBase.VRCPlayerApi requestingPlayer, VRC.SDKBase.VRCPlayerApi requestedOwner)
        {

            Debug.Log("processed request ownership");
            requestingP = requestingPlayer;
            requestedO = requestedOwner;

            if(requestingPlayer.isLocal || requestedOwner.isLocal)
            {
                return true;
            }

            //ownedObj.SendCustomEvent(eventName);
            return false;

        }
    }
}
