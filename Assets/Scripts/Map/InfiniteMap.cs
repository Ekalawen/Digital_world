using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using EZCameraShake;

public class InfiniteMap : MapManager {

    public enum DifficultyMode { CONSTANT, PROGRESSIVE }

    public static Vector3 FORWARD = Vector3.right;

    [Header("Parameters")]
    public List<BlockList> blockLists;
    public int nbBlocksForward = 3;
    public DifficultyMode difficultyMode = DifficultyMode.CONSTANT;
    [ConditionalHide("difficultyMode", DifficultyMode.CONSTANT)]
    public float timeDifficultyCoefficient = 1.0f;
    [ConditionalHide("difficultyMode", DifficultyMode.PROGRESSIVE)]
    public float timeDifficultyOffset = 1.85f;
    [ConditionalHide("difficultyMode", DifficultyMode.PROGRESSIVE)]
    public float timeDifficultyProgression = 6f;

    [Header("Start")]
    public GameObject firstBlock;
    public int nbFirstBlocks = 3;
    public int nbBlocksStartCubeDestruction = 1;

    [Header("Color Change")]
    public Color colorChangeColorBlocks;
    public float rangeChangeColorBlocks;
    public float speedChangeColorBlocks;
    public float distanceToStartChangeSkyboxColor = 40.0f;

    [Header("ScreenShaker")]
    public float distanceToStartScreenShake = 10;
    public Vector2 screenShakeMagnitudeInterval;
    public Vector2 screenShakeRoughnessInterval;

    [Header("Others")]
    public CounterDisplayer nbBlocksDisplayer;
    public GameObject bestScoreMarkerPrefab;
    public bool shouldResetAllBlocksTime = false;

    Transform blocksFolder;

    int indiceCurrentBlock = 0;
    int nbBlocksRun;
    int nbBlocksDestroyed;
    int nbBlocksCreated = 0;
    List<BlockWeight> blockWeights;
    List<Block> blocks; // Les blocks vont des blocks en train de se faire détruire jusqu'à nbBlocksForwards blocks devant la position du joueur
    Block lastlyDestroyedBlock = null;
    Timer destructionBlockTimer;
    Timer timerSinceLastBlock;
    bool hasMadeNewBestScore = false;
    bool startsBlockDestruction = false;
    CameraShakeInstance cameraShakeInstance;
    List<string> blocksNameToNotifyPlayerToPressShift = new List<string>();


    protected override void InitializeSpecific() {
        blocks = new List<Block>();
        blocksFolder = new GameObject("Blocks").transform;
        blocksFolder.transform.SetParent(cubesFolder.transform);
        nbBlocksDisplayer.Display(nbBlocksRun.ToString());
        timerSinceLastBlock = new Timer();
        nbBlocksRun = 0;
        nbBlocksDestroyed = 0;
        blockWeights = blockLists.Aggregate(new List<BlockWeight>(),
            (list, blockList) => list.Concat(blockList.blocks).ToList());

        ResetAllBlocksTime();
        CreateFirstBlocks();
        cameraShakeInstance = CameraShaker.Instance.StartShake(0, 0, 0);
    }

    protected override void Update() {
        base.Update();

        ManageBlockDestruction();

        MakeCubesLookDangerous();

        ShakeProportionalyToDangerosite();
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
        if (blocksNameToNotifyPlayerToPressShift.Contains(newBlock.name)) {
            newBlock.ShouldNotifyPlayerHowToPressShift();
            blocksNameToNotifyPlayerToPressShift.Remove(newBlock.name);
        }
        nbBlocksCreated += 1;

        blocks.Add(newBlock);

        AddBestScoreMarker(newBlock);
    }

    protected void AddBestScoreMarker(Block block) {
        int bestScore = (int)gm.eventManager.GetBestScore();
        if(bestScore != 0 && (nbBlocksCreated - nbFirstBlocks) == bestScore + 1) {
            Vector3 pos = block.endPoint.position + Vector3.up * 1.5f;
            GameObject marker = Instantiate(bestScoreMarkerPrefab, pos, Quaternion.identity, block.transform.parent);
            marker.transform.LookAt(pos + FORWARD);
        }
    }

    protected GameObject GetRandomBlockPrefab() {
        float totalWeight = blockWeights.Sum(bw => bw.weight);
        float randomNumber = UnityEngine.Random.Range(0f, 1f) * totalWeight;
        float sum = 0;
        for(int i = 0; i < blockWeights.Count; i++) {
            sum += blockWeights[i].weight;
            if (randomNumber <= sum)
                return blockWeights[i].block;
        }
        return blockWeights.Last().block;
    }

    private void DestroyFirstBlock() {
        if (blocks.Any()) {
            Block firstBlock = blocks.First();
            float destroyTime = firstBlock.GetAverageTime();
            destroyTime = ApplyTimeDifficulty(destroyTime);
            firstBlock.Destroy(destroyTime);
            nbBlocksDestroyed += 1;
            indiceCurrentBlock--;
            blocks.Remove(firstBlock);
            destructionBlockTimer = new Timer(destroyTime);
            lastlyDestroyedBlock = firstBlock;
        } else {
            gm.eventManager.LoseGame(EventManager.DeathReason.OUT_OF_BLOCKS);
        }
    }

    protected float ApplyTimeDifficulty(float destroyTime) {
        if (difficultyMode == DifficultyMode.CONSTANT)
            return destroyTime * timeDifficultyCoefficient;
        else {
            float difficultyCoefficient = timeDifficultyOffset / Mathf.Pow(Mathf.Max(nbBlocksDestroyed, 1), 1 / timeDifficultyProgression);
            //nbBlocksDisplayer.AddVolatileText(difficultyCoefficient.ToString(), Color.blue);
            return destroyTime * difficultyCoefficient;
        }
    }

    protected Cube GetFarestCube() {
        List<Cube> farestCubes = blocks.First().GetCubes();
        farestCubes = farestCubes.Where(c => c != null).ToList();
        Cube farestCube = farestCubes.OrderBy(cube => Vector3.Distance(cube.transform.position, Vector3.zero)).First();
        return farestCube;
    }

    protected float GetCurrentDistanceToDestruction() {
        Vector3 destructionPosition = GetCurrentDestructionPosition();
        float projectionFarestCube = Vector3.Dot(destructionPosition, FORWARD);
        float projectionPlayer = Vector3.Dot(gm.player.transform.position, FORWARD);
        float differenceProjections = projectionPlayer - projectionFarestCube;
        return Mathf.Max(differenceProjections, 0);
    }

    protected Vector3 GetCurrentDestructionPosition() {
        Block firstBlock = lastlyDestroyedBlock ?? blocks.First();
        float lengthBlock = Vector3.Distance(firstBlock.startPoint.position, firstBlock.endPoint.position);
        float averageTimeBlock = firstBlock.GetAverageTime();
        float timeWithDifficulty = ApplyTimeDifficulty(averageTimeBlock);
        float avancement = destructionBlockTimer.GetElapsedTime() / timeWithDifficulty;
        Vector3 direction = firstBlock.endPoint.position - firstBlock.startPoint.position;
        Vector3 position = firstBlock.startPoint.position + direction * avancement;
        return position;
    }

    protected void MakeCubesLookDangerous() {
        if (startsBlockDestruction) {
            Vector3 destructionPosition = GetCurrentDestructionPosition();
            List<ColorSource> closestSources = gm.colorManager.GetColorSourcesInRange(destructionPosition, rangeChangeColorBlocks);
            foreach (ColorSource source in closestSources) {
                source.GoToColor(colorChangeColorBlocks, speedChangeColorBlocks);
            }
            UpdateSkyboxColorBasedOnDangerProximity();
        }
    }

    protected void UpdateSkyboxColorBasedOnDangerProximity() {
        float distanceToDestruction = GetCurrentDistanceToDestruction();
        if (distanceToDestruction <= distanceToStartChangeSkyboxColor) {
            float avancement = distanceToDestruction / distanceToStartChangeSkyboxColor;
            Color color = gm.console.basicColor * avancement + gm.console.ennemiColor * (1 - avancement);
            RenderSettings.skybox.SetColor("_RectangleColor", color);
            float proportion = MathCurves.Linear(gm.postProcessManager.skyboxProportionRectanglesCriticalBound, gm.postProcessManager.skyboxProportionRectangles, avancement);
            RenderSettings.skybox.SetFloat("_ProportionRectangles", proportion);
        } else {
            RenderSettings.skybox.SetColor("_RectangleColor", gm.console.basicColor);
            RenderSettings.skybox.SetFloat("_ProportionRectangles", gm.postProcessManager.skyboxProportionRectangles);
        }
    }

    protected void ShakeProportionalyToDangerosite() {
        if (startsBlockDestruction) {
            float distanceToDestruction = GetCurrentDistanceToDestruction();
            if (distanceToDestruction <= distanceToStartScreenShake) {
                float avancement = 1 - distanceToDestruction / distanceToStartScreenShake;
                cameraShakeInstance.Magnitude = screenShakeMagnitudeInterval[0] + avancement * (screenShakeMagnitudeInterval[1] - screenShakeMagnitudeInterval[0]);
                cameraShakeInstance.Roughness = screenShakeRoughnessInterval[0] + avancement * (screenShakeRoughnessInterval[1] - screenShakeRoughnessInterval[0]);
            } else {
                cameraShakeInstance.Magnitude = 0;
                cameraShakeInstance.Roughness = 0;
            }
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
        if (startsBlockDestruction) {
            if (destructionBlockTimer.IsOver()) {
                DestroyFirstBlock();
            }
        }
    }

    public void OnEnterBlock(Block block) {
        int indice = blocks.IndexOf(block);
        if(indice > indiceCurrentBlock) {
            int nbBlocksAdded = indice - indiceCurrentBlock;
            AddBlockRun(nbBlocksAdded);
            CreateNewBlocks(nbBlocksAdded);
            RememberTimeNeededForBlock(indice, indiceCurrentBlock);
            timerSinceLastBlock.Reset();
            indiceCurrentBlock = indice;
            block.NotifyPlayerToPressShiftIfNeeded();
            if(GetNonStartNbBlocksRun() >= nbBlocksStartCubeDestruction)
                StartBlocksDestruction();
        }
    }

    public void AddBlockRun(int nbBlocksToAdd) {
        nbBlocksRun += nbBlocksToAdd;
        RewardPlayerForNewBlock(nbBlocksToAdd);
    }

    public void Add10BlockRun() {
        AddBlockRun(10);
    }

    protected void StartBlocksDestruction() {
        if (!startsBlockDestruction) {
            startsBlockDestruction = true;
            gm.console.InfiniteRunnerStartCubeDestruction();
            DestroyFirstBlock();
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
        if (nbBlocksRun > nbFirstBlocks)
        {
            nbBlocksDisplayer.Display(GetNonStartNbBlocksRun().ToString());
            nbBlocksDisplayer.AddVolatileText($"+ {nbBlocksAdded.ToString()}", nbBlocksDisplayer.GetTextColor());
            gm.soundManager.PlayNewBlockClip();
            if (IsNewBestScore(GetNonStartNbBlocksRun())) {
                gm.console.RewardBestScore();
                gm.soundManager.PlayRewardBestScore();
                hasMadeNewBestScore = true;
            }
        }
    }

    protected bool IsNewBestScore(int score) {
        int precedentBestScore = (int)gm.eventManager.GetBestScore();
        return precedentBestScore > 0 && score > precedentBestScore && !hasMadeNewBestScore;
    }

    public void OnExitBlock(Block block) {
    }

    protected void ResetAllBlocksTime() {
        if (shouldResetAllBlocksTime) {
            foreach (BlockList list in blockLists) {
                foreach (BlockWeight blockWeight in list.blocks) {
                    Block block = blockWeight.block.GetComponent<Block>();
                    block.timesForFinishing = new List<float>();
                }
            }
            Debug.LogWarning("All blocks time have been reseted !");
        }
    }

    public int GetNonStartNbBlocksRun() {
        return Mathf.Max(0, nbBlocksRun - nbFirstBlocks);
    }

    public void PlayerForgotToPressShift(Block block) {
        if (TryNotifyCurrentBlockIfSame(block)) {
        } else if (TryNotifyInstantiatedBlocks(block)) {
        } else {
            RegisterBlockNameToBlocksToNotify(block);
        }
    }

    protected bool RegisterBlockNameToBlocksToNotify(Block block) {
        blocksNameToNotifyPlayerToPressShift.Add(block.name);
        return true;
    }

    protected bool TryNotifyInstantiatedBlocks(Block block) {
        int indice = blocks.IndexOf(block);
        string blockName = block.name;
        for (int i = indice + 1; i < blocks.Count; i++) {
            Block b = blocks[i];
            if (b.name == blockName) {
                b.ShouldNotifyPlayerHowToPressShift();
                return true;
            }
        }
        return false;
    }

    protected bool TryNotifyCurrentBlockIfSame(Block block) {
        if (indiceCurrentBlock < 0)
            return false;
        Block currentBlock = blocks[indiceCurrentBlock];
        if(currentBlock.name == block.name) {
            currentBlock.NotifyPlayerToPressShift();
            return true;
        }
        return false;
    }
}
