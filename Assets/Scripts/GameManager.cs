using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Dùng để điều khiển TextMeshPro

public class GameManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject menuPanel;   // Màn chọn level
    public GameObject gamePanel;   // Màn chơi
    public GameObject losePanel;   // Bảng báo thua

    [Header("UI Text")]
    public TextMeshProUGUI scoreText; // Chữ hiển thị điểm
    public TextMeshProUGUI movesText; // Chữ hiển thị lượt còn lại

    [Header("Game Settings")]
    public LevelConfig[] allLevels;   // Danh sách level
    public GameObject cardPrefab;     // Prefab thẻ
    public Transform cardBoard;       // Nơi chứa các thẻ trong Canvas
    public List<Sprite> cardSprites;  // Danh sách ảnh mặt trước thẻ

    private LevelConfig currentLevel;

    private List<CardController> cardsInGame = new List<CardController>();

    private CardController firstCard;
    private CardController secondCard;

    private bool canClick = true;
    private bool isGameOver = false;

    private int matchesFound = 0;
    private int currentScore = 0;
    private int currentCombo = 0;
    private int movesRemaining = 0;

    void Start()
    {
        // Khi mới mở game: hiện menu, ẩn màn chơi
        if (menuPanel != null)
            menuPanel.SetActive(true);

        if (gamePanel != null)
            gamePanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(false);
    }

    public void StartLevel(int levelIndex)
    {
        // Kiểm tra tránh lỗi nếu chọn sai level
        if (allLevels == null || allLevels.Length == 0)
        {
            Debug.LogError("Chưa gán danh sách allLevels trong GameManager.");
            return;
        }

        if (levelIndex < 0 || levelIndex >= allLevels.Length)
        {
            Debug.LogError("Level index không hợp lệ: " + levelIndex);
            return;
        }

        currentLevel = allLevels[levelIndex];

        // Reset thông số khi bắt đầu level mới
        matchesFound = 0;
        currentScore = 0;
        currentCombo = 0;
        movesRemaining = currentLevel.maxMoves;

        firstCard = null;
        secondCard = null;

        canClick = true;
        isGameOver = false;

        UpdateUIText();

        // Chuyển từ menu sang màn chơi
        if (menuPanel != null)
            menuPanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(false);

        if (gamePanel != null)
            gamePanel.SetActive(true);

        GenerateBoard();
    }

    void UpdateUIText()
    {
        if (scoreText != null)
            scoreText.text = currentScore.ToString();

        if (movesText != null)
            movesText.text = movesRemaining.ToString();
    }

    void GenerateBoard()
    {
        // Xóa thẻ cũ nếu có
        foreach (Transform child in cardBoard)
        {
            Destroy(child.gameObject);
        }

        cardsInGame.Clear();

        int totalCards = currentLevel.rows * currentLevel.columns;
        int totalPairs = totalCards / 2;

        // Kiểm tra đủ ảnh chưa
        if (cardSprites == null || cardSprites.Count < totalPairs)
        {
            Debug.LogError("Không đủ ảnh thẻ. Level này cần ít nhất " + totalPairs + " ảnh mặt trước.");
            return;
        }

        List<int> cardIDs = new List<int>();

        // Tạo các cặp thẻ
        // Ví dụ totalPairs = 2 thì tạo: 0,0,1,1
        for (int i = 0; i < totalPairs; i++)
        {
            cardIDs.Add(i);
            cardIDs.Add(i);
        }

        // Trộn thẻ ngẫu nhiên
        Shuffle(cardIDs);

        // Sinh thẻ ra màn hình
        for (int i = 0; i < totalCards; i++)
        {
            GameObject newCardObj = Instantiate(cardPrefab, cardBoard);
            CardController card = newCardObj.GetComponent<CardController>();

            int id = cardIDs[i];

            card.Setup(id, cardSprites[id], OnCardSelected);

            cardsInGame.Add(card);
        }
    }

    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);

            int temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void OnCardSelected(CardController selectedCard)
    {
        if (!canClick || isGameOver)
            return;

        if (selectedCard == null)
            return;

        // Chống bấm lại đúng thẻ đầu tiên
        if (selectedCard == firstCard)
            return;

        selectedCard.FlipToFront();

        if (firstCard == null)
        {
            firstCard = selectedCard;
            return;
        }

        secondCard = selectedCard;
        canClick = false;

        // Lật đủ 2 thẻ mới tính là 1 lượt
        movesRemaining--;

        if (movesRemaining < 0)
            movesRemaining = 0;

        UpdateUIText();

        StartCoroutine(CheckMatch());
    }

    IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(0.6f);

        if (firstCard == null || secondCard == null)
        {
            canClick = true;
            yield break;
        }

        // Nếu 2 thẻ giống nhau
        if (firstCard.GetCardID() == secondCard.GetCardID())
        {
            firstCard.SetMatched();
            secondCard.SetMatched();

            matchesFound++;

            // Combo tăng khi đúng liên tiếp
            currentCombo++;

            // Điểm = điểm cơ bản x combo
            currentScore += currentLevel.matchScore * currentCombo;

            UpdateUIText();

            int totalPairs = (currentLevel.rows * currentLevel.columns) / 2;

            // Nếu tìm đủ cặp thì thắng
            if (matchesFound >= totalPairs)
            {
                StartCoroutine(LevelComplete());
            }
            else if (movesRemaining <= 0)
            {
                GameOver();
            }
        }
        else
        {
            // Nếu sai thì úp lại
            firstCard.FlipToBack();
            secondCard.FlipToBack();

            // Sai thì mất combo
            currentCombo = 0;

            if (movesRemaining <= 0)
            {
                GameOver();
            }
        }

        firstCard = null;
        secondCard = null;

        // Nếu chưa thua thì cho bấm tiếp
        if (!isGameOver)
        {
            canClick = true;
        }
    }

    void GameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        canClick = false;

        if (losePanel != null)
            losePanel.SetActive(true);

        // Sau khi hiện bảng thua, chờ 3 giây rồi quay lại menu chọn level
        StartCoroutine(ReturnToMenuAfterLose());
    }

    IEnumerator ReturnToMenuAfterLose()
    {
        yield return new WaitForSeconds(3f);

        ReturnToMenu();
    }

    public void ReturnToMenu()
    {
        isGameOver = false;
        canClick = true;

        firstCard = null;
        secondCard = null;

        if (gamePanel != null)
            gamePanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(false);

        if (menuPanel != null)
            menuPanel.SetActive(true);
    }

    IEnumerator LevelComplete()
    {
        canClick = false;

        yield return new WaitForSeconds(1.5f);

        ReturnToMenu();
    }
}