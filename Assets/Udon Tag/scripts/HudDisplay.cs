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
using TMPro;

namespace JetDog.Tag
{
    public class HudDisplay : UdonSharpBehaviour
    {
        public TextMeshProUGUI displaytext;
        #region cache references
        [HideInInspector]
        public GameSetting settings;
        #endregion

        void Start()
        {

        }

        public void DisplayGameMode()
        {
            displaytext.color = Color.white;
            switch (settings.GameMode)
            {
                case 0:
                    displaytext.text = "Tag";
                    break;
                case 1:
                    displaytext.text = "Freeze\nTag";
                    break;
                case 2:
                    displaytext.text = "Infection\nTag";
                    break;
                case 3:
                    displaytext.text = "Elimination\nTag";
                    break;
            }

            SendCustomEventDelayedSeconds(nameof(RemoveText), 3f, VRC.Udon.Common.Enums.EventTiming.Update);
        }

        public void RemoveText()
        {
            displaytext.text = string.Empty;
        }

        public void FrozenText()
        {
            displaytext.color = Color.cyan;
            displaytext.text = "Frozen";
            SendCustomEventDelayedSeconds(nameof(RemoveText), 3f, VRC.Udon.Common.Enums.EventTiming.Update);
        }

        public void UnFrozenText()
        {
            displaytext.color = Color.green;
            displaytext.text = "UnFrozen";
            SendCustomEventDelayedSeconds(nameof(RemoveText), 3f, VRC.Udon.Common.Enums.EventTiming.Update);
        }

        public void InfectedText()
        {
            displaytext.color = Color.red;
            displaytext.text = "Infected";
            SendCustomEventDelayedSeconds(nameof(RemoveText), 3f, VRC.Udon.Common.Enums.EventTiming.Update);
        }

        public void EliminatedText()
        {
            displaytext.color = Color.red;
            displaytext.text = "Eliminated";
            SendCustomEventDelayedSeconds(nameof(RemoveText), 3f, VRC.Udon.Common.Enums.EventTiming.Update);
        }
    }
}
