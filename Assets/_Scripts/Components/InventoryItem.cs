using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="InventoryItems")]
public class InventoryItem :ScriptableObject
{
    public InventoryItemType itemType;
    public float Amount;
}
