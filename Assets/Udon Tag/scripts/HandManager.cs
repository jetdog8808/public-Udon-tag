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
    public class HandManager : UdonSharpBehaviour
    {
        #region cache references
        [HideInInspector]
        public GameSetting settings;
        [HideInInspector]
        public capsuleManager capsuleManager;
        [HideInInspector]
        public NetTagMessage netTagMessager;
        #endregion

        [Range(.5f, 3f)]
        public float reach = 1f;
        [Range(.5f, 3f)]
        public float height = 2f;

        public HandCollider lHand;
        private Transform lHand_T;
        public HandCollider rHand;
        private Transform rHand_T;

        private VRCPlayerApi localuser;


        void Start()
        {
            localuser = Networking.LocalPlayer;

            lHand_T = lHand.GetComponent<Transform>();
            rHand_T = rHand.GetComponent<Transform>();
            lHand.manager = this;
            rHand.manager = this;
            lHand.hand = VRC_Pickup.PickupHand.Left;
            rHand.hand = VRC_Pickup.PickupHand.Right;

        }

        private void Update()
        {
            SetHandPosition();
        }

        public void SetHandPosition()
        {
            Vector3 pos;
            VRCPlayerApi.TrackingData playerhand;

            for (int i = 0; i < 2; i++)
            {
                if (!Utilities.IsValid(localuser))
                {
                    return;
                }
                pos = localuser.GetPosition();

                if (i == 0)
                {
                    playerhand = localuser.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
                }
                else
                {
                    playerhand = localuser.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
                }

                pos.y = Mathf.Clamp(playerhand.position.y, pos.y - (height / 2f), pos.y + height);
                pos.x = Mathf.Clamp(playerhand.position.x, pos.x - reach, pos.x + reach);
                pos.z = Mathf.Clamp(playerhand.position.z, pos.z - reach, pos.z + reach);

                if (i == 0)
                {
                    lHand_T.position = pos;
                }
                else
                {
                    rHand_T.position = pos;
                }
            }

        }

        public void SendNetMessage(PlayerCapsule capsule, VRC_Pickup.PickupHand hand)
        {
            /*
            switch (settings.GameMode)
            {
                case 0: //tag special case
                    //if you are tagger and you touch runner
                    if(capsuleManager.localCapsule.state == 4 && capsule.state == 3)
                    {
                        capsule.SendCustomNetworkEvent( VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(PlayerCapsule.Tagger));
                        capsuleManager.SetupRunner();
                    }
                    break;
                default: //faster message. receiver translats to correct message.
                    if(capsule.state != capsuleManager.localCapsule.state)
                    {
                        Networking.SetOwner(localuser, capsule.gameObject);
                        localuser.PlayHapticEventInHand(hand, 0.40f, 1.0f, 50f); //seconds, 0-1 amplitude, 0-320hz
                    }
                    break;
            }*/

            if (capsule.state == capsuleManager.localCapsule.state)
            {
                return;
                 
            }

            if(netTagMessager == null)
            {
                netTagMessager = capsuleManager.localCapsule.tagmessager;
            }

            if (!netTagMessager.clear || Networking.IsClogged)
            {
                Debug.Log("netsend hand manager not clear or clogged");
                return;
            }

            int id = Networking.GetOwner(capsule.gameObject).playerId;
            byte[] userid;

            if(id <= byte.MaxValue)
            {
                userid = new byte[1] { (byte)id };
            }
            else if(id <= ushort.MaxValue)
            {
                userid = new byte[2] { (byte)((((ushort)id) & 0xff00u) >> 8), (byte)(((ushort)id) & 0x00ffu) };
            }
            else if(id<= 0xffffffu)
            {
                userid = new byte[3] { (byte)((((uint)id) & 0xff0000u) >> 16), (byte)((((uint)id) & 0x00ff00u) >> 8), (byte)(((uint)id) & 0x0000ffu) };
            }
            else
            {
                userid = new byte[4] { (byte)((((uint)id) & 0xff000000u) >> 24), (byte)((((uint)id) & 0x00ff0000u) >> 16), (byte)((((uint)id) & 0x0000ff00u) >> 8), (byte)(((uint)id) & 0x000000ffu) };
            }

            netTagMessager.clear = false;
            netTagMessager.userid = userid;
            netTagMessager.RequestSerialization();
            localuser.PlayHapticEventInHand(hand, 0.40f, 1.0f, 50f); //seconds, 0-1 amplitude, 0-320hz
        }

    }
}
