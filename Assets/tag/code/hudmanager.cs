
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class hudmanager : UdonSharpBehaviour
{
    public Camera camera;
    public Text _fadetext;
    public Image _fadeimage;
    public Sprite s_taggerswin;
    public Sprite s_runnerswin;
    public float fademult;

    private bool texttimeron = false;
    private float texttimer;
    private float textfadetime;

    private bool imagetimeron = false;
    private float imagetimer;
    private float imagefadetime;

    void Start()
    {
        fadetext("Welcome to freeze tag\n" + Networking.LocalPlayer.displayName, 4f, Color.black);

        fadeimageclear();
    }

    private void Update()
    {
        if (texttimeron)
        {
            if (texttimer > 0)
            {
                texttimer -= Time.deltaTime;
            }
            else
            {
                textfadetime -= Time.deltaTime * fademult;
                if (textfadetime > 0)
                {
                    Color tempcolor = _fadetext.color;
                    tempcolor.a = textfadetime;
                    _fadetext.color = tempcolor;
                }
                else
                {
                    Color tempcolor = _fadetext.color;
                    tempcolor.a = 0f;
                    _fadetext.color = tempcolor;
                    texttimeron = false;
                }
                
            }
        }

        if (imagetimeron)
        {
            if (imagetimer > 0)
            {
                imagetimer -= Time.deltaTime;
            }
            else
            {
                imagefadetime -= Time.deltaTime * fademult;
                if (imagefadetime > 0)
                {
                    Color tempcolor = _fadeimage.color;
                    tempcolor.a = imagefadetime;
                    _fadeimage.color = tempcolor;
                }
                else
                {
                    Color tempcolor = _fadeimage.color;
                    tempcolor.a = 0f;
                    _fadeimage.color = tempcolor;
                    imagetimeron = false;
                }

            }
        }

    }

    public void fadetext(string text, float displaytime, Color color)
    {
        _fadetext.text = text;
        color.a = 1f;
        _fadetext.color = color;
        texttimer = displaytime;
        texttimeron = true;
        textfadetime = 1f;
    }

    public void fadetextclear()
    {
        _fadetext.color = new Color(1, 1, 1, 0); ;
        texttimeron = false;
    }

    public void fadeimage(Sprite image, float displaytime)
    {
        _fadeimage.sprite = image;
        _fadeimage.color = Color.white;
        imagetimer = displaytime;
        imagetimeron = true;
        imagefadetime = 1f;
    }

    public void fadeimageclear()
    {
        _fadeimage.color = new Color(1, 1, 1, 0);
        imagetimeron = false;
    }
    
    public void taggerswin()
    {
        fadeimage(s_taggerswin, 3f);
    }

    public void runnerswin()
    {
        fadeimage(s_runnerswin, 3f);
    }

    public void youtagger()
    {
        fadetext("You are a Tagger\nGet all the Runners", 2f, Color.red);
    }

    public void yourunner()
    {
        fadetext("You are a Runner\nAvoid all Taggers", 2f, Color.green);
    }

    public void frozen()
    {
        fadetext("You have been Frozen", 1f, Color.white);
    }

    public void refrozen()
    {
        fadetext("You have been ReFrozen", 1f, Color.white);
    }

    public void unfrozen()
    {
        fadetext("You have been UnFrozen", 1f, Color.green);
    }


}
