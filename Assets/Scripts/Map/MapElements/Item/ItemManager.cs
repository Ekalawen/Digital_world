using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ItemManager : MonoBehaviour {

    public List<GameObject> itemsPrefabs; // On récupère les items !
    public List<int> nbItems;
    public bool popItemInFrontOfPlayer = false;
    [ConditionalHide("popItemInFrontOfPlayer")]
    public GameObject itemInFrontOfPlayerPrefab;
    [ConditionalHide("popItemInFrontOfPlayer")]
    public Vector2 itemInFrontOfPlayerRange = new Vector2(1.5f, 5f);

    protected GameManager gm;
    protected GameObject itemsFolder;
    protected List<Item> items; // Tous les items
    protected List<OrbTrigger> orbTriggers = new List<OrbTrigger>(); // Toutes les OrbTriggers ! :3
    [HideInInspector]
    public UnityEvent<OrbTrigger> onOrbTriggerHacked;
    [HideInInspector]
    public UnityEvent<OrbTrigger> onOrbTriggerExit;
    [HideInInspector]
    public UnityEvent onTimeResetCatch;

    public virtual void Initialize() {
        gm = GameManager.Instance;
        itemsFolder = new GameObject("Items");

        GatherAlreadyExistingItems();
        GenerateItems();
        gm.onFirstFrame.AddListener(PopItemInFrontOfPlayer);
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

    protected void PopItemInFrontOfPlayer() {
        if(!popItemInFrontOfPlayer) {
            return;
        }
        Vector3 itemPos = GetPosForItemInFrontOfPlayerV2();
        GenerateItemFromPrefab(itemInFrontOfPlayerPrefab, itemPos);
    }

    protected Vector3 GetPosForItemInFrontOfPlayerV2() {
        Vector3 playerPos = gm.player.transform.position;
        Vector3 playerForward = gm.player.camera.transform.forward;
        List<Vector3> allVisiblePos = gm.map.GetAllEmptyPositionInPlayerSight(itemInFrontOfPlayerRange);
        if(allVisiblePos.Count == 0) {
            Debug.LogError($"Aucune position visible depuis la position du joueur {playerPos} !");
            return gm.map.GetFreeRoundedLocationWithoutLumiere();
        }
        Vector3 bestPos = allVisiblePos.OrderBy(p => Vector3.Dot(playerForward, (p - playerPos).normalized)).Last();
        return bestPos;
    }

    //protected Vector3 GetPosForItemInFrontOfPlayerV1() {
    //    Vector3 center = MathTools.Round(gm.player.transform.position + gm.player.camera.transform.forward * itemInFrontOfPlayerDistance);
    //    for (int radius = 2; radius <= 10; radius++) {
    //        List<Vector3> candidatePositions = gm.map.GetEmptyPositionsInSphere(center, radius);
    //        MathTools.Shuffle(candidatePositions);
    //        foreach (Vector3 pos in candidatePositions) {
    //            if (gm.map.CanSeePlayerFrom(pos, maxRange: 9999)) {
    //                return pos;
    //            }
    //        }
    //    }
    //    Debug.LogError($"Can't find any visible position from {center} to see player at {gm.player.transform.position} !");
    //    return gm.map.GetFreeRoundedLocationWithoutLumiere();
    //}

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

    public List<Item> GetItemsOfType(Item.Type itemType) {
        return GetItems().FindAll(i => i.type == itemType);
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
        return items != null && items.FindAll(i => i != null).Select(i => i.transform.position).Any(p => p == pos);
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

    internal ResetTimeItem PopItem(object resetTemporelPrefab)
    {
        throw new NotImplementedException();
    }
}
