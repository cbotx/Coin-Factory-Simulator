using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinFactorySim {

    public class NotationGenerator(MapDefinition map, Dictionary<Block, BlockDefinition> blockDefinitions) {
        private readonly MapDefinition map = map;
        private readonly Dictionary<Block, BlockDefinition> blockDefinitions = blockDefinitions;

        public string Generate(List<FrameState> frameStates, List<List<FrameAction>> frameActions, int frameStart, int frameEnd) {
            string notations = "";
            bool isPreviousGroup = false;
            for (int i = frameStart; i < frameEnd; i++) {
                if (frameActions[i].Count > 1) {
                    if (!isPreviousGroup) notations += "\n";
                    notations += $"\n[{Utils.GetTimeString(i - frameStart)}] ";
                } else if (frameActions[i].Count == 1) {
                    notations += $"\n[{Utils.GetTimeString(i - frameStart)}] ";
                }
                for (int j = 0; j < frameActions[i].Count; j++) {
                    FrameAction action = frameActions[i][j];
                    int x = action.Index % map.Width;
                    int y = map.Height - action.Index / map.Width;
                    string upgradeString = "";
                    if (action.Action == ActionType.Upgrade && action.Count > 1) upgradeString = " " + action.Count.ToString();
                    notations += $"{action.Action}{upgradeString} ({action.Block} at {(char)(97 + x)}{y}";
                    if (action.Action == ActionType.Rotate) notations += $" facing {action.Direction}";
                    notations += ")";
                    if (j < frameActions[i].Count - 1) notations += ", ";
                }
                if (frameActions[i].Count > 0) {
                    notations += $" Money: {Utils.FormatNumber(frameStates[i].Money)}";
                }
                if (frameActions[i].Count > 1) {
                    notations += $"\n";
                    isPreviousGroup = true;
                } else if (frameActions[i].Count == 1) {
                    isPreviousGroup = false;
                }
            }
            return notations;
        }
    }
}
