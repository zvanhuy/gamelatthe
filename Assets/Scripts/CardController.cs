using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class CardController : MonoBehaviour
{
    public Image frontImage;
    public GameObject backImage;
    public CanvasGroup canvasGroup;

    private int cardID;
    private bool isFlipped = false;
    private bool isMatched = false;
    private Action<CardController> onCardClicked;

    public void Setup(int id, Sprite frontSprite, Action<CardController> onClickCallback)
    {
        cardID = id;
        frontImage.sprite = frontSprite;
        onCardClicked = onClickCallback;
        GetComponent<Button>().onClick.AddListener(OnCardClick);
    }

    public int GetCardID() => cardID;
    public bool GetIsMatched() => isMatched;

    private void OnCardClick()
    {
        if (isFlipped || isMatched) return;

        // Gọi Event lật thẻ
        onCardClicked?.Invoke(this);
    }

    public void FlipToFront()
    {
        isFlipped = true;
        // Hiệu ứng lật nửa vòng, đổi ảnh, lật nốt nửa vòng
        transform.DORotate(new Vector3(0, 90, 0), 0.2f).OnComplete(() =>
        {
            backImage.SetActive(false);
            frontImage.gameObject.SetActive(true);
            transform.DORotate(new Vector3(0, 0, 0), 0.2f);
        });
    }

    public void FlipToBack()
    {
        isFlipped = false;
        transform.DORotate(new Vector3(0, 90, 0), 0.2f).OnComplete(() =>
        {
            frontImage.gameObject.SetActive(false);
            backImage.SetActive(true);
            transform.DORotate(new Vector3(0, 0, 0), 0.2f);
        });
    }

    public void SetMatched()
    {
        isMatched = true;
        // Giữ thẻ lại trên bàn chơi nhưng làm mờ đi khi ghép đúng
        canvasGroup.DOFade(0.5f, 0.5f);
    }
}