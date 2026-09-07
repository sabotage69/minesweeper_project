using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Minesweeper : MonoBehaviour
{
    public GameObject cellPrefab;

    const int Size = 10;
    const int MineCount = 10;
    const float Pitch = 60f;

    bool[,] mines = new bool[Size, Size]; //mine positions written on first click, never changed
    bool[,] revealed = new bool[Size, Size]; //has a cell been revealed
    bool[,] flagged = new bool[Size, Size]; //was a square flagged
    Image[,] images = new Image[Size, Size];//changes cell color on reveal
    TextMeshProUGUI[,] mineAmountLabel = new TextMeshProUGUI[Size, Size]; //text in the middle of a cell

    bool started;
    bool gameOver;

    void Start()
    {
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                GameObject cell = Instantiate(cellPrefab);
                cell.transform.SetParent(transform, false);
                RectTransform rect = cell.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(-270f + Pitch * x, 270f - Pitch * y);//measured from the centre of the board, edges are at -300/+300

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
    }

    //different from original MS Minsweeper in a way that first click doesnt necessarily burst opens big area
    public void Reveal(int x, int y)
    {
        if (gameOver || revealed[x, y] || flagged[x, y]) return;

        if (!started)
        {
            PlaceMines(x, y); //first click lays mines - first click never kills
            started = true;
        }

        revealed[x, y] = true;
        images[x, y].color = new Color(0.7f, 0.7f, 0.7f);

        if (mines[x, y])
        {
            ShowAllMines();
            gameOver = true;
            return;
        }

        int count = CountNeighbours(x, y);
        if (count > 0)
        {
            mineAmountLabel[x, y].text = count.ToString();
            return;
        }

        mineAmountLabel[x, y].text = " ";

        //burst opens a big area
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;
                if (nx >= 0 && nx < Size && ny >= 0 && ny < Size) Reveal(nx, ny);
            }
        }
    }

    public void Flag(int x, int y)
    {
        if (gameOver || revealed[x, y]) return;

        flagged[x, y] = !flagged[x, y];
        mineAmountLabel[x, y].text = flagged[x, y] ? "F" : "";
    }

    void PlaceMines(int safeX, int safeY)
    {
        int placed = 0;
        while (placed < MineCount)
        {
            int x = Random.Range(0, Size);
            int y = Random.Range(0, Size);
            if (mines[x, y] || (x == safeX && y == safeY)) continue;

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
                if (nx >= 0 && nx < Size && ny >= 0 && ny < Size && mines[nx, ny]) count++;
            }
        }
        return count;
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
