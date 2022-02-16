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
    public class PlayerCapsule : UdonSharpBehaviour
    {
        #region cache references
        public capsuleManager manager;
        private VRCPlayerApi localuser;
        [HideInInspector]
        public Renderer visual;
        [HideInInspector]
        public CapsuleCollider collider;
        public NetTagMessage tagmessager;
        #endregion

        [HideInInspector, UdonSynced(UdonSyncMode.None)]
        public byte state = byte.MaxValue;
        /*
         * 0 = offline
         * 1 = Queuing
         * 2 = queue as tagger
         * 3 = runner
         * 4 = tagger
         * 5 = frozen
         * 
         */     

        private void OnEnable()
        {
            if (localuser != null)
            {
                if (localuser.IsOwner(gameObject))
                {
                    if (manager.localCapsule == null)
                    {
                        manager.localCapsule = this;
                        collider.enabled = false;
                        visual.enabled = false;
                        manager.SendCustomEventDelayedSeconds(nameof(manager.SetupOffline), 1f, VRC.Udon.Common.Enums.EventTiming.Update);
                    }

                }
            }
        }

        void Start()
        {
            localuser = Networking.LocalPlayer;
            visual = GetComponent<Renderer>();
            collider = GetComponent<CapsuleCollider>();
        }

        public virtual void OnOwnershipTransferred(VRC.SDKBase.VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                if (manager.localCapsule == null)
                {
                    manager.localCapsule = this;
                    collider.enabled = false;
                    visual.enabled = false;
                    manager.SendCustomEventDelayedSeconds(nameof(manager.SetupOffline), 1f, VRC.Udon.Common.Enums.EventTiming.Update);
                }
            }
        }

        /*
        public virtual bool OnOwnershipRequest(VRC.SDKBase.VRCPlayerApi requestingPlayer, VRC.SDKBase.VRCPlayerApi requestedOwner)
        {
            Debug.Log("processed request ownership");

            if (requestingPlayer.isLocal || requestedOwner.isLocal)
            {
                return true;
            }

            manager.Tagged(requestingPlayer);
            return false;

        }*/

        public virtual void OnPostSerialization(VRC.Udon.Common.SerializationResult result) 
        {
            if (result.success)
            {
                
            }
            else
            {
                manager.CheckLocalClogg();
            }
        }

        public virtual void OnDeserialization() 
        {
            manager.SetCapsule(this);
        }

        public void WrongStateMessage()
        {
            /*
            if (localuser.IsOwner(gameObject))
            {
                manager.WrongStateDetected();
            }*/
        }

        public void LateJoinRequest()
        {
            if (localuser.isMaster)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(JRunner));
            }
        }

        //owner event receivers
        public void Offline()
        {
            if(manager.localCapsule != this)
            {
                return;
            }
            manager.SetupOffline();
        }

        public void Runner()
        {
            if (manager.localCapsule != this)
            {
                return;
            }
            manager.SetupRunner();
        }

        public void Tagger()
        {
            if (manager.localCapsule != this)
            {
                return;
            }

            manager.SetupTagger();
        }

        public void Frozen()
        {
            if (manager.localCapsule != this)
            {
                return;
            }
            manager.SetupFrozen();
        }

        public void Queuing()
        {
            if (manager.localCapsule != this)
            {
                return;
            }
            manager.SetupQueuing();
        }

        public void QTagger()
        {
            if (manager.localCapsule != this)
            {
                return;
            }
            manager.SetupQTagger();
        }

        public void JTagger()
        {
            if (manager.localCapsule != this)
            {
                return;
            }
            manager.JoinTaggers();
        }

        public void JRunner()
        {
            if (manager.localCapsule != this)
            {
                return;
            }
            manager.JoinRunners();
        }

    }
}
