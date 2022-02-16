
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;


//clock counter for game. sends events each second checking game state and when time is over.

public class gametimer : UdonSharpBehaviour
{
    [HideInInspector]
    public bool Ustatus = false;  // status for error checker
    [HideInInspector]
    public string errortext = "init text";  // status for error checker

    public bool timeon = false;  //timer is on
    public float seconds = 0;  //how many seconds are left
    public Text visualtext;   //visual of seconds left
    private int last;  //last rounded time
    public gamecontroller controler;  //reference to gamecontroler for sending event.

    private void Start()
    {
        visualtext.text = "";
    }

    public void starttimer(float time)  //set timer for amout of seconds.
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 1";

        last = (int)time;
        seconds = time;
        timeon = true;
        visualtext.text = last.ToString();
    }

    private void Update()
    {
        Ustatus = false;  // status for error checker
        errortext = "obj name:" + gameObject.name + "\n" + "Error 2";

        if (timeon) //if timer is on
        {
            errortext = "obj name:" + gameObject.name + "\n" + "Error 3";
            if (0 > seconds) //what to do when time is up
            {
                errortext = "obj name:" + gameObject.name + "\n" + "Error 4";
                timeon = false;
                controler.timeup();  //send event to game controle time is up.
                visualtext.text = "0";
                return;
            }
            else
            {
                seconds -= Time.deltaTime;
            }


            if (last != (int)seconds) //send event to gamecontroler each second to check game conditions, and update visual text
            {
                errortext = "obj name:" + gameObject.name + "\n" + "Error 5";
                last = (int)seconds;
                visualtext.text = last.ToString();
                controler.endingconditions();
                if(last == 299 && controler.controllist.ownedcontroler != null && controler.controllist.ownedcontroler.ingame)
                {
                    foreach (taghands hand in controler.controllist.ownedcontroler.hands)//sets up hand colliders.
                    {
                        //errortext = "obj name:" + gameObject.name + "\n" + "Error 34";
                        hand.visual.enabled = true;
                        hand.sphcollider.enabled = true;
                    }
                    

                }
            }


        }
        else if(visualtext.text != "")
        {
            visualtext.text = "";
        }
    }

    private void LateUpdate()
    {
        Ustatus = true;  // status for error checker
    }
}
