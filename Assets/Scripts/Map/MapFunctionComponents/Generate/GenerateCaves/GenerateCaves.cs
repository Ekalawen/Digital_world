using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GenerateCaves : GenerateCubesMapFunction {

    public enum GENERATE_MODE { PROPORTION, COUNT };

    [Header("Generating Mode")]
    public GENERATE_MODE generateMode = GENERATE_MODE.PROPORTION;
    [ConditionalHide("generateMode", GENERATE_MODE.PROPORTION)]
	public float proportionCaves = 0.3f;
    [ConditionalHide("generateMode", GENERATE_MODE.COUNT)]
    public int nbCaves = 0;

    [Header("Parameters")]
    public int tailleMinCave = 3;
    public int tailleMaxCave = 10;
    public bool makeSpaceArround = false;
    [ConditionalHide("makeSpaceArround")]
    public bool preserveMapBordure = false;
    public int nbLumieresPerCaves = 1;
    public int offsetLumieresFromCenter = 1;
    public int caveOffsetFromSides = 2;

    [Header("Fixed offsets")]
    public bool useCaveFixedOffset = false;
    [ConditionalHide("useCaveFixedOffset")]
    public bool useCaveFixedOffsetX = false;
    [ConditionalHide("useCaveFixedOffsetX")]
    public int caveFixedOffsetX;
    [ConditionalHide("useCaveFixedOffset")]
    public bool useCaveFixedOffsetY = false;
    [ConditionalHide("useCaveFixedOffsetY")]
    public int caveFixedOffsetY;
    [ConditionalHide("useCaveFixedOffset")]
    public bool useCaveFixedOffsetZ = false;
    [ConditionalHide("useCaveFixedOffsetZ")]
    public int caveFixedOffsetZ;

    public override void Activate() {
        // On veut générer des caves dangeureuses :3
        // Qui possèderont des lumières !
        GenerateAllCaves();
    }

	protected List<Cave> GenerateAllCaves() {
        List<Cave> caves = null;
        if (generateMode == GENERATE_MODE.PROPORTION)
            caves = GenerateAllCavesWithProportion();
        else if (generateMode == GENERATE_MODE.COUNT)
            caves = GenerateAllCavesWithNbCaves();

        return caves;
	}

    protected List<Cave> GenerateAllCavesWithProportion() {
        List<Cave> caves = new List<Cave>();
        float currentProportion = 0.0f;
        float volumeCaves = 0;
        while(currentProportion < proportionCaves) {
            Cave cave = GenerateCave();
            caves.Add(cave);
            volumeCaves += cave.GetVolume();
            currentProportion = volumeCaves / map.GetVolume();
		}
        return caves;
    }

    protected List<Cave> GenerateAllCavesWithNbCaves() {
        List<Cave> caves = new List<Cave>();
        for(int i = 0; i < nbCaves; i++) {
            Cave cave = GenerateCave();
            caves.Add(cave);
		}
        return caves;
    }


    protected virtual Cave GenerateCave() {
        // On définit la taille de la cave
        Vector3Int size = Vector3Int.zero;
        size.x = Random.Range(tailleMinCave, tailleMaxCave + 1);
        size.y = Random.Range(tailleMinCave, tailleMaxCave + 1);
        size.z = Random.Range(tailleMinCave, tailleMaxCave + 1);

        // On définit sa position sur la carte
        Vector3 position = GetPositionCave(size, caveOffsetFromSides);

        Cave cave = new Cave(position, size, makeSpaceArround, bDigInside: true, preserveMapBordure);

        // On y rajoute la lumière !
        cave.AddNLumiereInside(nbLumieresPerCaves, offsetLumieresFromCenter);

        return cave;
    }

    protected virtual Vector3 GetPositionCave(Vector3Int sizeCave, int caveOffsetFromSides) {
        Vector3 position = Vector3.zero;
        position.x = useCaveFixedOffset && useCaveFixedOffsetX ? caveFixedOffsetX :
            Random.Range(caveOffsetFromSides, map.tailleMap.x - sizeCave.x + 2 - caveOffsetFromSides);
        position.y = useCaveFixedOffset && useCaveFixedOffsetY ? caveFixedOffsetY :
            Random.Range(caveOffsetFromSides, map.tailleMap.y - sizeCave.y + 2 - caveOffsetFromSides);
        position.z = useCaveFixedOffset && useCaveFixedOffsetZ ? caveFixedOffsetZ :
            Random.Range(caveOffsetFromSides, map.tailleMap.z - sizeCave.z + 2 - caveOffsetFromSides);
        return position;
    }
}

//[CustomEditor(typeof(GenerateCaves)), CanEditMultipleObjects]
//public class GenerateCavesEditor : Editor {
//    public override void OnInspectorGUI() {
//        GenerateCaves generateCaves = target as GenerateCaves;
//        serializedObject.Update();
//        SerializedProperty property = serializedObject.GetIterator();
//        while(property.NextVisible(true)) {
//            switch(property.name) {
//                case "nbCaves":
//                    if(generateCaves.useNbCaves) EditorGUILayout.PropertyField(property);
//                    break;
//                case "proportionCaves":
//                    if(!generateCaves.useNbCaves) EditorGUILayout.PropertyField(property);
//                    break;
//                default:
//                    EditorGUILayout.PropertyField(property);
//                    break;
//            }
//        }
//        serializedObject.ApplyModifiedProperties();
//    }
//}
