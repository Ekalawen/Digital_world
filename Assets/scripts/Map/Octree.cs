using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Octree<T> where T : MonoBehaviour {

    protected Dictionary<Vector3Int, List<T>> octree;
    protected int cellSize;

    public Octree(int cellSize) {
        octree = new Dictionary<Vector3Int, List<T>>();
        this.cellSize = cellSize;
    }

    public Vector3Int GetCellIndice(T cube) {
        return GetCellIndice(cube.transform.position);
    }

    public Vector3Int GetCellIndice(Vector3 pos) {
        return MathTools.FloorToInt(pos / cellSize);
    }

    public void Add(T cube) {
        Vector3Int cellIndice = GetCellIndice(cube);
        if(!octree.ContainsKey(cellIndice)) {
            List<T> newCubesList = new List<T>() { cube };
            octree.Add(cellIndice, newCubesList);
        } else {
            octree[cellIndice].Add(cube);
        }
    }

    public T GetAt(Vector3 pos) {
        Vector3Int cellIndice = GetCellIndice(pos);
        if (!octree.ContainsKey(cellIndice)) {
            return null;
        } else {
            return octree[cellIndice].Find(c => c.transform.position == pos);
        }
    }

    public bool Contains(T cube) {
        Vector3Int cellIndice = GetCellIndice(cube);
        if (!octree.ContainsKey(cellIndice)) {
            return false;
        } else {
            return octree[cellIndice].Contains(cube);
        }
    }

    public T PopAt(Vector3 pos) {
        Vector3Int cellIndice = GetCellIndice(pos);
        if (!octree.ContainsKey(cellIndice)) {
            return null;
        } else {
            T cube = octree[cellIndice].Find(c => c.transform.position == pos);
            if(cube != null) {
                octree[cellIndice].Remove(cube);
            }
            return cube;
        }
    }

    public int Count {
        get {
            int nb = 0;
            foreach(Vector3Int cell in octree.Keys) {
                nb += octree[cell].Count;
            }
            return nb;
        }
    }

    public List<T> GetInSphere(Vector3 center, float radius) {
        List<T> cubes = new List<T>();
        List<Vector3Int> cells = GetCellsInSphere(center, radius);
        foreach(Vector3Int cell in cells) {
            cubes.AddRange(octree[cell].FindAll(c => Vector3.Distance(c.transform.position, center) <= radius));
        }
        return cubes;
    }

    public List<Vector3Int> GetCellsInSphere(Vector3 center, float radius) {
        List<Vector3Int> cells = new List<Vector3Int>();
        foreach(Vector3Int cell in octree.Keys) {
            CubeInt cellCube = GetCubeIntFromCell(cell);
            if(MathTools.AABBSphere(cellCube.center, cellCube.halfExtents, center, radius)) {
                cells.Add(cell);
            }
        }
        return cells;
    }

    public CubeInt GetCubeIntFromCell(Vector3Int cell) {
        return new CubeInt(cell * cellSize, Vector3Int.one * cellSize);
    }

    public void Remove(T cube) {
        Vector3Int cell = GetCellIndice(cube);
        if(octree.ContainsKey(cell)) {
            octree[cell].Remove(cube);
        }
    }

    public List<T> GetInBox(Vector3 center, Vector3 halfExtents) {
        List<T> cubes = new List<T>();
        List<Vector3Int> cells = GetCellsInBox(center, halfExtents);
        foreach(Vector3Int cell in cells) {
            CubeInt cellCube = GetCubeIntFromCell(cell);
            cubes.AddRange(octree[cell].FindAll(c => cellCube.Contains(c.transform.position)));
        }
        return cubes;
    }

    public List<Vector3Int> GetCellsInBox(Vector3 center, Vector3 halfExtents) {
        List<Vector3Int> cells = new List<Vector3Int>();
        foreach(Vector3Int cell in octree.Keys) {
            CubeInt cellCube = GetCubeIntFromCell(cell);
            if(cellCube.OverlapsBox(center, halfExtents)) {
                cells.Add(cell);
            }
        }
        return cells;
    }

    public List<T> GetAll() {
        List<T> cubes = new List<T>();
        foreach(Vector3Int cell in octree.Keys) {
            cubes.AddRange(octree[cell]);
        }
        return cubes;
    }
}
