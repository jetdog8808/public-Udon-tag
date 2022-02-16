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

namespace JetDog.ObjPool
{
    public class OwnershipPool : UdonSharpBehaviour
    {
        public VRCObjectPool objectPool;
        public Transform group;
        private VRCPlayerApi user;

        [UdonSynced(UdonSyncMode.None)]
        private bool netobj = false;
        [HideInInspector]
        public GameObject ownedObj;

        void Start()
        {
            Debug.Log("pool starting:" + Time.frameCount);
            user = Networking.LocalPlayer;

            SendCustomEventDelayedSeconds(nameof(SetupPool), 1f, VRC.Udon.Common.Enums.EventTiming.LateUpdate);


            //Debug.Log(string.Concat("start is obj ready?", Networking.IsObjectReady(gameObject)));
        }

        public void SetupPool()
        {
            if (!objectPool.gameObject.activeSelf)
            {
                GameObject[] objarray = new GameObject[transform.childCount];

                for (int i = 0; i < objarray.Length; i++)
                {
                    objarray[i] = transform.GetChild(i).gameObject;
                }

                objectPool.Pool = objarray;
                objarray = null;
                objectPool.gameObject.SetActive(true);
            }

            if (user.isMaster)
            {
                SendCustomEventDelayedSeconds(nameof(Ownersetup), 1f, VRC.Udon.Common.Enums.EventTiming.LateUpdate);
            }
        }

        public void Ownersetup()
        {
            if (user.isMaster)
            {
                GetObject(user);
                netobj = true;
                RequestSerialization();

            }
        }

        public virtual void OnOwnershipTransferred(VRC.SDKBase.VRCPlayerApi player)
        {
            if (player.isLocal && player.isMaster)
            {
                if (!ownedObj)
                {
                    GetObject(user);
                }
                if (!netobj)
                {
                    netobj = true;
                    RequestSerialization();
                }
            }
        }

        public virtual bool OnOwnershipRequest(VRC.SDKBase.VRCPlayerApi requestingPlayer, VRC.SDKBase.VRCPlayerApi requestedOwner)
        {
            if (user.isMaster)
            {
                GetObject(requestingPlayer);

                return false;
            }
            else
            {
                return true;
            }

        }

        public virtual void OnDeserialization()
        {
            if (netobj)
            {
                //Debug.Log("requestion from master");
                SendCustomEventDelayedSeconds(nameof(NetReady), 2, VRC.Udon.Common.Enums.EventTiming.Update);

            }

        }

        public void GetObject(VRCPlayerApi player)
        {
            //Debug.Log(string.Concat("getting object for:", player.playerId));
            GameObject spawnedobj = objectPool.TryToSpawn();
            if (spawnedobj)
            {
                //Debug.Log("spawned obj");

                if (player.isMaster)
                {

                    ownedObj = spawnedobj;
                }
                else
                {
                    GameObject[] gArray = spawnedobj.GetComponent<OwnershipPoolObj>().takeownership;

                    Networking.SetOwner(player, spawnedobj);

                    foreach (GameObject obj in gArray)
                    {
                        Networking.SetOwner(player, obj);
                    }
                }

            }
            else
            {
                //Debug.Log("nothing spawned");
            }

        }

        public void ReturnObject(GameObject poolobj)
        {
            objectPool.Return(poolobj);
        }

        public void NetReady()
        {
            if (ownedObj)
            {
                return;
            }
            else
            {
                Networking.SetOwner(user, gameObject);
                SendCustomEventDelayedSeconds(nameof(NetReady), 2, VRC.Udon.Common.Enums.EventTiming.Update);
            }
        }
    }
}

