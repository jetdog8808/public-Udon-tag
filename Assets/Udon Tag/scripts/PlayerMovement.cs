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
using UnityEngine.UI;

namespace JetDog.Tag
{
    public class PlayerMovement : UdonSharpBehaviour
    {
        #region cache references
        [HideInInspector]
        public GameSetting settings;
        [HideInInspector]
        public GameManager gameManager;
        private VRCPlayerApi localuser;
        #endregion

        //base speed
        public float speed = 4f;
        public float jump = 3f;

        private bool useBoost = false;

        public Slider gauge;
        private float gaugeamount = 1;
        private float boostamount = 1;

        private bool frozen = false;
        private Vector3 frozenPos;

        private Vector3 lastpos;
        private bool MovementChecking = false;
        public LayerMask rayhitmask;

        void Start()
        {
            localuser = Networking.LocalPlayer;
            NormalSpeed();
            SendCustomEventDelayedSeconds(nameof(MovementCheck), 1f, VRC.Udon.Common.Enums.EventTiming.Update);
        }

        public void NormalSpeed()
        {
            SetSpeed(speed);
            localuser.SetJumpImpulse(jump);
            gauge.gameObject.SetActive(false);
            MovementChecking = false;
        }

        public void Boost()
        {
            SetSpeed(speed + settings.PlayerSpeedMod + settings.BoostSpeed);
            boostamount = settings.BoostAmount;
            localuser.SetJumpImpulse(0f);
            lastpos = localuser.GetPosition();
        }

        public void GameSpeed()
        {
            SetSpeed(speed + settings.PlayerSpeedMod);
            boostamount = settings.BoostAmount;
            localuser.SetJumpImpulse(0f);
            gauge.gameObject.SetActive(true);
            MovementChecking = true;
            lastpos = localuser.GetPosition();
        }

        public void SetSpeed(float speed)
        {
            if (!Utilities.IsValid(localuser))
            {
                return;
            }

            localuser.SetWalkSpeed(speed);
            localuser.SetStrafeSpeed(speed);
            localuser.SetRunSpeed(speed);
            //localuser.SetJumpImpulse(jump);
        }

        public void MovementCheck()
        {
            if (MovementChecking)
            {
                if(Vector3.Distance(lastpos, localuser.GetPosition()) > (localuser.GetRunSpeed() + 1)) //checks if player is moving too fast
                {
                    //lastpos = localuser.GetVelocity();
                    localuser.TeleportTo((lastpos + Vector3.ClampMagnitude(localuser.GetPosition() - lastpos, localuser.GetRunSpeed())), localuser.GetRotation(), VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, true);
                    localuser.SetVelocity(Vector3.ClampMagnitude(localuser.GetPosition() - lastpos, localuser.GetRunSpeed()));
                    
                }

                RaycastHit hit;

                if (Physics.SphereCast(localuser.GetPosition() + new Vector3(0, .4f, 0), .2f, Vector3.down, out hit, 20f, rayhitmask, QueryTriggerInteraction.Ignore) && localuser.GetVelocity().y > -.5f) //checks if the player is flying or hovering in the air.
                {
                    if (hit.distance > .5f)
                    {
                        localuser.TeleportTo(hit.point, localuser.GetRotation(), VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, true);
                    }
                }

                lastpos = localuser.GetPosition();
            }

            SendCustomEventDelayedSeconds(nameof(MovementCheck), 1f, VRC.Udon.Common.Enums.EventTiming.Update);
        }

        public void Freeze()
        {
            localuser.Immobilize(true);
            frozenPos = localuser.GetPosition();
            frozen = true;

            SendCustomEventDelayedSeconds(nameof(FrozenCheck), 1f, VRC.Udon.Common.Enums.EventTiming.Update);

        }

        public void UnFreeze()
        {
            localuser.Immobilize(false);
            frozen = false;
        }

        public void FrozenCheck()
        {
            if (frozen)
            {
                Vector3 temp = localuser.GetPosition();

                if(Vector2.Distance(new Vector2(frozenPos.x, frozenPos.z), new Vector2(temp.x, temp.z)) > .5f) //if too far away from fozen position will move back.
                {
                    localuser.TeleportTo(frozenPos, localuser.GetRotation(), VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, true);
                }

                SendCustomEventDelayedSeconds(nameof(FrozenCheck), 1f, VRC.Udon.Common.Enums.EventTiming.Update);
            }
        }

        private void Update() //manages the boost guage on the hud.
        {

            if (useBoost)
            {
                gaugeamount = Mathf.Clamp(gaugeamount - Time.deltaTime, 0, boostamount);
            }
            else
            {
                if(localuser.GetVelocity().magnitude < 1f)
                {
                    gaugeamount = Mathf.Clamp(gaugeamount + (Time.deltaTime * .8f), 0, boostamount);
                }
                else
                {
                    gaugeamount = Mathf.Clamp(gaugeamount + (Time.deltaTime * .3f), 0, boostamount);
                }
                
            }

            gauge.value = Mathf.Clamp(gaugeamount / boostamount, 0, 1);

            if (gaugeamount < .0001f)
            {
                useBoost = false;
                GameSpeed();
                gaugeamount = .00011f;
            }
        }

        public virtual void InputUse(bool value, VRC.Udon.Common.UdonInputEventArgs args) 
        {

            if (!gameManager.InGame)
            {
                NormalSpeed();
                return;
            }

            if (localuser.IsUserInVR() && args.handType == VRC.Udon.Common.HandType.LEFT)
            {
                return;
            }
            
            if(value && gaugeamount < .1f)
            {
                return;
            }

            useBoost = value;

            if (value)
            {
                Boost();
            }
            else
            {
                GameSpeed();
            }
        }

    }
}
