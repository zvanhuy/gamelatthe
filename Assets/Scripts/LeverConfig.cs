using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "MemoryMatch/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public int levelID;
    public int rows;
    public int columns;
    public float timeLimit; // Thời gian tối đa cho màn chơi
    public int matchScore = 100; // Điểm cơ bản khi ghép đúng 1 cặp
    public int maxMoves; // Số lượt lật bài tối đa (MỚI THÊM)
}