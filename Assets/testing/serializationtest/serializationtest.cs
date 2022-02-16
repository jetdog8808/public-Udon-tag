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

namespace JetDog
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class serializationtest : UdonSharpBehaviour
    {
        [UdonSynced(UdonSyncMode.None)]
        public bool syncval;
        public GameObject visualoutput;
        public bool autodissable;

        void Start()
        {
            gameObject.SetActive(!autodissable);
        }

        public void DoSerialization()
        {
            RequestSerialization();
        }

        public virtual void OnPreSerialization() 
        {
            visualoutput.SetActive(!visualoutput.activeSelf);
            Debug.Log("preserialization called");
        }
        public virtual void OnDeserialization() 
        {
            visualoutput.SetActive(!visualoutput.activeSelf);
            Debug.Log("deserialization called");
        }
        public virtual void OnPostSerialization(VRC.Udon.Common.SerializationResult result) 
        {
            visualoutput.SetActive(!visualoutput.activeSelf);
            Debug.Log("postserialization called");
        }

    }
}
