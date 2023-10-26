using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Info")]
public class Information : ScriptableObject
{
    //public ValueTuple<int, int> _money = new ();
    public int money;
    public int keys;
}
