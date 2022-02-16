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
    public class ownerdelay : UdonSharpBehaviour
    {
        public int receiveOwnerrequest;
        void Start()
        {

        }

        public virtual bool OnOwnershipRequest(VRC.SDKBase.VRCPlayerApi requestingPlayer, VRC.SDKBase.VRCPlayerApi requestedOwner)
        {
            if (Networking.LocalPlayer.IsOwner(gameObject))
            {
                receiveOwnerrequest = Networking.GetServerTimeInMilliseconds() & 0x7FFFFFFF;
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
