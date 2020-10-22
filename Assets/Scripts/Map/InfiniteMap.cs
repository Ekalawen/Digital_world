using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InfiniteMap : MapManager {

    public int nbBlocksForward = 3;
    public float intervalDestructionBlocks = 3;
    public float speedDestructionBlocks = 1;
    public List<GameObject> blockPrefabs;

    Transform blocksFolder;

    int indiceCurrentBlock = 0;
    List<Block> blocks;
    Timer destructionBlockTimer;

    protected override void InitializeSpecific() {
        blocks = new List<Block>();
        blocksFolder = new GameObject("Blocks").transform;
        blocksFolder.transform.SetParent(cubesFolder.transform);
        destructionBlockTimer = new Timer(intervalDestructionBlocks);

        CreateFirstBlocks();
    }

    protected void CreateFirstBlocks() {
        for (int i = 0; i < nbBlocksForward + 1; i++)
            CreateBlock();
    }

    protected void CreateBlock() {
        GameObject blockPrefab = blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Count)];
        Vector3 blockStartPoint = blockPrefab.GetComponent<Block>().startPoint.position;
        Vector3 blockPosition = GetNextBlockPosition(blockStartPoint);

        Block newBlock = Instantiate(blockPrefab, blockPosition, Quaternion.identity, blocksFolder).GetComponent<Block>();
        newBlock.Initialize(blocksFolder);

        blocks.Add(newBlock);
    }

    private void DestroyFirstBlock() {
        if (blocks.Any()) {
            Block firstBlock = blocks.First();
            firstBlock.Destroy(speedDestructionBlocks);
            indiceCurrentBlock--;
            blocks.Remove(firstBlock);
        } else {
            gm.eventManager.LoseGame(EventManager.DeathReason.OUT_OF_BLOCKS);
        }
    }

    protected Vector3 GetNextBlockPosition(Vector3 blockStartPoint) {
        if(blocks.Any()) {
            return blocks.Last().endPoint.position - blockStartPoint;
        }
        return -blockStartPoint;
    }

    protected Quaternion GetNextBlockOrientation() {
        if(blocks.Any()) {
            return blocks.Last().endPoint.rotation;
        }
        return Quaternion.identity;
    }

    protected override void Update() {
        base.Update();
        if(destructionBlockTimer.IsOver()) {
            DestroyFirstBlock();
            destructionBlockTimer.Reset();
        }
    }

    public void OnEnterBlock(Block block) {
        int indice = blocks.IndexOf(block);
        if(indice > indiceCurrentBlock) {
            for(int i = indiceCurrentBlock; i < indice; i++)
                CreateBlock();
            indiceCurrentBlock = indice;
        }
    }

    public void OnExitBlock(Block block) {
    }
}
