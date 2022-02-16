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

namespace JetDog.ObjPool.FPS
{
    public class NetPositionManager : UdonSharpBehaviour
    {

        [HideInInspector]
        public shortnetposition owned;
        [HideInInspector]
        public int lastserialization;
        public float averageDelay = .05f;
        public bool sendData = false;
        public bool lerp = true;

        void Start()
        {

        }

        public void TurnOff()
        {
            sendData = false;
        }

        public void TurnOn()
        {
            sendData = true;
            owned.serializationretry();
        }

        public void calculateserialization()
        {
            int netTime = Networking.GetServerTimeInMilliseconds() & 0x7FFFFFFF;

            float delay = Mathf.Clamp((netTime - lastserialization) / 1000f, .05f, float.MaxValue);
            averageDelay = (averageDelay + delay) / 2;
            lastserialization = netTime;
        }
    }
}
