using UnityEngine;
using System.Collections.Generic;
using System;

public class SpriteManager : MonoBehaviour
{
    public static SpriteManager Instance { get; private set; }

    [Header("조커 카드 스프라이트 (id: joker_1 ~ joker_47)")]
    public List<Sprite> jokerSprites; // 인스펙터에서 id 순서대로 할당
    [Header("행성 카드 스프라이트 (id: planet_1 ~ planet_9)")]
    public List<Sprite> planetSprites;
    [Header("타로 카드 스프라이트 (id: tarot_1 ~ tarot_10)")]
    public List<Sprite> tarotSprites;

    [Header("백판 스프라이트")]
    public Sprite jokerBackSprite;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }
}