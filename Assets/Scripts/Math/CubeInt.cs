using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CubeInt {

    public Vector3Int start;
    public Vector3Int extents;

    public CubeInt(Vector3Int start, Vector3Int extents) {
        this.start = start;
        this.extents = extents;
    }

    public CubeInt(CubeInt original) : this(original.start, original.extents) {
    }

    public CubeInt(RectInt rectHorizontal, int y, int hauteur) : 
        this(new Vector3Int(rectHorizontal.x, y, rectHorizontal.y), 
             new Vector3Int(rectHorizontal.width, hauteur, rectHorizontal.height)){
    }

    public int x { get { return start.x; } set { start.x = value; } }
    public int y { get { return start.y; } set { start.y = value; } }
    public int z { get { return start.z; } set { start.z = value; } }

    public int xMin { get { return start.x; } set { extents.x += (start.x - value); start.x = value; } }
    public int yMin { get { return start.y; } set { extents.y += (start.y - value); start.y = value; } }
    public int zMin { get { return start.z; } set { extents.z += (start.z - value); start.z = value; } }

    public int xMax { get { return start.x + extents.x; } set { extents.x = value - start.x; } }
    public int yMax { get { return start.y + extents.y; } set { extents.y = value - start.y; } }
    public int zMax { get { return start.z + extents.z; } set { extents.z = value - start.z; } }

    public int width { get { return extents.x; } set { extents.x = value; } }
    public int height { get { return extents.y; } set { extents.y = value; } }
    public int depth { get { return extents.z; } set { extents.z = value; } }
    public int area { get { return extents.x * extents.y * extents.z; } }
    public int areaTranche { get { return extents.x * extents.z; } }

    public Vector3 center { get { return start + (Vector3)extents / 2.0f; } }
    public Vector3 mapCenter { get { return start + (Vector3)extents / 2.0f - 0.5f * Vector3.one; } }
    public Vector3Int size { get { return extents; } set { extents = value; } }
    public Vector3 halfExtents { get { return (Vector3)extents / 2.0f; } }

    public Vector3Int min { get { return start; } set { xMin = value.x; yMin = value.y; zMin = value.z; } }
    public Vector3Int max { get { return start + extents; } set { xMax = value.x; yMax = value.y; zMax = value.z; } }

    public bool Contains(CubeInt other) {
        return this.xMin <= other.xMin
            && this.yMin <= other.yMin
            && this.zMin <= other.zMin
            && other.xMax <= this.xMax
            && other.yMax <= this.yMax
            && other.zMax <= this.zMax;
    }
    public bool ContainsTranche(CubeInt other) {
        RectInt thisTranche = this.GetTranche();
        RectInt otherTranche = other.GetTranche();
        return thisTranche.xMin <= otherTranche.xMin
            && thisTranche.yMin <= otherTranche.yMin
            && otherTranche.xMax <= thisTranche.xMax
            && otherTranche.yMax <= thisTranche.yMax;
    }
    public bool Contains(Vector3 point) {
        return this.xMin <= point.x
            && this.yMin <= point.y
            && this.zMin <= point.z
            && point.x <= this.xMax
            && point.y <= this.yMax
            && point.z <= this.zMax;
    }
    public bool ContainsStrict(Vector3 point) {
        return this.xMin < point.x
            && this.yMin < point.y
            && this.zMin < point.z
            && point.x < this.xMax
            && point.y < this.yMax
            && point.z < this.zMax;
    }
    public bool Contains(Vector2 point) {
        return this.xMin <= point.x
            && this.zMin <= point.y
            && point.x <= this.xMax
            && point.y <= this.zMax;
    }
    public bool ContainsStrict(Vector2 point) {
        return this.xMin < point.x
            && this.zMin < point.y
            && point.x < this.xMax
            && point.y < this.zMax;
    }
    public bool ContainsStrictTranche(CubeInt other) {
        RectInt thisTranche = this.GetTranche();
        RectInt otherTranche = other.GetTranche();
        return thisTranche.xMin < otherTranche.xMin
            && thisTranche.yMin < otherTranche.yMin
            && otherTranche.xMax < thisTranche.xMax
            && otherTranche.yMax < thisTranche.yMax;
    }

    public List<Vector3Int> Corners() {
        List<Vector3Int> corners = new List<Vector3Int>();
        // Bottom
        corners.Add(new Vector3Int(xMin, yMin, zMin));
        corners.Add(new Vector3Int(xMin, yMin, zMax));
        corners.Add(new Vector3Int(xMax, yMin, zMin));
        corners.Add(new Vector3Int(xMax, yMin, zMax));
        // Top
        corners.Add(new Vector3Int(xMin, yMax, zMin));
        corners.Add(new Vector3Int(xMin, yMax, zMax));
        corners.Add(new Vector3Int(xMax, yMax, zMin));
        corners.Add(new Vector3Int(xMax, yMax, zMax));
        return corners;
    }

    public List<Vector2Int> TrancheCorners() {
        List<Vector2Int> corners = new List<Vector2Int>();
        corners.Add(new Vector2Int(xMin, zMin));
        corners.Add(new Vector2Int(xMin, zMax));
        corners.Add(new Vector2Int(xMax, zMin));
        corners.Add(new Vector2Int(xMax, zMax));
        return corners;
    }

    public bool OverlapsBox(Vector3 boxCenter, Vector3 boxHalfExtents) {
        return !(boxCenter.x + boxHalfExtents.x < xMin
            || boxCenter.x - boxHalfExtents.x > xMax
            || boxCenter.y + boxHalfExtents.y < yMin
            || boxCenter.y - boxHalfExtents.y > yMax
            || boxCenter.z + boxHalfExtents.z < zMin
            || boxCenter.z - boxHalfExtents.z > zMax);
    }

    public bool Overlaps(CubeInt other) {
        return OverlapsBox(other.center, other.halfExtents);
    }

    public bool OverlapsWithoutContains(CubeInt other) {
        List<Vector3Int> otherCorners = other.Corners();
        List<Vector3Int> thisCorners = this.Corners();
        return otherCorners.Any(corner => this.ContainsStrict(corner))
            && otherCorners.Any(corner => !this.Contains(corner))
            && thisCorners.Any(corner => other.ContainsStrict(corner))
            && thisCorners.Any(corner => !other.Contains(corner));
    }
    public bool OverlapsTrancheWithoutContains(CubeInt other) {
        List<Vector2Int> otherCorners = other.TrancheCorners();
        List<Vector2Int> thisCorners = this.TrancheCorners();
        return otherCorners.Any(corner => this.ContainsStrict(corner))
            && otherCorners.Any(corner => !this.Contains(corner))
            && thisCorners.Any(corner => other.ContainsStrict(corner))
            && thisCorners.Any(corner => !other.Contains(corner));
    }

    public RectInt GetTranche() {
        return new RectInt(xMin, zMin, width, depth);
    }

    public RectInt GetMutualTranche(CubeInt other) {
        int xMin = Mathf.Max(this.xMin, other.xMin);
        int yMin = Mathf.Max(this.zMin, other.zMin);
        int xMax = Mathf.Min(this.xMax, other.xMax);
        int yMax = Mathf.Min(this.zMax, other.zMax);
        int width = xMax - xMin;
        int height = yMax - yMin;
        return new RectInt(xMin, yMin, width, height);
    }

    public override bool Equals(object obj) {
        CubeInt other = obj as CubeInt;
        return other != null && this.start == other.start && this.extents == other.extents;
    }

    public override string ToString() {
        return $"(start={start}, extents={extents})";
    }

    public CubeInt Union(CubeInt other) {
        int xMin = Mathf.Min(this.xMin, other.xMin);
        int yMin = Mathf.Min(this.yMin, other.yMin);
        int zMin = Mathf.Min(this.zMin, other.zMin);
        int xMax = Mathf.Max(this.xMax, other.xMax);
        int yMax = Mathf.Max(this.yMax, other.yMax);
        int zMax = Mathf.Max(this.zMax, other.zMax);
        int width = xMax - xMin;
        int height = yMax - yMin;
        int depth = zMax - zMin;
        return new CubeInt(new Vector3Int(xMin, yMin, zMin), new Vector3Int(width, height, depth));
    }

    public CubeInt Intersect(CubeInt other) {
        int xMin = Mathf.Max(this.xMin, other.xMin);
        int yMin = Mathf.Max(this.yMin, other.yMin);
        int zMin = Mathf.Max(this.zMin, other.zMin);
        int xMax = Mathf.Min(this.xMax, other.xMax);
        int yMax = Mathf.Min(this.yMax, other.yMax);
        int zMax = Mathf.Min(this.zMax, other.zMax);
        int width = Mathf.Max(xMax - xMin, 0);
        int height = Mathf.Max(yMax - yMin, 0);
        int depth = Mathf.Max(zMax - zMin, 0);
        return new CubeInt(new Vector3Int(xMin, yMin, zMin), new Vector3Int(width, height, depth));
    }
}
