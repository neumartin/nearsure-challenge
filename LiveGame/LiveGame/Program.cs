// See https://aka.ms/new-console-template for more information


using LiveGameManager;

List<List<bool>> board = new List<List<bool>>()
{
    new() {false, false, false, false, false, false},
    new() {true, false, false, false, false, true},
    new() {false, true, true, false, false, false},
    new() {false, false, true, true, false, false},
    new() {true, false, true, false, false, false},
    new() {false, true, false, false, false, false},
    new() {true, false, true, false, true, false},
    new() {false, false, false, true, false, true},
};

string id;
IBoardManager boardManager = new BoardManager(board, "/Users/martin/Temp/nearsure.data", out id);

DrawBoard(id);

void DrawBoard(string boardId)
{
    var boardToUse = boardManager.GetBoard(boardId);
    for (int k = 0; k < 10; k++)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Board: " + boardId);

        for (int i = 0; i < boardToUse.Count; i++)
        {
            for (int j = 0; j < boardToUse[i].Count; j++)
            {
                Console.ForegroundColor = boardToUse[i][j] ? ConsoleColor.Green : ConsoleColor.DarkRed;
                Console.Write(boardToUse[i][j] ? "*" : "-");
                Console.Write(" ");
            }

            Console.WriteLine();
        }
    
        Thread.Sleep(1000);
        boardToUse = boardManager.GetNextState(boardId).Result;
        Console.ForegroundColor = ConsoleColor.Gray;
    }
}