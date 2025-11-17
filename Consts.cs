using CoinFactorySim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinFactorySim
{
    public static class Consts
    {
        public static readonly BlockDefinition[] BLOCKS = [
            new()
            {
                BlockType = Block.Factory,
                Notation = "Fac",
                Color = ColorTranslator.FromHtml("#DAC667"),
                SecondaryColor = ColorTranslator.FromHtml("#E47441"),
                IsDirectional = true,
                IsFirstFree = true,
                Prices = [4, 25, 75, 400, 1500, 20000, 400000, 200000000, 20000000000],
                Base = [1, 2, 3, 3, 5, 10, 20, 50, 100],
                Mul = [1, 1, 1, 2, 3, 5, 10, 100, 200],
            },
            new()
            {
                BlockType = Block.Central,
                Notation = "Cen",
                Color = ColorTranslator.FromHtml("#06CADD"),
                SecondaryColor = ColorTranslator.FromHtml("#077783"),
                IsDirectional = true,
                Prices = [120, 600, 5000, 60000, 9000000, 1600000000],
                Mul = [1, 1, 2, 4, 10, 15],
            },
            new()
            {
                BlockType = Block.Belt,
                Notation = "Blt",
                Color = ColorTranslator.FromHtml("#151515"),
                IsDirectional = true,
                Prices = [0],
            },
            new()
            {
                BlockType = Block.Jumper,
                Notation = "Jmp",
                Color = ColorTranslator.FromHtml("#CCD4E1"),
                IsDirectional = true,
                Prices = [200, 15000],
            },
            new()
            {
                BlockType = Block.Coffee,
                Notation = "Cof",
                Color = ColorTranslator.FromHtml("#583929"),
                SecondaryColor = ColorTranslator.FromHtml("#8E766B"),
                Prices = [50, 500, 2000, 20000],
                Base = [1, 2, 5, 10],
                Cycle = [0, 10, 7, 5]
            },
            new()
            {
                BlockType = Block.Park,
                Notation = "Prk",
                Color = ColorTranslator.FromHtml("#29963B"),
                Prices = [300, 2500000, 7500000],
                Mul = [0.3, 0.5, 0.75]
            },
            new()
            {
                BlockType = Block.Water,
                Notation = "Wat",
                Color = ColorTranslator.FromHtml("#1386B5"),
                Prices = [12000, 24000],
                Mul = [1.25, 1.3]
            },
            new()
            {
                BlockType = Block.Bishop,
                Notation = "Bis",
                Color = ColorTranslator.FromHtml("#416182"),
                SecondaryColor = ColorTranslator.FromHtml("#0D2E4F"),
                Prices = [2000, 4000000],
                Mul = [0.5, 1]
            },
            new()
            {
                BlockType = Block.Accelerator,
                Notation = "Acc",
                Color = ColorTranslator.FromHtml("#D86E3D"),
                SecondaryColor = ColorTranslator.FromHtml("#894527"),
                IsDirectional = true,
                IsCyclic = true,
                Prices = [3000, 80000, 200000],
                Base = [1, 1, 1],
                Cycle = [5, 3, 1]
            },
            new()
            {
                BlockType = Block.Firecamp,
                Notation = "Fir",
                Color = ColorTranslator.FromHtml("#CF4A4A"),
                SecondaryColor = ColorTranslator.FromHtml("#FEC82F"),
                IsDirectional = true,
                IsCyclic = true,
                Prices = [75000, 150000, 2000000, 70000000, 500000000],
                Base = [0.1, 0.5, 1, 2, 5],
                Cycle = [4, 4, 4, 4, 4] // level down 15 cycles
            },
            new()
            {
                BlockType = Block.Drum,
                Notation = "Drm",
                Color = ColorTranslator.FromHtml("#936746"),
                SecondaryColor = ColorTranslator.FromHtml("#BF9B7A"),
                IsCyclic = true,
                Prices = [20000, 4000000],
                Base = [2, 2],
                Cycle = [5, 5]
            },
            new()
            {
                BlockType = Block.Overclocker,
                Notation = "Over",
                Color = ColorTranslator.FromHtml("#7E61CF"),
                SecondaryColor = ColorTranslator.FromHtml("#271D40"),
                IsDirectional = true,
                IsCyclic = true,
                Prices = [150000, 12000000],
                Base = [3, 10],
                Cycle = [5, 5]
            },
            new()
            {
                BlockType = Block.Basketball,
                Notation = "Bask",
                Color = ColorTranslator.FromHtml("#E08132"),
                Prices = [1800000],
            },
            new()
            {
                BlockType = Block.Investor,
                Notation = "Inv",
                Color = ColorTranslator.FromHtml("#676798"),
                SecondaryColor = ColorTranslator.FromHtml("#363650"),
                IsCyclic = true,
                Prices = [1000000, 5000000, 40000000, 300000000000],
                Base = [1, 2, 3, 0],
                Cycle = [5, 3, 2, 1]
            },
            new()
            {
                BlockType = Block.Missile,
                Notation = "Mis",
                Color = ColorTranslator.FromHtml("#C82D2D"),
                SecondaryColor = ColorTranslator.FromHtml("#E6E6E6"),
                IsDirectional = true,
                IsCyclic = true,
                Prices = [1500000],
                Cycle = [3]
            },
            new()
            {
                BlockType = Block.PortalA,
                Notation = "PortA",
                Color = ColorTranslator.FromHtml("#1B212D"),
                SecondaryColor = ColorTranslator.FromHtml("#00A2FF"),
                IsDirectional = true,
                IsFirstFree = true,
                Prices = [1000],
            },
            new()
            {
                BlockType = Block.PortalB,
                Notation = "PortB",
                Color = ColorTranslator.FromHtml("#1E2432"),
                SecondaryColor = ColorTranslator.FromHtml("#FF9a00"),
                IsDirectional = true,
                IsFirstFree = true,
                Prices = [1000],
            },
            new()
            {
                BlockType = Block.Exit,
                Notation = "Ex",
                Color = Color.Black,
                Prices = [0],
            },
            new()
            {
                BlockType = Block.None,
                Notation = "_",
                Color = Color.White,
            },
        ];

        public static readonly MapDefinition Map_3x3 = new()
        {
            Name = "3x3",
            Width = 3,
            Height = 3,
            TileEffects =
            [
                [
                    new()
                    {
                        EffectType = EffectType.ProductionMul,
                        Value = 3,
                    },
                ],
                [
                    new()
                    {
                        EffectType = EffectType.ProductionAdd,
                        Value = 20,
                    },
                ],
                [
                    new()
                    {
                        EffectType = EffectType.Jumper,
                        Value = 1,
                    },
                ],
                [
                    new()
                    {
                        EffectType = EffectType.CycleAdd,
                        Value = 5,
                    },
                ],
                [
                    new()
                    {
                        EffectType = EffectType.ProductionPerCycleAdd,
                        Value = 1,
                    },
                ],
                [
                    new()
                    {
                        EffectType = EffectType.ProductionMul,
                        Value = 2,
                    },
                ],
                [
                    new()
                    {
                        EffectType = EffectType.Jumper,
                        Value = 1,
                    },
                ],
                [
                    new()
                    {
                        EffectType = EffectType.CycleAdd,
                        Value = 2,
                    },
                ],
                [
                    new()
                    {
                        EffectType = EffectType.ProductionPerCycleAdd,
                        Value = 0.1,
                    },
                ],
            ]
        };

        public static readonly MapDefinition Map_Tiny = new() {
            Name = "Tiny",
            Width = 5,
            Height = 5,
            TileEffects = [.. Enumerable.Repeat(new List<Effect>(), 25)],
        };
        public static readonly MapDefinition Map_Standard = new() {
            Name = "Standard",
            Width = 7,
            Height = 7,
            TileEffects = [.. Enumerable.Repeat(new List<Effect>(), 49)],
        };
        public static readonly MapDefinition Map_Huge = new() {
            Name = "Standard",
            Width = 9,
            Height = 9,
            TileEffects = [.. Enumerable.Repeat(new List<Effect>(), 81)],
        };
        public static readonly StringFormat CenterFormat = new() {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Center,
        };
    };
}
