
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


//information and events for each map.

public class tagmap : UdonSharpBehaviour
{
    [HideInInspector]
    public bool Ustatus = false;  // status for error checker
    [HideInInspector]
    public string errortext = "init text";  // status for error checker

    public Transform[] runnerspawns; //where runners should spawn
    public Transform[] taggerspawns; //where taggers should spawn
    public BoxCollider removebox;  //resetting collider. should cover whole map
    public gamecontroller gamecontrol; //reference to game controler
    public Camera cam;  //camera to spectate map.


    void Start()
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 1";

        if (removebox == null)//makes sure object has a collider
        {
            errortext = "obj name:" + gameObject.name + "\n" + "Error 2";
            removebox = gameObject.GetComponent<BoxCollider>();
            removebox.enabled = false; //and makes sure its false at start
        }
        //cam.enabled = false;
    }

    private void OnTriggerStay(Collider other) // respawns players who are still in the map when they shouldnt be.
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 3";

        if (other != null)  //if it gets the player it will be null
        {
            errortext = "obj name:" + gameObject.name + "\n" + "Error 4";
            tagplayercontroler player = other.GetComponent<tagplayercontroler>();
            if (player != null)
            {
                errortext = "obj name:" + gameObject.name + "\n" + "Error 5";
                bool owner = Networking.IsOwner(player.gameObject); //if its you localy teleport yourself.
                errortext = "obj name:" + gameObject.name + "\n" + "Error 6";
                if (owner)
                {
                    errortext = "obj name:" + gameObject.name + "\n" + "Error 7";
                    player.ownerapi.TeleportTo(gamecontrol.respawnpoint.position, gamecontrol.respawnpoint.rotation, VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, false);
                }
            }



        }
    }

    private void LateUpdate()
    {
        Ustatus = true;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 8";
    }

    public void respawnplayers() //event for when to turn on respawning collider
    {
        removebox.enabled = true;
    }

    public void removerespawn()  //removes collider
    {
        removebox.enabled = false;
    }
}
