
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class grabbehaviour : UdonSharpBehaviour
{

    private float pingcalc;
    public float ping;
    private bool pingfired = false;
    private bool gotsent = false;
    private int firesent = 0;
    private int firegot = 0;
    private int firelook = 0;

    public bool isLocal;


    private void Start()
    {
        isLocal = true;
    }

    private void Update()
    {
        if (pingfired)
        {
            pingcalc += Time.deltaTime;
        }

        if (!gotsent && Networking.GetNetworkDateTime().Second % 2 == 0)
        {
            if (isLocal)
            {
                netping();
                Debug.Log("netping sent");
            }
            else
            {
                firesent++;

                if (!pingfired)
                {
                    pingcalc = 0f;
                    pingfired = true;
                    firelook = firesent;
                }
            }
            gotsent = true;
        }
        else if (Networking.GetNetworkDateTime().Second % 2 != 0)
        {
            gotsent = false;
        }
    }

    public void netping()
    {
        if (isLocal)
        {
            firegot++;

            if (firegot == firelook)
            {
                pingfired = false;
                pingcalc = ping;
                //Debug.Log(ownerapi.displayName + "ping:" + ping);

            }
        }
        //visout.text = (ping + "\n" + pingcalc);
    }

}
