using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace LiveGameManager;

public class BoardManager : IBoardManager
{
    private readonly int[][] _directions =
    {
        new[] { -1, -1 }, new[] { -1, 0 }, new[] { -1, 1 },
        new[] { 0, -1 }, new[] { 0, 1 },
        new[] { 1, -1 }, new[] { 1, 0 }, new[] { 1, 1 }
    };

    private ConcurrentDictionary<string, List<List<bool>>> _boards = new();
    private ConcurrentDictionary<string, List<List<bool>>> _boardsEvols = new();

    private readonly string _pathFileSave;

    public BoardManager(string pathFileSave)
    {
        _pathFileSave = pathFileSave;

        if (!string.IsNullOrEmpty(_pathFileSave))
            if (!File.Exists(pathFileSave))
                SaveBoardsAsync().Wait();
            else
                LoadBoardsAsync().Wait();
    }

    public BoardManager(List<List<bool>> board, string pathFileSave, out string boardId)
    {
        if (string.IsNullOrWhiteSpace((pathFileSave)))
            throw new Exception("Path file save is empty");

        _pathFileSave = pathFileSave;

        if (File.Exists(pathFileSave))
            LoadBoardsAsync().Wait();

        // Defined 4 a minimum row board
        if (board is null || board.Count < 4 || board[0].Count < 4)
        {
            throw new Exception("Board size is too short");
        }

        // Check if all rows are same length
        int lenRow = board[0].Count;

        for (int row = 0; row < board.Count; row++)
        {
            if (board[row].Count != lenRow)
            {
                throw new Exception("Bad board size");
            }
        }

        AddBoard(board, out boardId);
    }

    public string AddBoard(List<List<bool>> board, out string boardId)
    {
        boardId = Guid.NewGuid().ToString();
        _boards.TryAdd(boardId, board);
        _boardsEvols.TryAdd(boardId, board);
        SaveBoardsAsync().Wait();

        return boardId;
    }

    public List<List<bool>> GetBoard(string boardId)
    {
        if (!_boardsEvols.TryGetValue(boardId, out var board))
            throw new Exception("Board not found");

        return board;
    }

    public List<List<bool>> GetInitialBoard(string boardId)
    {
        if (!_boards.TryGetValue(boardId, out var board))
            throw new Exception("Board not found");

        return board;
    }

    public async Task<List<List<bool>>> GetNextState(string boardId)
    {
        if (!_boardsEvols.TryGetValue(boardId, out var board))
            throw new Exception("Board not found");

        board = await EvolBoard(board);
        _boardsEvols[boardId] = board;
        await SaveBoardsAsync();

        return board;
    }

    public async Task<List<List<bool>>> GetState(string boardId, int stateNumber)
    {
        if (!_boards.TryGetValue(boardId, out var board))
            throw new Exception("Board not found");

        for (int i = 1; i <= stateNumber; i++)
        {
            board = await EvolBoard(board);
        }

        return board;
    }

    public async Task<List<List<bool>>> GetFinalState(string boardId, int maxTries)
    {
        if (!_boards.TryGetValue(boardId, out var board))
            throw new Exception("Board not found");
        
        List<List<bool>> lastBoard = new();
        int i;

        for (i = 1; i <= maxTries; i++)
        {
            lastBoard = CopyBoard(board);
            board = await EvolBoard(board);

            if (await BoardEquals(lastBoard, board))
                break;
        }

        if (i == maxTries && !(await BoardEquals(lastBoard, board)))
            throw new Exception("Max tries reached");
        
        return board;
    }

    private List<List<bool>> CopyBoard(List<List<bool>> board)
    {
        List<List<bool>> result = new List<List<bool>>();

        for (int i = 0; i < board.Count; i++)
        {
            List<bool> newRow = new List<bool>();

            for (int j = 0; j < board[i].Count; j++)
            {
                newRow.Add(board[i][j]); // Copy individual values
            }

            result.Add(newRow); // Add the new row to the result list
        }

        return result;
    }

    /*
        1 - Any live cell with fewer than two live neighbours dies, as if by underpopulation.
        2 - Any live cell with two or three live neighbours lives on to the next generation.
        3 - Any live cell with more than three live neighbours dies, as if by overpopulation.
        4 - Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.
    */
    private async Task<List<List<bool>>> EvolBoard(List<List<bool>> board)
    {
        return await Task.Run(() =>
        {
            int rows = board.Count;
            int cols = board[0].Count;
            List<List<bool>> newBoard = new List<List<bool>>();

            // Initialize
            for (int i = 0; i < rows; i++)
            {
                newBoard.Add(new List<bool>(new bool[cols])); // Initialize new row
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    int liveNeighbors = CountLiveNeighbors(board, i, j);

                    if (board[i][j])
                    {
                        // Rule 1 & 3: Cell dies if underpopulated (<2) or overpopulated (>3)
                        if (liveNeighbors < 2 || liveNeighbors > 3)
                            newBoard[i][j] = false;
                        else // Rule 2: Cell survives
                            newBoard[i][j] = true;
                    }
                    else
                    {
                        // Rule 4: A dead cell with exactly 3 neighbors becomes alive
                        if (liveNeighbors == 3)
                            newBoard[i][j] = true;
                    }
                }
            }

            return newBoard;
        });
    }

    public async Task<bool> BoardEquals(List<List<bool>> board1, List<List<bool>> board2)
    {
        return await Task.Run(() =>
        {
            for (int i = 0; i < board1.Count; i++)
            {
                for (int j = 0; j < board1[i].Count; j++)
                {
                    if (board1[i][j] != board2[i][j])
                    {
                        return false;
                    }
                }
            }

            return true;
        });
    }

    private int CountLiveNeighbors(List<List<bool>> board, int row, int col)
    {
        int rows = board.Count;
        int cols = board[0].Count;

        int count = 0;

        foreach (var dir in _directions)
        {
            int newRow = row + dir[0];
            int newCol = col + dir[1];

            // True is live, false is dead
            if (newRow >= 0 && newRow < rows && newCol >= 0 && newCol < cols && board[newRow][newCol])
            {
                count++;
            }
        }

        return count;
    }

    public async Task SaveBoardsAsync()
    {
        if (!string.IsNullOrEmpty(_pathFileSave))
        {
            try
            {
                var data = new
                {
                    Boards = _boards,
                    BoardsEvols = _boardsEvols
                };

                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = false });

                byte[] compressedData = CompressionManager.Compress(json);

                await File.WriteAllBytesAsync(_pathFileSave, compressedData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving boards: {ex.Message}");
            }
        }
    }

    private async Task LoadBoardsAsync()
    {
        if (string.IsNullOrEmpty(_pathFileSave) && !File.Exists(_pathFileSave))
            return;

        try
        {
            byte[] compressedData = await File.ReadAllBytesAsync(_pathFileSave);
            string json = CompressionManager.Decompress(compressedData);

            var data = JsonSerializer.Deserialize<BoardData>(json);

            if (data != null)
            {
                _boards = new ConcurrentDictionary<string, List<List<bool>>>(data.Boards);
                _boardsEvols = new ConcurrentDictionary<string, List<List<bool>>>(data.BoardsEvols);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading boards: {ex.Message}");
        }
    }

    private class BoardData
    {
        public Dictionary<string, List<List<bool>>> Boards { get; set; } = new();
        public Dictionary<string, List<List<bool>>> BoardsEvols { get; set; } = new();
    }
}