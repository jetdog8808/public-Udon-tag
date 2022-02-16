
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class damagetesting : UdonSharpBehaviour
{
    public VRCPlayerApi localuser;
    public Text visual;
    public GameObject damagevis;
    public GameObject damagenon;
    public bool deathrespawn;
    public float timer;
    public Transform location;


    void Start()
    {
        localuser = Networking.LocalPlayer;

    }
    
    private void Update()
    {
        string texx = "";
        texx += localuser.CombatGetCurrentHitpoints();
        visual.text = texx;
    }
    
    public void setup()
    {
        localuser.CombatSetup();/*
        localuser.CombatSetRespawn(true, timer, location);
        localuser.CombatSetDamageGraphic(damagevis);
        localuser.CombatSetMaxHitpoints(100f);
        localuser.CombatSetCurrentHitpoints(100f);*/
    }

    public void setmaxhealth()
    {
        localuser.CombatSetup();
        localuser.CombatSetMaxHitpoints(200f);
        localuser.CombatSetCurrentHitpoints(200f);
    }

    public void setcurrenthit()
    {
        //localuser.CombatSetup();
        localuser.CombatSetCurrentHitpoints(localuser.CombatGetCurrentHitpoints() - 10f);
    }

    public void damagegraph()
    {
        localuser.CombatSetup();
        localuser.CombatSetDamageGraphic(damagevis);
    }

    public void damagenonn()
    {
        localuser.CombatSetup();
        localuser.CombatSetDamageGraphic(damagenon);
    }

    public void respawner()
    {
        localuser.CombatSetup();
        localuser.CombatSetRespawn(deathrespawn, timer, location);
    }
  


}
