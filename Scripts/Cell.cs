using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

public class Cell
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Value { get; set; }
    public bool Revealed { get; set; }
    public bool Defused { get; set; }
    public bool Flagged { get; set; }
    public Vector3 WorldPosition { get; private set; }
    public GameObject UnreaveledTile { get; set; }
    public GameObject RevealedTile { get; set; }
    public GameObject FlaggedTile { get; set; }
    public GameObject bigBomb { get; set; }
    public Board MainBoard { get; set; }


    public Cell()
    {
        Value = 0;
        Revealed = false;
        Flagged = false;
    }

    public Cell(int x, int y, Vector3 boardPosition, float cellSize, Quaternion boardRotation, int rows, int cols, Board mainBoard)
    {
        X = x;
        Y = y;
        Value = 0;
        Revealed = false;
        Flagged = false;

        float boardWidth = cols * cellSize;
        float boardHeight = rows * cellSize;

        //float xOffset = -boardWidth + cellSize / 2;
        //float yOffset = -boardHeight + cellSize / 2;

        float xOffset = (cols - 1) * cellSize / 2f;
        float yOffset = (rows - 1) * cellSize / 2f;

        //Vector3 cellPosition = new Vector3(x*cellSize*10 + xOffset, 0, y*cellSize*10 + yOffset);

        Vector3 cellPosition = new Vector3(x * cellSize - xOffset, 0, y * cellSize - yOffset);

        WorldPosition = boardPosition + boardRotation * cellPosition;
        MainBoard = mainBoard;
        //WorldPosition = boardPosition + cellPosition;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Cell cell = hit.collider.GetComponent<Cell>();
                cell?.MainBoard.Click(cell.X, cell.Y);
            }

        }
    }
}
