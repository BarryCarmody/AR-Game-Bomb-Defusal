using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;


public class Board
{
    public int width;
    public int height;
    public int bombs;
    public int toBeRevealed;
    public bool boardSet;
    public Cell[,] board;
    public int defusers;

    public bool gameOver = false;

    public Vector3 position;
    public Quaternion rotation;
    public Vector3 size;
    public float cellSize;
    public GameObject cellPrefab;
    public List<GameObject> cellModels;

    public Dictionary<GameObject, Cell> CellMap { get; private set; } = new Dictionary<GameObject, Cell>();



    public Board(int w, int h, Vector3 pos, Quaternion rot, Vector3 siz, GameObject cellTile, List<GameObject> cModels)
    {
        width = w;
        height = h;
        bombs = (w * h) / 5;
        toBeRevealed = (w * h) - bombs;
        board = new Cell[w, h];
        boardSet = false;
        defusers = 3;

        position = pos;
        rotation = rot;
        size = siz;
        cellSize = siz.x * 10 / w;
        cellPrefab = cellTile;
        cellModels = cModels;

        InitalizeCells();

    }

    void InitalizeCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                board[x, y] = new Cell(x, y, position, cellSize, rotation, height, width, this);

                if (cellPrefab != null)
                {   
                    //Default Cell
                    GameObject cellInstance = GameObject.Instantiate(cellPrefab);
                    cellInstance.transform.position = board[x, y].WorldPosition;
                    cellInstance.transform.rotation = rotation; // Align with board rotation
                    cellInstance.transform.localScale = Vector3.one * cellSize; // Match cell size

                    board[x, y].UnreaveledTile = cellInstance;

                    CellMap[cellInstance] = board[x, y];

                    //Flagged Cell
                    cellInstance = GameObject.Instantiate(cellModels[10]);
                    cellInstance.transform.position = board[x, y].WorldPosition;
                    cellInstance.transform.rotation = rotation * Quaternion.Euler(0, 270, 0);  // Align with board rotation
                    cellInstance.transform.localScale = Vector3.one * cellSize; // Match cell size

                    board[x, y].FlaggedTile = cellInstance;

                    board[x, y].FlaggedTile.SetActive(false);
                    CellMap[cellInstance] = board[x, y];
                }
            }
        }
    }


    public void Click(int x, int y)
    {

        if (!boardSet)
        {
            SetBoard(x, y);
        }

        Reveal(x, y);

    }

    void SetBoard(int x, int y)
    {
        boardSet = true;

        //Cells surrounding the start point
        List<int> surrounding = new List<int> { y * width + x };

        //potential neighbours
        int[,] neighbours =
        {
            {-1,-1},{0,-1},{1,-1},
            {-1,0},        {1,0},
            {-1,1}, {0,1}, {1,1}
        };

        //Find neighbours of start spot TO AVOID PLACING BOMB
        for (int i = 0; i < neighbours.GetLength(0); i++)
        {
            int currX = x + neighbours[i, 0];
            int currY = y + neighbours[i, 1];

            if (currX >= 0 && currX < width && currY >= 0 && currY < height)
            {
                surrounding.Add(currY * width + currX);
            }
        }

        //Find Bomb spots
        System.Random r = new System.Random();
        HashSet<int> bombSpots = new HashSet<int>();

        while (bombSpots.Count < bombs)
        {
            int spot = r.Next(0, width * height);
            if (!surrounding.Contains(spot))
            {
                bombSpots.Add(spot);
            }

        }


        foreach (int spot in bombSpots)
        {
            int bombY = spot / width;
            int bombX = spot % width;

            board[bombX, bombY].Value = 9; // Value 9 represents a bomb
            board[bombX, bombY].Defused = false;
        }

        //Calculate value of remaining cells
        for (int col = 0; col < width; col++)
        {
            for (int row = 0; row < height; row++)
            {
                if (board[col, row].Value != 9)
                {

                    //For all neighbours add 1 if bomb
                    for (int i = 0; i < neighbours.GetLength(0); i++)
                    {
                        int currX = col + neighbours[i, 0];
                        int currY = row + neighbours[i, 1];

                        if (currX >= 0 && currX < width && currY >= 0 && currY < height)
                        {
                            if (board[currX, currY].Value == 9)
                            {
                                board[col, row].Value = board[col, row].Value + 1;
                            }
                        }
                    }
                }

                //Instantiate Revealed
                GameObject cellInstance = GameObject.Instantiate(cellModels[board[col, row].Value]);
                cellInstance.transform.position = board[col, row].WorldPosition;
                if (board[col, row].Value == 9)
                {
                    cellInstance.transform.rotation = rotation;
                }
                else
                {
                    cellInstance.transform.rotation = rotation * Quaternion.Euler(0, 180, 0); // Align with board rotation
                }
                cellInstance.transform.localScale = Vector3.one * cellSize; // Match cell size

                board[col, row].RevealedTile = cellInstance;
                //board[col, row].UnreaveledTile.SetActive(false);
                board[col, row].RevealedTile.SetActive(false);
            }
        }


    }

    void Reveal(int x, int y)
    {
        
        board[x, y].Revealed = true;
        board[x, y].UnreaveledTile.SetActive(false);
        board[x, y].FlaggedTile.SetActive(false);
        board[x, y].RevealedTile.SetActive(true);
        toBeRevealed--;

        if (board[x, y].Value == 9)
        {
            toBeRevealed++;
            gameOver = true;
        }

        else if (board[x, y].Value == 0)
        {
            int[,] neighbours =
            {
                {-1,-1},{0,-1},{1,-1},
                {-1,0},        {1,0},
                {-1,1}, {0,1}, {1,1}
            };

            for (int i = 0; i < neighbours.GetLength(0); i++)
            {
                int currX = x + neighbours[i, 0];
                int currY = y + neighbours[i, 1];
                if (currX >= 0 && currX < width && currY >= 0 && currY < height)
                {

                    if (!board[currX, currY].Revealed)
                    {
                        Reveal(currX, currY);
                    }
                }
            }
        }

        if (toBeRevealed == 0)
        {
            gameOver = true;
        }

 
    }

    public void RevealDefused(int x, int y)
    {
        board[x, y].Revealed = true;
        board[x, y].UnreaveledTile.SetActive(false);
        board[x, y].FlaggedTile.SetActive(false);
        board[x, y].RevealedTile.SetActive(true);

        int[,] neighbours =
            {
                {-1,-1},{0,-1},{1,-1},
                {-1,0},        {1,0},
                {-1,1}, {0,1}, {1,1}
            };

        for (int i = 0; i < neighbours.GetLength(0); i++)
        {
            int currX = x + neighbours[i, 0];
            int currY = y + neighbours[i, 1];
            if (currX >= 0 && currX < width && currY >= 0 && currY < height)
            {

                if (!board[currX, currY].Revealed && board[currX, currY].Value!=9)
                {
                    Reveal(currX, currY);
                }
            }
        }
    }
}