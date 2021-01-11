using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptedCube : NonBlackCube {

    public float dureeBeforeCorruption = 2.5f;
    public Material corruptionMaterial;
    public int sizeCorruption = 1;
    public int rangeCorruption = 2;
    public CubeType cubeCorruptedGeneratedType = CubeType.DEATH;
    public CubeType cubeAccessibleGeneratedType = CubeType.NORMAL;

    protected Color initialColor;
    protected Coroutine coroutine = null;
    protected Material initialMaterial;

    public override void Start() {
        base.Start();
        initialColor = GetColor();
        initialMaterial = GetComponent<MeshRenderer>().material;
    }

    protected IEnumerator CStartCorruption() {
        ColorManagerBlackAndWhite colorManager = (ColorManagerBlackAndWhite)gm.colorManager;
        //SetColor(colorManager.GetNotCurrentColor());
        GetComponent<MeshRenderer>().material = corruptionMaterial;
        Color color = ColorManager.GetColor(colorManager.GetNotCurrentTheme());
        color.a = 0.5f;
        GetComponent<MeshRenderer>().material.color = color;
        yield return new WaitForSeconds(dureeBeforeCorruption + Random.Range(-1.0f, 1.0f) * 0.02f * dureeBeforeCorruption);
        Corrupt();
    }

    public void CancelCorruption() {
        if(coroutine != null) {
            StopCoroutine(coroutine);
            coroutine = null;
            ColorManagerBlackAndWhite colorManager = (ColorManagerBlackAndWhite)gm.colorManager;
            //SetColor(ColorManager.GetColor(colorManager.GetCurrentTheme()));
            GetComponent<MeshRenderer>().material = initialMaterial;
        }
    }

    protected void CreateAllDeathCubes() {
        // On crée l'ensemble des cubes de la mort autour de nous
        for(int i = -sizeCorruption * 2; i <= sizeCorruption * 2; i++) {
            for(int j = -sizeCorruption * 2; j <= sizeCorruption * 2; j++) {
                for(int k = -sizeCorruption * 2; k <= sizeCorruption * 2; k++) {

                    Vector3 pos = transform.position + new Vector3(i, j, k);
                    if (Vector3.Distance(pos, transform.position) <= sizeCorruption
                     && IsEmptyOfDataAndItems(pos)) {
                        bool isAccessibleType = (i == 0 && j == 0 || i == 0 && k == 0 || j == 0 && k == 0);
                        CubeType type = isAccessibleType ? cubeAccessibleGeneratedType : cubeCorruptedGeneratedType;
                        Cube newCube = gm.map.AddCube(pos, type);
                        if(newCube != null && !isAccessibleType) {
                            ColorManagerBlackAndWhite colorManager = (ColorManagerBlackAndWhite)gm.colorManager;
                            newCube.SetColor(ColorManager.GetColor(colorManager.GetNotCurrentTheme()));
                            //newCube.SetColor(ColorManager.GetColor(colorManager.GetCurrentTheme()));
                        }
                        if(newCube != null && isAccessibleType) {
                            ColorManagerBlackAndWhite colorManager = (ColorManagerBlackAndWhite)gm.colorManager;
                            newCube.SetColor(colorManager.GetCurrentColor());
                        }
                    }
                }
            }
        }
    }

    protected void CorruptOtherCorruptedCubes() {
        // On essaye de corromptre d'autre cubes corruptibles à une certaine distance !
        List<Cube> cubesInRangeOfCorruption = gm.map.GetCubesInBox(transform.position, Vector3.one * rangeCorruption);
        foreach (Cube cube in cubesInRangeOfCorruption) {
            CorruptedCube corruptedCube = cube.gameObject.GetComponent<CorruptedCube>();
            if (corruptedCube != null) {
                corruptedCube.StartCorruption();
            }
        }
    }

    protected void Corrupt() {
        CreateAllDeathCubes();

        CorruptOtherCorruptedCubes();

        // On s'auto-détruit !
        Explode();
    }

    public void StartCorruption() {
        if(coroutine == null)
            coroutine = StartCoroutine(CStartCorruption());
    }

    public override void InteractWithPlayer() {
        StartCorruption();
    }

    protected bool IsEmptyOfDataAndItems(Vector3 pos) {
        foreach(Lumiere lumiere in gm.map.GetLumieres()) {
            if (lumiere.transform.position == pos)
                return false;
        }
        foreach(Item item in gm.itemManager.GetItems()) {
            if (item.transform.position == pos)
                return false;
        }
        return true;
    }
}
