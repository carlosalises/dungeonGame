using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Items")]
public class Items : ScriptableObject
{
    public string _itemName;
    public int _prize;
    public int _quantity;
    public bool _canBuy;
}
