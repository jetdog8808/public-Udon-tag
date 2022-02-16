
using UdonSharp;
using UnityEngine;
using System;
using VRC.SDKBase;
using VRC.Udon;

public class stringsync : UdonSharpBehaviour
{
    void Start()
    {
        
        byte[] arrayOne = new byte[] { 0,   1,   2,   4,   8,  16,  32,  64, 128, 255 };
        WriteByteArray(arrayOne, "arrayOne");
    }
    
    public void WriteByteArray(byte[] bytes, string name)
    {
        const string underLine = "--------------------------------";
        
        Debug.Log(name);
        Debug.Log(underLine.Substring(0,
            Mathf.Min(name.Length, underLine.Length)));
        //Debug.Log(BitConverter.ToString(bytes));
        Debug.Log("");
    }

}
