using System;
using System.Collections.Generic;
using System.Linq;
using BalatroOnline.Game;
using UnityEngine;
// HandContext, Suit, CardInfo는 HandContext.cs에서 정의됨

public enum JokerEffectTiming
{
    OnScoring,
    OnRoundStart,
    OnHandPlay,
    OnAfterScoring,
    OnRoundClear,
}

public class JokerEffect
{
    public JokerEffectTiming timing;
    public Action<HandContext, JokerCard> applyEffect;
}

public class JokerCard
{
    public string id;
    public string name;
    public string description;
    public int price;
    public int sprite;
    public float basevalue;
    public float increase;
    public float decrease;
    public int maxvalue;
    public List<JokerEffect> effects;

    public string GetDescription()
    {
        string desc = description;
        desc = desc.Replace("[basevalue]", basevalue.ToString());
        desc = desc.Replace("[increase]", increase.ToString());
        desc = desc.Replace("[decrease]", decrease.ToString());
        return desc;
    }
}

public class JokerManager : MonoBehaviour
{
    public static JokerManager Instance;
    public List<JokerCard> myJokers = new List<JokerCard>();
    public List<JokerCard> allJokers = new List<JokerCard>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
        RegisterAllJokers();
    }

    public void ApplyJokerEffects(JokerEffectTiming timing, HandContext context)
    {
        foreach (var joker in myJokers)
        {
            if (joker.effects == null) continue;
            foreach (var effect in joker.effects)
            {
                if (effect.timing == timing && effect.applyEffect != null)
                {
                    effect.applyEffect(context, joker);
                }
            }
        }
    }

    private void RegisterAllJokers()
    {
        allJokers.Clear();
        // 1~47번 조커 등록 (csv의 descText, 효과를 최대한 반영)
        allJokers.Add(new JokerCard
        {
            id = "joker_1",
            name = "조커 A",
            description = "다이아몬드로 득점 시마다 배수가 <color=red>+2</color> 된다.",
            price = 2,
            sprite = 0,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.currentCardData.suit == CardType.Diamonds){
                            ctx.Multiplier += 2;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_2",
            name = "조커 B",
            description = "내 플레이 카드에 페어가 포함되어있으면 배수 <color=red>+2</color> 한다.",
            price = 3,
            sprite = 1,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.HasPairInPlayedCards()) {
                            ctx.Multiplier += 2;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_3",
            name = "조커 C",
            description =
                "원페어 시 <color=red>x[basevalue]</color>배. 원페어가 플레이 될 때마다 <color=green>x[increase]</color>배가 성장한다. (최대 30)",
            price = 5,
            sprite = 2,
            basevalue = 1,
            increase = 0.2f,
            decrease = 0,
            maxvalue = 30,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnHandPlay,
                    applyEffect = (ctx, self) => {
                        if (ctx.PokerHand == PokerHand.OnePair) {
                            ctx.Multiplier *= self.basevalue;
                        }
                    }
                },
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.PokerHand == PokerHand.OnePair) {
                            self.basevalue += self.increase;
                            if (self.basevalue > self.maxvalue) self.basevalue = self.maxvalue;
                        }
                    }
                },
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_4",
            name = "조커 D",
            description =
                "투페어 시 <color=red>x[basevalue]</color>배. 투페어가 플레이 될 때마다 <color=green>x[increase]</color>배가 성장한다. (최대 30)",
            price = 4,
            sprite = 3,
            basevalue = 1,
            increase = 0.3f,
            decrease = 0,
            maxvalue = 30,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnHandPlay,
                    applyEffect = (ctx, self) => {
                        if (ctx.PokerHand == PokerHand.TwoPair) {
                            ctx.Multiplier *= self.basevalue;
                        }
                    }
                },
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.PokerHand == PokerHand.TwoPair) {
                            self.basevalue += self.increase;
                            if (self.basevalue > self.maxvalue) self.basevalue = self.maxvalue;
                        }
                    }
                },
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_5",
            name = "조커 E",
            description =
                "트리플 시 <color=red>x[basevalue]</color>배. 트리플이 플레이 될 때마다 <color=green>x[increase]</color>배가 성장한다. (최대 30)",
            price = 4,
            sprite = 4,
            basevalue = 1,
            increase = 0.4f,
            decrease = 0,
            maxvalue = 30,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnHandPlay,
                    applyEffect = (ctx, self) => {
                        if (ctx.PokerHand == PokerHand.ThreeOfAKind) {
                            ctx.Multiplier *= self.basevalue;
                        }
                    }
                },
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.PokerHand == PokerHand.ThreeOfAKind) {
                            self.basevalue += self.increase;
                            if (self.basevalue > self.maxvalue) self.basevalue = self.maxvalue;
                        }
                    }
                },
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_6",
            name = "조커 F",
            description =
                "포카드 시 <color=red>x[basevalue]</color>배. 포카드가 플레이 될 때마다 <color=green>x[increase]</color>배가 성장한다. (최대 30)",
            price = 4,
            sprite = 5,
            basevalue = 1,
            increase = 0.7f,
            decrease = 0,
            maxvalue = 30,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnHandPlay,
                    applyEffect = (ctx, self) => {
                        if (ctx.PokerHand == PokerHand.FourOfAKind) {
                            ctx.Multiplier *= self.basevalue;
                        }
                    }
                },
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.PokerHand == PokerHand.FourOfAKind) {
                            self.basevalue += self.increase;
                            if (self.basevalue > self.maxvalue) self.basevalue = self.maxvalue;
                        }
                    }
                },
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_7",
            name = "조커 G",
            description =
                "풀하우스 시 <color=red>x[basevalue]</color>배. 풀하우스가 플레이 될 때마다 <color=green>x[increase]</color>배가 성장한다. (최대 30)",
            price = 4,
            sprite = 6,
            basevalue = 1,
            increase = 0.5f,
            decrease = 0,
            maxvalue = 30,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnHandPlay,
                    applyEffect = (ctx, self) => {
                        if (ctx.PokerHand == PokerHand.FullHouse) {
                            ctx.Multiplier *= self.basevalue;
                        }
                    }
                },
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.PokerHand == PokerHand.FullHouse) {
                            self.basevalue += self.increase;
                            if (self.basevalue > self.maxvalue) self.basevalue = self.maxvalue;
                        }
                    }
                },
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_8",
            name = "조커 H",
            description =
                "하이카드 시 <color=red>x[basevalue]</color>배. 하이카드가 플레이 될 때마다 <color=green>x[increase]</color>배가 성장한다. (최대 30)",
            price = 4,
            sprite = 7,
            basevalue = 1,
            increase = 0.1f,
            decrease = 0,
            maxvalue = 30,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnHandPlay,
                    applyEffect = (ctx, self) => {
                        if (ctx.PokerHand == PokerHand.HighCard) {
                            ctx.Multiplier *= self.basevalue;
                        }
                    }
                },
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.PokerHand == PokerHand.HighCard) {
                            self.basevalue += self.increase;
                            if (self.basevalue > self.maxvalue) self.basevalue = self.maxvalue;
                        }
                    }
                },
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_9",
            name = "조커 I",
            description =
                "스트레이트 시 <color=red>x[basevalue]</color>배. 스트레이트가 플레이 될 때마다 <color=green>x[increase]</color>배가 성장한다. (최대 30)",
            price = 4,
            sprite = 8,
            basevalue = 1,
            increase = 0.4f,
            decrease = 0,
            maxvalue = 30,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnHandPlay,
                    applyEffect = (ctx, self) => {
                        if (ctx.PokerHand == PokerHand.Straight) {
                            ctx.Multiplier *= self.basevalue;
                        }
                    }
                },
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.PokerHand == PokerHand.Straight) {
                            self.basevalue += self.increase;
                            if (self.basevalue > self.maxvalue) self.basevalue = self.maxvalue;
                        }
                    }
                },
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_10",
            name = "조커 J",
            description =
                "플러시 시 <color=red>x[basevalue]</color>배. 플러시가 플레이 될 때마다 <color=green>x[increase]</color>배가 성장한다. (최대 30)",
            price = 4,
            sprite = 9,
            basevalue = 1,
            increase = 0.4f,
            decrease = 0,
            maxvalue = 30,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnHandPlay,
                    applyEffect = (ctx, self) => {
                        if (ctx.PokerHand == PokerHand.Flush) {
                            ctx.Multiplier *= self.basevalue;
                        }
                    }
                },
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.PokerHand == PokerHand.Flush) {
                            self.basevalue += self.increase;
                            if (self.basevalue > self.maxvalue) self.basevalue = self.maxvalue;
                        }
                    }
                },
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_11",
            name = "조커 K",
            description = "핸드플레이 시, 내 핸드에 페어가 남아있으면 배수 <color=red>3배</color>한다.",
            price = 4,
            sprite = 10,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            maxvalue = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.HasPairInUnUsedCards()) {
                            ctx.Multiplier += 3;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_12",
            name = "조커 M",
            description = "핸드플레이 시, 내 핸드에 트리플이 남아있으면 배수 <color=red>6배</color>한다.",
            price = 4,
            sprite = 12,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            maxvalue = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.HasTripleInUnUsedCards()) {
                            ctx.Multiplier += 6;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_13",
            name = "조커 N",
            description = "핸드플레이 시, 내 핸드에 포 카드가 남아있으면 배수 <color=red>25배</color>한다.",
            price = 4,
            sprite = 13,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            maxvalue = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.UnUsedPokerHand == PokerHand.FourOfAKind) {
                            ctx.Multiplier += 25;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_14",
            name = "조커 O",
            description = "내 패에 스트레이트가 포함되어있으면 배수 <color=red>+4</color> 한다.",
            price = 4,
            sprite = 14,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            maxvalue = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnHandPlay,
                    applyEffect = (ctx, self) => {
                        if (ctx.UnUsedPokerHand == PokerHand.Straight) {
                            ctx.Multiplier += 4;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_15",
            name = "조커 P",
            description = "무조건 배수 <color=red>+1</color> 한다.",
            price = 4,
            sprite = 15,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            maxvalue = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnHandPlay,
                    applyEffect = (ctx, self) => {
                        ctx.Multiplier += 1;
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_16",
            name = "조커 Q",
            description = "내 패에 트리플이 포함되어있으면 배수 <color=red>+3</color> 한다.",
            price = 4,
            sprite = 16,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            maxvalue = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnHandPlay,
                    applyEffect = (ctx, self) => {
                        if (ctx.HasTripleInUnUsedCards()) {
                            ctx.Multiplier += 3;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_17",
            name = "조커 R",
            description = "내 패에 포카드가 포함되어있으면 배수 <color=red>+5</color> 한다.",
            price = 4,
            sprite = 17,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            maxvalue = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnHandPlay,
                    applyEffect = (ctx, self) => {
                        if (ctx.UnUsedPokerHand == PokerHand.FourOfAKind) {
                            ctx.Multiplier += 5;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_18",
            name = "조커 S",
            description = "내 패에 풀하우스가 포함되어있으면 배수 <color=red>+4</color> 한다.",
            price = 4,
            sprite = 18,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            maxvalue = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnHandPlay,
                    applyEffect = (ctx, self) => {
                        if (ctx.UnUsedPokerHand == PokerHand.FullHouse) {
                            ctx.Multiplier += 4;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_19",
            name = "조커 T",
            description = "내 패에 플러시가 포함되어있으면 배수 <color=red>+4</color> 한다.",
            price = 4,
            sprite = 19,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            maxvalue = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnHandPlay,
                    applyEffect = (ctx, self) => {
                        if (ctx.UnUsedPokerHand == PokerHand.Flush) {
                            ctx.Multiplier += 4;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_20",
            name = "조커 U",
            description =
                "하트로 득점 시마다, 해당 카드의 득점 시 칩스가 <color=blue>+10</color> 성장한다.",
            price = 2,
            sprite = 20,
            basevalue = 0,
            increase = 10,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.currentCardData.suit == CardType.Hearts) {
                            ctx.currentCard.ShowAndStoreCardScore(self.basevalue);
                            ctx.Chips += self.basevalue;
                            self.basevalue += self.increase;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_21",
            name = "조커 V",
            description =
                "스페이드로 득점 시마다, 해당 카드의 득점 시 칩스가 <color=blue>+10</color> 성장한다.",
            price = 2,
            sprite = 21,
            basevalue = 0,
            increase = 10,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.currentCardData.suit == CardType.Spades) {
                            ctx.currentCard.ShowAndStoreCardScore(self.basevalue);
                            ctx.Chips += self.basevalue;
                            self.basevalue += self.increase;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_22",
            name = "조커 W",
            description =
                "클럽으로 득점 시마다, 해당 카드의 득점 시 칩스가 <color=blue>+10</color> 성장한다.",
            price = 2,
            sprite = 22,
            basevalue = 0,
            increase = 10,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.currentCardData.suit == CardType.Clubs) {
                            ctx.currentCard.ShowAndStoreCardScore(self.basevalue);
                            ctx.Chips += self.basevalue;
                            self.basevalue += self.increase;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_23",
            name = "조커 X",
            description =
                "핸드플레이 시, 내 핸드에 하트가 남아있는 카드 한 장당 배수가 <color=red>+2</color> 된다.",
            price = 2,
            sprite = 23,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        ctx.Multiplier += ctx.CountUnUsedCardsOfSuit(CardType.Hearts) * 2;
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_24",
            name = "조커 Y",
            description =
                "핸드플레이 시, 내 핸드에 스페이드가 남아있는 카드 한 장당 배수가 <color=red>+2</color> 된다.",
            price = 2,
            sprite = 24,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        ctx.Multiplier += ctx.CountUnUsedCardsOfSuit(CardType.Spades) * 2;
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_25",
            name = "조커 Z",
            description =
                "핸드플레이 시, 내 핸드에 클럽이 남아있는 카드 한 장당 배수가 <color=red>+2</color> 된다.",
            price = 2,
            sprite = 25,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        ctx.Multiplier += ctx.CountUnUsedCardsOfSuit(CardType.Clubs) * 2;
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_26",
            name = "조커 AA",
            description =
                "핸드플레이 시, 내 핸드에 다이아몬드가 남아있는 카드 한 장당 배수가 <color=red>+2</color> 된다.",
            price = 2,
            sprite = 26,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        ctx.Multiplier += ctx.CountUnUsedCardsOfSuit(CardType.Diamonds) * 2;
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_27",
            name = "조커 AB",
            description =
                "득점에 사용된 에이스 한 장당, 칩스 <color=red>+20</color> 배수 <color=blue>+4</color> 된다.",
            price = 4,
            sprite = 27,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnScoring,
                    applyEffect = (ctx, self) => {
                        int aceCount = ctx.CountAcesInUsedCards();
                        ctx.Chips += 20 * aceCount;
                        ctx.Multiplier += 4 * aceCount;
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_28",
            name = "조커 AC",
            description =
                "배수 <color=red>+[basevalue]</color>. 라운드 종료 시 마다 배수가 <color=blue>-[decrease]</color> 된다.",
            price = 4,
            sprite = 28,
            basevalue = 20,
            increase = 0,
            decrease = 4,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnHandPlay,
                    applyEffect = (ctx, self) => {
                        ctx.Multiplier += self.basevalue;
                    }
                },
                new JokerEffect {
                    timing = JokerEffectTiming.OnRoundClear,
                    applyEffect = (ctx, self) => {
                        self.basevalue -= self.decrease;
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_29",
            name = "조커 AD",
            description =
                "득점한 모든 카드의 득점 시 칩스가 <color=green>+3</color> 성장한다.",
            price = 4,
            sprite = 29,
            basevalue = 0,
            increase = 3,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnScoring,
                    applyEffect = (ctx, self) => {
                        ctx.currentCard.ShowAndStoreCardScore(self.basevalue);
                        ctx.Chips += self.basevalue;
                        self.basevalue += self.increase;
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_30",
            name = "조커 AE",
            description =
                "전체 덱에 보유한 7 한장 당 배수가 <color=red>+2</color> 된다.",
            price = 4,
            sprite = 30,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnHandPlay,
                    applyEffect = (ctx, self) => {
                        ctx.Multiplier += ctx.remainingSevens * 2;
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_31",
            name = "조커 AF",
            description =
                "전체 덱카드가 52장 보다 적으면, 그 차이 당 배수가 <color=red>+4</color> 된다.",
            price = 4,
            sprite = 31,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnHandPlay,
                    applyEffect = (ctx, self) => {
                        ctx.Multiplier += ctx.remainingDeck * 4;
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_32",
            name = "조커 AG",
            description =
                "스페이드 카드로 득점 시 배수 <color=red>x[basevalue]</color> 스페이드 카드 득점 시마다 배수가 <color=green>[increase]</color> 성장한다. 다른 카드 득점 시마다 배수가 <color=blue>[decrease]</color> 감퇴한다.",
            price = 4,
            sprite = 32,
            basevalue = 1,
            increase = 1,
            decrease = 2,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.currentCardData.suit == CardType.Spades) {
                            ctx.Multiplier *= self.basevalue;
                            self.basevalue += self.increase;
                        }
                        else {
                            self.basevalue -= self.decrease;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_33",
            name = "조커 AH",
            description =
                "다이아 카드로 득점 시 배수 <color=red>x[basevalue]</color> 다이아 카드 득점 시마다 배수가 <color=green>[increase]</color> 성장한다. 다른 카드 득점 시마다 배수가 <color=blue>[decrease]</color> 감퇴한다.",
            price = 4,
            sprite = 33,
            basevalue = 1,
            increase = 1,
            decrease = 2,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.currentCardData.suit == CardType.Diamonds) {
                            ctx.Multiplier *= self.basevalue;
                            self.basevalue += self.increase;
                        }
                        else {
                            self.basevalue -= self.decrease;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_34",
            name = "조커 AI",
            description =
                "하트 카드로 득점 시 배수 <color=red>x[basevalue]</color> 하트 카드 득점 시마다 배수가 <color=green>[increase]</color> 성장한다. 다른 카드 득점 시마다 배수가 <color=blue>[decrease]</color> 감퇴한다.",
            price = 4,
            sprite = 34,
            basevalue = 1,
            increase = 1,
            decrease = 2,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.currentCardData.suit == CardType.Hearts) {
                            ctx.Multiplier *= self.basevalue;
                            self.basevalue += self.increase;
                        }
                        else {
                            self.basevalue -= self.decrease;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_35",
            name = "조커 AJ",
            description =
                "클럽 카드로 득점 시 배수 <color=red>x[basevalue]</color> 클럽 카드 득점 시마다 배수가 <color=green>[increase]</color> 성장한다. 다른 카드 득점 시마다 배수가 <color=blue>[decrease]</color> 감퇴한다.",
            price = 4,
            sprite = 35,
            basevalue = 1,
            increase = 1,
            decrease = 2,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.currentCardData.suit == CardType.Clubs) {
                            ctx.Multiplier *= self.basevalue;
                            self.basevalue += self.increase;
                        }
                        else {
                            self.basevalue -= self.decrease;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_36",
            name = "조커 AK",
            description =
                "스페이드 1장으로 플레이 시, 칩스 <color=red>x20</color> 된다.",
            price = 4,
            sprite = 36,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.IsUsedCardsOfSuitCount(CardType.Spades, 1)) {
                            ctx.Chips *= 20;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_37",
            name = "조커 AL",
            description =
                "다이아몬드 4장으로 득점 시, 배수 <color=red>x12</color> 된다.",
            price = 4,
            sprite = 37,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.IsUsedCardsOfSuitCount(CardType.Diamonds, 4)) {
                            ctx.Multiplier *= 12;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_38",
            name = "조커 AM",
            description = "하트 2장으로 득점 시, 배수 <color=red>x18</color> 된다.",
            price = 4,
            sprite = 38,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.IsUsedCardsOfSuitCount(CardType.Hearts, 2)) {
                            ctx.Multiplier *= 18;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_39",
            name = "조커 AN",
            description = "클럽 3장으로 득점 시, 칩스 <color=red>x15</color> 된다.",
            price = 4,
            sprite = 39,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.IsUsedCardsOfSuitCount(CardType.Clubs, 3)) {
                            ctx.Chips *= 15;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_40",
            name = "조커 AO",
            description =
                "왼쪽 조커와 동일한 기능을 한다. (레벨은 자신의 레벨로 적용된다.)",
            price = 4,
            sprite = 40,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_41",
            name = "조커 AP",
            description = "남은 버리기 1 당 칩스가 <color=red>+20</color> 된다.",
            price = 5,
            sprite = 41,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.remainingDiscards > 0) {
                            ctx.Chips += 20 * ctx.remainingDiscards;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_42",
            name = "조커 AQ",
            description =
                "제거 예정.......남은 핸드플레이 1 당 배수가 <color=red>+2</color>, 칩스는 <color=blue>-30</color> 된다.",
            price = 5,
            sprite = 42,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            effects = new List<JokerEffect>()
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_43",
            name = "조커 AR",
            description = "버리기가 0번 남았을 때 배수가 <color=red>+15</color> 된다.",
            price = 5,
            sprite = 43,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnAfterScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.remainingDiscards <= 0) {
                            ctx.Multiplier += 15;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_44",
            name = "조커 AS",
            description = "랜덤으로 배수가 <color=red>+2 ~ 20</color> 된다.",
            price = 5,
            sprite = 44,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnHandPlay,
                    applyEffect = (ctx, self) => {
                        ctx.Multiplier += UnityEngine.Random.Range(2, 21);
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_45",
            name = "조커 AU",
            description = "짝수 카드 점수 시 마다, 배수 <color=red>+2</color> 된다.",
            price = 5,
            sprite = 46,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnScoring,
                    applyEffect = (ctx, self) => {
                        if (ctx.IsCurrentCardDataEvenRank()) {
                            ctx.Multiplier += 2;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_46",
            name = "조커 AV",
            description = "홀수 카드 점수 시 마다, 배수 <color=red>+2</color> 된다.",
            price = 5,
            sprite = 47,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnScoring,
                    applyEffect = (ctx, self) => {
                        if (!ctx.IsCurrentCardDataEvenRank()) {
                            ctx.Multiplier += 2;
                        }
                    }
                }
            }
        });
        allJokers.Add(new JokerCard
        {
            id = "joker_47",
            name = "조커 AW",
            description =
                "덱에 남아 있는 카드 1장 당 칩스가 <color=red>+2</color> 된다.",
            price = 5,
            sprite = 48,
            basevalue = 0,
            increase = 0,
            decrease = 0,
            effects = new List<JokerEffect> {
                new JokerEffect {
                    timing = JokerEffectTiming.OnHandPlay,
                    applyEffect = (ctx, self) => {
                        ctx.Chips += ctx.remainingDeck * 2;
                    }
                }
            }
        });
    }

    public void AddJokerById(string id)
    {
        var joker = allJokers.FirstOrDefault(j => j.id == id);
        if (joker != null && !myJokers.Any(j => j.id == id))
        {
            myJokers.Add(joker);
        }
    }
}