using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//to run tests window -> general -> test runner -> edit mode ->run all


//general testing pattern is 1.arrange, 2.act 3. assert, as per https://www.youtube.com/watch?v=3YvnGzuwDbk 
public class MinesweeperTests
{
    const int Size = 10;
    const int MineCount = 10;

    GameObject root;
    Minesweeper board;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("TestBoard");
        board = root.AddComponent<Minesweeper>();
        board.statusText = NewChild<TextMeshProUGUI>("Status");

        //Start() never runs in edit mode so this is how we create cells instead
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                board.images[x, y] = NewChild<Image>("Cell");
                board.mineAmountLabel[x, y] = NewChild<TextMeshProUGUI>("Label");
            }
        }
    }

    //garbage collection for tests, otherwise stray test objects may contaminate tests
    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);
    }

    [Test]
    public void PlaceMines_PlacesTenMinesAndNeverOnTheFirstClick()
    {
        board.PlaceMines(4, 4);

        int placed = 0;
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                if (board.mines[x, y]) placed++;
            }
        }

        Assert.AreEqual(MineCount, placed, "board should hold exactly ten mines");
        Assert.IsFalse(board.mines[4, 4], "the first click must never be a mine");
    }

    [Test]
    public void Reveal_OnAMine_ShowsEveryMine()
    {
        board.mines[0, 0] = true;
        board.mines[9, 9] = true;
        board.started = true;

        board.Reveal(0, 0);

        Assert.AreEqual("X", board.mineAmountLabel[0, 0].text, "the mine that was clicked should show");
        Assert.AreEqual("X", board.mineAmountLabel[9, 9].text, "every other mine should show too");
    }

    [Test]
    public void Reveal_OnAMine_EndsTheGameAsALoss()
    {
        board.mines[3, 3] = true;
        board.started = true;

        Assert.IsFalse(board.gameOver, "game should be running before the click");

        board.Reveal(3, 3);

        Assert.IsTrue(board.gameOver, "stepping on a mine ends the game");
        Assert.IsFalse(board.won, "ending on a mine is a loss, not a win");
    }

    [Test]
    public void Reveal_EverySafeSquare_WinsTheGame()
    {
        for (int y = 0; y < Size; y++) board.mines[0, y] = true;
        board.started = true;

        for (int x = 1; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                board.Reveal(x, y);
            }
        }

        Assert.IsTrue(board.won, "clearing all ninety safe squares is a win");
        Assert.IsTrue(board.gameOver, "a win also ends the game");
    }

    T NewChild<T>(string name) where T : Component
    {
        GameObject child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(root.transform, false);
        return child.AddComponent<T>();
    }
}
