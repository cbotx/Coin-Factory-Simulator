using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoinFactorySim
{
    public enum Block
    {
        None,
        Factory,
        Central,
        Belt,
        Jumper,
        Coffee,
        Park,
        Water,
        Bishop,
        Accelerator,
        Firecamp,
        Drum,
        Overclocker,
        Basketball,
        Investor,
        Exit,
        Missile,
        PortalA,
        PortalB,
        Distributor,
        Drill,
        Shield,
    }
    public enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right,
    }
    public class BlockDefinition
    {
        public Block BlockType { get; set; }
        public Color Color { get; set; }
        public Color? SecondaryColor { get; set; }
        public bool IsDirectional { get; set; } = false;
        public bool IsCyclic { get; set; } = false;
        public bool IsFirstFree { get; set; } = false;
        public long[] Prices { get; set; } = [];
        public double[]? Base { get; set; }
        public double[]? Mul { get; set; }
        public int[]? Cycle { get; set; }
    }

    public class BlockState
    {
        public Block BlockType { get; set; }
        public Direction Direction { get; set; }
        public int Level { get; set; }
        public int Cycle { get; set; } = 0;
        public int RemainingCycle { get; set; } = 0;
        public double Bonus { get; set; } = 0;
        public double Value { get; set; } = 0;
        public double Money { get; set; } = 0;
        public BlockState ShallowCopy() {
            return (BlockState)MemberwiseClone();
        }
    }
    public class FrameState {
        public List<BlockState> BlockStates { get; set; } = [];
        public double Money { get; set; }
        public bool IsKeyFrame { get; set; } = false;
        public bool Errored { get; set; } = false;
        public int Stage { get; set; } = 0;
        public int Size { get; set; }

        public FrameState(int size)
        {
            Size = size;
            for (int i = 0; i < size; i++)
            {
                BlockStates.Add(new BlockState());
            }
        }

        public FrameState Clone() {
            return new FrameState(Size) {
                BlockStates = BlockStates.ConvertAll(item => item.ShallowCopy()),
                Money = Money,
                IsKeyFrame = IsKeyFrame,
                Errored = Errored,
                Stage = Stage,
            };
        }
    }
    public enum EffectType
    {
        ProductionAdd,
        ProductionMul,
        CycleAdd,
        ProductionPerCycleAdd,
        Jumper,
        Rotation,
        Hole,
    }
    public class Effect
    {
        public EffectType EffectType { get; set; }
        public double Value { get; set; }
    }
    public class MapDefinition
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Size { get { return Width * Height; } }
        public List<List<Effect>> TileEffects { get; set; }
    }

    public enum ActionType {
        Buy,
        Upgrade,
        Rotate,
    }
    public class FrameAction {
        public Block Block { get; set; }
        public ActionType Action {  get; set; }
        public int Index { get; set; }
        public Direction? Direction { get; set; }
        public int? Count { get; set; }
        public long Spent { get; set; }
        public FrameAction ShallowCopy() {
            return (FrameAction)MemberwiseClone();
        }
    }
}
