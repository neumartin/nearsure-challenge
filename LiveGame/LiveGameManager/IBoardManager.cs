using System.Collections;

namespace LiveGameManager;

public interface IBoardManager
{
    string AddBoard(List<List<bool>> board, out string boardId);
    List<List<bool>> GetBoard(string boardId);
    List<List<bool>> GetInitialBoard(string boardId);
    Task<List<List<bool>>> GetNextState(string boardId);
    Task<List<List<bool>>> GetState(string boardId, int stateNumber);
    Task<List<List<bool>>> GetFinalState(string boardId, int maxTries);
    Task<bool> BoardEquals(List<List<bool>> board1, List<List<bool>> board2);
}