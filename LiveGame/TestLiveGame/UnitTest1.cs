using LiveGameManager;

namespace TestLiveGame;

public class Tests
{
    private IBoardManager _boardManager;

    [SetUp]
    public void Setup()
    {
        _boardManager = new BoardManager(string.Empty);
    }

    [Test]
    public void BoardResultOk()
    {
        List<List<bool>> board = new List<List<bool>>()
        {
            new() { false, false, false, false, false, false },
            new() { true, false, false, false, false, true },
            new() { false, true, true, false, false, false },
            new() { false, false, true, true, false, false },
            new() { true, false, true, false, false, false },
            new() { false, true, false, false, false, false },
            new() { true, false, true, false, true, false },
            new() { false, false, false, true, false, true },
        };

        List<List<bool>> boardResult = new List<List<bool>>()
        {
            new() { false, false, false, false, false, false },
            new() { false, false, false, false, false, false },
            new() { false, true, false, false, false, false },
            new() { true, false, true, false, false, false },
            new() { true, false, true, false, false, false },
            new() { false, true, false, false, false, false },
            new() { false, false, false, false, false, false },
            new() { false, false, false, false, false, false },
        };

        string id;
        _boardManager.AddBoard(board, out id);

        var finalBoard = _boardManager.GetFinalState(id, 10).Result;
        bool equal = _boardManager.BoardEquals(finalBoard, boardResult).Result;

        Assert.IsTrue(equal);
    }

    [Test]
    public void BoardResultMaxTriesError()
    {
        List<List<bool>> board = new List<List<bool>>()
        {
            new() { false, false, false, false, false, false },
            new() { true, false, false, false, false, true },
            new() { false, true, true, false, false, false },
            new() { false, false, true, true, false, false },
            new() { true, false, true, false, false, false },
            new() { false, true, false, false, false, false },
            new() { true, false, true, false, true, false },
            new() { false, false, false, true, false, true },
        };

        List<List<bool>> boardResult = new List<List<bool>>()
        {
            new() { false, false, false, false, false, false },
            new() { false, false, false, false, false, false },
            new() { false, true, false, false, false, false },
            new() { true, false, true, false, false, false },
            new() { true, false, true, false, false, false },
            new() { false, true, false, false, false, false },
            new() { false, false, false, false, false, false },
            new() { false, false, false, false, false, false },
        };

        string id;
        _boardManager.AddBoard(board, out id);

        var finalBoard = _boardManager.GetFinalState(id, 2).Result;
        bool equal = _boardManager.BoardEquals(finalBoard, boardResult).Result;

        Assert.IsFalse(equal);
    }

    [Test]
    public void BoardSizeTooSmall()
    {
        try
        {
            List<List<bool>> board = new List<List<bool>>()
            {
                new() { false, false },
                new() { true, false }
            };
            
            string id;
            _boardManager.AddBoard(board, out id);
            Assert.IsTrue(false);
        }
        catch (Exception e)
        {
            Assert.Pass();
        }
    }
}