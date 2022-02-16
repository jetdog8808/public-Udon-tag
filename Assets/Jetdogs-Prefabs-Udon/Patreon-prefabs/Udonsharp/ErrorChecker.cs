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

namespace JetDog
{
    public class ErrorChecker : UdonSharpBehaviour
    {
        public UdonSharpBehaviour[] udonBehaviours;

        public float checkRate = 5f;
        private int offset = 0;
        private System.Diagnostics.Stopwatch time;
        [HideInInspector]
        public string errorString;
        public TextMeshProUGUI errortext;

        [Space(10)]
        public UdonBehaviour udonBehaviour;
        public string eventName;
        public bool networkEvent;
        public VRC.Udon.Common.Interfaces.NetworkEventTarget target;

        void Start()
        {
            time = new System.Diagnostics.Stopwatch();

            SendCustomEventDelayedSeconds(nameof(_CheckBehaviours), checkRate, VRC.Udon.Common.Enums.EventTiming.Update);

        }

        public void _CheckBehaviours()
        {
            time.Reset();
            time.Start();

            for (int i = offset; i < udonBehaviours.Length; i++)
            {
                if (udonBehaviours[i])
                {
                    if (!udonBehaviours[i].enabled)
                    {
                        Debug.Log("Might have found error");
                        errorString = string.Concat(udonBehaviours[i].name, ":|: ", udonBehaviours[i].GetUdonTypeName());
                        errortext.text = errorString;
                        _SendEvent();
                        return;
                    }
                }


                if (time.ElapsedMilliseconds >= 3)
                {
                    offset = i++;
                    _WaitFrame();
                    return;
                }

            }

            if (offset != 0)
            {
                offset = 0;
            }

            time.Stop();
            Debug.Log("resetting time");
            SendCustomEventDelayedSeconds(nameof(_CheckBehaviours), checkRate, VRC.Udon.Common.Enums.EventTiming.Update);

        }

        public void _WaitFrame()
        {
            Debug.Log("Waiting a frame:" + time.ElapsedMilliseconds);
            time.Stop();
            SendCustomEventDelayedFrames(nameof(_CheckBehaviours), 1, VRC.Udon.Common.Enums.EventTiming.Update);
        }

        public void _SendEvent() //the event sending out the udonbehaviour event.
        {
            if (udonBehaviour)
            {
                if (networkEvent)
                {
                    udonBehaviour.SendCustomNetworkEvent(target, eventName);
                }
                else
                {
                    udonBehaviour.SendCustomEvent(eventName);
                }
            }

        }
    }
}
