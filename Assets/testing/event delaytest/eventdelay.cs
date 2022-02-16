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
    public class eventdelay : UdonSharpBehaviour
    {
        public ownerdelay refdelay;
        [UdonSynced(UdonSyncMode.None)]
        public int sendTime;

        public int receiveEvent;
        public int receiveSerialization;

        void Start()
        {

        }

        public void Ping()
        {
            receiveEvent = Networking.GetServerTimeInMilliseconds() & 0x7FFFFFFF;
            Debug.Log("owner request delay: " + (refdelay.receiveOwnerrequest - sendTime).ToString() + " || serialization delay: " + (receiveSerialization - sendTime).ToString() + " || event delay: " + (receiveEvent - sendTime).ToString());
        }

        public virtual void Interact() 
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            Networking.SetOwner(Networking.LocalPlayer, refdelay.gameObject);
            sendTime = Networking.GetServerTimeInMilliseconds() & 0x7FFFFFFF;
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Ping");
            RequestSerialization();
        }

        public virtual void OnPostSerialization(VRC.Udon.Common.SerializationResult result) 
        {
            receiveSerialization = Networking.GetServerTimeInMilliseconds() & 0x7FFFFFFF;
        }

        public virtual void OnDeserialization() 
        {
            receiveSerialization = Networking.GetServerTimeInMilliseconds() & 0x7FFFFFFF;
        }
    }
}
