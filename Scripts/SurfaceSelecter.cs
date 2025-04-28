using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SurfaceSelecter : MonoBehaviour
{

    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject scanScreen;
    [SerializeField] private GameObject GameScreen;
    [SerializeField] private GameObject BombDefusalScreen;
    [SerializeField] private GameObject VictoryScreen;
    [SerializeField] private GameObject LoserScreen;
    [SerializeField] private GameObject boardOutlinePrefab; // Transparent or wireframe outline of the board
    [SerializeField] private GameObject boardPrefab;
    [SerializeField] private TextMeshProUGUI rowsText;
    [SerializeField] private TextMeshProUGUI colsText;
    [SerializeField] private TextMeshProUGUI bombProblemText;
    [SerializeField] private TextMeshProUGUI userInputText;
    [SerializeField] private int userInput;
    [SerializeField] private BombModel bombModel;

    private int defusalCode;
    private bool isDefusalActive = false;
    private List<int> beingDefused;

    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject revealed0;
    [SerializeField] private GameObject revealed1;
    [SerializeField] private GameObject revealed2;
    [SerializeField] private GameObject revealed3;
    [SerializeField] private GameObject revealed4;
    [SerializeField] private GameObject revealed5;
    [SerializeField] private GameObject revealed6;
    [SerializeField] private GameObject revealed7;
    [SerializeField] private GameObject revealed8;
    [SerializeField] private GameObject revealedBomb;
    [SerializeField] private GameObject BombModel;
    [SerializeField] private GameObject flagged;
    private List<GameObject> cellModels;

    private int rows = 10;
    private int cols = 10;
    private float unitSize = 0.01f;
    private bool gameLive = false;
    

    public ARPlaneManager planeManager;
    //public Vector2 boardSize = new Vector2(0.1f, 0.1f);


    private GameObject boardOutline;
    private Pose currentBoardPose;
    private bool isPositionValid = false;

    private Board gameBoard;
    private Cell selectedCell;


    void Start()
    {
        planeManager.enabled = false;
        scanScreen.SetActive(false);
        GameScreen.SetActive(false);
        VictoryScreen.SetActive(false);
        LoserScreen.SetActive(false);
        BombDefusalScreen.SetActive(false);
        UpdateBoardSize();

        cellModels = new List<GameObject>
        {
            revealed0,
            revealed1,
            revealed2,
            revealed3,
            revealed4,
            revealed5,
            revealed6,
            revealed7,
            revealed8,
            revealedBomb,
            flagged,
            BombModel

        };

    }

    public void BeginSurfaceSelection()
    {
        startScreen.SetActive(false);
        scanScreen.SetActive(true);
        planeManager.enabled = true;

        if (boardOutline == null)
        {
            boardOutline = Instantiate(boardOutlinePrefab);
            UpdateBoardSize();
        }

    }

    public void Update()
    {
        if (planeManager.enabled && boardOutline != null)
        {
            UpdateBoardOutlinePosition();
        }

        else if (gameLive)
        {
            UpdateSelectedCell();
        }

        if (gameBoard!=null && gameBoard.gameOver)
        {
            gameBoard.gameOver = false;
            gameLive = false;
            GameScreen.SetActive(false);
            if (gameBoard.toBeRevealed == 0)
            {
                VictoryScreen.SetActive(true);
            }
            else
            {
                LoserScreen.SetActive(true);
            }
        }

    }

    private void UpdateBoardOutlinePosition()
    {
        var screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        var hits = new List<ARRaycastHit>();

        if (planeManager.GetComponent<ARRaycastManager>().Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = hits[0].pose;
            currentBoardPose = hitPose;
            isPositionValid = true;

            // Snap the board outline to the detected plane
            boardOutline.transform.position = hitPose.position;
            boardOutline.transform.rotation = hitPose.rotation;
        }
        else
        {
            isPositionValid = false;
        }

        // Toggle outline visibility based on position validity
        boardOutline.SetActive(isPositionValid);
    }

    public void LockBoardPosition()
    {
        if (isPositionValid && boardOutline != null)
        {

            scanScreen.SetActive(false);
            GameScreen.SetActive(true);
            gameLive = true;

            // Instantiate the actual board
            //var finalBoard = Instantiate(boardPrefab, currentBoardPose.position, currentBoardPose.rotation);
            //ResizeBoard(finalBoard);

            Vector3 boardSize = new Vector3(cols * unitSize, 0.1f, rows * unitSize);
            gameBoard = new Board(cols, rows, currentBoardPose.position, currentBoardPose.rotation, boardSize, cellPrefab, cellModels);


            // Clean up
            Destroy(boardOutline);
            boardOutline = null;

            planeManager.enabled = false;

            // Optionally disable ARPlanes
            foreach (var plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(false);
            }
        }
    }

    //SELECTED CELL SECTION
    private void UpdateSelectedCell()
    {
        // Get the center of the screen
        var screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        // Perform a raycast
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject hitObject = hit.collider.gameObject;
            Cell cell = GetCellFromGameObject(hitObject);

            if (cell != selectedCell)
            {

                if (cell != null)
                {
                    selectedCell = cell;
                }
            }
        }
        else
        {
            // Deselect the currently selected cell if the raycast doesn't hit any
            if (selectedCell != null)
            {
                selectedCell = null;
            }
        }
    }


    private Cell GetCellFromGameObject(GameObject obj)
    {
        if (gameBoard != null && gameBoard.CellMap.ContainsKey(obj))
        {
            return gameBoard.CellMap[obj];
        }
        return null;
    }


    public void IncreaseRows()
    {
        if (rows < 30)
        {
            rows++;
            UpdateBoardSize();
        }
    }

    public void DecreaseRows()
    {
        if (rows > 5)
        {
            rows--;
            UpdateBoardSize();
        }
    }

    public void IncreaseCols()
    {
        if (cols < 30)
        {
            cols++;
            UpdateBoardSize();
        }
    }

    public void DecreaseCols()
    {
        if (cols > 5)
        {
            cols--;
            UpdateBoardSize();
        }
    }


    public void UpdateBoardSize()
    {
        float width = cols * unitSize;
        float length = rows * unitSize;

        if (boardOutline != null)
        {
            boardOutline.transform.localScale = new Vector3(width, 1, length);
        }

        rowsText.text = $"Rows: {rows}";
        colsText.text = $"Columns: {cols}";

    }

    private void ResizeBoard(GameObject board)
    {
        float width = cols * unitSize;
        float length = rows * unitSize;
        board.transform.localScale = new Vector3(width, 1, length);
    }

    public void HandleCellClick()
    {
        if (selectedCell != null && !selectedCell.Flagged)
        {
            gameBoard.Click(selectedCell.X, selectedCell.Y);
        }
    }

    public void HandleCellFlag()
    {
        if(selectedCell != null)
        {
            if (!selectedCell.Revealed && selectedCell.Flagged)
            {
                selectedCell.Flagged = false;
                selectedCell.FlaggedTile.SetActive(false);
                selectedCell.UnreaveledTile.SetActive(true);
            }

            else if (!selectedCell.Revealed && !selectedCell.Flagged)
            {
                selectedCell.Flagged = true;
                selectedCell.FlaggedTile.SetActive(true);
                selectedCell.UnreaveledTile.SetActive(false);
            }
        }
    }

    public void HandleCellDefuse()
    {
        if (gameBoard.defusers > 0 && selectedCell != null)
        {
            if (selectedCell.Value == 9)
            {
                StartBombDefusal(selectedCell.X,selectedCell.Y);
            }
            else
            {
                gameBoard.defusers -= 1;
                gameBoard.Click(selectedCell.X, selectedCell.Y);
            }
        }
    }

    public void RestartScan()
    {
        // Clean up the current board
        foreach (var cell in gameBoard.board)
        {
            if (cell.RevealedTile != null) Destroy(cell.RevealedTile);
            if (cell.UnreaveledTile != null) Destroy(cell.UnreaveledTile);
            if (cell.FlaggedTile != null) Destroy(cell.FlaggedTile);
            if (cell.bigBomb != null) Destroy(cell.bigBomb);
        }

        gameBoard = null; // Clear the board reference

        // Reset screens
        GameScreen.SetActive(false);
        VictoryScreen.SetActive(false);
        LoserScreen.SetActive(false);

        // Reactivate scanning phase
        BeginSurfaceSelection();
    }

    public void RestartSameBoard()
    {

        foreach (var cell in gameBoard.board)
        {
            if (cell.RevealedTile != null) Destroy(cell.RevealedTile);
            if (cell.UnreaveledTile != null) Destroy(cell.UnreaveledTile);
            if (cell.FlaggedTile != null) Destroy(cell.FlaggedTile);
            if (cell.bigBomb != null) Destroy(cell.bigBomb);
        }

        gameBoard.CellMap.Clear();

        // Reset the board's internal state
        gameBoard = new Board(gameBoard.width, gameBoard.height, gameBoard.position, gameBoard.rotation, gameBoard.size, gameBoard.cellPrefab, gameBoard.cellModels);

        gameLive = true;
        VictoryScreen.SetActive(false);
        LoserScreen.SetActive(false);
        GameScreen.SetActive(true);

    }

    //Bomb Defusal Game

    public void StartBombDefusal(int x, int y)
    {
        beingDefused = new List<int> { x, y };
        defusalCode = Random.Range(1000, 9999);
        int num1 = Random.Range(1, defusalCode);
        int num2 = defusalCode - num1;

        GameScreen.SetActive(false);
        BombDefusalScreen.SetActive(true);
        bombModel.ShowModel();
        bombProblemText.text = $"{num1} + {num2} = ?";
        userInput = 0;
        isDefusalActive = false;
        userInputText.text = "0000";
    }

    public void NumberPress(int number)
    {
        if (userInput < 1000)
        {
            userInput = userInput * 10 + number;
            userInputText.text = userInput.ToString("D4");
            
        }
    }

    public void ClearPress()
    {
        userInput = 0;
        userInputText.text = "0000";
        
    }

    public void EnterPress()
    {
        if (userInput == defusalCode)
        {
            Defused();
        }
        else
        {
            FailDefusal();
        }
    }

    public void Defused()
    {
        isDefusalActive = false;
        BombDefusalScreen.SetActive(false);
        bombModel.HideModel();
        gameBoard.RevealDefused(beingDefused[0], beingDefused[1]);
        GameScreen.SetActive(true);

    }

    public void FailDefusal()
    {
        isDefusalActive = false;
        BombDefusalScreen.SetActive(false);
        bombModel.HideModel();
        GameScreen.SetActive(true);
        gameBoard.gameOver = true;
    }

}

