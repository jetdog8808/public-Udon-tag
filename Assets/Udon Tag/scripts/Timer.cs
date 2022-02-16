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
using System;

namespace JetDog.Tag
{
    //controls game tiem and notifies game mamager if time is up.
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Timer : UdonSharpBehaviour
    {
        #region cache references
        [HideInInspector]
        public GameSetting settings;
        [HideInInspector]
        public GameManager gameManager;
        [HideInInspector]
        public capsuleManager capsuleManager;
        [HideInInspector]
        public PlayerMovement playerMovement;
        #endregion

        [HideInInspector]
        public int gameLength;
        [HideInInspector, UdonSynced(UdonSyncMode.None)]
        public int gameStartTime;

        public TextMeshProUGUI visualTime;
        public int startSeconds = 10;

        void Start()
        {
            if (Networking.LocalPlayer.IsOwner(gameObject))
            {
                gameStartTime = 0;
                RequestSerialization();
            }

            visualTime.text = string.Empty;
        }

        private void Update()
        {
            CalculateClock();
        }

        public void CalculateClock()
        {
            if (gameStartTime == 0)
            {
                return;
            }

            int gametime = (int)Networking.GetServerTimeInSeconds() - gameStartTime;
            string temp;

            if (Mathf.Sign(gametime) == 1)// time till game is over
            {
                gametime = gameLength - gametime;
                temp = TimeSpan.FromSeconds(gametime).ToString(@"mm\:ss");

                if(Networking.IsMaster && gametime == 0) //check if timer is over
                {
                    gameManager.TimeUp();
                }
            }
            else// counting till game starts
            {
                temp = TimeSpan.FromSeconds(Mathf.Abs(gametime)).ToString(@"ss");
            }

            if (visualTime.text != temp)
            {
                visualTime.text = temp;

                if (Networking.IsMaster)
                {
                    if(gametime == gameLength)
                    {
                        playerMovement.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(playerMovement.UnFreeze));
                    }
                }

            }
        }

        public void StartTimer()
        {
            gameStartTime = (int)Networking.GetServerTimeInSeconds() + startSeconds;
            RequestSerialization();
        }

        public void EndTimer()
        {
            gameStartTime = 0;
            RequestSerialization();
        }

        public virtual void OnDeserialization() 
        {
            if (settings) // to fix a local testing bugg.
            {
                if (gameStartTime == 0)
                {
                    visualTime.text = string.Empty;
                }

                gameLength = (settings.Time + 1) * 60;
            }
        }
        public virtual void OnPostSerialization(VRC.Udon.Common.SerializationResult result) 
        {
            if (settings) // to fix a local testing bugg.
            {
                if (gameStartTime == 0)
                {
                    visualTime.text = string.Empty;
                }

                gameLength = (settings.Time + 1) * 60;
            }
            
        }

    }
}
