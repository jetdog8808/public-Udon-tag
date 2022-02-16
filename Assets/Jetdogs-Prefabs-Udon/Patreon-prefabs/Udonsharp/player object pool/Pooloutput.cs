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
using VRC.SDK3.Components;
using VRC.Udon;
using TMPro;

namespace JetDog.ObjPool
{
    public class Pooloutput : UdonSharpBehaviour
    {
        public VRCObjectPool poolref;
        private GameObject[] poolarray;
        public TextMeshPro outputdisplay;

        private void OnEnable()
        {
            if (poolarray != null)
            {
                SendCustomEventDelayedSeconds(nameof(output), 1f, VRC.Udon.Common.Enums.EventTiming.Update);
            }
        }
        void Start()
        {
            SendCustomEventDelayedSeconds(nameof(getpool), 2f, VRC.Udon.Common.Enums.EventTiming.LateUpdate);
        }

        public void getpool()
        {
            poolarray = poolref.Pool;
            SendCustomEventDelayedSeconds(nameof(output), 1f, VRC.Udon.Common.Enums.EventTiming.Update);
        }

        public void output()
        {
            if (gameObject.activeInHierarchy)
            {
                string tempstring = string.Empty;
                foreach (GameObject gobj in poolarray)
                {
                    if (gobj.activeInHierarchy && Utilities.IsValid(Networking.GetOwner(gobj)))
                    {

                        tempstring = string.Concat(tempstring, "\n", gobj.name, ":|:", Networking.GetOwner(gobj).displayName);
                    }
                }

                outputdisplay.text = tempstring;
                SendCustomEventDelayedSeconds("output", 1f, VRC.Udon.Common.Enums.EventTiming.Update);
            }
        }
    }
}
