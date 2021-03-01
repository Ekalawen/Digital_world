using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceBouncyCubesOnVerticalSpaces : GenerateCubesMapFunction {

    public Vector3Int minSpacesSize = new Vector3Int(3, 3 ,3);

    public override void Activate() {
        List<int> heights = new List<int>() { 3, 3, 3, 2, 3, 3, 4, 6, 6, 7, 5, 2, 3, 6, 4, 2, 2, 3, 3, 3};
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
        return rectangles;
    }

    protected static void RemoveFromStackUntilLowerNumber(List<int> heights, Vector2 minSize, List<RectInt> rectangles, Stack<int> stackIndices, int i) {
        for (int j = 0; j < stackIndices.Count; j++) {
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
        for (int j = 0; j < stackIndices.Count; j++) {
            int topIndice = stackIndices.Peek();
            int top = heights[topIndice];
            stackIndices.Pop();
            int length = i - topIndice;
            if (top >= minSize[1] && length >= minSize[0]) {
                rectangles.Add(new RectInt(topIndice, 0, length, top));
            }
        }
    }
}
