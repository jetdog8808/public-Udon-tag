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
using TMPro;

namespace JetDog.Tag
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class GameManager : UdonSharpBehaviour
    {
        public GameSetting settings;
        public Timer gameTimer;
        public capsuleManager capsuleManager;
        public HandManager handManager;
        public PlayerMovement playerMovement;
        public ErrorSystem errorSystem;
        public HudDisplay hudDisplay;

        public SpawnLocation respawn;
        public MapData[] maps;

        public float tickTime = 0.5f;
        private float tTime;

        public TextMeshProUGUI winnerText;
        public TextMeshProUGUI queueText;
        public TextMeshProUGUI queueNumberText;
        public TextMeshProUGUI voteText;

        private VRCPlayerApi localuser;

        public GameObject joinQObj;
        public GameObject tagQObj;
        public GameObject ExitQObj;

        public GameObject startObj;
        public GameObject lateJoinObj;
        public GameObject endGameObj;


        [HideInInspector, UdonSynced(UdonSyncMode.None)]
        public bool GameInProgress = false;
        [HideInInspector]
        public bool localBeenInGame = false;
        [HideInInspector]
        public bool InGame = false;

        private byte voteNumber;
        private bool hasVoted = false;

        void Start() //setup references
        {
            gameTimer.settings = settings;
            gameTimer.gameManager = this;
            gameTimer.capsuleManager = capsuleManager;
            gameTimer.playerMovement = playerMovement;

            capsuleManager.handManager = handManager;
            capsuleManager.settings = settings;
            capsuleManager.gameManager = this;
            capsuleManager.playerMovement = playerMovement;
            capsuleManager.errorSystem = errorSystem;
            capsuleManager.hudDisplay = hudDisplay;

            handManager.settings = settings;
            handManager.capsuleManager = capsuleManager;

            playerMovement.settings = settings;
            playerMovement.gameManager = this;

            settings.playerMovement = playerMovement;
            settings.gameManager = this;
            settings.capsuleManager = capsuleManager;

            hudDisplay.settings = settings;
            
            for (int i = 0; i < maps.Length; i++)
            {
                maps[i].gameManager = this;
                maps[i].settings = settings;
                maps[i].number = i;

                foreach(RespawnBounds bounds in maps[i].respawnBounds)
                {
                    bounds.gameManager = this;
                }
            }

            tTime = tickTime;

            localuser = Networking.LocalPlayer;

            joinQObj.SetActive(true);
            tagQObj.SetActive(true);
            ExitQObj.SetActive(false);

        }

        private void Update()
        {
            if (!Networking.IsMaster)
            {
                return;
            }

            tTime -= Time.deltaTime;

            if(tTime < 0)
            {
                tTime = tickTime;

                if (GameInProgress)
                {
                    GameCheck();
                }
            }
        }

        public void GameCheck()
        {
            switch (settings.GameMode)
            {
                case 0: //tag 
                    if (capsuleManager.GetPlayersOfState(3) == null)
                    {
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(RunnersLeft));
                        EndGame();
                    }
                    if (capsuleManager.GetPlayersOfState(4) == null)
                    {
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(TaggersLeft));
                        EndGame();
                    }
                    return;
                case 1: //freeze tag
                    if (capsuleManager.GetPlayersOfState(4) == null)
                    {
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(TaggersLeft));
                        EndGame();
                    }
                    if (capsuleManager.GetPlayersOfState(3) == null)
                    {
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(TaggersWin));
                        EndGame();
                    }
                    return;
                case 2: //infection
                    if (capsuleManager.GetPlayersOfState(4) == null)
                    {
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(TaggersLeft));
                        EndGame();
                    }
                    if (capsuleManager.GetPlayersOfState(3) == null)
                    {
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(TaggersWin));
                        EndGame();
                    }
                    return;
                case 3: //hunt
                    if (capsuleManager.GetPlayersOfState(4) == null)
                    {
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(TaggersLeft));
                        EndGame();
                    }
                    if (capsuleManager.GetPlayersOfState(3) == null)
                    {
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(TaggersWin));
                        EndGame();
                    }
                    return;
                default:
                    if (capsuleManager.GetPlayersOfState(3) == null || capsuleManager.GetPlayersOfState(4) == null)
                    {
                        EndGame();
                    }
                    return;
            }
        }

        public void StartButton()
        {
            if (!settings.MasterLock || localuser.IsOwner(settings.gameObject) || localuser.isMaster || localuser.isInstanceOwner)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(StartGame));
            }
        }

        public void LateJoin()
        {
            if(GameInProgress && settings.LateJoin && !localBeenInGame)
            {
                capsuleManager.localCapsule.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(PlayerCapsule.LateJoinRequest));
            }
        }

        public void EndButton()
        {
            if (localuser.isMaster || localuser.isInstanceOwner)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(EndGame));
            }
        }



        public void StartGame()
        {
            if (GameInProgress || !localuser.isMaster)
            {
                return;
            }

            
            PlayerCapsule[] players;
            int[] randomize;

            //check if people are in wrong state;
            for (int i = 3; i < 6; i++)
            {
                players = capsuleManager.GetPlayersOfState((byte)i);

                if (players != null)
                {
                    foreach (PlayerCapsule caps in players)
                    {
                        caps.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(PlayerCapsule.WrongStateMessage));
                    }
                }
            }

            //get all people who want to be taggers
            players = capsuleManager.GetPlayersOfState(2);

            if (players != null)
            {
                Debug.Log("has array of taggers");
                //Utilities.ShuffleArray<PlayerCapsule>(players);
                randomize = new int[players.Length];
                for (int i = 0; i < randomize.Length; i++)
                {
                    randomize[i] = i;
                }
                Utilities.ShuffleArray(randomize);


                if (players.Length >= settings.TaggerCount + 1)
                {
                    for (int i = 0; i < settings.TaggerCount + 1; i++)
                    {
                        players[randomize[i]].SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(PlayerCapsule.JTagger));
                    }

                    for (int i = settings.TaggerCount + 1; i < players.Length; i++)
                    {
                        players[randomize[i]].SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(PlayerCapsule.JRunner));
                    }


                    players = capsuleManager.GetPlayersOfState(1);

                    if(players != null)
                    {
                        for (int i = 0; i < players.Length; i++)
                        {
                            players[i].SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(PlayerCapsule.JRunner));
                        }
                    }
                    
                }
                else
                {
                    int count = 0;
                    for (int i = 0; i < players.Length; i++)
                    {
                        count++;
                        players[i].SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(PlayerCapsule.JTagger));
                    }

                    players = capsuleManager.GetPlayersOfState(1);
                    if(players != null)
                    {
                        //Utilities.ShuffleArray<PlayerCapsule>(players);
                        randomize = new int[players.Length];
                        for (int i = 0; i < randomize.Length; i++)
                        {
                            randomize[i] = i;
                        }
                        Utilities.ShuffleArray(randomize);

                        for (int i = 0; i < ((settings.TaggerCount + 1) - count); i++)
                        {
                            players[randomize[i]].SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(PlayerCapsule.JTagger));
                        }

                        for (int i = (settings.TaggerCount + 1 - count); i < players.Length; i++)
                        {
                            players[randomize[i]].SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(PlayerCapsule.JRunner));
                        }
                    }

                }
            }
            else
            {
                Debug.Log("does not have array of taggers");
                players = capsuleManager.GetPlayersOfState(1);
                if(players != null)
                {
                    //Utilities.ShuffleArray<PlayerCapsule>(players);
                    randomize = new int[players.Length];
                    for (int i = 0; i < randomize.Length; i++)
                    {
                        randomize[i] = i;
                    }

                    Utilities.ShuffleArray(randomize);

                    Debug.Log("should have this amount of taggers:" + settings.TaggerCount + 1);

                    for (int i = 0; i < Mathf.Clamp(settings.TaggerCount + 1, 1, players.Length); i++)
                    {
                        Debug.Log("send message of set to tagger");
                        players[randomize[i]].SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(PlayerCapsule.JTagger));
                    }

                    for (int i = settings.TaggerCount + 1; i < players.Length; i++)
                    {
                        Debug.Log("send message to set to runner");
                        players[randomize[i]].SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(PlayerCapsule.JRunner));
                    }
                }
            }


            gameTimer.StartTimer();
            tTime = 5f;
            GameInProgress = true;
            RequestSerialization();
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(PostStart));

        }

        public void PostStart()
        {
            QueueCount();
            ResetBoard();
            SetupMap();
            ResetVote();
            DesyncCheck();
        }

        public void UnlockVote()
        {
            if (!hasVoted)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(AddVote));
                hasVoted = true;
            }
            
        }

        public void AddVote()
        {
            voteNumber++;
            voteText.text = string.Concat("Vote Unlock\n", voteNumber, "/", (byte)(Mathf.CeilToInt(VRCPlayerApi.GetPlayerCount() / 3f)));
            

            if(localuser.isMaster && voteNumber >= (byte)(Mathf.CeilToInt(VRCPlayerApi.GetPlayerCount() / 3f)))
            {
                settings.Unlock();
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ResetVote));
            }
        }

        public void ResetVote()
        {
            voteNumber = 0;
            voteText.text = "Vote Unlock";
            hasVoted = false;
        }

        public void DesyncCheck()
        {
            if(capsuleManager.localCapsule == null)
            {
                return;
            }

            if(capsuleManager.localCapsule.state == 1 || capsuleManager.localCapsule.state == 2)
            {
                capsuleManager.GetNewCapsule();
                capsuleManager.SendCustomEventDelayedSeconds(nameof(LateJoin), 3f, VRC.Udon.Common.Enums.EventTiming.Update);
            }
        }

        public virtual void OnPostSerialization(VRC.Udon.Common.SerializationResult result) { ControlsSetState(); }

        public virtual void OnDeserialization() { ControlsSetState(); }

        public void SetupMap()
        {
            foreach(MapData map in maps)
            {
                if(map.number == settings.Map)
                {
                    map.SetBounds();
                }
                else
                {
                    map.TurnAllOff();
                }
            }
        }

        public void TimeUp()
        {
            switch (settings.GameMode)
            {
                case 0: //tag 
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(RunnersWin));
                    break;
                case 1: //freeze tag
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(RunnersWin));
                    break;
                case 2: //infection
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(RunnersWin));
                    break;
                case 3: //hunt
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(RunnersWin));
                    break;
            }

            EndGame();
        }

        public void EndGame()
        {
            if (!GameInProgress || !localuser.isMaster)
            {
                return;
            }

            gameTimer.EndTimer();
            GameInProgress = false;
            RequestSerialization();

            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(SendPlayersBack));
        }

        public void SendPlayersBack()
        {
            localBeenInGame = false;

            if(capsuleManager.localCapsule.state > 2)
            {
                capsuleManager.SetupQueuing();
                ForceRespawn();
            }
        }

        public void StatusText()
        {
            byte temp = capsuleManager.localCapsule.state;
            switch (temp)
            {
                case 0:
                    queueText.text = "Not Queued";
                    break;
                case 1:
                    queueText.text = "<color=green>Waiting for Next Game.</color>";
                    break;
                case 2:
                    queueText.text = "<color=red>Wanting to be Tagger Next Game.</color>";
                    break;
                default:
                    queueText.text = "Other";
                    break;
            }

            ExitQObj.SetActive(temp != 0);
            joinQObj.SetActive(temp != 1);
            tagQObj.SetActive(temp != 2);
        }

        public void ControlsSetState()
        {
            if(capsuleManager.localCapsule == null)
            {
                startObj.SetActive(false);
                lateJoinObj.SetActive(false);
                endGameObj.SetActive(false);
                return;
            }

            startObj.SetActive(!GameInProgress && ((!settings.MasterLock && capsuleManager.localCapsule.state != 0) || localuser.IsOwner(settings.gameObject) || localuser.isMaster || localuser.isInstanceOwner));
            lateJoinObj.SetActive(GameInProgress && settings.LateJoin);
            endGameObj.SetActive(GameInProgress && (localuser.isMaster || localuser.isInstanceOwner));
        }

        public virtual void OnPlayerLeft(VRC.SDKBase.VRCPlayerApi player) { QueueCount(); }

        public void QueueCount()
        {
            queueNumberText.text = string.Concat((capsuleManager.GetPlayerCountOfState(1) + capsuleManager.GetPlayerCountOfState(2)).ToString(), " In Queue.");
        }

        public virtual void OnPlayerRespawn(VRC.SDKBase.VRCPlayerApi player) 
        {
            if (player.isLocal)
            {
                if(capsuleManager.localCapsule != null)
                {
                    capsuleManager.SetupOffline();
                }
                ForceRespawn();
            }
        }

        public void ForceRespawn()
        {
            InGame = false;
            respawn.TeleportHere();
        }

        public void OutOfBounds()
        {
            capsuleManager.SetupOffline();
            InGame = false;
            respawn.TeleportHere();
        }

        public void ResetBoard()
        {
            winnerText.text = string.Empty;
        }

        public void TaggersWin()
        {
            winnerText.text = "Taggers Win";
        }

        public void RunnersWin()
        {
            winnerText.text = "runners Win";
        }

        public void TaggersLeft()
        {
            winnerText.text = "Taggers Left";
        }

        public void RunnersLeft()
        {
            winnerText.text = "Runners Left";
        }

    }
}
