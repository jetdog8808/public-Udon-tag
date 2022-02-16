
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class speedcheck : UdonSharpBehaviour
{
    private Vector3 lastpos;
    private VRCPlayerApi ownerapi;
    private int checkcount = 0;
    private float lastdist = 0;

    public float topspeed = 4f;

    private void Start()
    {
        ownerapi = Networking.LocalPlayer;
        lastpos = ownerapi.GetPosition();

        //rigidself.velocity.magnitude
    }

    private void Update()
    {
        /*
        Vector3 head = localuser.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
        Vector3 hip = localuser.GetBonePosition(HumanBodyBones.Hips);
        head.y = 0;
        hip.y = 0;
        
        float playvelo = Vector3.Magnitude((head - hip) / Time.fixedDeltaTime);
        float distchanged = playvelo - lastdist;
        lastdist = playvelo;
        */
        Vector3 movedirection = (ownerapi.GetPosition() - lastpos) / Time.fixedDeltaTime;
        movedirection.y = 0;
        //Debug.Log("direction" + movedirection);
        float velocity = Mathf.Round(Vector3.Magnitude(movedirection) * 100f) / 100f;
        if (velocity > topspeed)
        {
            checkcount++;
            //Debug.Log("clamped direction" + Vector3.ClampMagnitude(movedirection, topspeed));
            if (checkcount > 3)
            {
                Debug.Log("player going too fast");
                Vector3 newpos = lastpos + (Vector3.ClampMagnitude(movedirection, topspeed) * Time.deltaTime);
                //ownerapi.SetVelocity(Vector3.zero);
                ownerapi.TeleportTo(newpos, ownerapi.GetRotation(), VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, true);
                
                lastpos = newpos;

            }
            else
            {
                lastpos = ownerapi.GetPosition();
            }
            Debug.Log(velocity);


        }
        else
        {
            checkcount = 0;
            lastpos = ownerapi.GetPosition();
        }

        


    }
}
