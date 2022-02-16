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

namespace JetDog.ObjPool
{
    public class OwnershipPoolObj : UdonSharpBehaviour
    {
        private OwnershipPool pool;
        public GameObject[] takeownership;
        [HideInInspector]
        public VRCPlayerApi owner;

        private void Start()
        {
            Debug.Log("obj start:" + Time.frameCount);
            pool = GetComponentInParent<OwnershipPool>();
        }

        public virtual void OnOwnershipTransferred(VRC.SDKBase.VRCPlayerApi player)
        {
            Debug.Log("onownershiptransferred:" + gameObject.name);
            if (player.isLocal)
            {
                if (player.isMaster)
                {
                    pool.ReturnObject(gameObject);
                }
                else
                {
                    Debug.Log("onownershiptransferred is for local:" + gameObject.name);
                    if (pool.ownedObj)
                    {
                        Networking.SetOwner(Networking.GetOwner(pool.gameObject), gameObject);
                    }
                    else
                    {
                        pool.ownedObj = gameObject;
                    }

                }

            }
            if (!player.isMaster)
            {
                owner = Networking.GetOwner(gameObject);
            }

        }

        private void OnEnable()
        {
            owner = Networking.GetOwner(gameObject);
        }

        private void OnDisable()
        {
            owner = null;
        }

    }
}

