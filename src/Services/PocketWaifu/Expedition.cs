#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.Linq;
using Sanakan.Database.Models;
using Sanakan.Extensions;
using Sanakan.Services.Time;

namespace Sanakan.Services.PocketWaifu
{
    public class Expedition
    {
        public enum ItemDropType
        {
            None, Food, Common, Rare, Legendary
        }

        private static List<ItemType> _ultimateExpeditionItems = new List<(ItemType, int)>
        {
            (ItemType.FigureBodyPart,           12),
            (ItemType.FigureClothesPart,        12),
            (ItemType.FigureHeadPart,           12),
            (ItemType.FigureLeftArmPart,        12),
            (ItemType.FigureLeftLegPart,        12),
            (ItemType.FigureRightArmPart,       12),
            (ItemType.FigureRightLegPart,       12),
            (ItemType.FigureSkeleton,           8),
            (ItemType.FigureUniversalPart,      6),
            (ItemType.LotteryTicket,            2),
        }.ToRealList();

        private static List<ItemType> _ultimateExpeditionHardcoreItems = new List<(ItemType, int)>
        {
            (ItemType.FigureBodyPart,           12),
            (ItemType.FigureClothesPart,        12),
            (ItemType.FigureHeadPart,           12),
            (ItemType.FigureLeftArmPart,        12),
            (ItemType.FigureLeftLegPart,        12),
            (ItemType.FigureRightArmPart,       12),
            (ItemType.FigureRightLegPart,       12),
            (ItemType.FigureUniversalPart,      9),
            (ItemType.FigureSkeleton,           9),
            (ItemType.IncreaseUltimateAttack,   5),
            (ItemType.IncreaseUltimateDefence,  5),
            (ItemType.LotteryTicket,            4),
            (ItemType.IncreaseUltimateHealth,   1),
        }.ToRealList();

        private static Dictionary<ItemDropType, List<ItemType>> _itemsFromNormalExpedition = new Dictionary<ItemDropType, List<ItemType>>
        {
            {ItemDropType.Food, new List<(ItemType, int)>
                {
                    (ItemType.AffectionRecoverySmall,   15),
                    (ItemType.AffectionRecoveryNormal,  10),
                    (ItemType.AffectionRecoveryBig,     3),
                    (ItemType.AffectionRecoveryGreat,   1),
                }.ToRealList()
            },
            {ItemDropType.Common, new List<(ItemType, int)>
                {
                    (ItemType.DereReRoll,               3),
                    (ItemType.CardParamsReRoll,         3),
                    (ItemType.IncreaseExpSmall,         1),
                }.ToRealList()
            },
            {ItemDropType.Rare, new List<(ItemType, int)>
                {
                    (ItemType.IncreaseUpgradeCnt,       1),
                }.ToRealList()
            },
            {ItemDropType.Legendary, new List<(ItemType, int)>
                {
                    (ItemType.CreationItemBase,         1),
                }.ToRealList()
            },
        };

        private static Dictionary<ItemDropType, List<ItemType>> _itemsFromExtremeExpedition = new Dictionary<ItemDropType, List<ItemType>>
        {
            {ItemDropType.Food, new List<(ItemType, int)>
                {
                    (ItemType.AffectionRecoveryNormal,  7),
                    (ItemType.AffectionRecoveryBig,     5),
                    (ItemType.AffectionRecoveryGreat,   2),
                }.ToRealList()
            },
            {ItemDropType.Common, new List<(ItemType, int)>
                {
                    (ItemType.IncreaseExpSmall,         4),
                    (ItemType.IncreaseExpBig,           1),
                }.ToRealList()
            },
            {ItemDropType.Rare, new List<(ItemType, int)>
                {
                    (ItemType.IncreaseUpgradeCnt,       1),
                }.ToRealList()
            },
            {ItemDropType.Legendary, new List<(ItemType, int)>
                {
                    (ItemType.BetterIncreaseUpgradeCnt, 4),
                    (ItemType.BloodOfYourWaifu,         4),
                    (ItemType.CreationItemBase,         1),
                }.ToRealList()
            },
        };

        private static Dictionary<ItemDropType, List<ItemType>> _itemsFromDarkExpedition = new Dictionary<ItemDropType, List<ItemType>>
        {
            {ItemDropType.None, new List<ItemType>()},
            {ItemDropType.Food, new List<(ItemType, int)>
                {
                    (ItemType.AffectionRecoveryNormal,  5),
                    (ItemType.AffectionRecoverySmall,   3),
                    (ItemType.AffectionRecoveryBig,     2),
                    (ItemType.AffectionRecoveryGreat,   1),
                }.ToRealList()
            },
            {ItemDropType.Common, new List<(ItemType, int)>
                {
                    (ItemType.IncreaseExpSmall,         5),
                    (ItemType.DereReRoll,               3),
                    (ItemType.CardParamsReRoll,         2),
                    (ItemType.IncreaseExpBig,           1),
                }.ToRealList()
            },
            {ItemDropType.Rare, new List<(ItemType, int)>
                {
                    (ItemType.IncreaseUpgradeCnt,       1),
                }.ToRealList()
            },
            {ItemDropType.Legendary, new List<(ItemType, int)>
                {
                    (ItemType.BetterIncreaseUpgradeCnt, 20),
                    (ItemType.CreationItemBase,         1),
                }.ToRealList()
            },
        };

        private static Dictionary<ItemDropType, List<ItemType>> _itemsFromLightExpedition = new Dictionary<ItemDropType, List<ItemType>>
        {
            {ItemDropType.None, new List<ItemType>()},
            {ItemDropType.Food, new List<(ItemType, int)>
                {
                    (ItemType.AffectionRecoveryNormal,  5),
                    (ItemType.AffectionRecoverySmall,   3),
                    (ItemType.AffectionRecoveryBig,     2),
                    (ItemType.AffectionRecoveryGreat,   1),
                }.ToRealList()
            },
            {ItemDropType.Common, new List<(ItemType, int)>
                {
                    (ItemType.IncreaseExpSmall,         5),
                    (ItemType.DereReRoll,               3),
                    (ItemType.CardParamsReRoll,         2),
                    (ItemType.IncreaseExpBig,           1),
                }.ToRealList()
            },
            {ItemDropType.Rare, new List<(ItemType, int)>
                {
                    (ItemType.IncreaseUpgradeCnt,       1),
                }.ToRealList()
            },
            {ItemDropType.Legendary, new List<(ItemType, int)>
                {
                    (ItemType.BloodOfYourWaifu,         20),
                    (ItemType.CreationItemBase,         1),
                }.ToRealList()
            },
        };

        private static Dictionary<CardExpedition, List<ItemDropType>> _itemChanceOfItemTypeOnExpedition = new Dictionary<CardExpedition, List<ItemDropType>>
        {
            {CardExpedition.NormalItemWithExp, new List<(ItemDropType, int)>
                {
                    (ItemDropType.Food,       100),
                    (ItemDropType.Common,     60),
                    (ItemDropType.Rare,       30),
                    (ItemDropType.Legendary,  1),
                }.ToRealList()
            },
            {CardExpedition.ExtremeItemWithExp, new List<(ItemDropType, int)>
                {
                    (ItemDropType.Common,     40),
                    (ItemDropType.Rare,       25),
                    (ItemDropType.Food,       20),
                    (ItemDropType.Legendary,  15),
                }.ToRealList()
            },
            {CardExpedition.DarkItems, new List<(ItemDropType, int)>
                {
                    (ItemDropType.Common,     40),
                    (ItemDropType.Food,       30),
                    (ItemDropType.Rare,       25),
                    (ItemDropType.Legendary,  5),
                }.ToRealList()
            },
            {CardExpedition.DarkItemWithExp, new List<(ItemDropType, int)>
                {
                    (ItemDropType.None,       10),
                    (ItemDropType.Common,     35),
                    (ItemDropType.Rare,       30),
                    (ItemDropType.Food,       15),
                    (ItemDropType.Legendary,  2),
                }.ToRealList()
            },
            {CardExpedition.LightItems, new List<(ItemDropType, int)>
                {
                    (ItemDropType.Common,     40),
                    (ItemDropType.Food,       30),
                    (ItemDropType.Rare,       25),
                    (ItemDropType.Legendary,  5),
                }.ToRealList()
            },
            {CardExpedition.LightItemWithExp, new List<(ItemDropType, int)>
                {
                    (ItemDropType.None,       10),
                    (ItemDropType.Common,     35),
                    (ItemDropType.Rare,       30),
                    (ItemDropType.Food,       15),
                    (ItemDropType.Legendary,  2),
                }.ToRealList()
            },
        };

        private readonly ISystemTime _time;

        public Expedition(ISystemTime time)
        {
            _time = time;
        }

        public List<string> GetChancesFromExpedition(CardExpedition expedition)
        {
            if (expedition.HasDifferentQualitiesOnExpedition())
            {
                var chances = expedition switch
                {
                    CardExpedition.UltimateHardcore =>  _ultimateExpeditionHardcoreItems.GetChances(),
                    _ => _ultimateExpeditionItems.GetChances()
                };
                return chances.OrderByDescending(x => x.Item2).Select(x => $"{x.Item1} - {x.Item2:F}%").ToList();
            }

            var itemDropTypeChances = _itemChanceOfItemTypeOnExpedition[expedition].GetChances();
            var drop = expedition switch
            {
                CardExpedition.NormalItemWithExp    =>  _itemsFromNormalExpedition,
                CardExpedition.ExtremeItemWithExp   =>  _itemsFromExtremeExpedition,
                CardExpedition.DarkItemWithExp      =>  _itemsFromDarkExpedition,
                CardExpedition.DarkItems            =>  _itemsFromDarkExpedition,
                CardExpedition.LightItemWithExp     =>  _itemsFromLightExpedition,
                CardExpedition.LightItems           =>  _itemsFromLightExpedition,
                _ => null
            };

            var output = new List<string>();
            if (drop == null) return output;

            foreach (var type in itemDropTypeChances.OrderByDescending(x => x.Item2))
                output.Add($"**{type.Item1} ({type.Item2:F}%)**:\n{string.Join("\n", drop[type.Item1].GetChances().OrderByDescending(x => x.Item2).Select(x => $"{x.Item1} - {x.Item2:F}%"))}\n");

            return output;
        }

        public Item RandomizeItemFor(CardExpedition expedition, ItemDropType dropType)
        {
            var itemType = expedition switch
            {
                CardExpedition.UltimateHardcore => Fun.GetOneRandomFrom(_ultimateExpeditionHardcoreItems),
                CardExpedition.UltimateEasy     => Fun.GetOneRandomFrom(_ultimateExpeditionItems),
                CardExpedition.UltimateMedium   => Fun.GetOneRandomFrom(_ultimateExpeditionItems),
                CardExpedition.UltimateHard     => Fun.GetOneRandomFrom(_ultimateExpeditionItems),

                CardExpedition.NormalItemWithExp    =>  Fun.GetOneRandomFrom(_itemsFromNormalExpedition[dropType]),
                CardExpedition.ExtremeItemWithExp   =>  Fun.GetOneRandomFrom(_itemsFromExtremeExpedition[dropType]),
                CardExpedition.DarkItemWithExp      =>  Fun.GetOneRandomFrom(_itemsFromDarkExpedition[dropType]),
                CardExpedition.DarkItems            =>  Fun.GetOneRandomFrom(_itemsFromDarkExpedition[dropType]),
                CardExpedition.LightItemWithExp     =>  Fun.GetOneRandomFrom(_itemsFromLightExpedition[dropType]),
                CardExpedition.LightItems           =>  Fun.GetOneRandomFrom(_itemsFromLightExpedition[dropType]),

                _ => ItemType.AffectionRecoverySmall
            };

            if (itemType.HasDifferentQualities() && expedition.HasDifferentQualitiesOnExpedition())
            {
                return itemType.ToItem(1, RandomizeItemQualityFromExpedition(expedition));
            }

            return itemType.ToItem();
        }

        public ItemDropType RandomizeItemDropTypeFor(CardExpedition expedition)
        {
            return expedition switch
            {
                CardExpedition.UltimateHardcore => ItemDropType.Common,
                CardExpedition.UltimateEasy     => ItemDropType.Common,
                CardExpedition.UltimateMedium   => ItemDropType.Common,
                CardExpedition.UltimateHard     => ItemDropType.Common,

                CardExpedition.DarkExp          => ItemDropType.None,
                CardExpedition.LightExp         => ItemDropType.None,

                _ => Fun.GetOneRandomFrom(_itemChanceOfItemTypeOnExpedition[expedition])
            };
        }

        private Quality RandomizeItemQualityFromExpedition(CardExpedition type)
        {
            var num = Fun.GetRandomValue(100000);
            switch (type)
            {
                case CardExpedition.UltimateEasy:
                    if (num < 3000) return Quality.Delta;
                    if (num < 25000) return Quality.Gamma;
                    if (num < 45000) return Quality.Beta;
                    return Quality.Alpha;

                case CardExpedition.UltimateMedium:
                    if (num < 1000) return Quality.Zeta;
                    if (num < 2000) return Quality.Epsilon;
                    if (num < 5000) return Quality.Delta;
                    if (num < 35000) return Quality.Gamma;
                    if (num < 55000) return Quality.Beta;
                    return Quality.Alpha;

                case CardExpedition.UltimateHard:
                    if (num < 50) return Quality.Lambda;
                    if (num < 200) return Quality.Jota;
                    if (num < 600) return Quality.Theta;
                    if (num < 1500) return Quality.Zeta;
                    if (num < 5000) return Quality.Epsilon;
                    if (num < 12000) return Quality.Delta;
                    if (num < 25000) return Quality.Gamma;
                    if (num < 45000) return Quality.Beta;
                    return Quality.Alpha;

                case CardExpedition.UltimateHardcore:
                    if (num < 15) return Quality.Omega;
                    if (num < 50) return Quality.Sigma;
                    if (num < 150) return Quality.Lambda;
                    if (num < 1500) return Quality.Jota;
                    if (num < 5000) return Quality.Theta;
                    if (num < 10000) return Quality.Zeta;
                    if (num < 20000) return Quality.Epsilon;
                    if (num < 30000) return Quality.Delta;
                    if (num < 50000) return Quality.Gamma;
                    if (num < 80000) return Quality.Beta;
                    return Quality.Alpha;

                default:
                    return Quality.Broken;
            }
        }

        public double GetExpFromExpedition(double length, Card card)
        {
            var expPerHour = card.Expedition switch
            {
                CardExpedition.NormalItemWithExp    => 4,
                CardExpedition.ExtremeItemWithExp   => 6.5,
                CardExpedition.LightItemWithExp     => 3.5,
                CardExpedition.DarkItemWithExp      => 3.5,
                CardExpedition.LightExp             => 25,
                CardExpedition.DarkExp              => 25,
                _ => 0
            };

            return expPerHour / 60 * length;
        }

        public int GetItemsCountFromExpedition(double length, Card card)
        {
            bool yamiOrRaito = card.Dere == Dere.Yami || card.Dere == Dere.Raito;

            var itemsPerHour = card.Expedition switch
            {
                CardExpedition.NormalItemWithExp    => 5,
                CardExpedition.ExtremeItemWithExp   => yamiOrRaito ? 44 : 22.5,
                CardExpedition.LightItemWithExp     => yamiOrRaito ? 10 : 8,
                CardExpedition.DarkItemWithExp      => yamiOrRaito ? 10 : 8,
                CardExpedition.LightItems           => yamiOrRaito ? 20 : 16,
                CardExpedition.DarkItems            => yamiOrRaito ? 20 : 16,
                CardExpedition.UltimateEasy         => yamiOrRaito ? 8 : 4,
                CardExpedition.UltimateMedium       => yamiOrRaito ? 8 : 4,
                CardExpedition.UltimateHard         => yamiOrRaito ? 12 : 6,
                CardExpedition.UltimateHardcore     => yamiOrRaito ? 4 : 2,
                _ => 0
            };

            itemsPerHour *= card.Dere == Dere.Yato ? 1.3 : 1;

            return (int) (itemsPerHour / 60 * length);
        }

        public double GetKarmaCostOfExpedition(double length, Card card)
        {
            var karmaCostPerMinute = card.Expedition switch
            {
                CardExpedition.NormalItemWithExp    => 0.00225,
                CardExpedition.ExtremeItemWithExp   => 0.07,
                CardExpedition.LightItemWithExp     => card.Dere == Dere.Yato ? 0.003 : 0.008,
                CardExpedition.LightItems           => card.Dere == Dere.Yato ? 0.003 : 0.008,
                CardExpedition.LightExp             => card.Dere == Dere.Yato ? 0.003 : 0.008,
                CardExpedition.DarkItemWithExp      => card.Dere == Dere.Yato ? 0.007 : 0.0045,
                CardExpedition.DarkItems            => card.Dere == Dere.Yato ? 0.007 : 0.0045,
                CardExpedition.DarkExp              => card.Dere == Dere.Yato ? 0.007 : 0.0045,
                _ => 0
            };

            return karmaCostPerMinute * length;
        }

        public double GetAffectionCostOfExpedition(double length, Card card)
        {
            var qualityMod = 2d - card.Quality switch
            {
                Quality.Omega   => 1.95,
                Quality.Sigma   => 1.7,
                Quality.Lambda  => 1.6,
                Quality.Jota    => 1.55,
                Quality.Theta   => 1.5,
                Quality.Zeta    => 1.45,
                Quality.Epsilon => 1.4,
                Quality.Delta   => 1.35,
                Quality.Gamma   => 1.3,
                Quality.Beta    => 1.2,
                Quality.Alpha   => 1.1,
                _ => 1
            };

            var affectionCostPerMinute = card.Expedition switch
            {
                CardExpedition.NormalItemWithExp    => 0.02,
                CardExpedition.ExtremeItemWithExp   => 0.375,
                CardExpedition.LightExp             => 0.155,
                CardExpedition.DarkExp              => 0.155,
                CardExpedition.DarkItems            => 0.132 * qualityMod,
                CardExpedition.LightItems           => 0.132 * qualityMod,
                CardExpedition.LightItemWithExp     => 0.125,
                CardExpedition.DarkItemWithExp      => 0.125,
                CardExpedition.UltimateEasy         => 2.5,
                CardExpedition.UltimateMedium       => 2.5,
                CardExpedition.UltimateHard         => 5,
                CardExpedition.UltimateHardcore     => 1.5,
                _ => 0
            };

            var rarityMod = card.Rarity switch
            {
                Rarity.SSS => 0.85,
                Rarity.SS  => 0.95,
                Rarity.D   => 1.05,
                Rarity.E   => 1.1,
                _ => 1
            };

            return affectionCostPerMinute * length * rarityMod;
        }

        public double GetMaxPossibleLengthOfExpedition(User user, Card card, CardExpedition expedition = CardExpedition.None)
        {
            expedition = expedition == CardExpedition.None ? card.Expedition : expedition;
            var costOffset = user.GameDeck.Karma.IsKarmaNeutral() ? 23d : 6d;
            var costPerMinute = GetAffectionCostOfExpedition(1, card);
            var karmaBonus = user.GameDeck.Karma / 200d;
            var fuel = card.Affection;

            switch (expedition)
            {
                case CardExpedition.LightExp:
                case CardExpedition.LightItems:
                case CardExpedition.LightItemWithExp:
                    karmaBonus = Math.Min(7, karmaBonus);
                    break;

                case CardExpedition.DarkItems:
                case CardExpedition.DarkExp:
                case CardExpedition.DarkItemWithExp:
                    karmaBonus = Math.Min(12, Math.Abs(karmaBonus));
                    break;

                case CardExpedition.UltimateEasy:
                case CardExpedition.UltimateMedium:
                case CardExpedition.UltimateHard:
                case CardExpedition.UltimateHardcore:
                    fuel *= ((int)card.Quality * 0.7) + 1.4;
                    costOffset = 0;
                    karmaBonus = 0;
                    break;

                default:
                    break;
            }

            costPerMinute *= card.HasImage() ? 1 : 2;
            fuel += costOffset + karmaBonus;
            var time = fuel / costPerMinute;

            time = time > 10080 ? 10080 : time;
            time = time < 0.1 ? 0.1 : time;
            return time;
        }

        public double GetGuaranteedAffection(Card card, double affectionCost)
        {
            var affectionBaseReturn = card.Expedition switch
            {
                CardExpedition.NormalItemWithExp    => 1,
                CardExpedition.ExtremeItemWithExp   => 0.8,
                CardExpedition.DarkItems            => 1.1,
                CardExpedition.LightItems           => 1.1,
                CardExpedition.LightItemWithExp     => 0.6,
                CardExpedition.DarkItemWithExp      => 0.6,
                _ => 0
            };

            var dereMod = card.Dere switch
            {
                Dere.Tsundere   => 0.5,
                Dere.Kamidere   => 0.9,
                Dere.Yandere    => 0.9,
                Dere.Yami       => 1.2,
                Dere.Raito      => 1.2,
                Dere.Yato       => 1.4,
                _ => 1
            };

            return affectionBaseReturn * affectionCost * dereMod;
        }

        public (double CalcTime, double RealTime) GetLengthOfExpedition(User user, Card card)
        {
            var maxTimeBasedOnCardParamsInMinutes = GetMaxPossibleLengthOfExpedition(user, card);
            var realTimeInMinutes = (_time.Now() - card.ExpeditionDate).TotalMinutes;
            var timeToCalculateFrom = realTimeInMinutes;

            if (maxTimeBasedOnCardParamsInMinutes < timeToCalculateFrom)
                timeToCalculateFrom = maxTimeBasedOnCardParamsInMinutes;

            return (timeToCalculateFrom, realTimeInMinutes);
        }

        public bool IsValidToGo(User user, Card card, CardExpedition expedition, TagHelper helper)
        {
            if (card.Expedition != CardExpedition.None)
                return false;

            if (card.Curse == CardCurse.ExpeditionBlockade)
                return false;

            if (card.InCage || !card.CanFightOnPvEGMwK())
                return false;

            if (GetMaxPossibleLengthOfExpedition(user, card, expedition) < 1)
                return false;

            switch (expedition)
            {
                case CardExpedition.ExtremeItemWithExp:
                    return !card.FromFigure && !helper.HasTag(card, TagType.Favorite);

                case CardExpedition.NormalItemWithExp:
                    return !card.FromFigure;

                case CardExpedition.UltimateEasy:
                case CardExpedition.UltimateHard:
                case CardExpedition.UltimateMedium:
                    return card.Rarity == Rarity.SSS;

                case CardExpedition.UltimateHardcore:
                    return card.Rarity == Rarity.SSS && !helper.HasTag(card, TagType.Favorite);

                case CardExpedition.LightItems:
                    return user.GameDeck.Karma > 1000;
                case CardExpedition.LightExp:
                    return (user.GameDeck.Karma > 1000) && !card.FromFigure;
                case CardExpedition.LightItemWithExp:
                    return (user.GameDeck.Karma > 400) && !card.FromFigure;

                case CardExpedition.DarkItems:
                    return user.GameDeck.Karma < -1000;
                case CardExpedition.DarkExp:
                    return (user.GameDeck.Karma < -1000) && !card.FromFigure;
                case CardExpedition.DarkItemWithExp:
                    return (user.GameDeck.Karma < -400) && !card.FromFigure;

                default:
                case CardExpedition.None:
                    return false;
            }
        }
    }
}