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
    public class GameSetting : UdonSharpBehaviour
    {
        #region cache references
        private VRCPlayerApi localuser;
        [HideInInspector]
        public PlayerMovement playerMovement;
        [HideInInspector]
        public GameManager gameManager;
        [HideInInspector]
        public capsuleManager capsuleManager;
        #endregion

        #region Network Variables

        [HideInInspector, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(PackedInfo))]
        private uint _packedInfo;
        /* bits  | mask      |shift | what
         * _________________________|________________
         * 0-1   |0x00000003 | 00   | speed modifier
         * 2-4   |0x0000001C | 02   | booster amount
         * 5-6   |0x00000060 | 05   | speed increase amount
         * 7     |0x00000080 | 07   | can join late
         * 8     |0x00000100 | 08   | foot steps
         * 9-10  |0x00000600 | 09   | game mode
         * 11-15 |0x0000F800 | 11   | tagger count
         * 16-20 |0x001F0000 | 16   | map selection
         * 21-22 |0x00600000 | 21   | map size
         * 23-26 |0x07800000 | 23   | time limit of round
         * 27    |0x08000000 | 27   | Locked only for master
         */
        public uint PackedInfo
        {
            get => _packedInfo;

            set
            {
                if (localuser.IsOwner(gameObject))
                {
                    Debug.Log("data owner update");
                    if(_packedInfo != value)
                    {
                        Debug.Log("data has changed");
                        _packedInfo = value;
                        UpdateCanvas();
                        RequestSerialization();
                    }

                }
                else
                {
                    Debug.Log("data nonowner update");
                    if (_packedInfo != value)
                    {
                        Debug.Log("data has changed");
                        _packedInfo = value;

                        _playerspeedmod = (byte)(value & 0x00000003u);
                        _boostAmount = (byte)((value & 0x0000001Cu) >> 2);
                        _boostSpeed = (byte)((value & 0x00000060u) >> 5);
                        _lateJoin = ((value & 0x00000080u) >> 7) == 1 ? true : false;
                        _footSteps = ((value & 0x00000100u) >> 8) == 1 ? true : false;
                        _gameMode = (byte)((value & 0x00000600u) >> 9);
                        _taggerCount = (byte)((value & 0x0000F800u) >> 11);
                        _map = (byte)((value & 0x001F0000u) >> 16);
                        _mapSize = (byte)((value & 0x00600000u) >> 21);
                        _time = (byte)((value & 0x07800000u) >> 23);
                        _masterLock = ((value & 0x08000000u) >> 27) == 1 ? true : false;

                        UpdateCanvas();
                    }
                }


            }
        }

        [SerializeField]
        private byte _playerspeedmod = 0;//2 bits
        public byte PlayerSpeedMod
        {

            get => _playerspeedmod;
            set
            {
                Debug.Log("speed updated");
                if (_playerspeedmod != Mathf.Clamp(value, 0, 3))
                {
                    Debug.Log("speed has changed");
                    _playerspeedmod = (byte)Mathf.Clamp(value, 0, 3);

                    if (localuser.IsOwner(gameObject)) { PackedInfo = (PackedInfo & ~0x00000003u) | _playerspeedmod; }
                }
            }
        }

        [SerializeField]
        private byte _boostAmount = 4;//3 bits
        public byte BoostAmount
        {

            get => _boostAmount;
            set
            {
                if (_boostAmount != Mathf.Clamp(value, 0, 7))
                {
                    _boostAmount = (byte)Mathf.Clamp(value, 0, 7);

                    if (localuser.IsOwner(gameObject)) { PackedInfo = (PackedInfo & ~0x0000001Cu) | ((uint)_boostAmount << 2); }
                }
            }
        }

        [SerializeField]
        private byte _boostSpeed = 2;//2 bits
        public byte BoostSpeed
        {

            get => _boostSpeed;
            set
            {
                Debug.Log("boost speed updated");
                if (_boostSpeed != Mathf.Clamp(value, 0, 3))
                {
                    Debug.Log("boost speed has changed");
                    _boostSpeed = (byte)Mathf.Clamp(value, 0, 3);

                    if (localuser.IsOwner(gameObject)) { PackedInfo = (PackedInfo & ~0x00000060u) | ((uint)_boostSpeed << 5); }
                }
            }
        }

        [HideInInspector]
        private bool _lateJoin = true;//1 bit
        public bool LateJoin
        {
            get => _lateJoin;
            set
            {
                if (_lateJoin != value)
                {
                    _lateJoin = value;

                    if (localuser.IsOwner(gameObject)) { PackedInfo = (PackedInfo & ~0x00000080u) | ((_lateJoin ? 1u : 0u) << 7); }
                }
            }
        }

        [HideInInspector]
        private bool _footSteps = false;//1 bit
        public bool FootSteps
        {
            get => _footSteps;
            set
            {
                if (_footSteps != value)
                {
                    _footSteps = value;

                    if (localuser.IsOwner(gameObject)) { PackedInfo = (PackedInfo & ~0x00000100u) | ((_footSteps ? 1u : 0u) << 8); }
                }
            }
        }

        [HideInInspector]
        private byte _gameMode = 1;//2 bits
        /* 0 = tag
         * "when tagger touches a runner = tagger becomes a runner and runner becomes tagger. who ever is not a tagger at the end of the timer wins."
         * 1 = freeze tag
         * "taggers try and freeze all runners by thouching them. runners can touch frozzen players to free them. runners win if taggers dont freeze everyone.
         * 2 = infection
         * "if a tagger touches a runner they become a tagger. runners win if they can make it to the end of time and is not a runner."
         * 3 = Elimination
         * "tagger has to touch all runners. runners will respawn if touched. tagger wins if all runners are cought."
         */
        public byte GameMode
        {
            get => _gameMode;
            set
            {
                if (_gameMode != Mathf.Clamp(value, 1, 3))
                {
                    _gameMode = (byte)Mathf.Clamp(value, 1, 3);

                    if (localuser.IsOwner(gameObject)) { PackedInfo = (PackedInfo & ~0x00000600u) | ((uint)_gameMode << 9); }
                }
            }
        }

        [HideInInspector]
        private byte _taggerCount;//5 bits
        public byte TaggerCount
        {
            get => _taggerCount;
            set
            {
                if (_taggerCount != Mathf.Clamp(value, 0, 31))
                {
                    _taggerCount = (byte)Mathf.Clamp(value, 0, 31);

                    if (localuser.IsOwner(gameObject)) { PackedInfo = (PackedInfo & ~0x0000F800u) | ((uint)_taggerCount << 11); }
                }
            }
        }

        [HideInInspector]
        public byte _map = 0;//5 bits
        public byte Map
        {
            get => _map;
            set
            {
                if (_map != Mathf.Clamp(value, 0, 31))
                {
                    _map = (byte)Mathf.Clamp(value, 0, 31);

                    if (localuser.IsOwner(gameObject)) { PackedInfo = (PackedInfo & ~0x001F0000u) | ((uint)_map << 16); }
                }
            }
        }

        [HideInInspector]
        public byte _mapSize = 1;// 2 bits
        public byte MapSize
        {
            get => _mapSize;
            set
            {
                if (_mapSize != Mathf.Clamp(value, 0, 3))
                {
                    _mapSize = (byte)Mathf.Clamp(value, 0, 3);

                    if (localuser.IsOwner(gameObject)) { PackedInfo = (PackedInfo & ~0x00600000u) | ((uint)_mapSize << 21); }
                }
            }
        }

        [HideInInspector]
        public byte _time = 3;//4 bits
        public byte Time
        {
            get => _time;
            set
            {
                if (_time != Mathf.Clamp(value, 0, 15))
                {
                    _time = (byte)Mathf.Clamp(value, 0, 15);

                    if (localuser.IsOwner(gameObject)) { PackedInfo = (PackedInfo & ~0x07800000u) | ((uint)_time << 23); }
                }
            }
        }

        [HideInInspector]
        public bool _masterLock = false;//1 bit
        public bool MasterLock
        {
            get => _masterLock;
            set
            {
                if (_masterLock != value)
                {
                    _masterLock = value;

                    if (localuser.IsOwner(gameObject)) { PackedInfo = (PackedInfo & ~0x08000000u) | ((_masterLock ? 1u : 0u) << 27); }
                }
            }
        }

        #endregion

        #region ui items
        public Canvas canvas;
        public GameObject blocker;
        public GameObject voteButton;
        public TextMeshProUGUI lock_T;

        public TextMeshProUGUI gameMode_T;
        public TextMeshProUGUI speedModifier_T;
        public TextMeshProUGUI boostAmount_T;
        public TextMeshProUGUI speedIncrease_T;

        public TextMeshProUGUI lateJoin_T;
        public TextMeshProUGUI footSteps_T;
        public TextMeshProUGUI taggerCount_T;
        public TextMeshProUGUI mapSize_T;
        public TextMeshProUGUI time_T;

        public TextMeshProUGUI masterText;
        public TextMeshProUGUI editingSettings;
        #endregion

        public void Start()
        {
            localuser = Networking.LocalPlayer;
            if (Networking.IsMaster)
            {
                SendCustomEventDelayedSeconds(nameof(LateRequestSerial), 1f);
                blocker.SetActive(false);
            }
            else
            {
                blocker.SetActive(true);
            }

            if (Utilities.IsValid(Networking.GetOwner(blocker)))
            {
                masterText.text = string.Concat("GameMaster is: ", Networking.GetOwner(blocker).displayName);

                editingSettings.text = string.Concat("Editing Settings: ", Networking.GetOwner(gameObject).displayName);
            }

            SendCustomEventDelayedSeconds(nameof(UpdateCanvas), 3f, VRC.Udon.Common.Enums.EventTiming.Update);
        }

        public void LateRequestSerial()
        {
            uint temp = 0u;

            temp |= _playerspeedmod;
            temp |= (uint)_boostAmount << 2;
            temp |= (uint)_boostSpeed << 5;
            temp |= (_lateJoin ? 1u : 0u) << 7;
            temp |= (_footSteps ? 1u : 0u) << 8;
            temp |= (uint)_gameMode << 9;
            temp |= (uint)_taggerCount << 11;
            temp |= (uint)_map << 16;
            temp |= (uint)_mapSize << 21;
            temp |= (uint)_time << 23;
            temp |= (_masterLock ? 1u : 0u) << 27;

            PackedInfo = temp;

        }

        public void Unlock()
        {
            if (localuser.isMaster)
            {
                Networking.SetOwner(localuser, gameObject);
            }
        }

        public void UpdateCanvas()
        {
            canvas.enabled = false;

            switch (GameMode)
            {
                case 0:
                    gameMode_T.text = "Tag";
                    break;
                case 1:
                    gameMode_T.text = "Freeze Tag";
                    break;
                case 2:
                    gameMode_T.text = "Infection Tag";
                    break;
                case 3:
                    gameMode_T.text = "Elimination Tag";
                    break;

            }

            switch (MapSize)
            {
                case 0:
                    mapSize_T.text = "Small";
                    break;
                case 1:
                    mapSize_T.text = "Medium";
                    break;
                case 2:
                    mapSize_T.text = "Large";
                    break;
            }

            if (localuser.IsOwner(gameObject))
            {
                lock_T.text = MasterLock ? "Locked" : "UnLocked";
                voteButton.SetActive(false);
            }
            else
            {
                lock_T.text = MasterLock ? "Locked" : "Unlock";
                voteButton.SetActive(MasterLock);
            }

            if (playerMovement)
            {
                speedModifier_T.text = (playerMovement.speed + PlayerSpeedMod).ToString();
                speedIncrease_T.text = (playerMovement.speed + PlayerSpeedMod + BoostSpeed).ToString();
            }
            boostAmount_T.text = _boostAmount.ToString();

            lateJoin_T.text = _lateJoin.ToString();

            footSteps_T.text = _footSteps.ToString();
            taggerCount_T.text = (TaggerCount + 1).ToString();
            time_T.text = string.Concat(Time + 1, ":Min");

            if (gameManager)
            {
                gameManager.SetupMap();
                gameManager.ControlsSetState();
            }

            canvas.enabled = true;
        }

        public virtual void OnOwnershipTransferred(VRC.SDKBase.VRCPlayerApi player) 
        {
            if (player.isLocal)
            {
                blocker.SetActive(false);
                MasterLock = false;
                RequestSerialization();
            }
            else
            {
                blocker.SetActive(true);
            }
            editingSettings.text = string.Concat("Editing Settings: ", Networking.GetOwner(gameObject).displayName);

            masterText.text = string.Concat("GameMaster is: ", Networking.GetOwner(blocker).displayName);
        }
        public virtual void OnPlayerJoined(VRC.SDKBase.VRCPlayerApi player) { SetMaxTaggers(); masterText.text = string.Concat("Master is: ", Networking.GetOwner(blocker).displayName); }
        public virtual void OnPlayerLeft(VRC.SDKBase.VRCPlayerApi player) 
        { 
            SetMaxTaggers();
            if (Utilities.IsValid(Networking.GetOwner(blocker)))
            {
                masterText.text = string.Concat("Master is: ", Networking.GetOwner(blocker).displayName);
            }
            
        }

        public void SetMaxTaggers()
        {
            if (localuser.IsOwner(gameObject))
            {
                if (TaggerCount + 1 > (VRCPlayerApi.GetPlayerCount() / 2))
                {
                    TaggerCount = (byte)Mathf.Clamp(TaggerCount, 0, (capsuleManager.GetPlayerCountOfState(1) + capsuleManager.GetPlayerCountOfState(2)) / 2);
                }
            }
        }

        #region ui button events
        public void LockButton()
        {
            if (localuser.IsOwner(gameObject))
            {
                MasterLock = !MasterLock;
            }
            else
            {
                if(!MasterLock || localuser.isInstanceOwner || localuser.isInstanceOwner)
                {
                    //Debug.Log((!MasterLock).ToString()+ (localuser.isInstanceOwner).ToString()+ (localuser.isInstanceOwner).ToString());
                    Networking.SetOwner(localuser, gameObject);
                }
            }
        }

        public void ChangeGamemode()
        {
            if (!gameManager.GameInProgress)
            {
                if (GameMode + 1 > 3)
                {
                    GameMode = 1;
                }
                else
                {
                    ++GameMode;
                }
            }
        }

        public void TaggerDecrease()
        {
            if (!gameManager.GameInProgress)
            {
                TaggerCount = (byte)Mathf.Clamp(TaggerCount - 1, 0, (capsuleManager.GetPlayerCountOfState(1) + capsuleManager.GetPlayerCountOfState(2)) / 2);
            }
        }

        public void TaggerIncrease()
        {
            if (!gameManager.GameInProgress)
            {
                TaggerCount = (byte)Mathf.Clamp(Mathf.Clamp(TaggerCount + 1, 0, (capsuleManager.GetPlayerCountOfState(1) + capsuleManager.GetPlayerCountOfState(2)) / 2), 0, 31);
            }
        }

        public void TimerDecrease()
        {
            if (!gameManager.GameInProgress)
            {
                Time = (byte)Mathf.Clamp(Time - 1, byte.MinValue, byte.MaxValue);
            }
        }

        public void TimerIncrease()
        {
            if (!gameManager.GameInProgress)
            {
                Time = (byte)Mathf.Clamp(Time + 1, byte.MinValue, byte.MaxValue);
            }
        }

        public void SpeedModDecrease()
        {
            if (!gameManager.GameInProgress)
            {
                PlayerSpeedMod = (byte)Mathf.Clamp(PlayerSpeedMod - 1, byte.MinValue, byte.MaxValue);
            }
        }

        public void SpeedModIncrease()
        {
            if (!gameManager.GameInProgress)
            {
                PlayerSpeedMod = (byte)Mathf.Clamp(PlayerSpeedMod + 1, byte.MinValue, byte.MaxValue);
            }
        }

        public void BoostDecrease()
        {
            if (!gameManager.GameInProgress)
            {
                BoostAmount = (byte)Mathf.Clamp(BoostAmount - 1, byte.MinValue, byte.MaxValue);
            }
        }

        public void BoostIncrease()
        {
            if (!gameManager.GameInProgress)
            {
                BoostAmount = (byte)Mathf.Clamp(BoostAmount + 1, byte.MinValue, byte.MaxValue);
            }
        }

        public void SpeedIncrDecrease()
        {
            if (!gameManager.GameInProgress)
            {
                Debug.Log("speed increase");
                BoostSpeed = (byte)Mathf.Clamp(BoostSpeed - 1, byte.MinValue, byte.MaxValue);
            }
        }

        public void SpeedIncrIncrease()
        {
            if (!gameManager.GameInProgress)
            {
                Debug.Log("speed decrease");
                BoostSpeed = (byte)Mathf.Clamp(BoostSpeed + 1, byte.MinValue, byte.MaxValue);
            }
        }

        public void LateJoinToggle()
        {
            if (!gameManager.GameInProgress)
            {
                LateJoin = !LateJoin;
            }
        }

        public void MapSizeSwitch()
        {
            if (!gameManager.GameInProgress)
            {
                if (MapSize + 1 > 2)
                {
                    MapSize = 0;
                }
                else
                {
                    ++MapSize;
                }
            }
        }
        #endregion

    }
}
