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
    public class NetTagMessage : UdonSharpBehaviour
    {
        public capsuleManager capsulemanager;
        [HideInInspector, UdonSynced(UdonSyncMode.None)]
        public byte[] userid;
        [HideInInspector]
        public bool clear = true;

        void Start()
        {
            clear = true;
        }

        public virtual void OnDeserialization() 
        { 
            if(userid == null)
            {
                //Debug.Log("number was null");
                return;
            }

            int id;

            switch (userid.Length)
            {
                case 1:
                    id = userid[0];
                    break;
                case 2:
                    id = (int)(((uint)userid[0] << 8) | userid[1]);
                    break;
                case 3:
                    id = (int)(((uint)userid[0] << 16) | ((uint)userid[1] << 8) | userid[2]);
                    break;
                case 4:
                    id = (int)(((uint)userid[0] << 24) | ((uint)userid[1] << 16) | ((uint)userid[2] << 8) | userid[3]);
                    break;
                default: 
                    return;
            }

            if (!Utilities.IsValid(VRCPlayerApi.GetPlayerById(id)))
            {
                //Debug.Log("number was not valid");
                return;
            }

            if(VRCPlayerApi.GetPlayerById(id).isLocal)
            {
                //Debug.Log("was local id");
                capsulemanager.Tagged(Networking.GetOwner(gameObject));
            }/*
            else
            {
                Debug.Log("was not local id");
            }*/
            
        }

        public virtual void OnPostSerialization(VRC.Udon.Common.SerializationResult result) 
        {
            //Debug.Log("post send clear");
            clear = true;
        }
    }
}
