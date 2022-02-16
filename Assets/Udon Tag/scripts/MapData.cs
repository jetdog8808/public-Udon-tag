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
    public class MapData : UdonSharpBehaviour
    {
        #region cache references
        [HideInInspector]
        public GameManager gameManager;
        [HideInInspector]
        public GameSetting settings;
        #endregion

        [Space(10f)]
        public SpawnLocation[] s_RunnerSpawns;
        public SpawnLocation[] s_TaggerSpawns;
        public GameObject s_Bounds;
        [Space(10f)]
        public SpawnLocation[] m_RunnerSpawns;
        public SpawnLocation[] m_TaggerSpawns;
        public GameObject m_Bounds;
        [Space(10f)]
        public SpawnLocation[] l_RunnerSpawns;
        public SpawnLocation[] l_TaggerSpawns;
        public GameObject l_Bounds;
        [Space(10f)]

        public RespawnBounds[] respawnBounds;

        [HideInInspector]
        public int number;

        void Start()
        {
            //respawner.enabled = false;
            TurnAllOff();
        }
        public virtual void OnPlayerTriggerEnter(VRC.SDKBase.VRCPlayerApi player) 
        {
            if (player.isLocal)
            {
                gameManager.ForceRespawn();
            }
            
        }

        public void SetAsMap()
        {
            if (!gameManager.GameInProgress)
            {
                settings.Map = (byte)number;
                settings.RequestSerialization();
            }
            
        }

        public void SetBounds()
        {
            byte size = settings.MapSize;

            s_Bounds.SetActive(size == 0);
            m_Bounds.SetActive(size == 1);
            l_Bounds.SetActive(size == 2);
        }

        public void TurnAllOff()
        {
            s_Bounds.SetActive(false);
            m_Bounds.SetActive(false);
            l_Bounds.SetActive(false);
        }

        public void RunnerSpawn()
        {
            SetBounds();

            switch (settings.MapSize)
            {
                case 0:
                    s_RunnerSpawns[Random.Range(0, s_RunnerSpawns.Length)].TeleportHere();
                    break;
                case 1:
                    m_RunnerSpawns[Random.Range(0, m_RunnerSpawns.Length)].TeleportHere();
                    break;
                case 2:
                    l_RunnerSpawns[Random.Range(0, l_RunnerSpawns.Length)].TeleportHere();
                    break;
            }
        }

        public void TaggerSpawn()
        {
            SetBounds();

            switch (settings.MapSize)
            {
                case 0:
                    s_TaggerSpawns[Random.Range(0, s_TaggerSpawns.Length)].TeleportHere();
                    break;
                case 1:
                    m_TaggerSpawns[Random.Range(0, m_TaggerSpawns.Length)].TeleportHere();
                    break;
                case 2:
                    l_TaggerSpawns[Random.Range(0, l_TaggerSpawns.Length)].TeleportHere();
                    break;
            }
        }
    }
}
