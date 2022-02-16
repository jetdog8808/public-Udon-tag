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
    public class capsuleManager : UdonSharpBehaviour
    {
        #region cache references
        private VRCPlayerApi localuser;
        [HideInInspector]
        public PlayerCapsule localCapsule;
        [HideInInspector]
        public VRCPlayerApi touchedCache;
        [HideInInspector]
        public HandManager handManager;
        [HideInInspector]
        public GameSetting settings;
        [HideInInspector]
        public GameManager gameManager;
        [HideInInspector]
        public PlayerMovement playerMovement;
        [HideInInspector]
        public ErrorSystem errorSystem;
        [HideInInspector]
        public HudDisplay hudDisplay;
        public JetDog.ObjPool.OwnershipPool pooloptions;
        public PlayerCapsule[] players;
        #endregion

        public string player_T = "state";

        public Material offline_M;
        public Material runner_M;
        public Material tagger_M;
        public Material frozen_M;
        public Material error_M;


        void Start()
        {
            localuser = Networking.LocalPlayer;
            //SendCustomEventDelayedSeconds(nameof(RecursiveDataSend), 3f, VRC.Udon.Common.Enums.EventTiming.Update);

        }

        public void GetNewCapsule()
        {
            localCapsule = null;
            handManager.netTagMessager = null;
            Networking.SetOwner(Networking.GetOwner(pooloptions.gameObject), pooloptions.ownedObj);
            pooloptions.ownedObj = null;
            pooloptions.NetReady();

        }

        public void SetCapsule(PlayerCapsule capsule)
        {
            switch (capsule.state)
            {
                /*
                 * 0 = offline
                 * 1 = runner
                 * 2 = tagger
                 * 3 = frozen
                 * 4 = Queuing
                 */

                case 0:
                    capsule.visual.sharedMaterial = offline_M;
                    Networking.GetOwner(capsule.gameObject).SetPlayerTag(player_T, "offline");
                    if (gameManager)
                    {
                        gameManager.QueueCount();
                    }
                    break;
                case 1:
                    capsule.visual.sharedMaterial = offline_M;
                    Networking.GetOwner(capsule.gameObject).SetPlayerTag(player_T, "queuing");
                    if (gameManager)
                    {
                        gameManager.QueueCount();
                    }
                    break;
                case 2:
                    capsule.visual.sharedMaterial = offline_M;
                    Networking.GetOwner(capsule.gameObject).SetPlayerTag(player_T, "qtagger");
                    if (gameManager)
                    {
                        gameManager.QueueCount();
                    }
                    break;
                case 3:
                    capsule.visual.sharedMaterial = runner_M;
                    Networking.GetOwner(capsule.gameObject).SetPlayerTag(player_T, "runner");
                    break;
                case 4:
                    capsule.visual.sharedMaterial = tagger_M;
                    Networking.GetOwner(capsule.gameObject).SetPlayerTag(player_T, "tagger");
                    break;
                case 5:
                    capsule.visual.sharedMaterial = frozen_M;
                    Networking.GetOwner(capsule.gameObject).SetPlayerTag(player_T, "frozen");
                    break;
                default:
                    capsule.visual.sharedMaterial = error_M;
                    Networking.GetOwner(capsule.gameObject).SetPlayerTag(player_T, "error");
                    break;
            }
        }

        public void Tagged(VRCPlayerApi tagger)
        {
            if(localCapsule.state < 3 || tagger == null || !Utilities.IsValid(tagger))
            {
                return;
            }

            switch (settings.GameMode)
            {
                /*
                case 0: //tag  handled from hand manager
                    
                    break;
                */
                case 1: //freeze tag
                    switch (tagger.GetPlayerTag(player_T))
                    {

                        case "runner":
                            if(localCapsule.state == 5)
                            {
                                hudDisplay.UnFrozenText();
                                SetupRunner();
                            }
                            break;
                        case "tagger":
                            if(localCapsule.state != 4)
                            {
                                hudDisplay.FrozenText();
                                SetupFrozen();
                            }
                            break;
                        default:
                            Debug.Log("tagged by:" + tagger.GetPlayerTag(player_T));
                            break;
                    }
                    break;
                case 2: //infection
                    switch (tagger.GetPlayerTag(player_T))
                    {
                        case "tagger":
                            if (localCapsule.state != 4)
                            {
                                hudDisplay.InfectedText();
                                SetupTagger();
                            }
                            break;
                        default:
                            Debug.Log("tagged by:" + tagger.GetPlayerTag(player_T));
                            break;
                    }
                    break;
                case 3: //elimination
                    switch (tagger.GetPlayerTag(player_T))
                    {
                        case "tagger":
                            if(localCapsule.state != 4)
                            {
                                hudDisplay.EliminatedText();
                                SetupQueuing();
                                gameManager.SendCustomEventDelayedFrames(nameof(gameManager.ForceRespawn), 1, VRC.Udon.Common.Enums.EventTiming.Update);

                            }
                            //respawn here
                            break;
                        default:
                            Debug.Log("tagged by:" + tagger.GetPlayerTag(player_T));
                            break;
                    }
                    break;
                default:
                    Debug.Log("game mode not right");
                    return;
            }

            
        }

        public PlayerCapsule[] GetPlayersOfState(byte GetState)
        {
            int arraysize = 0;
            PlayerCapsule[] Rplayers = null;

            for (int i = 0; i < players.Length; i++)
            {
                if(players[i].state == GetState && players[i].gameObject.activeInHierarchy)
                {
                    arraysize++;
                }
            }
            
            if( arraysize == 0)
            {
                return null;
            }

            Rplayers = new PlayerCapsule[arraysize];
            arraysize--;

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].state == GetState && players[i].gameObject.activeInHierarchy)
                {
                    Rplayers[Mathf.Clamp(arraysize, 0, Rplayers.Length - 1)] = players[i];
                    arraysize--;
                }
            }

            return Rplayers;
        }

        public int GetPlayerCountOfState(byte GetState)
        {
            int size = 0;

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].state == GetState && players[i].gameObject.activeInHierarchy)
                {
                    size++;
                }
            }

            return size;
        }

        /*
        public void RecursiveDataSend() 
        {
            if (localCapsule && !Networking.IsClogged)
            {
                localCapsule.RequestSerialization();
            }

            SendCustomEventDelayedSeconds(nameof(RecursiveDataSend), 3f, VRC.Udon.Common.Enums.EventTiming.Update);
        }
        */

        public void CheckLocalClogg()
        {
            if (Networking.IsClogged)
            {
                SendCustomEventDelayedSeconds(nameof(CheckLocalClogg), 1f, VRC.Udon.Common.Enums.EventTiming.Update);
            }
            else
            {
                localCapsule.RequestSerialization();
            }
        }

        public void WrongStateDetected()
        {
            errorSystem.WrongState();

        }


        public void SetupOffline()
        {
            if(localCapsule == null)
            {
                return;
            }
           
            localCapsule.state = 0;
            SetCapsule(localCapsule);
            CheckLocalClogg();
            handManager.lHand.visual.sharedMaterial = offline_M;
            handManager.rHand.visual.sharedMaterial = offline_M;
            handManager.lHand.trigger.enabled = false;
            handManager.rHand.trigger.enabled = false;
            playerMovement.UnFreeze();
            playerMovement.NormalSpeed();
            gameManager.StatusText();
            
        }

        public void SetupQueuing()
        {
            if (localCapsule == null)
            {
                return;
            }

            localCapsule.state = 1;
            SetCapsule(localCapsule);
            CheckLocalClogg();
            handManager.lHand.visual.sharedMaterial = offline_M;
            handManager.rHand.visual.sharedMaterial = offline_M;
            handManager.lHand.trigger.enabled = false;
            handManager.rHand.trigger.enabled = false;
            playerMovement.UnFreeze();
            playerMovement.NormalSpeed();
            gameManager.StatusText();
        }

        public void SetupQTagger()
        {
            if (localCapsule == null)
            {
                return;
            }

            localCapsule.state = 2;
            SetCapsule(localCapsule);
            CheckLocalClogg();
            handManager.lHand.visual.sharedMaterial = offline_M;
            handManager.rHand.visual.sharedMaterial = offline_M;
            handManager.lHand.trigger.enabled = false;
            handManager.rHand.trigger.enabled = false;
            playerMovement.UnFreeze();
            playerMovement.NormalSpeed();
            gameManager.StatusText();
        }

        public void SetupRunner()
        {
            if (localCapsule == null)
            {
                return;
            }

            localCapsule.state = 3;
            SetCapsule(localCapsule);
            CheckLocalClogg();
            handManager.lHand.visual.sharedMaterial = runner_M;
            handManager.rHand.visual.sharedMaterial = runner_M;
            handManager.lHand.trigger.enabled = true;
            handManager.rHand.trigger.enabled = true;
            playerMovement.UnFreeze();
            playerMovement.GameSpeed();
        }

        public void SetupTagger()
        {
            if (localCapsule == null)
            {
                return;
            }

            localCapsule.state = 4;
            SetCapsule(localCapsule);
            CheckLocalClogg();
            handManager.lHand.visual.sharedMaterial = tagger_M;
            handManager.rHand.visual.sharedMaterial = tagger_M;
            handManager.lHand.trigger.enabled = true;
            handManager.rHand.trigger.enabled = true;
            playerMovement.UnFreeze();
            playerMovement.GameSpeed();
        }

        public void SetupFrozen()
        {
            if (localCapsule == null)
            {
                return;
            }

            localCapsule.state = 5;
            SetCapsule(localCapsule);
            CheckLocalClogg();
            handManager.lHand.visual.sharedMaterial = frozen_M;
            handManager.rHand.visual.sharedMaterial = frozen_M;
            handManager.lHand.trigger.enabled = false;
            handManager.rHand.trigger.enabled = false;
            playerMovement.GameSpeed();
            playerMovement.Freeze();
        }

        public void JoinTaggers()
        {
            
            gameManager.maps[settings.Map].TaggerSpawn();
            hudDisplay.DisplayGameMode();
            SetupTagger();
            playerMovement.Freeze();
            gameManager.localBeenInGame = true;
            gameManager.InGame = true;
        }

        public void JoinRunners()
        {
            
            gameManager.maps[settings.Map].RunnerSpawn();
            hudDisplay.DisplayGameMode();
            SetupRunner();
            gameManager.localBeenInGame = true;
            gameManager.InGame = true;
        }
    }
}
