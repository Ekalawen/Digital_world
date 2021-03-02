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

    public Vector3 center { get { return start + (Vector3)extents / 2.0f; } }
    public Vector3Int size { get { return extents; } set { extents = value; } }

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

    public bool OverlapsWithoutContains(CubeInt other) {
        List<Vector3Int> otherCorners = other.Corners();
        List<Vector3Int> thisCorners = this.Corners();
        return otherCorners.Any(corner => this.ContainsStrict(corner))
            && otherCorners.Any(corner => !this.Contains(corner))
            && thisCorners.Any(corner => other.ContainsStrict(corner))
            && thisCorners.Any(corner => !other.Contains(corner));
    }
}
