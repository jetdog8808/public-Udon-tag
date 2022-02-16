
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class pingvisual : UdonSharpBehaviour
{
    public controlerdeligator controllist;
    public Text visual;
    public Text secvisual;

    private void Update()
    {
        string stringtwo = "id:" + controllist.id + "|| your id: " + controllist.localuser.playerId + "|| oid:" + controllist.oid + "\n";
        string stringone = (controllist.gamecontrol.gameclock.seconds + "\n");
        //visual.text = stringtwo;
        



        for (int i = 0; i < controllist.controlers.Length; i++)
        {
            if(controllist.controlers[i].ownerapi != null && controllist.ownedcontroler != null)
            {
                stringone += (controllist.controlers[i].ownerapi.displayName + ":" + controllist.controlers[i].ping + "|" + controllist.controlers[i].pingcalc + "\n");
                stringtwo += (controllist.controlers[i].ownerapi.displayName + ":" + controllist.ownedcontroler.tagtime[i] + "\n");
            }

        }


        secvisual.text = stringtwo;
        visual.text = stringone;

    }
}
