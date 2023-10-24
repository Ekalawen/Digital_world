using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using EZCameraShake;
using UnityEngine.Events;

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
    public bool forceNotCompletedBlocks = false;


    [Header("Start")]
    public List<GameObject> firstBlocks;
    public int nbFirstBlocks = 3;
    public int nbBlocksStartCubeDestruction = 1;
    public bool useCustomNbBlocksToCreateAtStart = false;
    [ConditionalHide("useCustomNbBlocksToCreateAtStart")]
    public int nbBlocksToCreateAtStart;

    [Header("Color Change")]
    public Color colorChangeColorBlocks;
    public float rangeChangeColorBlocks;
    public float speedChangeColorBlocks;
    public float distanceToStartChangeSkyboxColor = 40.0f;
    public float distanceAtCriticalSkyboxColor = 5.0f;

    [Header("End Game Audio Source")]
    public Vector2 distanceRangeToUpdateEndGameAudioSourceVolume = new Vector2(5, 40);

    [Header("ScreenShaker")]
    public float distanceToStartScreenShake = 10;
    public Vector2 screenShakeMagnitudeInterval;
    public Vector2 screenShakeRoughnessInterval;
    public float startLogoutScreenShakeMagnitude;
    public float startLogoutScreenShakeRoughness;
    public float startLogoutScreenShakeDecreaseTime;
    public float infiniteModeReachedScreenShakeMagnitude;
    public float infiniteModeReachedScreenShakeRoughness;
    public float infiniteModeReachedScreenShakeDecreaseTime;

    [Header("Others")]
    public CounterDisplayer nbBlocksDisplayer;
    public GameObject bestScoreMarkerPrefab;
    public GameObject newTresholdMarkerPrefab;
    public bool shouldResetAllBlocksTime = false;
    public float dureeDecompose = 5.0f;
    public bool useCustomLastTresholdForPhases = false;
    [ConditionalHide("useCustomLastTresholdForPhases")]
    public int customLastTresholdForPhases;

    protected Transform blocksFolder;
    protected int indiceCurrentBlock = 0;
    protected int indiceCurrentAllBlocks = 0;
    protected int nbBlocksRun;
    protected int nbBlocksDestroyed;
    protected int nbBlocksCreated = 0;
    protected List<BlockWeight> blockWeights;
    protected List<Block> blocks; // Les blocks vont des blocks en train de se faire détruire jusqu'à nbBlocksForwards blocks devant la position du joueur
    protected List<Block> allBlocks; // Les allBlocks vont du premier block crée jusqu'au dernier block crée. Certains pourront donc être null après leurs destructions !
    protected Block lastlyDestroyedBlock = null;
    protected Timer destructionBlockTimer;
    protected Timer timerSinceLastBlock;
    protected bool hasMadeNewBestScore = false;
    protected bool startsBlockDestruction = false;
    protected CameraShakeInstance cameraShakeInstance;
    protected List<string> blocksNameToNotifyPlayerToPressShift = new List<string>();
    protected bool hasReachInfiniteMode = false;
    protected BlockForcerInIR blockForcer;
    protected AddHiddenTextureOnCubeInIR textureAdder;
    [HideInInspector]
    public UnityEvent<int> onBlocksCrossed;
    [HideInInspector]
    public UnityEvent<Block> onCreateBlock;

    protected override void InitializeSpecific()
    {
        blocks = new List<Block>();
        allBlocks = new List<Block>();
        blocksFolder = new GameObject("Blocks").transform;
        blocksFolder.transform.SetParent(cubesFolder.transform);
        string nextTresholdSymbol = gm.goalManager.GetNextNotUnlockedTresholdSymbolFor(nbBlocksRun);
        nbBlocksDisplayer.SetColor(gm.console.allyColor);
        nbBlocksDisplayer.Display($"{nbBlocksRun.ToString()}/{nextTresholdSymbol}");
        timerSinceLastBlock = new Timer();
        nbBlocksRun = 0;
        nbBlocksDestroyed = 0;
        blockWeights = blockLists.Aggregate(new List<BlockWeight>(),
            (list, blockList) => list.Concat(blockList.blocks).ToList());
        blockForcer = GetComponent<BlockForcerInIR>();
        InitTextureAdder();

        ResetAllBlocksTime();
        CreateFirstBlocks();
        cameraShakeInstance = CameraShaker.Instance.StartShake(0, 0, 0);

        //StartCoroutine(AddFirstBlocksToHistory());
    }

    protected void InitTextureAdder() {
        textureAdder = GetComponent<AddHiddenTextureOnCubeInIR>();
        if(textureAdder != null) {
            textureAdder.Initialize();
        }
    }

    protected IEnumerator AddFirstBlocksToHistory() {
        yield return new WaitForSeconds(0.1f);
        for (int i = nbFirstBlocks; i < allBlocks.Count; i++) {
            gm.historyManager.AddBlockPassed(allBlocks[i]);
        }
        gm.eventManager.LoseGame(EventManager.DeathReason.FALL_OUT);
    }

    protected override void Update() {
        base.Update();

        ManageBlockDestruction();

        MakeCubesLookDangerous();

        ShakeProportionalyToDangerosite();
    }

    protected void CreateFirstBlocks() {
        int nbToCreate = useCustomNbBlocksToCreateAtStart ? nbBlocksToCreateAtStart : nbBlocksForward;
        for (int i = 0; i < nbToCreate + 1; i++) {
            CreateNextBlock();
        }
    }

    protected void CreateNextBlock() {
        if (nbBlocksCreated < firstBlocks.Count) {
            CreateBlock(firstBlocks[nbBlocksCreated]);
        } else {
            if (!forceNotCompletedBlocks) {
                CreateBlock(GetRandomBlockPrefab());
            } else {
                CreateBlock(GetRandomNotCompletedBlockPrefab());
            }
        }
    }

    protected void CreateBlock(GameObject blockPrefab) {
        Vector3 blockStartPoint = blockPrefab.GetComponent<Block>().startPoint.position;
        Vector3 blockPosition = GetNextBlockPosition(blockStartPoint);
        Quaternion blockRotation = GetNextBlockRotation();

        Block newBlock = Instantiate(blockPrefab, blockPosition, blockRotation, blocksFolder).GetComponent<Block>();
        newBlock.Initialize(blocksFolder, blockPrefab.GetComponent<Block>());
        if (blocksNameToNotifyPlayerToPressShift.Contains(newBlock.name)) {
            newBlock.ShouldNotifyPlayerHowToPressShift();
            blocksNameToNotifyPlayerToPressShift.Remove(newBlock.name);
        }
        nbBlocksCreated += 1;

        blocks.Add(newBlock);
        allBlocks.Add(newBlock);

        ApplyTextureAdderOnNewBlock(newBlock);
        AddBestScoreMarker(newBlock);
        AddNewTresholdMarker(newBlock);

        onCreateBlock.Invoke(newBlock);
    }

    protected void ApplyTextureAdderOnNewBlock(Block newBlock) {
        if(textureAdder != null) {
            textureAdder.ApplyOn(newBlock, nbBlocksCreated - nbFirstBlocks);
        }
    }

    protected void AddBestScoreMarker(Block block) {
        int bestScore = (int)gm.eventManager.GetBestScore();
        if(bestScore != 0 && (nbBlocksCreated - nbFirstBlocks) == bestScore + 1) {
            Vector3 pos = block.endPoint.position + Vector3.up * 1f;
            GameObject marker = Instantiate(bestScoreMarkerPrefab, pos, Quaternion.identity, block.transform.parent);
            marker.transform.LookAt(pos + FORWARD);
        }
    }

    protected void AddNewTresholdMarker(Block block) {
        List<int> tresholds = gm.goalManager.GetAllTresholds();
        if(tresholds.Any(t => t != 0 && (nbBlocksCreated - nbFirstBlocks) == t)) {
            Vector3 pos = block.endPoint.position + Vector3.up * 1f;
            GameObject marker = Instantiate(newTresholdMarkerPrefab, pos, Quaternion.identity, block.transform.parent);
            marker.transform.LookAt(pos + FORWARD);
        }
    }

    protected GameObject GetRandomBlockPrefab() {
        int indiceBlock = allBlocks.Count - nbFirstBlocks;
        if (blockForcer == null || !blockForcer.ShoulForceBlockAt(indiceBlock)) {
            return MathTools.ChoseOneWeighted(blockWeights.Select(bw => bw.block).ToList(), blockWeights.Select(bw => bw.weight).ToList());
        }
        return blockForcer.GetForcedBlockAt(indiceBlock);
    }

    protected GameObject GetRandomNotCompletedBlockPrefab() {
        float totalWeight = blockWeights.Sum(bw => Block.maxTimesCountForAveraging - bw.block.GetComponent<Block>().timesForFinishing.Count);
        if(totalWeight <= 0) {
            return GetRandomBlockPrefab();
        }
        float randomNumber = UnityEngine.Random.Range(0f, 1f) * totalWeight;
        float sum = 0;
        for(int i = 0; i < blockWeights.Count; i++) {
            sum += Block.maxTimesCountForAveraging - blockWeights[i].block.GetComponent<Block>().timesForFinishing.Count;
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
            firstBlock.Destroy(destroyTime, dureeDecompose);
            nbBlocksDestroyed += 1;
            indiceCurrentBlock--;
            blocks.Remove(firstBlock);
            destructionBlockTimer = new Timer(destroyTime);
            lastlyDestroyedBlock = firstBlock;
        } else {
            destructionBlockTimer = new Timer(0.0f);
            //gm.eventManager.LoseGame(EventManager.DeathReason.OUT_OF_BLOCKS);
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
        return Mathf.Max(GetCurrentDistanceToDestructionUnclamped(), 0);
    }

    protected float GetCurrentDistanceToDestructionUnclamped() {
        Vector3 destructionPosition = GetCurrentDestructionPosition();
        float projectionFarestCube = Vector3.Dot(destructionPosition, FORWARD);
        float projectionPlayer = Vector3.Dot(gm.player.transform.position, FORWARD);
        float differenceProjections = projectionPlayer - projectionFarestCube;
        return differenceProjections;
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
            UpdateEndGameAudioSourceVolumeBasedOnDangerProximity();
        }
    }

    protected void UpdateSkyboxColorBasedOnDangerProximity() {
        float distanceToDestruction = GetCurrentDistanceToDestruction();
        if (distanceToDestruction <= distanceToStartChangeSkyboxColor) {
            float avancement = MathCurves.LinearReversed(distanceAtCriticalSkyboxColor, distanceToStartChangeSkyboxColor, distanceToDestruction);
            Color color = ColorManager.InterpolateHSV(gm.console.basicColor, gm.console.ennemiColor, 1 - avancement);
            color = gm.postProcessManager.GetSkyboxHDRColor(color);
            RenderSettings.skybox.SetColor("_RectangleColor", color);
            float proportion = MathCurves.Linear(gm.postProcessManager.skyboxProportionRectanglesCriticalBound, gm.postProcessManager.skyboxProportionRectangles, avancement);
            RenderSettings.skybox.SetFloat("_ProportionRectangles", proportion);
        } else {
            Color color = gm.postProcessManager.GetSkyboxHDRColor(gm.console.basicColor);
            RenderSettings.skybox.SetColor("_RectangleColor", color);
            RenderSettings.skybox.SetFloat("_ProportionRectangles", gm.postProcessManager.skyboxProportionRectangles);
        }
    }

    protected void UpdateEndGameAudioSourceVolumeBasedOnDangerProximity() {
        float distanceToDestruction = GetCurrentDistanceToDestructionUnclamped();
        Vector2 range = distanceRangeToUpdateEndGameAudioSourceVolume;
        float avancement = 1 - MathCurves.LinearReversed(range.x, range.y, distanceToDestruction);
        gm.soundManager.UpdateIREndGameVolume(endGameVolume: avancement);
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
        if(allBlocks.Any()) {
            return allBlocks.Last().endPoint.position - blockStartPoint;
        }
        return -blockStartPoint;
    }

    protected Quaternion GetNextBlockRotation() {
        if(allBlocks.Any()) {
            return allBlocks.Last().endPoint.rotation;
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
        int indice = allBlocks.IndexOf(block);
        if(indice > indiceCurrentAllBlocks) {
            int nbBlocksCrossed = indice - indiceCurrentAllBlocks;
            AddBlockRun(nbBlocksCrossed);

            RegisterPassedBlocksToHistory(indiceCurrentAllBlocks, nbBlocksCrossed);

            int indiceLastBlock = allBlocks.Count - 1;
            int nbBlocksAdded = nbBlocksForward - (indiceLastBlock - indice);
            CreateNewBlocks(nbBlocksAdded);

            RememberTimeNeededForBlock(indice, indiceCurrentAllBlocks);

            timerSinceLastBlock.Reset();

            indiceCurrentBlock = blocks.IndexOf(block);
            indiceCurrentAllBlocks += nbBlocksCrossed;

            block.NotifyPlayerToPressShiftIfNeeded();

            if (GetNonStartNbBlocksRun() >= nbBlocksStartCubeDestruction) {
                StartBlocksDestruction();
            }

            gm.timerManager.TryUpdatePhase(GetNonStartNbBlocksRun(), GetLastTresholdToUseForPhases());

            onBlocksCrossed.Invoke(nbBlocksCrossed);
        }
    }

    protected int GetLastTresholdToUseForPhases() {
        return useCustomLastTresholdForPhases ? customLastTresholdForPhases : gm.goalManager.GetLastTresholdNotInfinite();
    }

    public void AddBlockRun(int nbBlocksToAdd) {
        nbBlocksRun += nbBlocksToAdd;
        IncrementTotalBlocksCrossed(nbBlocksToAdd);
        RewardPlayerForNewBlock(nbBlocksToAdd);
    }

    protected void IncrementTotalBlocksCrossed(int nbBlocksToAdd) {
        Block.AddToTotalBlocksCrossed(nbBlocksToAdd);
    }

    public void Add10BlockRun() {
        AddBlockRun(10);
    }

    public void Add100BlockRun() {
        AddBlockRun(100);
    }

    protected void StartBlocksDestruction() {
        if (!startsBlockDestruction) {
            startsBlockDestruction = true;
            gm.console.InfiniteRunnerStartCubeDestruction();
            gm.soundManager.PlayLogoutStartedClip();
            CameraShaker.Instance.ShakeOnce(startLogoutScreenShakeMagnitude, startLogoutScreenShakeRoughness, 0.1f, startLogoutScreenShakeDecreaseTime);
            DestroyFirstBlock();
        }
    }

    protected void RememberTimeNeededForBlock(int indiceBlockEntered, int indiceCurrentAllBlock) {
        if (indiceBlockEntered == indiceCurrentAllBlock + 1 && nbBlocksRun > nbFirstBlocks) {
            if (indiceCurrentAllBlock >= 0) {
                Block passedBlock = allBlocks[indiceCurrentAllBlock];
                passedBlock.RememberTime(timerSinceLastBlock.GetElapsedTime(), nbBlocksDisplayer);
            }
        }
    }

    private void CreateNewBlocks(int nbBlocksAdded) {
        for (int i = 0; i < nbBlocksAdded; i++) {
            CreateNextBlock();
        }
    }

    protected void RewardPlayerForNewBlock(int nbBlocksAdded) {
        if (nbBlocksRun > nbFirstBlocks) {
            int nbBlocksNonStart = GetNonStartNbBlocksRun();
            string nextTresholdSymbol = gm.goalManager.GetNextNotUnlockedTresholdSymbolFor(nbBlocksNonStart);
            nbBlocksDisplayer.Display($"{nbBlocksNonStart.ToString()}/{nextTresholdSymbol}");
            nbBlocksDisplayer.AddVolatileText($"+ {nbBlocksAdded.ToString()}", nbBlocksDisplayer.GetTextColor());
            gm.soundManager.PlayNewBlockClip();
            if (IsNewTreshold(nbBlocksNonStart)) {
                RewardNewTreshold();
            }
            if (IsNewBestScore(nbBlocksNonStart)) {
                RewardNewBestScore();
            } else {
                bool unlockNewTreshold = RewardNewInifiniteTresholdReached(nbBlocksNonStart);
                if (!unlockNewTreshold) {
                    RewardForReachingInfiniteMode(nbBlocksNonStart);
                }
            }
        }
    }

    protected bool IsNewTreshold(int nbBlocksNonStart) {
        return gm.goalManager.GetAllTresholds().Contains(nbBlocksNonStart);
    }

    protected void RewardNewBestScore() {
        gm.console.RewardBestScore();
        gm.soundManager.PlayRewardBestScore();
        gm.postProcessManager.ShakeOnceOnMarker();
        gm.postProcessManager.AddSkyboxChromaticShiftOf(gm.postProcessManager.skyboxChromaticShiftDurationOnMarker);
        hasMadeNewBestScore = true;
    }

    protected void RewardNewTreshold() {
        gm.soundManager.PlayGetLumiereClip(gm.player.transform.position);
        gm.postProcessManager.ShakeOnceOnMarker();
        gm.postProcessManager.AddSkyboxChromaticShiftOf(gm.postProcessManager.skyboxChromaticShiftDurationOnMarker);
    }

    protected bool RewardNewInifiniteTresholdReached(int nbBlocksNonStart) {
        if (gm.goalManager.GetAllNotUnlockedTresholds().Contains(nbBlocksNonStart)) {
            gm.console.RewardNewInfiniteTreshold(nbBlocksNonStart);
            gm.soundManager.PlayRewardBestScore();
            CameraShaker.Instance.ShakeOnce(infiniteModeReachedScreenShakeMagnitude, infiniteModeReachedScreenShakeRoughness, 0.1f, infiniteModeReachedScreenShakeDecreaseTime);
            return true;
        }
        return false;
    }

    protected void RewardForReachingInfiniteMode(int nbBlocksNonStart) {
        if(!hasReachInfiniteMode && gm.goalManager.GetLastTresholdNotInfinite() <= nbBlocksNonStart) {
            hasReachInfiniteMode = true;
            gm.console.RewardInfiniteModeReached();
            gm.soundManager.PlayRewardBestScore();
            CameraShaker.Instance.ShakeOnce(infiniteModeReachedScreenShakeMagnitude, infiniteModeReachedScreenShakeRoughness, 0.1f, infiniteModeReachedScreenShakeDecreaseTime);
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
            UnityEngine.Debug.LogWarning("All blocks time have been reseted !");
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

    protected void RegisterPassedBlocksToHistory(int indiceCurrentAllBlocks, int nbBlocksAdded) {
        for(int i = Mathf.Max(indiceCurrentAllBlocks, nbFirstBlocks); i < indiceCurrentAllBlocks + nbBlocksAdded; i++) {
            gm.historyManager.AddBlockPassed(allBlocks[i]);
        }
    }

    public void RememberLastKillingBlock() {
        RegisterPassedBlocksToHistory(indiceCurrentAllBlocks, 1);
    }

    public override float GetVolumeForPath() {
        return 1000; // The limit where we will stop Path research :)
    }

    public List<Block> GetAllBlocks() {
        return allBlocks;
    }
}
