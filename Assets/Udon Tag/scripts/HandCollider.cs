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

namespace JetDog.Tag
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class HandCollider : UdonSharpBehaviour
    {
        #region cache references
        private VRCPlayerApi localuser;
        #endregion
        public Renderer visual;
        public Collider trigger;
        [HideInInspector]
        public HandManager manager;
        [HideInInspector]
        public VRC_Pickup.PickupHand hand;

        void Start()
        {
            localuser = Networking.LocalPlayer;

        }

        private void OnTriggerEnter(Collider other)
        {
            if (!Utilities.IsValid(other) || other == null)
            {
                return;
            }

            PlayerCapsule capsule = other.GetComponent<PlayerCapsule>();

            if (capsule)
            {
                manager.SendNetMessage(capsule, hand);
            }
        }

        
    }
}
