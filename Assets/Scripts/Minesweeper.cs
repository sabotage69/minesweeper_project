using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[assembly: InternalsVisibleTo("Tests")]

public class Minesweeper : MonoBehaviour
{
    public GameObject cellPrefab;
    public TextMeshProUGUI statusText;
    public Button restartButton;

    const int Size = 10;
    const int MineCount = 10;
    const float Pitch = 60f;

    internal bool[,] mines = new bool[Size, Size]; //mine positions written on first click, never changed
    bool[,] revealed = new bool[Size, Size]; //has a cell been revealed
    bool[,] flagged = new bool[Size, Size]; //was a square flagged
    internal Image[,] images = new Image[Size, Size];//changes cell color on reveal
    internal TextMeshProUGUI[,] mineAmountLabel = new TextMeshProUGUI[Size, Size]; //text in the middle of a cell

    internal bool started;
    internal bool gameOver;
    internal bool won;
    int flagCount;
    int revealedCount;

    void Start()
    {
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                GameObject cell = Instantiate(cellPrefab);
                cell.transform.SetParent(transform, false);
                RectTransform rect = cell.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(-270f + Pitch * x, 270f - Pitch * y); //measured from the centre of the board, edges are at -300/+300

                Cell click = cell.AddComponent<Cell>();
                click.board = this;
                click.x = x;
                click.y = y;

                GameObject labelObject = new GameObject("Label");
                labelObject.transform.SetParent(cell.transform, false);
                TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
                label.rectTransform.sizeDelta = new Vector2(58f, 58f);
                label.alignment = TextAlignmentOptions.Center;
                label.fontSize = 28f;
                label.color = Color.black;
                label.raycastTarget = false;
                label.text = "";

                images[x, y] = cell.GetComponent<Image>();
                mineAmountLabel[x, y] = label;
            }
        }

        restartButton.onClick.AddListener(Restart);
        UpdateStatus();
    }

    public void Reveal(int x, int y)
    {
        Open(x, y);
        UpdateStatus();
    }

    public void Flag(int x, int y)
    {
        if (gameOver) return;
        if (revealed[x, y]) return;

        flagged[x, y] = !flagged[x, y];

        if (flagged[x, y])
        {
            flagCount++;
            mineAmountLabel[x, y].text = "F";
        }
        else
        {
            flagCount--;
            mineAmountLabel[x, y].text = "";
        }

        UpdateStatus();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Open(int x, int y)
    {
        if (gameOver) return;
        if (revealed[x, y]) return;
        if (flagged[x, y]) return;

        //first click lays mines - first click never kills
        if (!started)
        {
            PlaceMines(x, y);
            started = true;
        }

        revealed[x, y] = true;
        images[x, y].color = new Color(0.7f, 0.7f, 0.7f); //different shade for opened cells

        if (mines[x, y])
        {
            ShowAllMines();
            gameOver = true;
            return;
        }

        revealedCount++;
        if (revealedCount == Size * Size - MineCount)
        {
            won = true;
            gameOver = true;
        }

        int count = CountNeighbours(x, y);
        if (count > 0)
        {
            mineAmountLabel[x, y].text = count.ToString();
            return;
        }

        mineAmountLabel[x, y].text = " ";
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;
                if (InBounds(nx, ny)) Open(nx, ny);
            }
        }
    }

    void UpdateStatus()
    {
        if (won) statusText.text = "you win!";
        else if (gameOver) statusText.text = "you lose";
        else statusText.text = (MineCount - flagCount) + " mines remaining";
    }



    internal void PlaceMines(int safeX, int safeY)
    {
        int placed = 0;
        while (placed < MineCount)
        {
            int x = Random.Range(0, Size);
            int y = Random.Range(0, Size);

            if (mines[x, y]) continue;
            if (x == safeX && y == safeY) continue;

            mines[x, y] = true;
            placed++;
        }
    }

    int CountNeighbours(int x, int y)
    {
        int count = 0;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;

                if (!InBounds(nx, ny)) continue;
                if (mines[nx, ny]) count++;
            }
        }
        return count;
    }

    bool InBounds(int x, int y)
    {
        if (x < 0) return false;
        if (x >= Size) return false;
        if (y < 0) return false;
        if (y >= Size) return false;
        return true;
    }

    void ShowAllMines()
    {
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                if (mines[x, y]) mineAmountLabel[x, y].text = "X";
            }
        }
    }
}
