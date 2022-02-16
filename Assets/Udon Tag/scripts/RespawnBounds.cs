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
    public class RespawnBounds : UdonSharpBehaviour
    {
        #region cache references
        public GameManager gameManager;
        #endregion

        void Start()
        {

        }

        public virtual void OnPlayerTriggerEnter(VRC.SDKBase.VRCPlayerApi player) 
        {
            if (player.isLocal)
            {
                gameManager.OutOfBounds();
            }
            
        }
        public virtual void OnPlayerTriggerStay(VRC.SDKBase.VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                gameManager.OutOfBounds();
            }

        }
    }
}
