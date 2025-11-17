using CoinFactorySim;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace CoinFactorySim {
    public partial class Form1 : Form {
        private List<Label> tiles = [];
        private List<Label> pickers = [];
        private List<Label> frames = [];

        private List<FrameState> frameStates = [];
        private Dictionary<Block, int> blockCount = [];
        private List<List<FrameAction>> frameActions = []; 

        private List<Label> copyButtons = [];
        private List<Label> pasteButtons = [];
        private List<Label> clearButtons = [];
        private List<Label> saveButtons = [];
        private List<Label> loadButtons = [];
        private List<Label> notationButtons = [];

        private int currentFrame = 0;
        private int currentPicker = -1;
        private int currentTile = -1;
        private int copiedFrameSet = -1;
        private int frameOffset = 0;

        const int FRAME_SETS = 4;
        const int FRAME_COUNT = 500;
        const int FRAME_DISPLAY_COUNT = 150;
        const int CTRL_SCROLL_COUNT = 10;

        double SCALE = 2;
        double FONT_SCALE = 1;
        int BUTTON_START_X;
        int BUTTON_START_Y;
        int BUTTON_WIDTH;
        int BUTTON_HEIGHT;
        int BUTTON_DISTANCE;
        int FRAME_START_X;
        int FRAME_START_Y;
        int BLOCK_SIZE;
        int FRAME_SIZE;
        int FRAME_HEIGHT;
        int FRAME_DISTANCE;
        int UI_START_X;
        int UI_START_Y;
        int TO_SIDE;
        int TO_BORDER;
        int THICKNESS;

        
        private readonly Dictionary<Block, BlockDefinition> BLOCKS_BY_TYPE;
        private readonly MapDefinition map = Consts.Map_Tiny;

        private Simulator simulator;
        private NotationGenerator notationGenerator;

        public Form1() {
            InitializeComponent();
            Text = this.ClientSize.Width.ToString();
            SCALE = ClientSize.Width / 1500.0;
            FONT_SCALE = ClientSize.Width / 2700.0;
            BUTTON_START_X = (int)(2 * SCALE);
            BUTTON_START_Y = (int)(20 * SCALE);
            BUTTON_WIDTH = (int)(40 * SCALE);
            BUTTON_HEIGHT = (int)(18 * SCALE);
            BUTTON_DISTANCE = (int)(8 * SCALE);
            FRAME_START_X = BUTTON_START_X + BUTTON_WIDTH * 2 + BUTTON_DISTANCE * 2;
            FRAME_START_Y = BUTTON_START_Y;
            BLOCK_SIZE = (int)(60 * SCALE);
            FRAME_SIZE = (int)(9 * SCALE);
            FRAME_HEIGHT = (int)(70 * SCALE);
            FRAME_DISTANCE = (int)(28 * SCALE);
            UI_START_X = (int)(50 * SCALE);
            UI_START_Y = (int)(450 * SCALE);
            TO_SIDE = (int)(15 * SCALE);
            TO_BORDER = (int)(10 * SCALE);
            THICKNESS = (int)(5 * SCALE);

            BLOCKS_BY_TYPE = Consts.BLOCKS.ToDictionary(b => b.BlockType);
            Label_Money.Location = new Point((int)(UI_START_X + (BLOCK_SIZE * map.Width) / 2.0f - 56), UI_START_Y - 50);

            for (int i = 0; i < FRAME_SETS; i++) {
                for (int j = 0; j < FRAME_COUNT; j++) {
                    frameActions.Add([]);
                }
            }
            simulator = new(map, BLOCKS_BY_TYPE);
            notationGenerator = new(map, BLOCKS_BY_TYPE);
        }

        private void Form1_Load(object? sender, EventArgs e) {
            for (int i = 0; i < FRAME_SETS; i++) {
                copyButtons.Add(new Label {
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(BUTTON_START_X, BUTTON_START_Y + (FRAME_HEIGHT + FRAME_DISTANCE) * i),
                    Size = new Size(BUTTON_WIDTH, BUTTON_HEIGHT),
                    Text = "Copy",
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White,
                });
                pasteButtons.Add(new Label {
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(BUTTON_START_X + BUTTON_WIDTH + BUTTON_DISTANCE, BUTTON_START_Y + (FRAME_HEIGHT + FRAME_DISTANCE) * i),
                    Size = new Size(BUTTON_WIDTH, BUTTON_HEIGHT),
                    Text = "Paste",
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White,
                });
                saveButtons.Add(new Label {
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(BUTTON_START_X, BUTTON_START_Y + BUTTON_HEIGHT + BUTTON_DISTANCE + (FRAME_HEIGHT + FRAME_DISTANCE) * i),
                    Size = new Size(BUTTON_WIDTH, BUTTON_HEIGHT),
                    Text = "Save",
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White,
                });
                loadButtons.Add(new Label {
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(BUTTON_START_X + BUTTON_WIDTH + BUTTON_DISTANCE, BUTTON_START_Y + BUTTON_HEIGHT + BUTTON_DISTANCE + (FRAME_HEIGHT + FRAME_DISTANCE) * i),
                    Size = new Size(BUTTON_WIDTH, BUTTON_HEIGHT),
                    Text = "Load",
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White,
                });
                clearButtons.Add(new Label {
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(BUTTON_START_X, BUTTON_START_Y + (BUTTON_HEIGHT + BUTTON_DISTANCE) * 2 + (FRAME_HEIGHT + FRAME_DISTANCE) * i),
                    Size = new Size(BUTTON_WIDTH, BUTTON_HEIGHT),
                    Text = "Clear",
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White,
                });
                notationButtons.Add(new Label {
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(BUTTON_START_X + BUTTON_WIDTH + BUTTON_DISTANCE, BUTTON_START_Y + (BUTTON_HEIGHT + BUTTON_DISTANCE) * 2 + (FRAME_HEIGHT + FRAME_DISTANCE) * i),
                    Size = new Size(BUTTON_WIDTH, BUTTON_HEIGHT),
                    Text = "Export",
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White,
                });
                copyButtons[i].MouseDown += On_Any_Button_MouseDown;
                pasteButtons[i].MouseDown += On_Any_Button_MouseDown;
                saveButtons[i].MouseDown += On_Any_Button_MouseDown;
                loadButtons[i].MouseDown += On_Any_Button_MouseDown;
                clearButtons[i].MouseDown += On_Any_Button_MouseDown;
                notationButtons[i].MouseDown += On_Any_Button_MouseDown;
                copyButtons[i].MouseUp += On_Any_Button_MouseUp;
                pasteButtons[i].MouseUp += On_Any_Button_MouseUp;
                saveButtons[i].MouseUp += On_Any_Button_MouseUp;
                loadButtons[i].MouseUp += On_Any_Button_MouseUp;
                clearButtons[i].MouseUp += On_Any_Button_MouseUp;
                notationButtons[i].MouseUp += On_Any_Button_MouseUp;
                copyButtons[i].MouseUp += On_Copy_Click;
                pasteButtons[i].MouseUp += On_Paste_Click;
                saveButtons[i].MouseUp += On_Save_Click;
                loadButtons[i].MouseUp += On_Load_Click;
                clearButtons[i].MouseUp += On_Clear_Click;
                notationButtons[i].MouseUp += On_Notation_Click;
                Controls.Add(copyButtons[i]);
                Controls.Add(pasteButtons[i]);
                Controls.Add(saveButtons[i]);
                Controls.Add(loadButtons[i]);
                Controls.Add(clearButtons[i]);
                Controls.Add(notationButtons[i]);
            }
            for (int i = 0; i < map.Height; i++) {
                for (int j = 0; j < map.Width; j++) {
                    int idx = i * map.Width + j;
                    tiles.Add(new Label {
                        TextAlign = ContentAlignment.MiddleCenter,
                        Location = new Point(UI_START_X + BLOCK_SIZE * j, UI_START_Y + BLOCK_SIZE * i),
                        BorderStyle = BorderStyle.FixedSingle,
                        Size = new Size(BLOCK_SIZE, BLOCK_SIZE),
                        BackColor = Color.White,
                        Margin = new Padding(0),
                        ClientSize = new Size(BLOCK_SIZE, BLOCK_SIZE),
                    });
                    tiles[idx].Click += On_Tile_Click;
                    tiles[idx].Paint += On_Tile_Paint;
                    tiles[idx].MouseMove += On_Tile_MouseMove;
                    tiles[idx].MouseWheel += On_Tile_MouseWheel;
                    Controls.Add(tiles[idx]);
                }
            }
            for (int i = 0; i < Consts.BLOCKS.Length - 1; i++) {
                pickers.Add(new Label {
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(UI_START_X + BLOCK_SIZE * (1 + map.Width + i % 4), UI_START_Y + BLOCK_SIZE * (i - i % 4) / 4),
                    BorderStyle = BorderStyle.FixedSingle,
                    Size = new Size(BLOCK_SIZE, BLOCK_SIZE),
                    BackColor = Color.White,
                    Margin = new Padding(0),
                    ClientSize = new Size(BLOCK_SIZE, BLOCK_SIZE),
                });
                pickers[i].Click += On_Picker_Click;
                pickers[i].Paint += On_Picker_Paint;
                Controls.Add(pickers[i]);
            }
            for (int i = 0; i < FRAME_SETS; i++) {
                for (int j = 0; j < FRAME_DISPLAY_COUNT; j++) {
                    int idx = i * FRAME_DISPLAY_COUNT + j;
                    frames.Add(new Label {
                        TextAlign = ContentAlignment.MiddleCenter,
                        Location = new Point(FRAME_START_X + FRAME_SIZE * j, FRAME_START_Y + i * (FRAME_HEIGHT + FRAME_DISTANCE)),
                        BorderStyle = BorderStyle.FixedSingle,
                        Size = new Size(FRAME_SIZE, FRAME_HEIGHT),
                        BackColor = Color.White,
                        Margin = new Padding(0),
                        ClientSize = new Size(FRAME_SIZE, FRAME_HEIGHT),
                    });
                    frames[idx].Click += On_Frame_Click;
                    frames[idx].Paint += On_Frame_Paint;
                    Controls.Add(frames[idx]);
                }
            }
            for (int i = 0; i < FRAME_SETS; i++) {
                for (int j = 0; j < FRAME_COUNT; j++) {
                    int idx = i * FRAME_COUNT + j;
                    frameStates.Add(new FrameState(map.Size));
                }
            }
        }
        private void On_Any_Button_MouseDown(object? sender, EventArgs e) {
            Label label = sender as Label;
            int index = copyButtons.IndexOf(label);
            label.BackColor = Color.LightGray;
        }
        private void On_Any_Button_MouseUp(object? sender, EventArgs e) {
            Label label = sender as Label;
            int index = copyButtons.IndexOf(label);
            label.BackColor = Color.White;
        }
        private void On_Copy_Click(object? sender, EventArgs e) {
            Label label = sender as Label;
            int index = copyButtons.IndexOf(label);
            if (copiedFrameSet >= 0) copyButtons[copiedFrameSet].BackColor = Color.White;
            copiedFrameSet = index;
            label.BackColor = Color.Orange;
        }
        private void On_Paste_Click(object? sender, EventArgs e) {
            Label label = sender as Label;
            int index = pasteButtons.IndexOf(label);
            if (copiedFrameSet >= 0) {
                copyButtons[copiedFrameSet].BackColor = Color.White;
                if (copiedFrameSet != index) {
                    for (int i = 0; i < FRAME_COUNT; i++) {
                        frameStates[FRAME_COUNT * index + i] = frameStates[FRAME_COUNT * copiedFrameSet + i].Clone();
                        frameActions[FRAME_COUNT * index + i] = frameActions[FRAME_COUNT * copiedFrameSet + i].ConvertAll(item => item.ShallowCopy());
                    }
                    InvalidateFrameSet(index);
                }
                copyButtons[copiedFrameSet].BackColor = Color.White;
                copiedFrameSet = -1;
                Render_Frame();
            }
        }
        private void On_Save_Click(object? sender, EventArgs e) {
            Label label = sender as Label;
            int index = saveButtons.IndexOf(label);
            List<FrameState> tempFrameStates = [];
            for (int i = 0; i < FRAME_COUNT; i++) {
                tempFrameStates.Add(frameStates[FRAME_COUNT * index + i]);
            }
            Utils.SaveToFile($"saves/{map.Name}_{index}.txt", tempFrameStates);
        }
        private void On_Load_Click(object? sender, EventArgs e) {
            Label label = sender as Label;
            int index = loadButtons.IndexOf(label);
            try {
                List<FrameState> tempFrameStates = Utils.LoadFromFile<List<FrameState>>($"saves/{map.Name}_{index}.txt");
                for (int i = 0; i < Math.Min(FRAME_COUNT, tempFrameStates.Count); i++) {
                    frameStates[FRAME_COUNT * index + i] = tempFrameStates[i];
                }
                InvalidateFrameSet(index);
                frames[GetFrameLabelIndex(currentFrame)].Invalidate();
                currentFrame = (currentFrame % FRAME_COUNT) + FRAME_COUNT * index;
                Update_Frames();
            } catch { }
            Render_Frame();
        }
        private void On_Clear_Click(object? sender, EventArgs e) {
            Label label = sender as Label;
            int index = clearButtons.IndexOf(label);
            for (int i = 0; i < FRAME_COUNT; i++) {
                frameStates[FRAME_COUNT * index + i] = new(map.Size);
                frameActions[FRAME_COUNT * index + i].Clear();
            }
            InvalidateFrameSet(index);
            Render_Frame();
        }
        private void On_Notation_Click(object? sender, EventArgs e) {
            Label label = sender as Label;
            int index = notationButtons.IndexOf(label);
            string notations = notationGenerator.Generate(frameStates, frameActions, index * FRAME_COUNT, (index + 1) * FRAME_COUNT);
            if (notations.Length > 0) {
                Clipboard.SetText(notations);
            }
        }

        private void On_Frame_Paint(object? sender, PaintEventArgs e) {
            Label label = sender as Label;
            int frameLabelIndex = frames.IndexOf(label);
            int index = GetFrameIndex(frameLabelIndex);

            // Draw keyframe
            if (frameStates[index].IsKeyFrame) {
                Color color = frameStates[index].Errored ? Color.Red : Color.LightGray;
                using Brush fillBrush = new SolidBrush(color);
                e.Graphics.FillRectangle(fillBrush, label.ClientRectangle);
            }
            // Draw actions
            if (!frameStates[index].Errored && frameActions[index].Count > 0) {
                float maxHeight = label.Height - (float)(20f * SCALE);
                float unitHeight = Math.Min(maxHeight / frameActions[index].Count, (float)(20f * SCALE));
                for (int i = 0; i < frameActions[index].Count; i++) {
                    ActionType actionType = frameActions[index][i].Action;
                    if (actionType == ActionType.Upgrade || actionType == ActionType.Buy) {
                        Color color = BLOCKS_BY_TYPE[frameActions[index][i].Block].Color;
                        using Brush fillBrush = new SolidBrush(color);
                        e.Graphics.FillRectangle(fillBrush, new RectangleF(0, i * unitHeight, label.Width, unitHeight));
                        if (actionType == ActionType.Upgrade) {
                            using Font font = new("Arial", (int)(9 * FONT_SCALE), FontStyle.Bold);
                            using Brush brush = new SolidBrush(Color.Black);
                            e.Graphics.DrawString("U", font, brush, new PointF(label.Width / 2, (i + 1) * unitHeight - 10), Consts.CenterFormat);
                        }
                    }
                }
            }
            // Draw border
            if (index == currentFrame) {
                DrawSelectionBorder(e.Graphics, label);
            } else {
                ControlPaint.DrawBorder(e.Graphics, label.DisplayRectangle, Color.Black, ButtonBorderStyle.Solid);
            }
            // Draw stage
            if (frameStates[index].Stage > 0) {
                string stage = frameStates[index].Stage.ToString();
                using Font font = new("Arial", (int)((stage.Length == 1 ? 12 : 9) * FONT_SCALE), FontStyle.Bold);
                using Brush brush = new SolidBrush(Color.Black);
                e.Graphics.DrawString(stage, font, brush, new PointF(label.Width / 2, label.Height - 18), Consts.CenterFormat);
            }
        }
        private void On_Frame_Click(object? sender, EventArgs e) {
            MouseEventArgs me = (MouseEventArgs)e;
            Label label = sender as Label;
            int index = frames.IndexOf(label);

            var oldFrame = currentFrame;
            currentFrame = GetFrameIndex(index);
            frames[GetFrameLabelIndex(oldFrame)].Invalidate();
            label.Invalidate();
            if (me.Button == MouseButtons.Right) {
                frameStates[currentFrame].IsKeyFrame = false;
                Update_Frames();
            }
            Render_Frame();
        }
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            for (int i = 0; i < FRAME_DISPLAY_COUNT; i++) {
                if ((i + frameOffset) % 30 == 0) {
                    e.Graphics.DrawString(Utils.GetTimeString(i + frameOffset), Font, Brushes.Black, (int)((i + 0.5) * FRAME_SIZE + FRAME_START_X), FRAME_START_Y - 15, Consts.CenterFormat);
                }
            }
        }

        private void InvalidateFrameSet(int index) {
            for (int i = 0; i < FRAME_DISPLAY_COUNT; i++) {
                frames[FRAME_DISPLAY_COUNT * index + i].Invalidate();
            }
        }

        private int GetFrameIndex(int frameLabelIndex) {
            return frameLabelIndex / FRAME_DISPLAY_COUNT * FRAME_COUNT + (frameLabelIndex % FRAME_DISPLAY_COUNT) + frameOffset;
        }
        private int GetFrameLabelIndex(int frameIndex) {
            return frameIndex / FRAME_COUNT * FRAME_DISPLAY_COUNT + (frameIndex - frameOffset) % FRAME_COUNT;
        }
        private void On_Picker_Paint(object? sender, PaintEventArgs e) {
            Label label = sender as Label;
            int index = pickers.IndexOf(label);

            Block block = Consts.BLOCKS[index].BlockType;
            DrawBlock(e.Graphics, label, block);

            if (index == currentPicker) {
                DrawSelectionBorder(e.Graphics, label);
            } else {
                ControlPaint.DrawBorder(e.Graphics, label.DisplayRectangle, Color.Black, ButtonBorderStyle.Solid);
            }

            if (block != Block.Exit && block != Block.Belt) {
                using Font font = new("Arial", (int)(14 * FONT_SCALE), FontStyle.Bold);
                long mul = 1;
                if (blockCount.TryGetValue(block, out int count)) mul <<= count;
                if (BLOCKS_BY_TYPE[block].IsFirstFree && count == 0) mul = 0;
                long price = Consts.BLOCKS[index].Prices[0] * mul;
                bool canPurchase = price <= frameStates[currentFrame].Money;
                RenderBorderedText(e.Graphics, Utils.FormatNumber(price), font, new PointF(label.Width / 2f, label.Height / 2f), canPurchase ? Color.Lime : Color.White);
            }
        }
        private void On_Picker_Click(object? sender, EventArgs e) {
            Label label = sender as Label;
            int index = pickers.IndexOf(label);

            var oldPicker = currentPicker;
            currentPicker = index;
            if (oldPicker >= 0) pickers[oldPicker].Invalidate();
            label.Invalidate();
        }
        private void On_Tile_Paint(object? sender, PaintEventArgs e) {
            Label label = sender as Label;
            int index = tiles.IndexOf(label);

            BlockState bs = frameStates[currentFrame].BlockStates[index];
            DrawBlock(e.Graphics, label, bs.BlockType);

            if (index == currentTile) {
                DrawSelectionBorder(e.Graphics, label);
            } else {
                BlockState prev = new();
                if (currentFrame > 0) prev = frameStates[currentFrame - 1].BlockStates[index];
                if (bs.BlockType == prev.BlockType && (frameActions[currentFrame].Any(item => item.Index == index && item.Action == ActionType.Upgrade) || bs.Direction != prev.Direction)) {
                    using Pen pen = new(Color.Lime, 3f);
                    pen.Alignment = PenAlignment.Inset;
                    var rect = label.ClientRectangle;
                    e.Graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                }
                ControlPaint.DrawBorder(e.Graphics, label.DisplayRectangle, Color.Black, ButtonBorderStyle.Solid);
            }

            if (bs.BlockType != Block.None && bs.Direction != Direction.None) {
                using Pen myPen = new(Color.Magenta, THICKNESS);
                if (bs.Direction == Direction.Down) {
                    e.Graphics.DrawLine(myPen, new Point(TO_SIDE, label.Height - TO_BORDER), new Point(label.Width - TO_SIDE, label.Height - TO_BORDER));
                } else if (bs.Direction == Direction.Left) {
                    e.Graphics.DrawLine(myPen, new Point(TO_BORDER, TO_SIDE), new Point(TO_BORDER, label.Height - TO_SIDE));
                } else if (bs.Direction == Direction.Up) {
                    e.Graphics.DrawLine(myPen, new Point(TO_SIDE, TO_BORDER), new Point(label.Width - TO_SIDE, TO_BORDER));
                } else if (bs.Direction == Direction.Right) {
                    e.Graphics.DrawLine(myPen, new Point(label.Width - TO_BORDER, TO_SIDE), new Point(label.Width - TO_BORDER, label.Height - TO_SIDE));
                }
            }
            if (bs.BlockType != Block.None && BLOCKS_BY_TYPE[bs.BlockType].Prices.Length > 1) {
                long upgradePrice = GetBlockUpgradePrice(bs);
                bool canUpgrade = upgradePrice >= 0 && upgradePrice <= frameStates[currentFrame].Money;
                using Font font = new("Arial", (int)(11 * FONT_SCALE), FontStyle.Bold);
                RenderBorderedText(e.Graphics, $"{bs.Level}/{BLOCKS_BY_TYPE[bs.BlockType].Prices.Length}", font, new PointF(TO_BORDER, TO_BORDER), canUpgrade ? Color.Lime : Color.White);
                RenderBorderedText(e.Graphics, Utils.FormatNumber(upgradePrice), font, new PointF(label.Width - TO_BORDER, TO_BORDER), canUpgrade ? Color.Lime : Color.White);
            }
            using Font centerFont = new("Arial", (int)(14 * FONT_SCALE), FontStyle.Bold);
            if (new List<Block> { Block.Belt, Block.Jumper, Block.PortalA, Block.PortalB, Block.Exit, Block.None}.Contains(bs.BlockType)) {
                if (bs.Money > 0) {
                    RenderBorderedText(e.Graphics, Utils.FormatNumber(bs.Money), centerFont, new PointF(label.Width / 2f, label.Height / 2f), Color.Red);
                }
            } else {
                RenderBorderedText(e.Graphics, Utils.FormatNumber(bs.Value), centerFont, new PointF(label.Width / 2f, label.Height / 2f), Color.White);
            }
            using Font cycleFont = new("Arial", (int)(11 * FONT_SCALE), FontStyle.Bold);
            if (BLOCKS_BY_TYPE[bs.BlockType].IsCyclic) {
                RenderBorderedText(e.Graphics, Utils.FormatNumber(GetBlockCycle(bs) - bs.Cycle), cycleFont, new PointF(label.Width / 2f - 15, label.Height - TO_BORDER), Color.LightGreen);
            }
            if ((bs.BlockType == Block.Coffee || bs.BlockType == Block.Firecamp) && bs.Level > 1) {
                RenderBorderedText(e.Graphics, Utils.FormatNumber(bs.RemainingCycle), cycleFont, new PointF(label.Width / 2f + 15, label.Height - TO_BORDER), Color.CornflowerBlue);
            }
        }
        private void On_Tile_Click(object? sender, EventArgs e) {
            MouseEventArgs me = (MouseEventArgs)e;

            Label label = sender as Label;
            int index = tiles.IndexOf(label);

            FrameState state = frameStates[currentFrame];
            List<BlockState> blockStates = state.BlockStates;

            if (me.Button == MouseButtons.Left) { // Buy
                if (currentPicker < 0) return;
                var blockType = Consts.BLOCKS[currentPicker].BlockType;

                blockStates[index] = new BlockState() {
                    BlockType = Consts.BLOCKS[currentPicker].BlockType,
                    Direction = Consts.BLOCKS[currentPicker].IsDirectional ? Direction.Down : Direction.None,
                    Level = 1,
                    Cycle = 0,
                };
                state.IsKeyFrame = true;
                Update_Frames();
            } else if (me.Button == MouseButtons.Right) { // Delete
                blockStates[index].BlockType = Block.None;
                blockStates[index].Direction = Direction.None;
                state.IsKeyFrame = true;
                Update_Frames();
            } else if (me.Button == MouseButtons.Middle) { // Upgrade
                if (blockStates[index].BlockType != Block.None && blockStates[index].Level < BLOCKS_BY_TYPE[blockStates[index].BlockType].Prices.Length) {
                    blockStates[index].Level++;
                    state.IsKeyFrame = true;
                    Update_Frames();
                }
            }
            Render_Frame();
        }

        private void On_Tile_MouseMove(object? sender, EventArgs e) {
            Label label = sender as Label;
            int index = tiles.IndexOf(label);
            if (currentTile != index) {
                var oldTile = currentTile;
                currentTile = index;
                if (oldTile >= 0) tiles[oldTile].Refresh();
                tiles[currentTile].Refresh();
            }
        }
        private void On_Tile_MouseWheel(object? sender, MouseEventArgs e) {
            Label label = sender as Label;
            int index = tiles.IndexOf(label);

            BlockState bs = frameStates[currentFrame].BlockStates[index];
            if (bs.BlockType == Block.None || !BLOCKS_BY_TYPE[bs.BlockType].IsDirectional) return;

            if (e.Delta > 0) {
                if (bs.Direction == Direction.Up) {
                    bs.Direction = Direction.Left;
                } else if (bs.Direction == Direction.Left) {
                    bs.Direction = Direction.Down;
                } else if (bs.Direction == Direction.Down) {
                    bs.Direction = Direction.Right;
                } else {
                    bs.Direction = Direction.Up;
                }
            } else {
                if (bs.Direction == Direction.Up) {
                    bs.Direction = Direction.Right;
                } else if (bs.Direction == Direction.Left) {
                    bs.Direction = Direction.Up;
                } else if (bs.Direction == Direction.Down) {
                    bs.Direction = Direction.Left;
                } else {
                    bs.Direction = Direction.Down;
                }
            }
            Update_Frames();
            tiles[currentTile].Invalidate();
        }

        private void On_KeyDown(object? sender, KeyEventArgs e) {
            int frameSetStart = currentFrame - (currentFrame % FRAME_COUNT);
            int frameSetEnd = frameSetStart + FRAME_COUNT;
            int frameWindowStart = frameSetStart + frameOffset;
            int frameWindowEnd = frameWindowStart + FRAME_DISPLAY_COUNT;
            if (e.KeyCode == Keys.Left) {
                int targetFrame = Math.Max(frameSetStart, e.Control ? currentFrame - CTRL_SCROLL_COUNT : currentFrame - 1);
                if (targetFrame >= frameWindowStart) {
                    frames[GetFrameLabelIndex(currentFrame)].Invalidate();
                    frames[GetFrameLabelIndex(targetFrame)].Invalidate();
                    currentFrame = targetFrame;
                    Render_Frame();
                } else if (currentFrame != targetFrame) {
                    currentFrame = targetFrame;
                    frameOffset = currentFrame - frameSetStart;
                    for (int i = 0; i < FRAME_SETS; i++) InvalidateFrameSet(i);
                    Invalidate();
                    Render_Frame();
                }
            } else if (e.KeyCode == Keys.Right) {
                int targetFrame = Math.Min(frameSetEnd - 1, e.Control ? currentFrame + CTRL_SCROLL_COUNT : currentFrame + 1);
                if (targetFrame <= frameWindowEnd - 1) {
                    frames[GetFrameLabelIndex(currentFrame)].Invalidate();
                    frames[GetFrameLabelIndex(targetFrame)].Invalidate();
                    currentFrame = targetFrame;
                    Render_Frame();
                } else if (currentFrame != targetFrame) {
                    currentFrame = targetFrame;
                    frameOffset = currentFrame - frameSetStart - FRAME_DISPLAY_COUNT + 1;
                    for (int i = 0; i < FRAME_SETS; i++) InvalidateFrameSet(i);
                    Invalidate();
                    Render_Frame();
                }
            } else if (e.KeyCode == Keys.Up && currentFrame / FRAME_COUNT > 0) {
                currentFrame -= FRAME_COUNT;
                frames[GetFrameLabelIndex(currentFrame)].Invalidate();
                frames[GetFrameLabelIndex(currentFrame) + FRAME_DISPLAY_COUNT].Invalidate();
                Render_Frame();
            } else if (e.KeyCode == Keys.Down && currentFrame / FRAME_COUNT < FRAME_SETS - 1) {
                currentFrame += FRAME_COUNT;
                frames[GetFrameLabelIndex(currentFrame)].Invalidate();
                frames[GetFrameLabelIndex(currentFrame) - FRAME_DISPLAY_COUNT].Invalidate();
                Render_Frame();
            } else if (e.KeyCode == Keys.Subtract) { // Remove empty frame
                if (!frameStates[currentFrame].IsKeyFrame) {
                    for (int i = currentFrame; i < frameSetEnd - 1; i++) frameStates[i] = frameStates[i + 1];
                    frameStates[frameSetEnd - 1] = new FrameState(map.Size);
                    Update_Frames();
                    Render_Frame();
                }
            } else if (e.KeyCode == Keys.Add) { // Add empty frame
                for (int i = frameSetEnd - 1; i > currentFrame; i--) frameStates[i] = frameStates[i - 1];
                frameStates[currentFrame] = new FrameState(map.Size);
                Update_Frames();
                Render_Frame();
            }
        }
        private void DrawBlock(Graphics g, Label label, Block blockType) {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color fillColor = blockType == Block.None ? Color.White : BLOCKS_BY_TYPE[blockType].Color;
            Color? borderColor = blockType == Block.None ? null : BLOCKS_BY_TYPE[blockType].SecondaryColor;

            using Brush fillBrush = new SolidBrush(fillColor);
            g.FillRectangle(fillBrush, label.ClientRectangle);
            
            if (borderColor != null) {
                float penWidth = (float)(3f * SCALE);
                if (blockType == Block.PortalA || blockType == Block.PortalB) penWidth = (float)(7f * SCALE); ;
                using Pen pen = new((Color)borderColor, penWidth);
                var rc = label.ClientRectangle;
                float diameter = Math.Min(rc.Width, rc.Height) * 0.6f;
                float x = rc.Left + (rc.Width - diameter) / 2f;
                float y = rc.Top + (rc.Height - diameter) / 2f;
                var rect = new RectangleF(x, y, diameter, diameter);

                if (blockType != Block.Missile) {
                    g.DrawEllipse(pen, rect);
                } else {
                    g.DrawRectangle(pen, rect);
                }
            }
        }
        private void DrawSelectionBorder(Graphics g, Label label) {
            using Pen pen = new(Color.Red, 3f);
            pen.Alignment = PenAlignment.Inset;
            var rect = label.ClientRectangle;
            g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }
        private void Render_Frame() {
            FrameState state = frameStates[currentFrame];
            blockCount.Clear();
            for (int i = 0; i < tiles.Count; i++) {
                Block blockType = state.BlockStates[i].BlockType;
                if (blockType != Block.None) {
                    if (!blockCount.ContainsKey(blockType)) {
                        blockCount[blockType] = 0;
                    }
                    blockCount[blockType]++;
                }
                tiles[i].Invalidate();
            }
            Label_Money.Text = $"Money: {Utils.FormatNumber(state.Money)}";

            foreach (var picker in pickers) picker.Invalidate();
            this.Text = Utils.GetTimeString(currentFrame % FRAME_COUNT);
        }
        private void Update_Frames() {
            int frameStart = currentFrame - (currentFrame % FRAME_COUNT);
            int frameEnd = frameStart + FRAME_COUNT;
            InvalidateFrameSet(currentFrame / FRAME_COUNT);
            foreach (var picker in pickers) picker.Invalidate();
            simulator.SimulateFrames(frameStates, frameActions, frameStart, frameEnd);
        }

        private void RenderBorderedText(Graphics g, string text, Font font, PointF position, Color fontColor) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using GraphicsPath path = new();
            path.AddString(text, font.FontFamily, (int)font.Style, font.Size * 2f, position, Consts.CenterFormat);
            using Pen outlinePen = new(Color.FromArgb(15, 16, 17), (float)(3f * SCALE));
            g.DrawPath(outlinePen, path);
            using SolidBrush textBrush = new(fontColor);
            g.FillPath(textBrush, path);
        }
        private int GetBlockCycle(BlockState block) {
            return BLOCKS_BY_TYPE[block.BlockType].Cycle?[block.Level - 1] ?? 1;
        }
        private long GetBlockUpgradePrice(BlockState block) {
            if (block.Level >= BLOCKS_BY_TYPE[block.BlockType].Prices.Length) return -1;
            return BLOCKS_BY_TYPE[block.BlockType].Prices[block.Level];
        }
    }
}