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
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual), RequireComponent(typeof(Rigidbody))]
    public class shortnetposition : UdonSharpBehaviour
    {
        [UdonSynced(UdonSyncMode.None), HideInInspector]
        public short[] p = new short[3];
        /* 0=x
         * 1=y
         * 2=z
         */

        /*
        [UdonSynced(UdonSyncMode.None), HideInInspector]
        public byte b;
        */

        /* 0=rotation
         * 1=delay
         */

        public NetPositionManager manager;
        private int lastserialization;
        private Vector3 lastpos;
        private float lastrot;

        //private Vector3 velocity;
        //private float rotvelocity;

        private Transform transform;
        private Rigidbody rigidbody;
        private VRCPlayerApi localuser;
        public JetDog.ObjPool.OwnershipPoolObj ownerRef;

        public float maxVelocity;
        private bool lagging = false;


        private void OnEnable()
        {
            if (localuser != null)
            {
                //lastserialization = Networking.GetServerTimeInMilliseconds() & 0x7FFFFFFF;

                if (localuser.IsOwner(gameObject))
                {
                    if(manager.owned == null)
                    {
                        manager.owned = this;
                        SendCustomEventDelayedSeconds("serializationretry", 1f, VRC.Udon.Common.Enums.EventTiming.Update);
                    }
                                        
                }
            }
        }

        void Start()
        {
            transform = GetComponent<Transform>();
            rigidbody = GetComponent<Rigidbody>();
            localuser = Networking.LocalPlayer;
        }

        private void Update()
        {
            if(p == null)
            {
                VRCPlayerApi user = ownerRef.owner;
                if (Utilities.IsValid(user))
                {
                    rigidbody.MovePosition(user.GetPosition());
                    //transform.SetPositionAndRotation(user.GetPosition(), user.GetRotation());
                    rigidbody.velocity = Vector3.zero;
                }
            }
            else
            {
                if ((p[0] + p[1] + p[2]) == 0 || lagging)
                {
                    VRCPlayerApi user = ownerRef.owner;
                    if (Utilities.IsValid(user))
                    {
                        rigidbody.MovePosition(user.GetPosition());
                        //transform.SetPositionAndRotation(user.GetPosition(), user.GetRotation());
                        rigidbody.velocity = Vector3.zero;
                    }

                }
            }

            
            
        }

        public void DistanceCheck()
        {
            if(Vector3.Distance(lastpos, transform.position) > rigidbody.velocity.magnitude)
            {
                lagging = true;
            }
            
        }

        public virtual void OnOwnershipTransferred(VRC.SDKBase.VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                if (manager.owned == null)
                {
                    manager.owned = this;
                    SendCustomEventDelayedSeconds("serializationretry", 1f, VRC.Udon.Common.Enums.EventTiming.Update);
                }
            }
        }

        public virtual void OnPreSerialization()
        {
            if (manager.sendData)
            {
                Vector3 pos = localuser.GetPosition();
                Quaternion rot = localuser.GetRotation();
                transform.SetPositionAndRotation(pos, rot);

                p[0] = (short)Mathf.Clamp((int)(pos.x * 100), short.MinValue, short.MaxValue);
                p[1] = (short)Mathf.Clamp((int)(pos.y * 100), short.MinValue, short.MaxValue);
                p[2] = (short)Mathf.Clamp((int)(pos.z * 100), short.MinValue, short.MaxValue);

                //b = (byte)(Mathf.Clamp(rot.eulerAngles.y / 1.40625f, byte.MinValue, byte.MaxValue));
                manager.calculateserialization();
            }
            else
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;

                p[0] = 0;
                p[1] = 0;
                p[2] = 0;

                //b = 0;
                manager.calculateserialization();
            }

            
        }

        public virtual void OnPostSerialization(VRC.Udon.Common.SerializationResult result)
        {
            if (!result.success || Networking.IsClogged)
            {
                SendCustomEventDelayedSeconds("serializationretry", 1f, VRC.Udon.Common.Enums.EventTiming.Update);
                return;
            }

            if (manager.sendData)
            {
                serializationretry();
            }

        }

        public virtual void OnDeserialization()
        {
            Vector3 pos;
            if (p == null)
            {
                pos = Vector3.zero;
            }
            else
            {
                pos = new Vector3(p[0] / 100f, p[1] / 100f, p[2] / 100f);
            }
            
            //float rot = b * 1.40625f;
            //int nettime = (Networking.GetServerTimeInMilliseconds() & 0x7FFFFFFF);
            //float delay = (nettime - lastserialization) / .055f;

            //transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, rot, 0f));
            rigidbody.MovePosition(pos);
            //rigidbody.MoveRotation(Quaternion.Euler(0f, rot, 0f));

            if (manager.lerp)
            {
                rigidbody.velocity = Vector3.ClampMagnitude((pos - lastpos) / manager.averageDelay, maxVelocity);

                //Debug.Log("velocity:" + (pos - lastpos) / delay);
                SendCustomEventDelayedSeconds(nameof(DistanceCheck), 1f, VRC.Udon.Common.Enums.EventTiming.Update);
                lagging = false;
            }

            lastpos = pos;
            

            //lastserialization = nettime;


            //velocity = (pos - lastpos) / delay;
            //rotvelocity = (rot - lastrot) / delay;

        }

        public void serializationretry()
        {
            if (localuser.IsOwner(gameObject))
            {
                if (Networking.IsClogged)
                {
                    SendCustomEventDelayedSeconds("serializationretry", 1f, VRC.Udon.Common.Enums.EventTiming.Update);
                }
                else
                {
                    RequestSerialization();
                }
                
            }
        }

    }
}
