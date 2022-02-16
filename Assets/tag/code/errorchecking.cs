
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;


//this checks if any of the udon scripts have errored or broke. useful to make sure if anything has boken the player wont play the game broken.
//since there is not a way to check this properly i used the idea that we can get variables even if the script errors.
//in each script is a Ustatus bool before every event it will be set to false. then in late update after everything is finished it will be set to true to indicate it has successfuly finished everythin.
//onwillrenderobject event will happen after late update so i check through an array of all the scripts to check the ustatus. if true its working if false something broke.


public class errorchecking : UdonSharpBehaviour
{
    public UdonSharpBehaviour[] checkthese; //array of all scripts to check.
    public float checkrate; //how often to check
    private float timer; //how much time left till next check.
    private bool shouldcheck; //when to run the check.
    public GameObject visualerror; // visual object that gets enabled when a error comes up.
    public Text errorvisualtext; // output for if there is a error.
    private VRCPlayerApi localuser; //local players api for teleport.
    public Transform respawn; //where to teleport the player when something breaks.
    private bool beentriped = false; //turns true when something broke. to stop constant checking.

    private void Start()
    {
        //errorvisualtext.text = "";
        timer = checkrate;
        shouldcheck = false;
        visualerror.SetActive(false);
        localuser = Networking.LocalPlayer;
    }

    private void Update()
    {
        if (0 > timer && !shouldcheck) //timer for how often to check.
        {
            shouldcheck = true;
            timer = checkrate;

        }
        else if (!shouldcheck)
        {
            timer -= Time.deltaTime;
        }
    }

    private void OnWillRenderObject()
    {
        if (shouldcheck && !beentriped) //if its time for the check
        {
            //Debug.Log("error checking");

            //bool ustatus;
            for (int i = 0; i < checkthese.Length; i++)
            {
                //ustatus = (bool)checkthese[i].GetProgramVariable("Ustatus");
                if (!(bool)checkthese[i].GetProgramVariable("Ustatus"))
                {
                    errorvisualtext.text = errorvisualtext.text + (string)checkthese[i].GetProgramVariable("errortext") + "\n";
                    if (!beentriped)
                    {
                        if (localuser.isMaster)
                        {
                            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "masterbroke");

                        }
                        masterbroke();
                    }

                }

            }

            shouldcheck = false;
        }


    }

    public void masterbroke()
    {
        //Debug.Log("error was found");
        visualerror.SetActive(true);
        localuser.TeleportTo(respawn.position, respawn.rotation, VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, false);
        beentriped = true;
    }

}



//errortext = "obj name:" + gameObject.name + "\n" + "Error "; //error message if something boke here