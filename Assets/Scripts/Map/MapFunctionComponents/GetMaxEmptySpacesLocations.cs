using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetMaxEmptySpacesLocations {
    public static List<CubeInt> Get(Cube[,,] cubesMap, Vector3Int minSpacesSize) {
        int Y = cubesMap.GetLength(1);
        List<CubeInt> mainCubes = new List<CubeInt>();
        List<CubeInt> finalCubes = new List<CubeInt>();
        Vector2Int minSpacesSizeHorizontal = new Vector2Int(minSpacesSize.x, minSpacesSize.z);

        for (int y = 0; y < Y; y++) {
            Cube[,] map2D = GetHorizontalMap(cubesMap, y);
            List<RectInt> rectangles = GetAllSufficiantRectanglesIn2DMap(map2D, minSpacesSizeHorizontal);
            List<CubeInt> rectCubes = rectangles.Select(rectangle => new CubeInt(rectangle, y, 1)).ToList();
            HashSet<CubeInt> nextCubes = new HashSet<CubeInt>();

            // On ajoute les anciens cubes étendus
            foreach(CubeInt mainCube in mainCubes) {
                CubeInt mainCubeExtended = new CubeInt(mainCube);
                mainCubeExtended.yMax += 1;
                foreach(CubeInt rectCube in rectCubes) {
                    if(rectCube.ContainsTranche(mainCube)) {
                        nextCubes.Add(mainCubeExtended);
                        break;
                    }
                }
                // Si on ne l'a pas ajouté, alors il faut essayer de le valider, car on sait qu'il ne pourra pas être plus grand !
                if(nextCubes.Count == 0 || nextCubes.Last() != mainCubeExtended) {
                    if(mainCube.height >= minSpacesSize.y) {
                        finalCubes.Add(mainCube);
                    }
                }
            }

            foreach(CubeInt rectCube in rectCubes) {
                // Si on est strictement compris dans un autre mainCube, alors on rajoute le rectCube étendu le plus grand !
                int outsideMainCubeHeight = 0;
                int outsideMainCubeY = 0;
                foreach(CubeInt mainCube in mainCubes) {
                    if(mainCube.ContainsTranche(rectCube) && !rectCube.GetTranche().Equals(mainCube.GetTranche())) {
                        if (mainCube.height > outsideMainCubeHeight) {
                            outsideMainCubeHeight = mainCube.height;
                            outsideMainCubeY = mainCube.y;
                        }
                    }
                }
                if (outsideMainCubeHeight > 0) {
                    CubeInt rectCubeExtended = new CubeInt(rectCube.GetTranche(), outsideMainCubeY, outsideMainCubeHeight + 1);
                    nextCubes.Add(rectCubeExtended);
                }
                // Si on a pas été ajouté de cette façon, on vérifie qu'on est pas égal à la tranche d'un autre mainCube, car dans ce cas on en a déjà rajouté une plus grande version !
                if (outsideMainCubeHeight == 0) {
                    RectInt rectCubeTranche = rectCube.GetTranche();
                    if (!mainCubes.Any(mainCube => mainCube.GetTranche().Equals(rectCubeTranche))) {
                        nextCubes.Add(rectCube);
                    }
                }
                // Ensuite il ne reste plus que les intersections strictes ! :)
                foreach (CubeInt mainCube in mainCubes) {
                    if(rectCube.OverlapsTrancheWithoutContains(mainCube)) {
                        RectInt mutualTranche = rectCube.GetMutualTranche(mainCube);
                        if (mutualTranche.width >= minSpacesSize.x && mutualTranche.height >= minSpacesSize.z) {
                            CubeInt mutualCube = new CubeInt(mutualTranche, mainCube.y, mainCube.height + 1);
                            nextCubes.Add(mutualCube);
                        }
                    }
                }
            }

            mainCubes = nextCubes.ToList();
        }

        finalCubes.AddRange(mainCubes.FindAll(mainCube => mainCube.height >= minSpacesSize.y));

        return finalCubes;
    }

    protected static Cube[,] GetHorizontalMap(Cube[,,] cubesMap, int y) {
        int X = cubesMap.GetLength(0);
        int Z = cubesMap.GetLength(2);
        Cube[,] map2D = new Cube[X, Z];
        for (int x = 0; x < X; x++) {
            for (int z = 0; z < Z; z++) {
                map2D[x, z] = cubesMap[x, y, z];
            }
        }
        return map2D;
    }

    public static List<RectInt> GetAllSufficiantRectanglesIn2DMap(Cube[,] map2D, Vector2Int minSize) {
        int x = map2D.GetLength(0);
        int z = map2D.GetLength(1);
        List<RectInt> rectangles = new List<RectInt>();
        List<int> histogramme = new List<int>();

        for(int j = 0; j < z; j++) {
            histogramme = GetNewHistogramme(map2D, x, histogramme, j);
            List<RectInt> histogrammeRectangles = GetAllSufficiantRectanglesInHistogramme(histogramme, minSize);
            foreach(RectInt rect in histogrammeRectangles) {
                rectangles.Add(new RectInt(rect.x, j - rect.height + 1, rect.width, rect.height));
            }
        }

        rectangles = RemoveIncludedRectangles(rectangles);
        return rectangles;
    }

    protected static List<int> GetNewHistogramme(Cube[,] map2D, int x, List<int> histogramme, int j) {
        if (j == 0) {
            for (int i = 0; i < x; i++) {
                int value = map2D[i, 0] == null ? 1 : 0;
                histogramme.Add(value);
            }
        } else {
            for (int i = 0; i < x; i++) {
                int value = map2D[i, j] == null ? (histogramme[i] + 1) : 0;
                histogramme[i] = value;
            }
        }
        return histogramme;
    }

    public static List<RectInt> GetAllSufficiantRectanglesInHistogramme(List<int> heights, Vector2 minSize) {
        int n = heights.Count;
        List<RectInt> rectangles = new List<RectInt>();
        Stack<int> stackIndices = new Stack<int>();
        int i;
        for (i = 0; i < n; i++) {
            RemoveFromStackUntilLowerNumber(heights, minSize, rectangles, stackIndices, i); // Modify rectangles and StackIndices !
            stackIndices.Push(i);
        }
        EmptyStack(heights, minSize, rectangles, stackIndices, i); // Modify rectangles and StackIndices !
        rectangles = RemoveIncludedRectangles(rectangles);
        return rectangles;
    }

    protected static void RemoveFromStackUntilLowerNumber(List<int> heights, Vector2 minSize, List<RectInt> rectangles, Stack<int> stackIndices, int i) {
        int stackSize = stackIndices.Count;
        for (int j = 0; j < stackSize; j++) {
            int topIndice = stackIndices.Peek();
            int top = heights[topIndice];
            if (top <= heights[i]) {
                break;
            }
            stackIndices.Pop();
            bool isStackEmpty = stackIndices.Count == 0;
            int length = isStackEmpty ? i :  i - stackIndices.Peek() - 1;
            if (top >= minSize[1] && length >= minSize[0]) {
                int startIndice = isStackEmpty ? 0 : stackIndices.Peek() + 1;
                rectangles.Add(new RectInt(startIndice, 0, length, top));
            }
        }
    }

    protected static void EmptyStack(List<int> heights, Vector2 minSize, List<RectInt> rectangles, Stack<int> stackIndices, int i) {
        int stackSize = stackIndices.Count;
        for (int j = 0; j < stackSize; j++) {
            int topIndice = stackIndices.Peek();
            int top = heights[topIndice];
            stackIndices.Pop();
            bool isStackEmpty = stackIndices.Count == 0;
            int length = isStackEmpty ? i :  i - stackIndices.Peek() - 1;
            if (top >= minSize[1] && length >= minSize[0]) {
                int startIndice = isStackEmpty ? 0 : stackIndices.Peek() + 1;
                rectangles.Add(new RectInt(startIndice, 0, length, top));
            }
        }
    }

    protected static List<RectInt> RemoveIncludedRectangles(List<RectInt> rectangles) {
        rectangles = rectangles.OrderByDescending(r => r.width).ThenByDescending(r => r.height).ToList();
        for(int i = 0; i < rectangles.Count; i++) {
            for(int j = i + 1; j < rectangles.Count; j++) {
                if(IsRectangleInRectangle(rectangles[i], rectangles[j])) {
                    rectangles.RemoveAt(j);
                    j--;
                }
            }
        }
        return rectangles;
    }

    public static bool IsRectangleInRectangle(RectInt outRect, RectInt inRect) {
        return outRect.xMin <= inRect.xMin
            && outRect.yMin <= inRect.yMin
            && inRect.xMax <= outRect.xMax
            && inRect.yMax <= outRect.yMax;
    }
}
