using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlaceBouncyCubesOnVerticalSpaces : GenerateCubesMapFunction {

    public Vector3Int minSpacesSize = new Vector3Int(3, 3 ,3);

    public override void Activate() {
        //List<int> heights = new List<int>() { 3, 4, 5, 5, 5, 4, 3, 3, 3, 3, 0, 3, 4, 4, 3};
        //List<RectInt> rectangles = GetAllSufficiantRectanglesInHistogramme(heights, new Vector2Int(3, 3));
        //foreach(var r in rectangles) {
        //    Debug.Log($"r = {r}");
        //}
        int height = 5;
        foreach(Cube cube in map.GetAllCubes()) {
            if(cube.transform.position.y != height) {
                map.DeleteCube(cube);
            }
        }
        Cube[,,] cubesMap = map.GetRegularCubes();
        int x = cubesMap.GetLength(0);
        int y = cubesMap.GetLength(1);
        int z = cubesMap.GetLength(2);
        Cube[,] map2D = new Cube[x, z];
        for(int i = 0; i < x; i++) {
            for(int j = 0; j < z; j++) {
                map2D[i, j] = cubesMap[i, height, j];
                //map2D[i, j] = null;
            }
        }
        //Cube cube1 = map.AddCube(new Vector3(6, 5, 1), Cube.CubeType.NORMAL);
        //Cube cube2 = map.AddCube(new Vector3(6, 5, 6), Cube.CubeType.NORMAL);
        //Cube cube3 = map.AddCube(new Vector3(2, 5, 1), Cube.CubeType.NORMAL);
        //Cube cube4 = map.AddCube(new Vector3(2, 5, 6), Cube.CubeType.NORMAL);
        //map2D[6, 1] = cube1;
        //map2D[6, 6] = cube2;
        //map2D[2, 1] = cube3;
        //map2D[2, 6] = cube4;
        List<RectInt> rectangles = GetAllSufficiantRectanglesIn2DMap(map2D, new Vector2Int(4, 4));
        foreach(RectInt rectangle in rectangles) {
            float halfExtentX = (rectangle.xMax - rectangle.xMin) / 2.0f;
            float halfExtentZ = (rectangle.yMax - rectangle.yMin) / 2.0f;
            float centerX = rectangle.xMin + halfExtentX - 0.5f;
            float centerZ = rectangle.yMin + halfExtentZ - 0.5f;
            List<ColorManager.Theme> theme = new List<ColorManager.Theme>() { ColorManager.GetRandomTheme() };
            PosVisualisator.CreateCube(new Vector3(centerX, height, centerZ),
                new Vector3(halfExtentX, 0.5f + UnityEngine.Random.Range(-0.1f, +0.1f), halfExtentZ), ColorManager.GetColor(theme));
            Debug.Log($"rectangle = {rectangle}");
        }
        PosVisualisator.CreateCube(new Vector3(5, 5, 5), new Vector3(5.5f, 0.6f, 5.5f), Color.green);
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
            //int length = i - topIndice;
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
            //int length = i - topIndice;
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
