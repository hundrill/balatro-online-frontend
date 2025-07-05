using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class CardJoker : MonoBehaviour
{
    public Image cardImage;
    public TextMeshProUGUI priceText;

    [HideInInspector] public string id;
    [HideInInspector] public string type; // "joker", "planet", "tarot"
    [HideInInspector] public int price;
    [HideInInspector] public string cardName;
    [HideInInspector] public string cardDesc;

    private Action<CardJoker> onClick; // 외부 콜백
    private bool isSelected = false; // 선택 상태

    public void SetData(string id, string type, int price, Sprite sprite, string name, string desc, Action<CardJoker> onClickCallback = null)
    {
        this.id = id;
        this.type = type;
        this.price = price;
        this.cardName = name;
        this.cardDesc = desc;
        this.onClick = onClickCallback;
        if (priceText) priceText.text = string.Format("${0}", price);
        if (cardImage) cardImage.sprite = sprite;
        
        // 초기 선택 상태 해제
        SetSelected(false);
        
        // 버튼 클릭 이벤트 자동 연결 (있으면)
        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClickShowInfo);
        }

        // Debug.Log(cardName + " " + cardDesc + " 222222");
    }

    // 샵에서 카드 클릭 시 외부 콜백 호출
    public void OnClickShowInfo()
    {
        if (onClick != null)
            onClick(this);
    }

    // 선택 상태 설정
    public void SetSelected(bool selected)
    {
        isSelected = selected;
                
        // 선택된 카드는 약간 확대 효과 (선택사항)
        if (selected)
        {
            transform.localScale = Vector3.one * 1.1f;
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }

    // 현재 선택 상태 반환
    public bool IsSelected()
    {
        return isSelected;
    }

    // // 샵에서 선택/구매 등 UI 이벤트 연결
    // public void OnClickBuy()
    // {
    //     // ShopManager.Instance.BuyJoker(this);
    // }
}