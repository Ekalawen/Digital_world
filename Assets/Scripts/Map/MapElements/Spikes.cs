using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Définit une zone carré et une direction, puis permet de créer des piques qui partent de ce mur dans la direction choisit
// jusqu'à atteindre un objectif ! :)
public class Spikes : CubeEnsemble
{

    // Base of the wall
    public Vector3Int departMur;
    public Vector3Int direction1Mur;
    public int nbCubesInDirection1Mur;
    public Vector3Int direction2Mur;
    public int nbCubesInDirection2Mur;

    // Parameters of the spikes
    public int offsetBetweenSpikes; // Number of entire free cubes between spikes, usually 1
    public bool bGoToEnd = false; // By default, spikes go until they found a wall, and stop 2 unit before (so that there is a space between the wall and the spike). Spikes go until they touch the wall if set to true.
    public int spikesMaxRange; // The max range of the spikes
    public Vector3Int spikesDirection; // The direction of the spikes

    protected List<Vector3> startsOfSpikes;

    public Spikes(Vector3Int departMur, Vector3Int direction1Mur, int nbCubesInDirection1Mur,
                               Vector3Int direction2Mur, int nbCubesInDirection2Mur,
                               int offsetBetweenSpikes, bool bGoToEnd, int spikesMaxRange, Vector3Int spikesDirection) : base()
    {
        this.departMur = departMur;
        this.direction1Mur = direction1Mur;
        this.nbCubesInDirection1Mur = nbCubesInDirection1Mur;
        this.direction2Mur = direction2Mur;
        this.nbCubesInDirection2Mur = nbCubesInDirection2Mur;
        this.offsetBetweenSpikes = offsetBetweenSpikes;
        this.bGoToEnd = bGoToEnd;
        this.spikesMaxRange = spikesMaxRange;
        this.spikesDirection = spikesDirection;

        GenerateStartsOfSpikes();
    }

    public static Spikes GenerateSpikesFromMur(Mur mur, int offsetBetweenSpikes, bool bGoToEnd, int spikesMaxRange, Vector3 aPointInTheInside) {
        Vector3 spikesDirectionFromPoint = Vector3.Cross(mur.direction1, mur.direction2).normalized;
        if (Vector3.Dot(spikesDirectionFromPoint, (aPointInTheInside - mur.depart)) < 0) {
            spikesDirectionFromPoint *= -1;
        }
        return new Spikes(MathTools.RoundToInt(mur.depart),
            MathTools.RoundToInt(mur.direction1),
            mur.nbCubesInDirection1,
            MathTools.RoundToInt(mur.direction2),
            mur.nbCubesInDirection2,
            offsetBetweenSpikes,
            bGoToEnd,
            spikesMaxRange,
            MathTools.RoundToInt(spikesDirectionFromPoint));
    }

    public override string GetName() {
        return "Spikes";
    }

    protected void GenerateStartsOfSpikes() {
        startsOfSpikes = new List<Vector3>();
        for (int i = offsetBetweenSpikes; i < nbCubesInDirection1Mur - offsetBetweenSpikes; i += offsetBetweenSpikes + 1) {
            for (int j = offsetBetweenSpikes; j < nbCubesInDirection2Mur - offsetBetweenSpikes; j += offsetBetweenSpikes + 1) {
                Vector3 start = departMur + i * (Vector3)direction1Mur + j * (Vector3)direction2Mur;
                startsOfSpikes.Add(start);
            }
        }
    }

    public void GenerateASpike() {
        int indStart = Random.Range(0, startsOfSpikes.Count);
        Vector3 start = startsOfSpikes[indStart];
        startsOfSpikes.RemoveAt(indStart);

        Vector3 currentPosition = start;
        int spikeSize = 0;
        while (CanContinueSpike(currentPosition, spikeSize)) {
            CreateCube(currentPosition);
            currentPosition += spikesDirection;
            spikeSize += 1;
        }
    }

    protected bool CanContinueSpike(Vector3 currentPosition, int spikeSize) {
        if (spikeSize > spikesMaxRange)
            return false;

        Vector3 posPlusOne = currentPosition + 1 * (Vector3)spikesDirection;
        Vector3 posPlusTwo = currentPosition + 2 * (Vector3)spikesDirection;
        if (bGoToEnd) {
            return map.GetCubeAt(posPlusOne) == null;
        } else {
            return map.GetCubeAt(posPlusOne) == null && map.GetCubeAt(posPlusTwo) == null;
        }
    }

    public void GenerateAllSpikes() {
        while (startsOfSpikes.Count > 0) {
            GenerateASpike();
        }
    }

    public bool HasStarts() {
        return startsOfSpikes.Count > 0;
    }
}