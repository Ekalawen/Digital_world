using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlaceBouncyCubesOnVerticalSpaces : GenerateCubesMapFunction {

    public Vector3Int minSpacesSize = new Vector3Int(3, 3 ,3);

    public override void Activate() {
        List<int> heights = new List<int>() { 3, 4, 5, 5, 5, 4, 3, 3, 3, 3, 0, 3, 4, 4, 3};
        List<RectInt> rectangles = GetAllSufficiantRectanglesInHistogramme(heights, new Vector2Int(3, 3));
        foreach(var r in rectangles) {
            Debug.Log($"r = {r}");
        }
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
            if (top <= heights[i])
                break;
            stackIndices.Pop();
            int length = i - topIndice;
            if (top >= minSize[1] && length >= minSize[0]) {
                rectangles.Add(new RectInt(topIndice, 0, length, top));
            }
        }
    }

    protected static void EmptyStack(List<int> heights, Vector2 minSize, List<RectInt> rectangles, Stack<int> stackIndices, int i) {
        int stackSize = stackIndices.Count;
        for (int j = 0; j < stackSize; j++) {
            int topIndice = stackIndices.Peek();
            int top = heights[topIndice];
            stackIndices.Pop();
            int length = i - topIndice;
            if (top >= minSize[1] && length >= minSize[0]) {
                rectangles.Add(new RectInt(topIndice, 0, length, top));
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
