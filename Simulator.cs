using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CoinFactorySim {
    public class Simulator(MapDefinition map, Dictionary<Block, BlockDefinition> blockDefinitions) {
        private readonly MapDefinition map = map;
        private readonly Dictionary<Block, BlockDefinition> blockDefinitions = blockDefinitions;
        private double[] tempMoneyArray = new double[map.Size];

        public void SimulateFrames(List<FrameState> frameStates, List<List<FrameAction>> frameActions, int frameStart, int frameEnd) {
            FrameState tempFrameState = new(map.Size);
            int maxStage = 0;

            for (int i = frameStart; i < frameEnd; i++) {
                frameActions[i].Clear();
            }
            for (int idx = frameStart; idx < frameEnd; idx++) {
                frameStates[idx].Stage = 0;
                while (tempFrameState.Money >= Math.Pow(10, maxStage + 1)) {
                    maxStage++;
                    frameStates[idx].Stage = maxStage;
                }
                var fs = frameStates[idx];
                // 1. if keyframe calculate money spent
                if (fs.IsKeyFrame) {
                    Dictionary<Block, int> purchases = [];
                    Dictionary<Block, int> keeps = [];
                    long totalPay = 0;
                    bool errored = false;
                    for (int i = 0; i < fs.BlockStates.Count; i++) {
                        BlockState prev = tempFrameState.BlockStates[i];
                        BlockState current = fs.BlockStates[i];
                        if (prev.BlockType == Block.Exit && current.BlockType != prev.BlockType) { // Can't remove exit
                            errored = true;
                            break;
                        }
                        if (current.BlockType != Block.None) {
                            int startingLevel = 1;
                            if (current.BlockType != prev.BlockType) { // purchase
                                if (current.BlockType != Block.Belt) frameActions[idx].Add(
                                    new FrameAction {
                                        Action = ActionType.Buy,
                                        Block = current.BlockType,
                                        Index = i,
                                        Direction = current.Direction,
                                    }
                                );
                                if (!purchases.ContainsKey(current.BlockType)) purchases[current.BlockType] = 0;
                                purchases[current.BlockType]++;
                                current.Cycle = 0;
                                current.Bonus = 0;
                                current.Value = 0;
                                current.RemainingCycle = 0;
                                if ((prev.BlockType == Block.None || prev.BlockType == Block.Belt) && (current.BlockType == Block.Belt || current.BlockType == Block.Jumper)) {
                                    current.Money = prev.Money;
                                } else {
                                    current.Money = 0;
                                }
                            } else {
                                if (!keeps.ContainsKey(current.BlockType)) keeps[current.BlockType] = 0;
                                keeps[current.BlockType]++;
                                startingLevel = prev.Level;
                                current.Cycle = prev.Cycle;
                                current.Bonus = prev.Bonus;
                                current.Value = prev.Value;
                                current.Money = prev.Money;
                                current.RemainingCycle = prev.RemainingCycle;
                            }
                            if (current.Level > startingLevel) { // upgrade
                                
                                long spent = 0;
                                for (int j = startingLevel; j < current.Level; j++) {
                                    spent += blockDefinitions[current.BlockType].Prices[j];
                                }
                                totalPay += spent;
                                frameActions[idx].Add(
                                    new FrameAction {
                                        Action = ActionType.Upgrade,
                                        Block = current.BlockType,
                                        Index = i,
                                        Count = current.Level - startingLevel,
                                        Spent = spent,
                                    }
                                );
                                if (current.BlockType == Block.Firecamp) {
                                    current.RemainingCycle = 15;
                                } else if (current.BlockType == Block.Coffee) {
                                    current.RemainingCycle = GetBlockCycle(current);
                                }
                            } else if (current.Level < startingLevel) {
                                current.Level = startingLevel;
                            }
                            if (current.BlockType == prev.BlockType && current.Direction != prev.Direction) { // rotate
                                frameActions[idx].Add(
                                    new FrameAction {
                                        Action = ActionType.Rotate,
                                        Block = current.BlockType,
                                        Index = i,
                                        Direction = current.Direction,
                                    }
                                );
                            }
                        } else {
                            current.Cycle = 0;
                            current.Bonus = 0;
                            current.Value = 0;
                            current.RemainingCycle = 0;
                            if (current.BlockType != prev.BlockType) { // delete
                                current.Money = 0;
                            } else {
                                current.Money = prev.Money;
                            }
                        }
                    }
                    if (errored) {
                        fs.Errored = true;
                        break;
                    }
                    foreach (var item in purchases) {
                        long multiplier = 1;
                        if (keeps.ContainsKey(item.Key)) multiplier = 1 << keeps[item.Key];
                        totalPay += multiplier * ((1 << purchases[item.Key]) - 1) * blockDefinitions[item.Key].Prices[0];
                        if (blockDefinitions[item.Key].IsFirstFree && multiplier == 1) totalPay -= blockDefinitions[item.Key].Prices[0];
                        if (item.Key == Block.Exit && multiplier > 1) {
                            fs.Errored = true;
                            break;
                        }
                    }
                    double money = tempFrameState.Money;
                    if (money < totalPay) {
                        fs.Errored = true;
                        break;
                    } else {
                        fs.Errored = false;
                        money -= totalPay;
                    }
                    fs.Money = money;
                }
                // 2. Apply effect ProductionPerCycleAdd
                for (int i = 0; i < fs.BlockStates.Count; i++) {
                    var block = fs.BlockStates[i];
                    if (new List<Block> { Block.Factory, Block.Central, Block.Firecamp, Block.Accelerator, Block.Coffee, Block.Investor }.Contains(block.BlockType)) {
                        foreach (Effect effect in map.TileEffects[i]) {
                            if (effect.EffectType == EffectType.ProductionPerCycleAdd) {
                                fs.BlockStates[i].Bonus += effect.Value;
                            }
                        }
                    }
                }
                // 3. Calculate park value
                for (int i = 0; i < fs.BlockStates.Count; i++) {
                    var block = fs.BlockStates[i];
                    if (block.BlockType != Block.Park) continue;

                    double waterBonus = 0;
                    foreach (var j in GetSurroundings(i)) {
                        var block_j = fs.BlockStates[j];
                        if (block_j.BlockType == Block.Water) waterBonus += GetBlockMul(block_j);
                    }
                    block.Value = GetBlockMul(block) * (1 + waterBonus);
                }
                // 4. Calculate coffee value
                for (int i = 0; i < fs.BlockStates.Count; i++) {
                    var block = fs.BlockStates[i];
                    if (block.BlockType != Block.Coffee) continue;

                    block.Value = GetBlockBase(block) + block.Bonus;
                }
                // 5. If next frame is keyframe: backup
                if (idx == frameEnd - 1) {
                    tempFrameState = new(map.Size); // final state
                } else if (frameStates[idx + 1].IsKeyFrame) {
                    tempFrameState = new(map.Size);
                } else {
                    tempFrameState = frameStates[idx + 1];
                }
                tempFrameState.BlockStates = fs.BlockStates.ConvertAll(item => item.ShallowCopy());
                tempFrameState.Money = fs.Money;
                // 6. Do distributor cycle
                DoDistributorCycle(tempFrameState);
                // 7. Do other cycles
                DoCycle(tempFrameState);
                
                for (int i = 0; i < fs.BlockStates.Count; i++) {
                    fs.BlockStates[i].Value = tempFrameState.BlockStates[i].Value;
                }
            }
        }
        private void DoDistributorCycle(FrameState fs) {
            for (int i = 0; i < fs.BlockStates.Count; i++) {
                var block = fs.BlockStates[i];
                int cycles = GetEffectCycle(i) + fs.BlockStates[i].Cycle;
                int jumper = GetEffectJumper(i);
                if (block.BlockType == Block.Distributor) {
                    var fireCount = cycles / GetBlockCycle(block);
                    if (fireCount > 0) {
                        int facingTile = GetFacingTile(i, block.Direction);
                        int facingTileWithJumper = GetFacingTile(i, block.Direction, jumper);
                        var affectedTiles = GetOrthogonals(i);
                        var targetTile = -1;
                        List<Block> allowList = [Block.Factory, Block.Central, Block.Firecamp, Block.Accelerator, Block.Coffee, Block.Investor];
                        if (facingTile >= 0 && allowList.Contains(fs.BlockStates[facingTile].BlockType)) {
                            targetTile = facingTile;
                        } else if (facingTileWithJumper >= 0 && allowList.Contains(fs.BlockStates[facingTileWithJumper].BlockType)) {
                            targetTile = facingTileWithJumper;
                        }
                        if (targetTile >= 0) {
                            int stolenCycles = 0;
                            foreach (var j in affectedTiles) {
                                if (j == targetTile) continue;
                                if (blockDefinitions[fs.BlockStates[j].BlockType].IsCyclic && fs.BlockStates[j].BlockType != Block.Distributor) {
                                    fs.BlockStates[j].Cycle -= (int)GetBlockBase(block) * fireCount;
                                    stolenCycles += (int)GetBlockBase(block) * fireCount;
                                }
                            }
                            fs.BlockStates[targetTile].Cycle += stolenCycles;
                        }
                    }
                    fs.BlockStates[i].Cycle = cycles % GetBlockCycle(block);
                }
            }
        }
        private void DoCycle(FrameState fs) {
            Array.Clear(tempMoneyArray, 0, tempMoneyArray.Length);
            for (int i = 0; i < fs.BlockStates.Count; i++) {
                var block = fs.BlockStates[i];
                int cycles = GetEffectCycle(i) + fs.BlockStates[i].Cycle;
                if (cycles < 0) cycles = 0;
                int jumper = GetEffectJumper(i);
                if (block.BlockType == Block.None) {
                    List<Block> allowList = [Block.None, Block.Belt, Block.Jumper, Block.PortalA, Block.PortalB];
                    if (block.Money > 0 && allowList.Contains(fs.BlockStates[i].BlockType)) {
                        tempMoneyArray[i] += block.Money;
                    }
                } else if (block.BlockType == Block.Factory || block.BlockType == Block.Central) {
                    if (block.BlockType == Block.Factory) {
                        CalculateFactoryValue(fs, i);
                    } else if (block.BlockType == Block.Central) {
                        CalculateCentralValue(fs, i);
                    }
                    int facingTile = GetFacingTile(i, block.Direction, jumper);
                    if (facingTile >= 0) {
                        int portalTile = -1;
                        if (fs.BlockStates[facingTile].BlockType == Block.PortalA) {
                            portalTile = GetPortalTile(fs, Block.PortalB);
                        } else if (fs.BlockStates[facingTile].BlockType == Block.PortalB) {
                            portalTile = GetPortalTile(fs, Block.PortalA);
                        }
                        if (portalTile >= 0) {
                            facingTile = portalTile;
                        }
                        tempMoneyArray[facingTile] += block.Value * (cycles / GetBlockCycle(block)) * GetBasketballMul(fs, i, facingTile);
                    }
                    fs.BlockStates[i].Cycle = cycles % GetBlockCycle(block);
                } else if (block.BlockType == Block.Belt || block.BlockType == Block.Jumper || block.BlockType == Block.PortalA || block.BlockType == Block.PortalB) {
                    int facingTile = -1;
                    if (block.BlockType == Block.Belt) {
                        facingTile = GetFacingTile(i, block.Direction);
                    } else if (block.BlockType == Block.Jumper) {
                        facingTile = GetFacingTile(i, block.Direction, block.Level);
                    } else {
                        facingTile = GetFacingTile(i, block.Direction, jumper);
                    }
                    if (facingTile >= 0) {
                        int portalTile = -1;
                        if (fs.BlockStates[facingTile].BlockType == Block.PortalA) {
                            portalTile = GetPortalTile(fs, Block.PortalB);
                        } else if (fs.BlockStates[facingTile].BlockType == Block.PortalB) {
                            portalTile = GetPortalTile(fs, Block.PortalA);
                        }
                        if (portalTile >= 0) {
                            facingTile = portalTile;
                        }
                        List<Block> allowList = [Block.Belt, Block.Jumper, Block.PortalA, Block.PortalB, Block.Exit, Block.None];
                        if (allowList.Contains(fs.BlockStates[facingTile].BlockType)) {
                            tempMoneyArray[facingTile] += block.Money * GetBasketballMul(fs, i, facingTile);
                        } else {
                            tempMoneyArray[i] += block.Money;
                        }
                    }
                } else if (block.BlockType == Block.Exit) {
                    fs.Money += block.Money;
                } else if (block.BlockType == Block.Accelerator) {
                    block.Value = GetBlockBase(block) + block.Bonus;

                    int facingTile = GetFacingTile(i, block.Direction);
                    int facingTileWithJumper = GetFacingTile(i, block.Direction, jumper);
                    List<Block> allowList = [Block.Factory, Block.Central];
                    if (facingTile >= 0 && allowList.Contains(fs.BlockStates[facingTile].BlockType)) {
                        fs.BlockStates[facingTile].Bonus += block.Value * (cycles / GetBlockCycle(block));
                        fs.BlockStates[facingTile].Bonus = fs.BlockStates[facingTile].Bonus;
                    } else if (facingTileWithJumper >= 0 && allowList.Contains(fs.BlockStates[facingTileWithJumper].BlockType)) {
                        fs.BlockStates[facingTileWithJumper].Bonus += block.Value * (cycles / GetBlockCycle(block));
                        fs.BlockStates[facingTileWithJumper].Bonus = fs.BlockStates[facingTileWithJumper].Bonus;
                    }
                    fs.BlockStates[i].Cycle = cycles % GetBlockCycle(block);
                } else if (block.BlockType == Block.Firecamp) {
                    block.Value = GetBlockBase(block) + block.Bonus;

                    int facingTile = GetFacingTile(i, block.Direction);
                    int facingTileWithJumper = GetFacingTile(i, block.Direction, jumper);
                    List<Block> allowList = [Block.Factory, Block.Central, Block.Accelerator, Block.Coffee, Block.Investor];
                    var blockCycle = GetBlockCycle(block);
                    if (facingTile >= 0 && allowList.Contains(fs.BlockStates[facingTile].BlockType)) {
                        fs.BlockStates[facingTile].Bonus += block.Value * (cycles / blockCycle);
                        fs.BlockStates[facingTile].Bonus = fs.BlockStates[facingTile].Bonus;
                    } else if (facingTileWithJumper >= 0 && allowList.Contains(fs.BlockStates[facingTileWithJumper].BlockType)) {
                        fs.BlockStates[facingTileWithJumper].Bonus += block.Value * (cycles / blockCycle);
                        fs.BlockStates[facingTileWithJumper].Bonus = fs.BlockStates[facingTileWithJumper].Bonus;
                    }
                    if (block.Level > 1) {
                        block.RemainingCycle -= cycles / blockCycle;
                        if (block.RemainingCycle <= 0) {
                            block.Level--;
                            if (block.Level > 1) block.RemainingCycle += 15;
                        }
                    }
                    block.Cycle = cycles % blockCycle;
                } else if (block.BlockType == Block.Coffee) {
                    if (block.Level > 1) {
                        block.RemainingCycle = block.RemainingCycle - cycles;
                        if (block.RemainingCycle <= 0) {
                            block.Bonus += new List<int>() { 1, 2, 5 }[block.Level - 2];
                            block.Level--;
                            if (block.Level > 1) block.RemainingCycle += GetBlockCycle(block);
                        }
                        block.Cycle = 0;
                    }
                } else if (block.BlockType == Block.Investor) {
                    block.Value = GetBlockBase(block) + block.Bonus;

                    List<Block> allowList = [Block.Factory, Block.Central];
                    for (int j = 0; j < fs.BlockStates.Count; j++) {
                        if (allowList.Contains(fs.BlockStates[j].BlockType)) {
                            if (block.Level < 4) {
                                fs.BlockStates[j].Bonus += block.Value * (cycles / GetBlockCycle(block));
                            } else {
                                for (int k = 0; k < cycles / GetBlockCycle(block); k++) {
                                    fs.BlockStates[j].Bonus *= 1.01;
                                    fs.BlockStates[j].Bonus += block.Value;
                                }
                            }
                            fs.BlockStates[j].Bonus = fs.BlockStates[j].Bonus;
                        }
                    }
                    fs.BlockStates[i].Cycle = cycles % GetBlockCycle(block);
                } else if (block.BlockType == Block.Overclocker) {
                    int facingTile = GetFacingTile(i, block.Direction);
                    int facingTileWithJumper = GetFacingTile(i, block.Direction, jumper);
                    List<Block> allowList = [Block.Factory, Block.Central, Block.Firecamp, Block.Accelerator, Block.Coffee, Block.Investor, Block.Distributor];
                    if (facingTile >= 0 && allowList.Contains(fs.BlockStates[facingTile].BlockType)) {
                        fs.BlockStates[facingTile].Cycle += (int)GetBlockBase(block) * (cycles / GetBlockCycle(block));
                    } else if (facingTileWithJumper >= 0 && allowList.Contains(fs.BlockStates[facingTileWithJumper].BlockType)) {
                        fs.BlockStates[facingTileWithJumper].Cycle += (int)GetBlockBase(block) * (cycles / GetBlockCycle(block));
                    }
                    fs.BlockStates[i].Cycle = cycles % GetBlockCycle(block);
                } else if (block.BlockType == Block.Drum) {
                    var affectedTiles = block.Level == 1 ? GetOrthogonals(i) : GetSurroundings(i);
                    foreach (var j in affectedTiles) {
                        if (new List<Block> { Block.Factory, Block.Central, Block.Firecamp, Block.Accelerator, Block.Coffee, Block.Investor, Block.Distributor }.Contains(fs.BlockStates[j].BlockType)) {
                            fs.BlockStates[j].Cycle += (int)GetBlockBase(block) * (cycles / GetBlockCycle(block));
                        }
                    }
                    fs.BlockStates[i].Cycle = cycles % GetBlockCycle(block);
                } else if (block.BlockType == Block.Missile) {
                    var fireCount = cycles / GetBlockCycle(block);
                    while (fireCount > 0) {
                        var targetTile = GetMissileTarget(fs, i, block.Direction, jumper);
                        if (targetTile >= 0) {
                            if (fs.BlockStates[targetTile].Level > fireCount) {
                                fs.BlockStates[targetTile].Level -= fireCount;
                                fireCount = 0;
                            } else {
                                fs.BlockStates[targetTile].BlockType = Block.None;
                                fs.BlockStates[targetTile].Value = 0;
                                fs.BlockStates[targetTile].Bonus = 0;
                                fs.BlockStates[targetTile].Cycle = 0;
                                fs.BlockStates[targetTile].Money = 0;
                                fireCount -= fs.BlockStates[targetTile].Level;
                            }
                        } else {
                            break;
                        }
                    }
                    fs.BlockStates[i].Cycle = cycles % GetBlockCycle(block);
                }
            }
            for (int i = 0; i < fs.BlockStates.Count; i++) {
                fs.BlockStates[i].Money = tempMoneyArray[i];
            }
        }

        private void CalculateFactoryValue(FrameState fs, int idx) {
            var block = fs.BlockStates[idx];
            block.Value = (GetBlockBase(block) + block.Bonus + GetCoffeeBonus(fs, idx)) * GetBlockMul(block) * GetParkMul(fs, idx) * GetBishopMul(fs, idx) * GetEffectMul(idx) + GetEffectBonus(idx);
        }
        private void CalculateCentralValue(FrameState fs, int idx) {
            var block = fs.BlockStates[idx];
            double bonus = GetFactoryBonus(fs, idx, true) + block.Bonus + GetCoffeeBonus(fs, idx);
            if (bonus <= 0) {
                block.Value = 0;
            } else {
                block.Value = bonus * GetBlockMul(block) * GetBishopMul(fs, idx) * GetEffectMul(idx) + GetEffectBonus(idx);
            }
        }

        private double GetBlockBase(BlockState block) {
            return blockDefinitions[block.BlockType].Base?[block.Level - 1] ?? 0;
        }
        private double GetBlockMul(BlockState block) {
            return blockDefinitions[block.BlockType].Mul?[block.Level - 1] ?? 1;
        }
        private int GetBlockCycle(BlockState block) {
            return blockDefinitions[block.BlockType].Cycle?[block.Level - 1] ?? 1;
        }
        private double GetCoffeeBonus(FrameState fs, int idx) {
            double coffeeBonus = 0;
            foreach (var i in GetSurroundings(idx)) {
                var block = fs.BlockStates[i];
                if (block.BlockType == Block.Coffee) coffeeBonus += GetBlockBase(block) + block.Bonus;
            }
            return coffeeBonus;
        }
        private double GetFactoryBonus(FrameState fs, int idx, bool recurse = false) {
            double factoryBonus = 0;
            var blocksToCheck = fs.BlockStates[idx].Level == 1 ? GetOrthogonals(idx) : fs.BlockStates[idx].Level == 6 ? Enumerable.Range(0, map.Size) : GetSurroundings(idx);
            foreach (var j in blocksToCheck) {
                var block_j = fs.BlockStates[j];
                if (block_j.BlockType == Block.Factory) {
                    if (recurse) CalculateFactoryValue(fs, j);
                    factoryBonus += block_j.Value;
                }
            }
            return factoryBonus;
        }
        private double GetParkMul(FrameState fs, int idx) {
            double parkMul = 1;
            foreach (var j in GetSurroundings(idx)) {
                var block_j = fs.BlockStates[j];
                if (block_j.BlockType == Block.Park) parkMul += block_j.Value;
            }
            return parkMul;
        }
        private double GetBishopMul(FrameState fs, int idx) {
            double bishopMul = 1;
            double bishopDiv = 1;
            foreach (var j in GetDiagonals(idx)) {
                var block_j = fs.BlockStates[j];
                if (block_j.BlockType == Block.Bishop) bishopMul += GetBlockMul(block_j);
            }
            foreach (var j in GetOrthogonals(idx)) {
                var block_j = fs.BlockStates[j];
                if (block_j.BlockType == Block.Bishop) bishopDiv += GetBlockMul(block_j);
            }
            return bishopMul / bishopDiv;
        }
        private double GetEffectBonus(int idx) {
            double effectBonus = 0;
            foreach (Effect effect in map.TileEffects[idx]) {
                if (effect.EffectType == EffectType.ProductionAdd) {
                    effectBonus += effect.Value;
                }
            }
            return effectBonus;
        }
        private double GetEffectMul(int idx) {
            double effectMul = 1;
            foreach (Effect effect in map.TileEffects[idx]) {
                if (effect.EffectType == EffectType.ProductionMul) {
                    effectMul *= effect.Value;
                }
            }
            return effectMul;
        }
        private int GetEffectCycle(int idx) {
            int effectCycle = 1;
            foreach (Effect effect in map.TileEffects[idx]) {
                if (effect.EffectType == EffectType.CycleAdd) {
                    return (int)effect.Value;
                }
            }
            return effectCycle;
        }
        private int GetEffectJumper(int idx) {
            int effectJumper = 0;
            foreach (Effect effect in map.TileEffects[idx]) {
                if (effect.EffectType == EffectType.Jumper) {
                    effectJumper += (int)effect.Value;
                }
            }
            return effectJumper;
        }
        private bool IsHole(int idx) {
            foreach (Effect effect in map.TileEffects[idx]) {
                if (effect.EffectType == EffectType.Hole) {
                    return true;
                }
            }
            return false;
        }
        private int GetBasketballMul(FrameState fs, int i, int j) {
            int mul = 1;
            if (j - i > map.Width) {
                for (int k = i + map.Width; k < j; k += map.Width) {
                    if (fs.BlockStates[k].BlockType == Block.Basketball) mul++;
                }
            } else if (i - j > map.Width) {
                for (int k = j + map.Width; k < i; k += map.Width) {
                    if (fs.BlockStates[k].BlockType == Block.Basketball) mul++;
                }
            } else if (j - i < map.Width && j > i + 1) {
                for (int k = i + 1; k < j; k++) {
                    if (fs.BlockStates[k].BlockType == Block.Basketball) mul++;
                }
            } else if (i - j < map.Width && j < i + 1) {
                for (int k = j + 1; k < i; k++) {
                    if (fs.BlockStates[k].BlockType == Block.Basketball) mul++;
                }
            }
            return mul;
        }

        private int GetPortalTile(FrameState fs, Block portalType) {
            for (int idx = 0; idx < map.Size; idx++) {
                if (fs.BlockStates[idx].BlockType == portalType) {
                    return idx;
                }
            }
            return -1;
        }
        private int GetFacingTile(int idx, Direction direction, int jumper = 0) {
            int x = idx % map.Width;
            int y = idx / map.Width;
            jumper++;
            if (direction == Direction.Up) {
                if (y < jumper) return -1;
                y -= jumper;
            } else if (direction == Direction.Down) {
                if (y >= map.Height - jumper) return -1;
                y += jumper;
            } else if (direction == Direction.Left) {
                if (x < jumper) return -1;
                x -= jumper;
            } else if (direction == Direction.Right) {
                if (x >= map.Width - jumper) return -1;
                x += jumper;
            }
            return x + y * map.Width;
        }
        private int GetMissileTarget(FrameState fs, int idx, Direction direction, int jumper = 0) {
            int x = idx % map.Width;
            int y = idx / map.Width;
            jumper++;
            while (x >= 0 && x < map.Width && y >= 0 && y < map.Height && !IsHole(idx)) {
                if (direction == Direction.Up) {
                    if (y < jumper) return -1;
                    y -= jumper;
                } else if (direction == Direction.Down) {
                    if (y >= map.Height - jumper) return -1;
                    y += jumper;
                } else if (direction == Direction.Left) {
                    if (x < jumper) return -1;
                    x -= jumper;
                } else if (direction == Direction.Right) {
                    if (x >= map.Width - jumper) return -1;
                    x += jumper;
                }
                idx = x + y * map.Width;
                if (fs.BlockStates[idx].BlockType != Block.None) return idx;
            }
            return -1;
        }
        private IEnumerable<int> GetSurroundings(int idx) {
            for (int dx = -1; dx <= 1; dx++) {
                for (int dy = -1; dy <= 1; dy++) {
                    if (dx == 0 && dy == 0) continue;
                    int x = (idx % map.Width) + dx;
                    int y = (idx / map.Width) + dy;
                    if (x < 0 || y < 0 || x >= map.Width || y >= map.Height) continue;
                    yield return y * map.Width + x;
                }
            }
        }
        private IEnumerable<int> GetOrthogonals(int idx) {
            foreach (var d in new List<(int, int)> { (-1, 0), (1, 0), (0, -1), (0, 1) }) {
                int x = (idx % map.Width) + d.Item1;
                int y = (idx / map.Width) + d.Item2;
                if (x < 0 || y < 0 || x >= map.Width || y >= map.Height) continue;
                yield return y * map.Width + x;
            }
        }
        private IEnumerable<int> GetDiagonals(int idx) {
            foreach (var d in new List<(int, int)> { (-1, -1), (1, -1), (1, 1), (-1, 1) }) {
                int x = (idx % map.Width) + d.Item1;
                int y = (idx / map.Width) + d.Item2;
                if (x < 0 || y < 0 || x >= map.Width || y >= map.Height) continue;
                yield return y * map.Width + x;
            }
        }
    }
}
