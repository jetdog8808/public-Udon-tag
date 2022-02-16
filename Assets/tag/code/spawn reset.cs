
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


//bounds checking and resetting players when entered.

public class spawnreset : UdonSharpBehaviour
{
    [HideInInspector]
    public bool Ustatus = false;  // status for error checker
    [HideInInspector]
    public string errortext = "init text";  // status for error checker

    public bool isbound = false;  //if player should be teleported
    public gamecontroller gamecontrol;  //reference to game controler

    /*
    private void OnTriggerExit(Collider other)
    {
        Debug.Log("obj has exited");
        if (other != null)
        {
            Debug.Log("someone has left spawn");
            tagplayercontroler player = other.GetComponent<tagplayercontroler>();

            if (player != null)
            {
                Debug.Log("remove exit from game");
                player.ingame = false;

            }


        }
    }
    */
    private void OnTriggerEnter(Collider other) //resests user when they enter collider
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 1";

        if (other != null)  //if it gets the player it will be null
        {
            errortext = "obj name:" + gameObject.name + "\n" + "Error 2";
            Debug.Log("player detected");
            tagplayercontroler player = other.GetComponent<tagplayercontroler>();
            errortext = "obj name:" + gameObject.name + "\n" + "Error 3";
            if (player != null) //checks if obj has correct script
            {
                errortext = "obj name:" + gameObject.name + "\n" + "Error 4";
                if (player.ownerapi != null && Networking.IsOwner(player.gameObject))
                {
                    errortext = "obj name:" + gameObject.name + "\n" + "Error 5";
                    player.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "nonuseset");
                    if (isbound)
                    {
                        errortext = "obj name:" + gameObject.name + "\n" + "Error 6";
                        Debug.Log("bounds respawner");
                        player.ownerapi.TeleportTo(gamecontrol.respawnpoint.position, gamecontrol.respawnpoint.rotation, VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, false);
                    }
                    errortext = "obj name:" + gameObject.name + "\n" + "Error 7";
                    gamecontrol.controllist.ownedcontroler.frozepoint = gamecontrol.respawnpoint.position;

                }

            }

        }
    }

    private void OnTriggerStay(Collider other)  //redundent setter to make sure the player gets set correctly.
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 8";

        if (other != null)
        {
            errortext = "obj name:" + gameObject.name + "\n" + "Error 9";
            tagplayercontroler player = other.GetComponent<tagplayercontroler>();
            errortext = "obj name:" + gameObject.name + "\n" + "Error 10";
            if (player != null)
            {
                errortext = "obj name:" + gameObject.name + "\n" + "Error 11";
                player.nonuseset();
            }
        }
    }

    private void LateUpdate()
    {
        Ustatus = true;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 12";
    }
}
