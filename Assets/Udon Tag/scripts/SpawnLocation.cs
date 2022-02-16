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
    public class SpawnLocation : UdonSharpBehaviour
    {
        #region cache references
        private VRCPlayerApi localuser;
        [HideInInspector]
        public BoxCollider zone;
        #endregion

        public bool randomRotation = false;
        private Vector3 pos;
        private Quaternion rotation;
        private Vector3 offsets;

        void Start()
        {
            localuser = Networking.LocalPlayer;
            zone = GetComponent<BoxCollider>();

            if (zone)
            {
                zone.enabled = enabled;
                offsets = zone.bounds.extents;
                zone.isTrigger = true;
                zone.enabled = false;
            }
            
            pos = transform.position;
            rotation = transform.rotation;
        }

        public void TeleportHere()
        {
            if (randomRotation)
            {
                rotation = Random.rotation;
            }

            if (zone)
            {
                pos = transform.position;
                pos.x += Random.Range(-offsets.x, offsets.x);
                pos.z += Random.Range(-offsets.z, offsets.z);

                localuser.TeleportTo(pos, rotation, VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, false);
            }
            else
            {
                localuser.TeleportTo(pos, rotation, VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, false);
            }
        }
    }
}
