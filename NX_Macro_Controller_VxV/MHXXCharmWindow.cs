// MHXXCharmWindow.cs
// MHXXお守りタブをダブルクリックしたときに開くポップアウトウィンドウ
// 検索結果をカード形式で横一覧表示する

using System;
using System.Drawing;
using System.Windows.Forms;

namespace NX_Macro_Controller_VxV
{
    public sealed class MHXXCharmWindow : Form
    {
        private readonly MHXXCharmRNG _rng;
        private readonly FlowLayoutPanel _cardsFlow;
        private readonly Label _lblCount;

        // ツール本体のダークテーマに合わせたカラー
        private static readonly Color BgMain  = Color.FromArgb(33, 33, 35);
        private static readonly Color BgDark  = Color.FromArgb(28, 28, 28);
        private static readonly Color BgBar   = Color.FromArgb(40, 40, 42);
        private static readonly Color Divider = Color.FromArgb(55, 55, 60);

        public MHXXCharmWindow()
        {
            // ── フォーム設定 ──
            Text            = "MHXX お守り  —  詳細検索ビュー";
            BackColor       = BgMain;
            ForeColor       = Color.White;
            Size            = new Size(1280, 720);
            MinimumSize     = new Size(900, 560);
            Font            = new Font("Segoe UI", 9f);
            StartPosition   = FormStartPosition.CenterParent;

            // ── レイアウト ──
            // 左: 検索コントロール (MHXXCharmRNG のカードモード)
            // 右: カード一覧パネル

            var splitMain = new SplitContainer
            {
                Dock             = DockStyle.Fill,
                Orientation      = Orientation.Vertical,
                BackColor        = Divider,
                SplitterWidth    = 3,
                Panel1MinSize    = 360,
                Panel2MinSize    = 400
            };
            splitMain.Panel1.BackColor = BgMain;
            splitMain.Panel2.BackColor = BgDark;
            Controls.Add(splitMain);

            // ── 左パネル: 検索 UI ──
            _rng = new MHXXCharmRNG(cardMode: false);   // グリッドは非表示でなく、ここでは結果表示しない
            _rng.Dock = DockStyle.Fill;
            // 検索ヒット時にカードを右パネルに追加
            _rng.CharmFound += OnCharmFound;
            splitMain.Panel1.Controls.Add(_rng);

            // ── 右パネル: カード表示 ──

            // 上段ツールバー
            var toolbar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 32,
                BackColor = BgBar,
                Padding   = new Padding(6, 4, 6, 4)
            };

            _lblCount = new Label
            {
                Dock      = DockStyle.Left,
                Width     = 120,
                Text      = "0 件",
                ForeColor = Color.LightGray,
                TextAlign = ContentAlignment.MiddleLeft,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            var btnClearCards = new Button
            {
                Text      = "🗑 カードをクリア",
                Dock      = DockStyle.Right,
                Width     = 130,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(63, 63, 70),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 8.5f)
            };
            btnClearCards.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 85);
            btnClearCards.Click += (s, e) => { _cardsFlow.Controls.Clear(); _lblCount.Text = "0 件"; };

            var lblHint = new Label
            {
                Dock      = DockStyle.Fill,
                Text      = "左の検索ボタンで結果が右にカードとして表示されます",
                ForeColor = Color.FromArgb(120, 120, 130),
                TextAlign = ContentAlignment.MiddleCenter,
                Font      = new Font("Segoe UI", 8f)
            };

            toolbar.Controls.Add(btnClearCards);
            toolbar.Controls.Add(lblHint);
            toolbar.Controls.Add(_lblCount);
            splitMain.Panel2.Controls.Add(toolbar);

            // カードが流れるFlowLayoutPanel (横スクロール)
            _cardsFlow = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                AutoScroll    = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = true,
                BackColor     = BgDark,
                Padding       = new Padding(8)
            };
            splitMain.Panel2.Controls.Add(_cardsFlow);

            // 初期スプリッタ位置
            splitMain.SplitterDistance = 370;
        }

        // ── MHXXCharmRNG からのコールバック ──
        private void OnCharmFound(MHXXCharmRNG.CharmResult result, string[] skillNames)
        {
            if (_cardsFlow.InvokeRequired)
            {
                _cardsFlow.BeginInvoke((Action)(() => AppendCard(result, skillNames)));
                return;
            }
            AppendCard(result, skillNames);
        }

        private void AppendCard(MHXXCharmRNG.CharmResult c, string[] skNames)
        {
            string sk1Name = (c.Skill1Id >= 0 && c.Skill1Id < skNames.Length)
                ? skNames[c.Skill1Id] : $"#{c.Skill1Id}";
            string sk2Name = (c.Skill2Id < 0) ? "---"
                : (c.Skill2Id < skNames.Length ? skNames[c.Skill2Id] : $"#{c.Skill2Id}");
            string sp2Str  = (c.Skill2Id < 0 || c.SP2 == 0) ? "" : $" +{c.SP2}";

            Color rareCol = RareColor(c.Rare);

            // ── カードパネル ──
            var card = new Panel
            {
                Width       = 188,
                Height      = 148,
                BackColor   = Color.FromArgb(36, 36, 40),
                Margin      = new Padding(6),
                Padding     = new Padding(0)
            };

            // カード外枠（レア度カラー）
            card.Paint += (s, pe) =>
            {
                using var pen = new Pen(rareCol, 2f);
                pe.Graphics.DrawRectangle(pen, 1, 1, card.Width - 2, card.Height - 2);
            };

            // ── ヘッダー: フレーム番号 ──
            var headerBg = Color.FromArgb(
                Math.Min(255, rareCol.R / 5 + 28),
                Math.Min(255, rareCol.G / 5 + 28),
                Math.Min(255, rareCol.B / 5 + 35));

            var header = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 30,
                BackColor = headerBg
            };
            var lblFrame = new Label
            {
                Dock      = DockStyle.Fill,
                Text      = $"F: {c.Frame:N0}",
                ForeColor = rareCol,
                Font      = new Font("Consolas", 9f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            header.Controls.Add(lblFrame);

            // ── 時間 ──
            var lblTime = new Label
            {
                Dock      = DockStyle.Top,
                Height    = 20,
                Text      = c.TimeStr,
                ForeColor = Color.FromArgb(160, 160, 175),
                Font      = new Font("Consolas", 7.5f),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // ── 区切り ──
            var sep1 = MakeSep();

            // ── スキル1 ──
            var rowSk1 = MakeRow("SK1", $"{sk1Name}", $"+{c.SP1}", Color.White, Color.LightYellow);

            // ── スキル2 ──
            Color sk2fg = c.Skill2Id >= 0 ? Color.White : Color.FromArgb(90, 90, 100);
            var rowSk2 = MakeRow("SK2", $"{sk2Name}", sp2Str, sk2fg, Color.FromArgb(180, 230, 180));

            // ── 区切り ──
            var sep2 = MakeSep();

            // ── スロット + レア度 ──
            Color slotFg = c.Slot >= 3 ? Color.LightGreen
                         : c.Slot >= 2 ? Color.FromArgb(140, 230, 140)
                         : Color.White;
            var rowInfo = MakeRow("SLT", $"S{c.Slot}", $"RARE{c.Rare}", slotFg, rareCol);

            // コントロールは下から積む（DockStyle.Top は後から追加した順で下に行く）
            card.Controls.Add(rowInfo);
            card.Controls.Add(sep2);
            card.Controls.Add(rowSk2);
            card.Controls.Add(rowSk1);
            card.Controls.Add(sep1);
            card.Controls.Add(lblTime);
            card.Controls.Add(header);

            // ダブルクリックでフレーム番号をコピー
            card.DoubleClick += (s, e) =>
            {
                try { Clipboard.SetText(c.Frame.ToString()); } catch { }
            };
            foreach (Control child in card.Controls)
                child.DoubleClick += (s, e) =>
                {
                    try { Clipboard.SetText(c.Frame.ToString()); } catch { }
                };

            _cardsFlow.SuspendLayout();
            _cardsFlow.Controls.Add(card);
            _cardsFlow.ResumeLayout();

            _lblCount.Text = $"{_cardsFlow.Controls.Count} 件";
        }

        // ── ヘルパー ──
        private static Panel MakeSep() =>
            new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(60, 60, 68) };

        private static Panel MakeRow(string tag, string left, string right, Color leftCol, Color rightCol)
        {
            var row = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 24,
                BackColor = Color.Transparent
            };
            var lblTag = new Label
            {
                Text      = tag,
                Location  = new Point(8, 4),
                Size      = new Size(28, 16),
                ForeColor = Color.FromArgb(110, 110, 125),
                Font      = new Font("Consolas", 7.5f),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var lblLeft = new Label
            {
                Text      = left,
                Location  = new Point(38, 4),
                Size      = new Size(84, 16),
                ForeColor = leftCol,
                Font      = new Font("Consolas", 8f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var lblRight = new Label
            {
                Text      = right,
                Location  = new Point(122, 4),
                Size      = new Size(58, 16),
                ForeColor = rightCol,
                Font      = new Font("Consolas", 8f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };
            row.Controls.AddRange(new Control[] { lblTag, lblLeft, lblRight });
            return row;
        }

        private static Color RareColor(int rare) => rare switch
        {
            10 => Color.FromArgb(220, 150, 255),
            9  => Color.FromArgb(255, 200, 100),
            8  => Color.FromArgb(100, 220, 255),
            7  => Color.FromArgb(255, 100, 100),
            6  => Color.FromArgb(255, 130, 80),
            5  => Color.FromArgb(130, 230, 130),
            4  => Color.FromArgb(200, 140, 220),
            3  => Color.FromArgb(230, 210, 60),
            2  => Color.FromArgb(100, 160, 255),
            _  => Color.FromArgb(160, 160, 160)
        };
    }
}
