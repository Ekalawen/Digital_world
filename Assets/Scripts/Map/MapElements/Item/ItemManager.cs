using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ItemManager : MonoBehaviour {

    public List<GameObject> itemsPrefabs; // On récupère les items !
    public List<int> nbItems;

    protected GameManager gm;
    protected GameObject itemsFolder;
    protected List<Item> items; // Tous les items
    protected List<OrbTrigger> orbTriggers = new List<OrbTrigger>(); // Toutes les OrbTriggers ! :3
    [HideInInspector]
    public UnityEvent<OrbTrigger> onOrbTriggerHacked;

    public virtual void Initialize() {
        gm = GameManager.Instance;
        itemsFolder = new GameObject("Items");

        GatherAlreadyExistingItems();
        GenerateItems();
    }

    protected void GatherAlreadyExistingItems() {
        Item[] newItems = FindObjectsOfType<Item>();
        items = new List<Item>();
        foreach (Item itemPrefab in newItems) {
            Item newItem = Instantiate(itemPrefab.gameObject, itemPrefab.transform.position, itemPrefab.transform.rotation, itemPrefab.transform.parent).GetComponent<Item>();
            Register(newItem, itemPrefab.gameObject);
            Destroy(itemPrefab.gameObject);
        }
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

    public virtual Item PopItem(GameObject itemPrefab) {
        Vector3 pos = gm.map.GetFreeRoundedLocationWithoutLumiere();
        return GenerateItemFromPrefab(itemPrefab, pos);
    }

    public Item GenerateItemFromPrefab(GameObject itemPrefab, Vector3 pos, Transform parent = null) {
        Transform parentFolder = parent ?? itemsFolder.transform;
        Item item = Instantiate(itemPrefab, pos, Quaternion.identity, parentFolder).GetComponent<Item>();
        item.SetPrefab(itemPrefab);
        Register(item, itemPrefab);

        return item;
    }

    public List<Item> GetItems() {
        items = items.FindAll(i => i != null);
        return items;
    }

    public List<Vector3> GetItemsPositions() {
        return GetItems().Select(i => i.transform.position).ToList();
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

    public void RemoveAllItems() {
        foreach(Item item in items) {
            Destroy(item.gameObject);
        }
        items.Clear();
    }

    public void RemoveAllPouvoirsGivers() {
        for(int i = 0; i < items.Count; i++) {
            PouvoirGiverItem pouvoirGiver = items[i].GetComponent<PouvoirGiverItem>();
            if(pouvoirGiver != null) {
                Destroy(pouvoirGiver.gameObject);
                items.RemoveAt(i);
                i--;
            }
        }
    }

    public bool IsItemAt(Vector3 pos) {
        return items != null && items.Select(i => i.transform.position).Any(p => p == pos);
    }

    public List<OrbTrigger> GetAllOrbTriggers() {
        return orbTriggers;
    }

    public void AddOrbTrigger(OrbTrigger orbTrigger) {
        if (!orbTriggers.Contains(orbTrigger)) {
            orbTriggers.Add(orbTrigger);
        }
    }

    public bool RemoveOrbTrigger(OrbTrigger orbTrigger) {
        return orbTriggers.Remove(orbTrigger);
    }

    public void Register(Item item, GameObject itemPrefab) {
        if(!items.Contains(item)) {
            items.Add(item);
            gm.historyManager.AddItemHistory(item, itemPrefab);
        }
    }
}
