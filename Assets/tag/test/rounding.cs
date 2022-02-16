
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;


namespace test
{

    public class rounding : UdonSharpBehaviour
    {

        public Text pingdisplay;
        public float ping;
        private bool pingfired;
        private bool gottime;


        void Start()
        {

            pingfired = false;
            gottime = false;

        }

        void Update()
        {
            Debug.Log("ping:" + ping + "|" + "pingfired:" + pingfired + "|" + "gottime:" + gottime + "|" + "time==5" + (Networking.GetNetworkDateTime().Second % 5 == 0));

            if (!pingfired && Networking.GetNetworkDateTime().Second % 5 == 0)
            {
                Debug.Log("start ping");
                ping = 0f;
                pingfired = true;
                gottime = false;
                //netping();
                if (Networking.IsMaster)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "netping");
                }

            }
            else if (pingfired && !gottime)
            {
                Debug.Log("add time to ping");
                ping += Time.deltaTime;
                //Debug.Log(ping);
                pingdisplay.text = ping.ToString();
            }

            if (pingfired && Networking.GetNetworkDateTime().Second % 5 == 3)
            {
                Debug.Log("5==3");

                //netping();
                pingfired = false;
            }

        }

        public void netping()
        {
            Debug.Log("gotping");
            gottime = true;
            //Debug.Log(ping.ToString());
            pingdisplay.text = ping.ToString();
        }
    }

}
