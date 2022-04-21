using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    private static ResourceManager instance;

    public static ResourceManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindObjectOfType<ResourceManager>();
            }

            return instance;
        }
    }

    public Dictionary<InventoryItemType, InventoryItem> InventoryItem = new Dictionary<InventoryItemType, InventoryItem>();

    private void Awake()
    {
        InventoryItem[] items = Resources.LoadAll<InventoryItem>("ScriptableObjects");

        foreach(InventoryItem i in items)
        {
            if(!InventoryItem.ContainsKey(i.itemType))
            {
                i.Amount = 0; // resetting value..
                InventoryItem.Add(i.itemType, i);
            }
        }
    }
}
