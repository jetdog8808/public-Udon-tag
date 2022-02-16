
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class tagmod : UdonSharpBehaviour
{
    [SerializeField]
    private string name;
    private bool can;
    public Transform respawn;
    public GameObject error;
    public GameObject blocker;
    public Camera dcamera;


    private void Start()
    {
        if (Networking.LocalPlayer.displayName == name)
        {
            can = true;
        }
        else
        {
            can = false;
        }
    }

    private void Update()
    {
        if (can)
        {
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "respawnhere");
            }

            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "closedoor");
            }

            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "opendoor");
            }

            if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                dcamera.gameObject.SetActive(!dcamera.gameObject.activeSelf);
                dcamera.enabled = true;
            }

            if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "block");
            }

            if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "unblock");
            }

            if (Input.GetKeyDown(KeyCode.Keypad6))
            {

            }

            if (Input.GetKeyDown(KeyCode.Keypad7))
            {

            }

            if (Input.GetKeyDown(KeyCode.Keypad8))
            {

            }

            if (Input.GetKeyDown(KeyCode.Keypad9))
            {

            }
        }


    }

    public void respawnhere()
    {
        Networking.LocalPlayer.TeleportTo(respawn.position, respawn.rotation, VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, false);
    }

    public void closedoor()
    {
        error.SetActive(true);
    }

    public void opendoor()
    {
        error.SetActive(false);
    }

    public void block()
    {
        blocker.SetActive(true);
    }

    public void unblock()
    {
        blocker.SetActive(false);
    }
}
