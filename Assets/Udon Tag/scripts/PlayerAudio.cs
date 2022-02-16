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
    public class PlayerAudio : UdonSharpBehaviour
    {
        #region cache references
        private VRCPlayerApi localuser;
        #endregion

        [Range(0, 24)]
        public float voiceGain = 15;
        [Range(0, 1000000)]
        public float voiceFar = 25;
        [Range(0, 1000000)]
        public float voiceNear = 0;
        [Range(0, 1000)]
        public float voiceVolumetricRadius = 0;
        public bool voiceDisableLowpass = true;

        void Start()
        {
            localuser = Networking.LocalPlayer;
        }

        public void SetNormalAudio()
        {
            localuser.SetVoiceGain(voiceGain);
            localuser.SetVoiceDistanceFar(voiceFar);
            localuser.SetVoiceDistanceNear(voiceNear);
            localuser.SetVoiceVolumetricRadius(voiceVolumetricRadius);
            localuser.SetVoiceLowpass(!voiceDisableLowpass);
        }
    }
}
