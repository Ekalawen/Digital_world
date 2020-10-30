﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InfiniteMap : MapManager {

    public bool shouldResetAllBlocksTime = false;
    public int nbBlocksForward = 3;
    public int nbFirstBlocks = 3;
    public float timeBeforeFirstDestruction = 3;
    public float timeDifficultyCoefficient = 1.0f;
    public GameObject firstBlock;
    public List<BlockList> blockLists;
    public CounterDisplayer nbBlocksDisplayer;
    public Color colorChangeColorBlocks;
    public float rangeChangeColorBlocks;
    public float speedChangeColorBlocks;

    Transform blocksFolder;

    int indiceCurrentBlock = 0;
    int nbBlocksRun;
    List<Block> blocks;
    Timer destructionBlockTimer;
    Timer timerBeforeChangeColorBlocks;
    Timer timerSinceLastBlock;


    protected override void InitializeSpecific() {
        blocks = new List<Block>();
        blocksFolder = new GameObject("Blocks").transform;
        blocksFolder.transform.SetParent(cubesFolder.transform);
        destructionBlockTimer = new Timer(timeBeforeFirstDestruction);
        nbBlocksDisplayer.Display(nbBlocksRun.ToString());
        timerBeforeChangeColorBlocks = new Timer(timeBeforeFirstDestruction);
        timerSinceLastBlock = new Timer();
        nbBlocksRun = 0;

        ResetAllBlocksTime();
        CreateFirstBlocks();
    }

    protected override void Update() {
        base.Update();

        ManageBlockDestruction();

        MakeCubesLookDangerous();
    }

    protected void CreateFirstBlocks() {
        for (int i = 0; i < nbBlocksForward + 1; i++) {
            if(i < nbFirstBlocks)
                CreateBlock(firstBlock);
            else
                CreateBlock(GetRandomBlockPrefab());
        }
    }

    protected void CreateBlock(GameObject blockPrefab) {
        Vector3 blockStartPoint = blockPrefab.GetComponent<Block>().startPoint.position;
        Vector3 blockPosition = GetNextBlockPosition(blockStartPoint);

        Block newBlock = Instantiate(blockPrefab, blockPosition, Quaternion.identity, blocksFolder).GetComponent<Block>();
        newBlock.Initialize(blocksFolder, blockPrefab.GetComponent<Block>());

        blocks.Add(newBlock);
    }

    protected GameObject GetRandomBlockPrefab() {
        BlockList blockList = blockLists[UnityEngine.Random.Range(0, blockLists.Count)];
        GameObject block = blockList.blocks[UnityEngine.Random.Range(0, blockList.blocks.Count)];
        return block;
    }

    private void DestroyFirstBlock() {
        if (blocks.Any()) {
            Block firstBlock = blocks.First();
            float destroyTime = firstBlock.GetAverageTime() * timeDifficultyCoefficient;
            firstBlock.Destroy(destroyTime);
            indiceCurrentBlock--;
            blocks.Remove(firstBlock);
            destructionBlockTimer = new Timer(destroyTime);
        } else {
            gm.eventManager.LoseGame(EventManager.DeathReason.OUT_OF_BLOCKS);
        }
    }

    protected void MakeCubesLookDangerous() {
        if (timerBeforeChangeColorBlocks.IsOver()) {
            List<Cube> farestCubes = blocks.First().GetCubes();
            farestCubes = farestCubes.Where(c => c != null).ToList();
            Cube farestCube = farestCubes.OrderBy(cube => Vector3.Distance(cube.transform.position, gm.player.transform.position)).Last();
            List<ColorSource> closestSources = gm.colorManager.GetColorSourcesInRange(farestCube.transform.position, rangeChangeColorBlocks);
            foreach (ColorSource source in closestSources)
                source.GoToColor(colorChangeColorBlocks, speedChangeColorBlocks);
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

    protected void ManageBlockDestruction() {
        if(destructionBlockTimer.IsOver()) {
            DestroyFirstBlock();
        }
    }

    public void OnEnterBlock(Block block) {
        int indice = blocks.IndexOf(block);
        if(indice > indiceCurrentBlock) {
            int nbBlocksAdded = indice - indiceCurrentBlock;
            nbBlocksRun += nbBlocksAdded;
            RewardPlayerForNewBlock(nbBlocksAdded);
            CreateNewBlocks(nbBlocksAdded);
            RememberTimeNeededForBlock(indice, indiceCurrentBlock);
            timerSinceLastBlock.Reset();
            indiceCurrentBlock = indice;
        }
    }

    protected void RememberTimeNeededForBlock(int indiceBlockEntered, int indiceCurrentBlock) {
        if (indiceBlockEntered == indiceCurrentBlock + 1 && nbBlocksRun > nbFirstBlocks) {
            if (indiceCurrentBlock >= 0) {
                Block passedBlock = blocks[indiceCurrentBlock];
                passedBlock.RememberTime(timerSinceLastBlock.GetElapsedTime(), nbBlocksDisplayer);
            }
        }
    }

    private void CreateNewBlocks(int nbBlocksAdded) {
        for (int i = 0; i < nbBlocksAdded; i++) {
            CreateBlock(GetRandomBlockPrefab());
        }
    }

    protected void RewardPlayerForNewBlock(int nbBlocksAdded) {
        if (nbBlocksRun > nbFirstBlocks) {
            nbBlocksDisplayer.Display((nbBlocksRun - nbFirstBlocks).ToString());
            nbBlocksDisplayer.AddVolatileText($"+ {nbBlocksAdded.ToString()}", nbBlocksDisplayer.GetTextColor());
            gm.soundManager.PlayNewBlockClip();
        }
    }

    public void OnExitBlock(Block block) {
    }

    protected void ResetAllBlocksTime() {
        if (shouldResetAllBlocksTime) {
            foreach (BlockList list in blockLists) {
                foreach (GameObject blockPrefab in list.blocks) {
                    Block block = blockPrefab.GetComponent<Block>();
                    block.timesForFinishing = new List<float>();
                }
            }
            Debug.LogWarning("All blocks time have been reseted !");
        }
    }
}
