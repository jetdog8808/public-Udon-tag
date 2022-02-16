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

using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using TMPro;

namespace JetDog.Tag
{
    public class ErrorSystem : UdonSharpBehaviour
    {
        public byte versionNum;
        [UdonSynced(UdonSyncMode.None)]
        private byte syncnum;
        private VRCPlayerApi localuser;
        public float teleportRadius = 3;
        public SpawnLocation teleportLocation;
        public TextMeshPro errorText;


        void Start()
        {
            localuser = Networking.LocalPlayer;
            if (localuser.isMaster)
            {
                syncnum = versionNum;
                RequestSerialization();
            }
            else
            {
                SendCustomEventDelayedSeconds(nameof(checkNum), 4f, VRC.Udon.Common.Enums.EventTiming.Update);
            }

        }
        /*
        public virtual void OnDeserialization() 
        { 
            if(syncnum != versionNum)
            {
                checkNum();
            }
        }*/

        public void checkNum()
        {
            if (syncnum != versionNum)
            {
                errorText.text = "Master is not on the same build create a new instance.";
                StayTeleport();
            }
        }

        public void ErrorDetected()
        {
            errorText.text = "An Error occurred. Rejoin the isntance to try and fix this issue. If this issue still persists try creating a new instance.";
            StayTeleport();
        }

        public void WrongState()
        {
            errorText.text = "An Error was detected. Master says you are in the wrong state. Please rejoin the instance.";
            StayTeleport();
        }

        public void StayTeleport()
        {
            if(Vector3.Distance(localuser.GetPosition(), transform.position) > teleportRadius)
            {
                teleportLocation.TeleportHere();
            }

            SendCustomEventDelayedSeconds(nameof(StayTeleport), 1f, VRC.Udon.Common.Enums.EventTiming.Update);
        }
    }
}
