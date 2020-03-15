using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour {

    public List<GameObject> itemsPrefabs; // On récupère les items !
    public List<int> nbItems;

    protected GameManager gm;
    protected GameObject itemsFolder;
    protected List<Item> items; // Tous les items

    public virtual void Initialize() {
        gm = GameManager.Instance;
        itemsFolder = new GameObject("Items");
        items = new List<Item>();

        GenerateItems();
    }

    protected void GenerateItems() {
        for(int i = 0; i < itemsPrefabs.Count; i++) {
            GameObject itemPrefab = itemsPrefabs[i];
            int nbEnnemi = nbItems[i];
            for (int j = 0; j < nbEnnemi; j++) {
                PopItem(itemPrefab);
            }
        }
    }

    public virtual void PopItem(GameObject itemPrefab) {
        Vector3 pos = gm.map.GetFreeRoundedLocation();
        GenerateItemFromPrefab(itemPrefab, pos);
    }

    public Item GenerateItemFromPrefab(GameObject itemPrefab, Vector3 pos) {
        Item item = Instantiate(itemPrefab, pos, Quaternion.identity, itemsFolder.transform).GetComponent<Item>();
        item.SetPrefab(itemPrefab);
        items.Add(item);

        // Ca pour le moment c'est en standby ! :)
        gm.historyManager.AddItemHistory(item, itemPrefab);

        return item;
    }

    public List<Item> GetItems() {
        return items;
    }

    public List<Item> GetItemsType(Type itemType) {
        List<Item> res = new List<Item>();
        foreach(Item item in items) {
            if(item.GetType().IsAssignableFrom(itemType)) {
                res.Add(item);
            }
        }
        return res;
    }

    public void RemoveItem(Item item) {
        items.Remove(item);
    }
}
