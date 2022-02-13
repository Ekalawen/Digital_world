using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GenerateVerticalLabyrinthe : GenerateCubesMapFunction {

    public static int LARGEUR_MUR = 1;

    public class Cell {
        public Vector2Int pos;
        public bool isAccessible = false;
        public List<Cell> voisins;

        public Cell(Vector2Int pos, List<Cell> voisins) {
            this.pos = pos;
            this.voisins = new List<Cell>(); // Un voisin est une cellule adjacente séparé par un MUR !
            foreach(Cell voisin in voisins.FindAll(v => v != null)) {
                AddVoisin(voisin);
            }
        }

        public void AddVoisin(Cell voisin) {
            if(!voisins.Contains(voisin)) {
                voisins.Add(voisin);
            }
            if(!voisin.voisins.Contains(this)) {
                voisin.voisins.Add(this);
            }
        }
        
        public Vector3 GetCenter(int largeurCouloir) {
            return new Vector3(
                1 + largeurCouloir / 2.0f + pos.x * (largeurCouloir + LARGEUR_MUR),
                0,
                1 + largeurCouloir / 2.0f + pos.y * (largeurCouloir + LARGEUR_MUR));
        }

        //public Vector3 GetMurOrigineWithCell(Cell voisin, int largeurCouloir) {
        //    if(pos.x == voisin.pos.x - 1) {
        //        return new Vector3(1 + pos.x * (largeurCouloir + LARGEUR_MUR), 0, pos.y * (largeurCouloir + LARGEUR_MUR));
        //    }
        //}
    }

    public class Wall {
    }

    public int largeurCouloir = 2;
    public Vector2Int tailleMapToUse = new Vector2Int(10, 10);

    protected int largeurCell;
    protected Vector2Int tailleGrid;
    protected Cell[,] grid;
    protected Transform cubesFolder;
    protected List<Mur> murs = new List<Mur>();

    public override void Activate() {
        Assert.IsTrue((tailleMapToUse.x - LARGEUR_MUR) % (largeurCouloir + LARGEUR_MUR) == 0);
        Assert.IsTrue((tailleMapToUse.y - LARGEUR_MUR) % (largeurCouloir + LARGEUR_MUR) == 0);
        largeurCell = largeurCouloir + LARGEUR_MUR;
        grid = GenerateGrid();
        grid = GenerateMazeInGrid(grid);
        CreateCubesInWallsOfGrid();
    }

    protected Cell[,] GenerateGrid() {
        tailleGrid = new Vector2Int((tailleMapToUse.x - LARGEUR_MUR) / (largeurCouloir + LARGEUR_MUR), (tailleMapToUse.y - LARGEUR_MUR) / (largeurCouloir + LARGEUR_MUR));
        Cell[,] grid = new Cell[tailleGrid.x, tailleGrid.y];
        for(int i = 0; i < tailleGrid.x; i++) {
            for(int j = 0; j < tailleGrid.y; j++) {
                Vector2Int pos = new Vector2Int(i, j);
                List<Cell> voisins = new List<Cell>() {
                    pos.x + 1 < tailleGrid.x ? grid[pos.x + 1, pos.y] : null,
                    pos.x - 1 >= 0 ? grid[pos.x - 1, pos.y] : null,
                    pos.y + 1 < tailleGrid.y ? grid[pos.x, pos.y + 1] : null,
                    pos.y - 1 >= 0 ? grid[pos.x, pos.y - 1] : null,
                };
                grid[i, j] = new Cell(pos, voisins);
            }
        }
        return grid;
    }

    protected Cell[,] GenerateMazeInGrid(Cell[,] grid) {
        Cell startingCell = grid[UnityEngine.Random.Range(0, tailleGrid.x), UnityEngine.Random.Range(0, tailleGrid.y)];
        startingCell.isAccessible = true;
        List<Cell> openCells = new List<Cell>() { startingCell };

        while(openCells.Count > 0) {
            Cell currentCell = MathTools.ChoseOne(openCells);
            List<Cell> potentialVoisins = currentCell.voisins.FindAll(v => !v.isAccessible);
            if(potentialVoisins.Count > 0) {
                Cell nextCell = MathTools.ChoseOne(potentialVoisins);
                nextCell.voisins.Remove(currentCell);
                currentCell.voisins.Remove(nextCell);
                nextCell.isAccessible = true;
                openCells.Add(nextCell);
            } else {
                openCells.Remove(currentCell);
            }
        }

        return grid;
    }

    //protected void CreateCubesInWallsOfGrid() {
    //    cubesFolder = new GameObject("VerticalLabyrinthe").transform;
    //    cubesFolder.SetParent(map.cubesFolder.transform);
    //    HashSet<Tuple<Cell, Cell>> pairsDone = new HashSet<Tuple<Cell, Cell>>();
    //    foreach(Cell cell in grid) {
    //        foreach(Cell voisin in cell.voisins) {
    //            if(!pairsDone.Contains(new Tuple<Cell, Cell>(cell, voisin))) {
    //                CreateWallBetween(cell, voisin);
    //                pairsDone.Add(new Tuple<Cell, Cell>(voisin, cell));
    //            }
    //        }
    //        CreateEdgeWallsAround(cell);
    //    }
    //}

    protected void CreateCubesInWallsOfGrid() {
        cubesFolder = new GameObject("VerticalLabyrinthe").transform;
        cubesFolder.SetParent(map.cubesFolder.transform);
        for(int i = 0; i < tailleMapToUse.x; i++) {
            for(int j = 0; j < tailleMapToUse.y; j++) {
                TryCreateColonne(new Vector2Int(i, j));
            }
        }
    }

    protected void TryCreateColonne(Vector2Int pos) {
        if (IsInWallPosition(pos)) {
            if (IsCorner(pos)) {
                CreateColonne(pos);
                return;
            }
            if(IsEdge(pos)) {
                CreateColonne(pos);
                return;
            }
            Tuple<Cell, Cell> cells = GetSuroundingCellsForPosInWallPositionNotInCornerAndEge(pos);
            if(cells.Item1.voisins.Contains(cells.Item2)) {
                CreateColonne(pos);
            }
        }
    }

    protected Tuple<Cell, Cell> GetSuroundingCellsForPosInWallPositionNotInCornerAndEge(Vector2Int pos) {
        if(pos.x % largeurCell == 0) {
            return new Tuple<Cell, Cell>(
                grid[pos.x / largeurCell - 1, pos.y / largeurCell],
                grid[pos.x / largeurCell, pos.y / largeurCell]);
        }
        // pos.y % largeurCell = 0;
        return new Tuple<Cell, Cell>(
            grid[pos.x / largeurCell, pos.y / largeurCell - 1],
            grid[pos.x / largeurCell, pos.y / largeurCell]);
    }

    protected void CreateColonne(Vector2Int pos) {
        Vector3 departColonne = new Vector3(pos.x, 0, pos.y);
        murs.Add(new Mur(departColonne, Vector3.up, map.tailleMap.y, Vector3.forward, 1));
    }

    protected bool IsEdge(Vector2Int pos) {
        return pos.x == 0 || pos.y == 0 || pos.x == tailleMapToUse.y - 1 || pos.y == tailleMapToUse.y - 1;
    }

    protected bool IsCorner(Vector2Int pos) {
        return pos.x % largeurCell == 0 && pos.y % largeurCell == 0;
    }

    private bool IsInWallPosition(Vector2Int pos) {
        return pos.x % largeurCell == 0 || pos.y % largeurCell == 0;
    }
}
