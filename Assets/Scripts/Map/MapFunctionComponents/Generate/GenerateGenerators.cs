using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateGenerators : MapFunctionComponent {

    public List<GameObject> generatorPrefabs;
    public List<int> nbGenerators;
    public GetEmptyPositionsHelper getEmptyPositionsHelper;

    protected List<Vector3> emptyPossiblePositions = null;

    public override void Activate() {
        for(int i = 0; i < generatorPrefabs.Count; i++) {
            for(int j = 0; j < nbGenerators[i]; j++) {
                CreateGenerator(generatorPrefabs[i]);
            }
        }
    }

    public void CreateGenerator(GameObject generatorPrefab) {
        Vector3 position = ComputePosition();
        IGenerator generator = Instantiate(generatorPrefab, position, Quaternion.identity, map.zonesFolder.transform).GetComponent<IGenerator>();
        generator.Initialize();
    }

    protected Vector3 ComputePosition() {
        if (getEmptyPositionsHelper == null) {
            return map.GetFreeRoundedLocationWithoutLumiere();
        } else {
            if(emptyPossiblePositions == null || emptyPossiblePositions.Count <= 0) {
                emptyPossiblePositions = getEmptyPositionsHelper.Get();
            }
            Vector3 chosenOne = MathTools.ChoseOne(emptyPossiblePositions);
            emptyPossiblePositions.Remove(chosenOne);
            return chosenOne;
        }
    }
}
