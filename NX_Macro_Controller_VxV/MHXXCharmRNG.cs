// MHXXCharmRNG.cs
// MHXX (Monster Hunter XX / Generations Ultimate) お守り乱数計算ツール

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NX_Macro_Controller_VxV
{
    public sealed class MHXXCharmRNG : UserControl
    {
        // ═══════════════════════════════════════════════════════
        //  XorShift128 State
        // ═══════════════════════════════════════════════════════
        private uint _x, _y, _z, _w, _t;
        private long _f;
        private readonly uint[] _r = new uint[7];

        private static readonly uint[] DefaultSeed = { 0x0194FD72, 0x79E6C985, 0x08DD9701, 0x41CFCE91 };

        private static readonly BigInteger CharPoly =
            BigInteger.Parse("00100000201a8362f671442057eea368001",
                             NumberStyles.HexNumber);

        // ═══════════════════════════════════════════════════════
        //  Charm Table State
        // ═══════════════════════════════════════════════════════
        private int _kind;
        private int _origin;

        private int[] _skill1Ids;
        private (int Min, int Max)[] _sp1;
        private int[] _skill2Ids;
        private (int Min, int Max)[] _sp2;
        private (int T1, int T2, int T3)[] _slotValue;
        private int _th;

        // ═══════════════════════════════════════════════════════
        //  Skill Name Tables
        // ═══════════════════════════════════════════════════════
        private static readonly string[] SkillJA = {
            "毒","麻痺","睡眠","気絶","聴覚","風圧","耐震","だる","耐暑","耐寒",
            "寒冷","炎熱","盗み","対防","狂撃","細菌","裂傷","攻撃","防御","体力",
            "火耐","水耐","雷耐","氷耐","龍耐","属耐","火攻","水攻","雷攻","氷攻",
            "龍攻","属攻","特攻","研師","匠","斬味","剣術","研磨","鈍器","抜会",
            "抜減","納刀","納研","刃鱗","装速","反動","精密","通強","貫強","散強",
            "重強","通追","貫追","散追","榴追","拡追","毒追","麻追","睡追","強追",
            "属追","接追","減追","爆追","速射","射法","装数","変則","弾節","達人",
            "痛撃","連撃","特会","属会","会心","裏会","溜短","スタ","体術","気力",
            "走行","回性","回距","泡沫","ガ性","ガ強","ＫＯ","減攻","笛","砲術",
            "重撃","爆弾","本気","闘魂","無傷","チャ","龍気","底力","逆境","逆上",
            "窮地","根性","気配","采配","号令","乗り","跳躍","無心","我慢","ＳＰ",
            "千里","観察","狩人","運搬","加護","英雄","回量","回速","効果","広域",
            "腹減","食い","食事","節食","肉食","茸食","野草","調成","調数","高速",
            "採取","ハチ","護石","気ま","運気","剥取","捕獲","ベル","ここ","ポッ",
            "ユク","龍識","飛行","紅兜","大雪","矛砕","岩穿","紫毒","宝纏","白疾",
            "隻眼","黒炎","金雷","荒鉤","燼滅","朧隠","鎧裂","天眼","青電","銀嶺",
            "鏖魔","真紅","真大","真矛","真岩","真紫","真宝","真白","真隻","真黒",
            "真金","真荒","真燼","真朧","真鎧","真天","真青","真銀","真鏖","北辰",
            "斬術","食欲","職工","剛腕","祈願","裏稼","刀匠","射手","状態","怒",
            "回術","居合","頑強","剛撃","盾持","潔癖","増幅","護収","強欲","対鋼",
            "対霞","対炎","胴倍","秘術","護強"
        };

        private static readonly string[] SkillEN = {
            "Poison","Paralysis","Sleep","Stun","Hearing","Wind Res","Tremor Res","Bind Res","Heat Res","Cold Res",
            "ColdBlooded","HotBlooded","Anti-Theft","Def Lock","Frenzy Res","Biology","Bleeding","Attack","Defense","Health",
            "Fire Res","Water Res","Thunder Res","Ice Res","Dragon Res","Blight Res","Fire Atk","Water Atk","Thunder Atk","Ice Atk",
            "Dragon Atk","Elemental","Status","Sharpener","Handicraft","Sharpness","Fencing","Grinder","Blunt","Crit Draw",
            "Punish Draw","Sheathing","Sheathe Sharp","Bladescale","Reload Spd","Recoil","Precision","Normal Up","Pierce Up","Pellet Up",
            "Heavy Up","Normal S+","Pierce S+","Pellet S+","Crag S+","Clust S+","Poison C+","Para C+","Sleep C+","Power C+",
            "Elem C+","C.Range C+","Exhaust C+","Blast C+","Rapid Fire","Dead Eye","Loading","Haphazard","Ammo Saver","Expert",
            "Tenderizer","Chain Crit","Crit Status","Crit Element","Critical Up","Neg. Crit","FastCharge","Stamina","Constitution","Stam Recov",
            "Dist Runner","Evasion","Evade Dist","Bubble","Guard","Guard Up","KO","Stam Drain","Maestro","Artillery",
            "Destroyer","Bomb Boost","Gloves Off","Spirit","Unscathed","Chance","Dragon Spirit","Potential","Survivor","Furor",
            "Crisis","Guts","Sense","Team Player","TeamLeader","Mounting","Vault","Insight","Endurance","Prolong SP",
            "Psychic","Perception","Ranger","Transporter","Protection","Hero Shield","Rec Level","Rec Speed","Lasting Pwr","Wide-Range",
            "Hunger","Gluttony","Eating","Light Eater","Carnivore","Mycology","Botany","Combo Rate","Combo Plus","Speed Setup",
            "Gathering","Honey","Charmer","Whim","Fate","Carving","Capturer","Bherna","Kokoto","Pokke",
            "Yukumo","Soaratorium","Flying Pub","Redhelm","Snowbaron","Stonefist","Drilltusk","Dreadqueen","C.beard","Silverwind",
            "Deadeye","Dreadking","Thunderlord","Grimclaw","Hellblade","Nightcloak","Rustrazor","Soulseer","Boltreaver","Elderfrost",
            "Bloodbath","Redhelm X","Snowbaron X","Stonefist X","Drilltusk X","Dreadqueen X","Crystalb. X","Silverwind X","Deadeye X","Dreadking X",
            "Thunderlord X","Grimclaw X","Hellblade X","Nightcloak X","Rustrazor X","Soulseer X","Boltreaver X","Elderfrost X","Bloodbath X","D. Fencing",
            "Edge Lore","PowerEater","Mechanic","Brawn","Prayer","Covert","Edgemaster","SteadyHand","Status Res","Fury",
            "Nimbleness","Readiness","Resilience","Brutality","Stalwart","Prudence","Amplify","Hoarding","Avarice","Anti-Kushala",
            "Anti-Chameleos","Anti-Teostra","Torso Up","Secret Arts","Talisman Boost"
        };

        private string[] _skillNames;

        // ═══════════════════════════════════════════════════════
        //  Charm Result Model  (public for MHXXCharmWindow)
        // ═══════════════════════════════════════════════════════
        public struct CharmResult
        {
            public long Frame;
            public string TimeStr;
            public int Skill1Id;
            public int SP1;
            public int Skill2Id;
            public int SP2;
            public int Slot;
            public int Fill;
            public int Rare;
        }

        public delegate void CharmFoundHandler(CharmResult result, string[] skillNames);
        public event CharmFoundHandler CharmFound;

        // ═══════════════════════════════════════════════════════
        //  UI Controls
        // ═══════════════════════════════════════════════════════
        private ComboBox _cmbLang, _cmbKind, _cmbOrigin;
        private ComboBox _cmbSkill1, _cmbSkill2;
        private NumericUpDown _nudStartFrame, _nudSteps, _nudSP1, _nudSP2, _nudSlot;
        private RadioButton _rbExact, _rbGreater;
        private Button _btnSearch, _btnStop, _btnClear;
        private DataGridView _dgvResults;
        private FlowLayoutPanel _cardsPanel;
        private Label _lblStatus, _lblSearchCount;
        private TextBox _txtFrameConv;
        private Label _lblFrameConvResult;

        private CancellationTokenSource _cts;
        private bool _uiReady;
        private readonly bool _cardMode;

        // ─────────────────────────────────────────────────────
        public MHXXCharmRNG(bool cardMode = false)
        {
            _cardMode = cardMode;
            _skillNames = SkillJA;
            SetBlue();
            BuildUI();
            _uiReady = true;
            RefreshKindCombo();
            RefreshOriginCombo();
            _cmbKind.SelectedIndex = 0;
        }

        // ═══════════════════════════════════════════════════════
        //  UI Construction
        // ═══════════════════════════════════════════════════════
        private void BuildUI()
        {
            BackColor = Color.FromArgb(33, 33, 35);
            ForeColor = Color.White;
            Dock      = DockStyle.Fill;

            var top = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 218,   // ← 修正: 218px に拡張（最終行が切れないように）
                BackColor = Color.FromArgb(33, 33, 35),
                Padding   = new Padding(0)
            };
            Controls.Add(top);

            var bottom = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(28, 28, 28) };
            Controls.Add(bottom);

            // ---- Status bar ----
            var statusPanel = new Panel
            {
                Dock = DockStyle.Bottom, Height = 22,
                BackColor = Color.FromArgb(40, 40, 42)
            };
            _lblStatus = new Label
            {
                Dock = DockStyle.Left, Width = 260,
                ForeColor = Color.LightGray, TextAlign = ContentAlignment.MiddleLeft,
                Text = "待機中", Font = new Font("Segoe UI", 8f), Padding = new Padding(4, 0, 0, 0)
            };
            _lblSearchCount = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Color.LightGray, TextAlign = ContentAlignment.MiddleRight,
                Text = "0 件", Font = new Font("Segoe UI", 8f), Padding = new Padding(0, 0, 6, 0)
            };
            statusPanel.Controls.Add(_lblSearchCount);
            statusPanel.Controls.Add(_lblStatus);
            bottom.Controls.Add(statusPanel);

            if (!_cardMode)
            {
                // ---- DataGridView (通常モード) ----
                _dgvResults = new DataGridView
                {
                    Dock                   = DockStyle.Fill,
                    BackgroundColor        = Color.FromArgb(28, 28, 28),
                    ForeColor              = Color.White,
                    GridColor              = Color.FromArgb(55, 55, 58),
                    AllowUserToAddRows     = false,
                    ReadOnly               = true,
                    RowHeadersVisible      = false,
                    AutoSizeColumnsMode    = DataGridViewAutoSizeColumnsMode.AllCells,
                    BorderStyle            = BorderStyle.None,
                    Font                   = new Font("Consolas", 8.5f),
                    EnableHeadersVisualStyles = false,
                    SelectionMode          = DataGridViewSelectionMode.FullRowSelect,
                    MultiSelect            = false
                };
                _dgvResults.DefaultCellStyle.BackColor           = Color.FromArgb(28, 28, 28);
                _dgvResults.DefaultCellStyle.ForeColor           = Color.White;
                _dgvResults.DefaultCellStyle.SelectionBackColor  = Color.FromArgb(0, 100, 180);
                _dgvResults.DefaultCellStyle.SelectionForeColor  = Color.White;
                _dgvResults.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
                _dgvResults.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                _dgvResults.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                _dgvResults.RowsDefaultCellStyle.BackColor          = Color.FromArgb(28, 28, 28);
                _dgvResults.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(32, 32, 35);

                _dgvResults.Columns.Add("colFrame", "フレーム");
                _dgvResults.Columns.Add("colTime",  "時間 (d/h/m/s/f)");
                _dgvResults.Columns.Add("colSk1",   "スキル1");
                _dgvResults.Columns.Add("colSP1",   "PT1");
                _dgvResults.Columns.Add("colSk2",   "スキル2");
                _dgvResults.Columns.Add("colSP2",   "PT2");
                _dgvResults.Columns.Add("colSlot",  "スロット");
                _dgvResults.Columns.Add("colFill",  "充填値");
                _dgvResults.Columns.Add("colRare",  "レア度");

                foreach (DataGridViewColumn c in _dgvResults.Columns)
                    c.SortMode = DataGridViewColumnSortMode.NotSortable;

                var cm = new ContextMenuStrip();
                cm.Items.Add("選択行をコピー", null, (s, e) => CopySelectedRow());
                cm.Items.Add("すべてクリア",   null, (s, e) => { _dgvResults.Rows.Clear(); _lblSearchCount.Text = "0 件"; });
                _dgvResults.ContextMenuStrip = cm;
                _dgvResults.BackgroundColor  = Color.FromArgb(28, 28, 28);

                bottom.Controls.Add(_dgvResults);
            }
            else
            {
                // ---- FlowLayoutPanel カードモード ----
                _cardsPanel = new FlowLayoutPanel
                {
                    Dock          = DockStyle.Fill,
                    AutoScroll    = true,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents  = true,
                    BackColor     = Color.FromArgb(28, 28, 28),
                    Padding       = new Padding(6)
                };
                var cardCm = new ContextMenuStrip();
                cardCm.Items.Add("カードをすべてクリア", null, (s, e) =>
                {
                    _cardsPanel.Controls.Clear();
                    _lblSearchCount.Text = "0 件";
                });
                _cardsPanel.ContextMenuStrip = cardCm;
                bottom.Controls.Add(_cardsPanel);
            }

            PopulateTopPanel(top);
        }

        private void PopulateTopPanel(Panel top)
        {
            const int L = 6;
            const int H = 22;
            int y = 6;

            // ── Row 1: Language | Kind | Origin ──
            Add(top, DkLabel("言語:", L, y + 3));
            _cmbLang = DkCombo(L + 34, y, 74);
            _cmbLang.Items.AddRange(new object[] { "日本語", "English" });
            _cmbLang.SelectedIndex = 0;
            _cmbLang.SelectedIndexChanged += OnLangChanged;
            Add(top, _cmbLang);

            Add(top, DkLabel("種類:", L + 115, y + 3));
            _cmbKind = DkCombo(L + 149, y, 106);
            _cmbKind.SelectedIndexChanged += OnKindChanged;
            Add(top, _cmbKind);

            Add(top, DkLabel("入手:", L + 262, y + 3));
            _cmbOrigin = DkCombo(L + 292, y, 56);
            _cmbOrigin.SelectedIndexChanged += (s, e) =>
            {
                if (_uiReady && _cmbOrigin.SelectedIndex >= 0)
                    _origin = _cmbOrigin.SelectedIndex;
            };
            Add(top, _cmbOrigin);
            y += 27;

            // ── Row 2: Start frame | Steps ──
            Add(top, DkLabel("開始F:", L, y + 3));
            _nudStartFrame = DkNUD(L + 41, y, 110, 0, 9_999_999_999m, 0);
            _nudStartFrame.DecimalPlaces = 0;
            Add(top, _nudStartFrame);

            Add(top, DkLabel("検索数:", L + 158, y + 3));
            _nudSteps = DkNUD(L + 197, y, 106, 1, 100_000_000m, 10_000_000); // ← デフォルト1000万に変更
            _nudSteps.DecimalPlaces = 0;
            Add(top, _nudSteps);
            y += 27;

            // ── Row 3: Skill1 | SP1 ──
            Add(top, DkLabel("スキル1:", L, y + 3));
            _cmbSkill1 = DkCombo(L + 52, y, 196);
            Add(top, _cmbSkill1);

            Add(top, DkLabel("PT:", L + 254, y + 3));
            _nudSP1 = DkNUD(L + 270, y, 56, 1, 20, 5);
            Add(top, _nudSP1);
            y += 27;

            // ── Row 4: Skill2 | SP2 ──
            Add(top, DkLabel("スキル2:", L, y + 3));
            _cmbSkill2 = DkCombo(L + 52, y, 196);
            Add(top, _cmbSkill2);

            Add(top, DkLabel("PT:", L + 254, y + 3));
            _nudSP2 = DkNUD(L + 270, y, 56, -20, 20, 0);
            Add(top, _nudSP2);
            y += 27;

            // ── Row 5: Slot | Search mode ──
            Add(top, DkLabel("スロット≥:", L, y + 3));
            _nudSlot = DkNUD(L + 63, y, 40, 0, 3, 0);
            Add(top, _nudSlot);

            _rbExact = new RadioButton
            {
                Text = "完全一致", Location = new Point(L + 110, y + 2),
                ForeColor = Color.White, AutoSize = true, Checked = true,
                Font = new Font("Segoe UI", 8.25f)
            };
            _rbGreater = new RadioButton
            {
                Text = "以上検索", Location = new Point(L + 205, y + 2),
                ForeColor = Color.White, AutoSize = true,
                Font = new Font("Segoe UI", 8.25f)
            };
            Add(top, _rbExact);
            Add(top, _rbGreater);
            y += 27;

            // ── Row 6: Buttons ──
            _btnSearch = DkButton("🔍 検索開始", L, y, 92);
            _btnSearch.BackColor = Color.FromArgb(0, 100, 180);
            _btnSearch.Click += BtnSearch_Click;
            Add(top, _btnSearch);

            _btnStop = DkButton("⏹ 停止", L + 98, y, 68);
            _btnStop.Enabled = false;
            _btnStop.Click += (s, e) => _cts?.Cancel();
            Add(top, _btnStop);

            _btnClear = DkButton("🗑 クリア", L + 172, y, 68);
            _btnClear.Click += (s, e) =>
            {
                if (_cardMode) { _cardsPanel?.Controls.Clear(); }
                else           { _dgvResults?.Rows.Clear(); }
                _lblSearchCount.Text = "0 件";
            };
            Add(top, _btnClear);

            Add(top, DkLabel("F→時:", L + 247, y + 3));
            _txtFrameConv = new TextBox
            {
                Location = new Point(L + 278, y), Width = 68, Height = H,
                BackColor = Color.FromArgb(45, 45, 48), ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 8.5f)
            };
            _txtFrameConv.TextChanged += TxtFrameConv_TextChanged;
            Add(top, _txtFrameConv);
            y += 27;

            // ── Row 7: Frame conv result ──
            _lblFrameConvResult = new Label
            {
                Location = new Point(L, y), Width = 348, Height = 18,
                ForeColor = Color.LightSkyBlue, TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Consolas", 8f), Text = "フレーム数を入力すると時間に変換します"
            };
            Add(top, _lblFrameConvResult);
            y += 20;

            // ── Row 8: Hint ──
            var hint = new Label
            {
                Location = new Point(L, y), Width = 348, Height = 16,
                ForeColor = Color.FromArgb(130, 130, 130),
                Font = new Font("Segoe UI", 7.5f),
                Text = "※1日=2592000F  1時間=108000F  1分=1800F  1秒=30F"
            };
            Add(top, hint);
        }

        // ═══════════════════════════════════════════════════════
        //  Helper factories
        // ═══════════════════════════════════════════════════════
        private static void Add(Control parent, Control child) => parent.Controls.Add(child);

        private static Label DkLabel(string text, int x, int y) =>
            new Label
            {
                Text = text, Location = new Point(x, y), AutoSize = true,
                ForeColor = Color.White, Font = new Font("Segoe UI", 8.25f)
            };

        private static ComboBox DkCombo(int x, int y, int w) =>
            new ComboBox
            {
                Location = new Point(x, y), Width = w, Height = 22,
                BackColor = Color.FromArgb(45, 45, 48), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8.25f)
            };

        private static NumericUpDown DkNUD(int x, int y, int w, decimal min, decimal max, decimal val)
        {
            var n = new NumericUpDown
            {
                Location = new Point(x, y), Width = w, Height = 22,
                BackColor = Color.FromArgb(45, 45, 48), ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Minimum = min, Maximum = max, Value = val,
                Font = new Font("Consolas", 9f)
            };
            return n;
        }

        private static Button DkButton(string text, int x, int y, int w) =>
            new Button
            {
                Text = text, Location = new Point(x, y), Width = w, Height = 24,
                BackColor = Color.FromArgb(63, 63, 70), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.25f),
                FlatAppearance = { BorderColor = Color.FromArgb(80, 80, 82), BorderSize = 1 }
            };

        // ═══════════════════════════════════════════════════════
        //  Frame → Time converter
        // ═══════════════════════════════════════════════════════
        private void TxtFrameConv_TextChanged(object sender, EventArgs e)
        {
            if (long.TryParse(_txtFrameConv.Text, out long fv) && fv >= 0)
            {
                long dd = fv / 2592000; fv %= 2592000;
                long hh = fv / 108000;  fv %= 108000;
                long mm = fv / 1800;    fv %= 1800;
                long ss = fv / 30;      long ff = fv % 30;
                _lblFrameConvResult.Text = $"{dd}日 {hh}時間 {mm}分 {ss}秒 {ff}f";
            }
            else
            {
                _lblFrameConvResult.Text = "フレーム数を入力すると時間に変換します";
            }
        }

        // ═══════════════════════════════════════════════════════
        //  Combo refresh
        // ═══════════════════════════════════════════════════════
        private void RefreshKindCombo()
        {
            string[] kinds = (_cmbLang.SelectedIndex == 0)
                ? new[] { "青(マカ/炭鉱)", "赤(マカ/炭鉱)", "黄(マカ)", "白(炭鉱)" }
                : new[] { "Blue(Maca/Mine)", "Red(Maca/Mine)", "Yellow(Maca)", "White(Mine)" };
            int sel = Math.Max(0, _cmbKind.SelectedIndex);
            _cmbKind.Items.Clear();
            _cmbKind.Items.AddRange(kinds);
            _cmbKind.SelectedIndex = sel < kinds.Length ? sel : 0;
        }

        private void RefreshOriginCombo()
        {
            string[] orgs = (_cmbLang.SelectedIndex == 0)
                ? new[] { "マカ錬金", "炭鉱夫" }
                : new[] { "Maca", "Miner" };
            int sel = Math.Max(0, _cmbOrigin.SelectedIndex);
            _cmbOrigin.Items.Clear();
            _cmbOrigin.Items.AddRange(orgs);
            _cmbOrigin.SelectedIndex = sel < orgs.Length ? sel : 0;
            _origin = _cmbOrigin.SelectedIndex;
        }

        private void RefreshSkillCombos()
        {
            int prevS1 = _cmbSkill1.SelectedIndex;
            int prevS2 = _cmbSkill2.SelectedIndex;

            _cmbSkill1.BeginUpdate();
            _cmbSkill2.BeginUpdate();
            _cmbSkill1.Items.Clear();
            _cmbSkill2.Items.Clear();

            string noSkill = (_cmbLang.SelectedIndex == 0) ? "--- (指定なし)" : "--- (Any)";
            _cmbSkill2.Items.Add(noSkill);

            foreach (int id in _skill1Ids)
            {
                string name = (id < _skillNames.Length) ? _skillNames[id] : $"#{id}";
                _cmbSkill1.Items.Add(name);
                _cmbSkill2.Items.Add(name);
            }

            _cmbSkill1.SelectedIndex = (prevS1 >= 0 && prevS1 < _cmbSkill1.Items.Count) ? prevS1 : 0;
            _cmbSkill2.SelectedIndex = (prevS2 >= 0 && prevS2 < _cmbSkill2.Items.Count) ? prevS2 : 0;

            _cmbSkill1.EndUpdate();
            _cmbSkill2.EndUpdate();
        }

        // ═══════════════════════════════════════════════════════
        //  Event handlers
        // ═══════════════════════════════════════════════════════
        private void OnLangChanged(object sender, EventArgs e)
        {
            _skillNames = (_cmbLang.SelectedIndex == 0) ? SkillJA : SkillEN;
            RefreshKindCombo();
            RefreshOriginCombo();
            if (_uiReady) RefreshSkillCombos();
        }

        private void OnKindChanged(object sender, EventArgs e)
        {
            if (!_uiReady || _cmbKind.SelectedIndex < 0) return;
            _kind = _cmbKind.SelectedIndex;
            switch (_kind)
            {
                case 0: SetBlue();   break;
                case 1: SetRed();    break;
                case 2: SetYellow(); break;
                case 3: SetWhite();  break;
            }
            RefreshSkillCombos();
        }

        // ═══════════════════════════════════════════════════════
        //  Charm tables
        // ═══════════════════════════════════════════════════════
        private void SetBlue()
        {
            _skill1Ids = new[] {
                17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,
                37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,
                57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,
                77,78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,
                97,98,99,100,101,102,103,104,105,106,107,108,109,110,111,112,113,114
            };
            _sp1 = Enumerable.Repeat((1, 10), _skill1Ids.Length).ToArray();
            _skill2Ids = _skill1Ids;
            _sp2 = _sp1;
            _slotValue = new[] {
                (40,70,95),(30,60,90),(20,50,85),(15,40,80),(10,30,70),(5,20,60),(0,10,50)
            };
            _th = 30;
        }

        private void SetRed()
        {
            _skill1Ids = new[] {
                0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                115,116,117,118,119,120,121,122,123,124,125,126,
                127,128,129,130,131,132,133,134,135,136,137,138,139,
                140,141,142,143,144,145,146,147,148,149,150,151,152,153,154
            };
            _sp1 = Enumerable.Repeat((1, 8), _skill1Ids.Length).ToArray();
            _skill2Ids = _skill1Ids;
            _sp2 = _sp1;
            _slotValue = new[] {
                (50,75,97),(40,65,92),(30,55,87),(20,45,82),(10,35,75),(5,25,65),(0,15,55)
            };
            _th = 40;
        }

        private void SetYellow()
        {
            _skill1Ids = new[] {
                155,156,157,158,159,160,161,162,163,164,165,166,167,168,169,
                170,171,172,173,174,175,176,177,178,179,180,181,182,183,184,
                185,186,187,188,189,190,191,192,193,194,195,196,197,198,199,
                200,201,202,203,204
            };
            _sp1 = Enumerable.Repeat((1, 6), _skill1Ids.Length).ToArray();
            _skill2Ids = _skill1Ids;
            _sp2 = _sp1;
            _slotValue = new[] {
                (60,80,98),(50,70,95),(40,60,90),(30,50,85),(20,40,78),(10,30,68),(0,20,58)
            };
            _th = 50;
        }

        private void SetWhite()
        {
            _skill1Ids = new[] {
                155,156,157,158,159,160,161,162,163,164,165,166,167,168,169,
                170,171,172,173,174,175,176,177,178,179,180,181,182,183,184,
                185,186,187,188,189
            };
            _sp1 = Enumerable.Repeat((1, 5), _skill1Ids.Length).ToArray();
            _skill2Ids = _skill1Ids;
            _sp2 = _sp1;
            _slotValue = new[] {
                (65,85,99),(55,75,96),(45,65,92),(35,55,88),(25,45,82),(15,35,72),(5,25,62)
            };
            _th = 60;
        }

        // ═══════════════════════════════════════════════════════
        //  RNG
        // ═══════════════════════════════════════════════════════
        private static BigInteger PolyMul(BigInteger a, BigInteger b, BigInteger mod)
        {
            BigInteger res = BigInteger.Zero;
            while (b > BigInteger.Zero)
            {
                if ((b & BigInteger.One) == BigInteger.One) res ^= a;
                b >>= 1;
                a <<= 1;
                if (a >= mod) a ^= mod;
            }
            return res;
        }

        private static BigInteger PolyPowMod(BigInteger baseVal, BigInteger exp, BigInteger mod)
        {
            BigInteger result = BigInteger.One;
            baseVal %= mod;
            while (exp > BigInteger.Zero)
            {
                if ((exp & BigInteger.One) == BigInteger.One)
                    result = PolyMul(result, baseVal, mod);
                exp >>= 1;
                baseVal = PolyMul(baseVal, baseVal, mod);
            }
            return result;
        }

        // ═══════════════════════════════════════════════════════
        //  Search
        // ═══════════════════════════════════════════════════════
        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            if (_btnStop.Enabled) { _cts?.Cancel(); return; }

            long  startFrame = (long)_nudStartFrame.Value;
            int   steps      = (int)_nudSteps.Value;
            int   skill1Sel  = _cmbSkill1.SelectedIndex;
            int   sp1Target  = (int)_nudSP1.Value;
            int   skill2Sel  = _cmbSkill2.SelectedIndex;
            int   sp2Target  = (int)_nudSP2.Value;
            int   slotTarget = (int)_nudSlot.Value;
            bool  exact      = _rbExact.Checked;
            int   origin     = _origin;
            int   tgtId1     = skill1Sel;
            int   tgtId2     = (skill2Sel <= 0) ? -1 : (skill2Sel - 1);

            int[] sk1 = (int[])_skill1Ids.Clone(), sk2 = (int[])_skill2Ids.Clone();
            var   p1  = ((int, int)[])_sp1.Clone();
            var   p2  = ((int, int)[])_sp2.Clone();
            var   sv  = ((int, int, int)[])_slotValue.Clone();
            int   th  = _th, kind = _kind;
            string[] skNames = (string[])_skillNames.Clone();

            _btnSearch.Enabled = false;
            _btnStop.Enabled   = true;
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            SetStatus("検索中...");

            try
            {
                await Task.Run(() =>
                {
                    uint x = DefaultSeed[0], y = DefaultSeed[1], z = DefaultSeed[2], w = DefaultSeed[3], t = 0;
                    long f = 0;
                    uint[] r = new uint[7];

                    void AscendL()
                    {
                        t = (x ^ (x << 15)) & 0xFFFFFFFF;
                        x = y; y = z; z = w;
                        w = (w ^ (w >> 21) ^ t ^ (t >> 4)) & 0xFFFFFFFF;
                        f++;
                    }

                    void RollL()
                    {
                        r[0]=r[1]; r[1]=r[2]; r[2]=r[3]; r[3]=r[4]; r[4]=r[5]; r[5]=r[6];
                        r[6] = w;
                        AscendL();
                    }

                    x = DefaultSeed[0]; y = DefaultSeed[1]; z = DefaultSeed[2]; w = DefaultSeed[3];
                    t = 0; f = 0;

                    if (startFrame != 0)
                    {
                        BigInteger period = (BigInteger.One << 128) - BigInteger.One;
                        BigInteger fB = ((new BigInteger(startFrame) % period) + period) % period;
                        BigInteger rp = PolyPowMod(new BigInteger(2), fB, CharPoly);

                        uint sx = 0, sy = 0, sz = 0, sw2 = 0;
                        while (rp > BigInteger.Zero)
                        {
                            if ((rp & BigInteger.One) == BigInteger.One)
                            { sx ^= x; sy ^= y; sz ^= z; sw2 ^= w; }
                            rp >>= 1; AscendL();
                        }
                        x = sx; y = sy; z = sz; w = sw2; f = startFrame;
                    }
                    for (int i = 0; i < 7; i++) RollL();

                    var batch = new List<CharmResult>(256);
                    int len1 = sk1.Length, len2 = sk2.Length;

                    for (int i = 0; i < steps && !ct.IsCancellationRequested; i++)
                    {
                        RollL();

                        if (r[0] % (uint)len1 != (uint)tgtId1) continue;

                        bool hasSkill2 = (tgtId2 >= 0);
                        if (hasSkill2)
                        {
                            if (r[2] % 100 < (uint)th) continue;
                            if (r[3] % (uint)len2 != (uint)tgtId2) continue;
                        }

                        int id1 = (int)(r[0] % (uint)len1);
                        int id2 = (int)(r[3] % (uint)len2);
                        int s1  = p1[id1].Item2, s2 = p2[id2].Item2;
                        int tmp1 = (int)(r[1] % (uint)(p1[id1].Item2 - p1[id1].Item1 + 1)) + p1[id1].Item1;

                        int sk2id = -1, tmp2 = 0, sp2raw = 0;
                        uint q5;
                        if (r[2] % 100 >= (uint)th)
                        {
                            sk2id = sk2[id2];
                            uint q4;
                            if (origin == 1 && r[4] % 2 == 0)
                            { q4 = r[5]; q5 = r[6]; tmp2 = (int)(q4 % (uint)(p2[id2].Item1 + 1)) - p2[id2].Item1; }
                            else
                            {
                                if (origin == 1) { q4 = r[5]; q5 = r[6]; } else { q4 = r[4]; q5 = r[5]; }
                                tmp2 = (int)(q4 % (uint)p2[id2].Item2) + 1;
                            }
                            sp2raw = tmp2;
                            if (sk1[id1] == sk2[id2] || tmp2 < 0) tmp2 = 0;
                        }
                        else { q5 = r[3]; sp2raw = 0; }

                        int fillVal = (tmp1 * s2 + tmp2 * s1) * 10 / (s1 * s2);
                        int q5m    = (int)(q5 % 100);

                        int slotNum;
                        if (fillVal < 1 || fillVal > sv.Length) slotNum = 0;
                        else
                        {
                            var sv_ = sv[fillVal - 1];
                            slotNum = q5m >= sv_.Item3 ? 3 : q5m >= sv_.Item2 ? 2 : q5m >= sv_.Item1 ? 1 : 0;
                        }

                        int rn   = slotNum * 2 + fillVal;
                        int rare = kind switch
                        {
                            0 => rn >= 13 ? 10 : rn >= 8 ? 9 : 8,
                            1 => rn >= 13 ? 7  : rn >= 8 ? 6 : 5,
                            2 => rn >= 8  ? 4  : 3,
                            _ => rn >= 8  ? 2  : 1
                        };

                        // ── フィルタ（スロットは常に≥で評価） ──
                        if (exact)
                        {
                            if (tmp1 != sp1Target || slotNum < slotTarget) continue; // ← slot は ≥ に統一
                            if (hasSkill2) { if (sp2raw != sp2Target) continue; }
                            else { if (sk2id != -1 && sk2id != sk1[id1] && sp2raw != 0) continue; }
                        }
                        else
                        {
                            if (tmp1 < sp1Target || slotNum < slotTarget) continue;
                            if (hasSkill2 && (sk2id < 0 || sp2raw < sp2Target)) continue;
                        }

                        long charmFrame = f - 7;
                        long fv = charmFrame;
                        long dd = fv / 2592000; fv %= 2592000;
                        long hh = fv / 108000;  fv %= 108000;
                        long mm = fv / 1800;    fv %= 1800;
                        long ss = fv / 30; long ff = fv % 30;
                        string timeStr = $"{dd}d {hh}h {mm}m {ss}s {ff}f";

                        batch.Add(new CharmResult
                        {
                            Frame    = charmFrame, TimeStr = timeStr,
                            Skill1Id = sk1[id1],   SP1     = tmp1,
                            Skill2Id = sk2id,       SP2     = sp2raw,
                            Slot     = slotNum,     Fill    = fillVal,
                            Rare     = rare
                        });

                        if (batch.Count >= 100)
                        {
                            var copy = batch.ToList(); batch.Clear();
                            BeginInvoke((Action<List<CharmResult>, string[]>)FlushBatch, copy, skNames);
                        }

                        if (i % 500_000 == 0 && i > 0)
                        {
                            int prog = (int)(i * 100L / steps);
                            BeginInvoke((Action<int>)(p => SetStatus($"検索中... {p}%")), prog);
                        }
                    }

                    if (batch.Count > 0)
                        BeginInvoke((Action<List<CharmResult>, string[]>)FlushBatch, batch, skNames);

                }, ct);
            }
            catch (OperationCanceledException) { SetStatus("停止しました"); }
            catch (Exception ex)               { SetStatus($"エラー: {ex.Message}"); }
            finally
            {
                int cnt = _cardMode
                    ? (_cardsPanel?.Controls.Count ?? 0)
                    : (_dgvResults?.Rows.Count ?? 0);
                if (!ct.IsCancellationRequested)
                    SetStatus($"完了 — {cnt} 件ヒット");
                _lblSearchCount.Text = $"{cnt} 件";
                _btnSearch.Enabled  = true;
                _btnStop.Enabled    = false;
            }
        }

        private void FlushBatch(List<CharmResult> batch, string[] skNames)
        {
            foreach (var c in batch)
            {
                // イベント発火（MHXXCharmWindow が購読）
                CharmFound?.Invoke(c, skNames);

                if (!_cardMode && _dgvResults != null)
                    AddGridRow(c, skNames);
                else if (_cardMode && _cardsPanel != null)
                    AddCard(c, skNames);
            }
            int cnt = _cardMode ? (_cardsPanel?.Controls.Count ?? 0) : (_dgvResults?.Rows.Count ?? 0);
            _lblSearchCount.Text = $"{cnt} 件";
        }

        private void AddGridRow(CharmResult c, string[] skNames)
        {
            string sk1Name = (c.Skill1Id >= 0 && c.Skill1Id < skNames.Length) ? skNames[c.Skill1Id] : $"#{c.Skill1Id}";
            string sk2Name = (c.Skill2Id < 0) ? "---" : (c.Skill2Id < skNames.Length ? skNames[c.Skill2Id] : $"#{c.Skill2Id}");
            string sp2Str  = (c.Skill2Id < 0 || c.SP2 == 0) ? "---" : c.SP2.ToString();

            _dgvResults.SuspendLayout();
            int rowIdx = _dgvResults.Rows.Add(
                c.Frame, c.TimeStr,
                sk1Name, c.SP1,
                sk2Name, sp2Str,
                "S" + c.Slot, c.Fill,
                "RARE" + c.Rare);

            Color fc = RareColor(c.Rare);
            _dgvResults.Rows[rowIdx].Cells["colRare"].Style.ForeColor = fc;
            _dgvResults.Rows[rowIdx].Cells["colRare"].Style.Font = new Font("Consolas", 8.5f, FontStyle.Bold);
            if (c.Slot >= 2)
                _dgvResults.Rows[rowIdx].Cells["colSlot"].Style.ForeColor = Color.LightGreen;
            _dgvResults.ResumeLayout();
        }

        // ── カードを _cardsPanel に追加 ──
        internal void AddCard(CharmResult c, string[] skNames)
        {
            string sk1Name = (c.Skill1Id >= 0 && c.Skill1Id < skNames.Length) ? skNames[c.Skill1Id] : $"#{c.Skill1Id}";
            string sk2Name = (c.Skill2Id < 0) ? "---" : (c.Skill2Id < skNames.Length ? skNames[c.Skill2Id] : $"#{c.Skill2Id}");
            string sp2Str  = (c.Skill2Id < 0 || c.SP2 == 0) ? "---" : ("+" + c.SP2);

            Color rareCol = RareColor(c.Rare);

            var card = new Panel
            {
                Width       = 172,
                Height      = 130,
                BackColor   = Color.FromArgb(38, 38, 42),
                BorderStyle = BorderStyle.None,
                Margin      = new Padding(5),
                Padding     = new Padding(0)
            };

            // 枠線（レア度カラー）
            card.Paint += (s, pe) =>
            {
                using var pen = new Pen(rareCol, 1.5f);
                pe.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            // ヘッダー（フレーム番号）
            var header = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 26,
                BackColor = Color.FromArgb(rareCol.R / 4, rareCol.G / 4, rareCol.B / 4 + 30)
            };
            var lblFrame = new Label
            {
                Dock      = DockStyle.Fill,
                Text      = $"F: {c.Frame:N0}",
                ForeColor = rareCol,
                Font      = new Font("Consolas", 8.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            header.Controls.Add(lblFrame);
            card.Controls.Add(header);

            // 時間
            var lblTime = new Label
            {
                Dock      = DockStyle.Top,
                Height    = 18,
                Text      = c.TimeStr,
                ForeColor = Color.FromArgb(170, 170, 180),
                Font      = new Font("Consolas", 7.5f),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblTime);

            // 区切り線
            card.Controls.Add(MakeSep());

            // スキル1
            card.Controls.Add(MakeInfoRow("SK1", $"{sk1Name}  +{c.SP1}", Color.White));

            // スキル2
            Color sk2col = c.Skill2Id < 0 ? Color.FromArgb(100, 100, 110) : Color.White;
            card.Controls.Add(MakeInfoRow("SK2", $"{sk2Name}  {sp2Str}", sk2col));

            // スロット + レア度
            Color slotCol = c.Slot >= 2 ? Color.LightGreen : Color.White;
            card.Controls.Add(MakeInfoRow("SLT", $"S{c.Slot}   RARE{c.Rare}", slotCol, rareCol));

            _cardsPanel.Controls.Add(card);
        }

        private static Panel MakeSep()
        {
            return new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 1,
                BackColor = Color.FromArgb(60, 60, 65)
            };
        }

        private static Panel MakeInfoRow(string label, string value, Color valColor, Color? valColor2 = null)
        {
            var row = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 20,
                BackColor = Color.Transparent
            };
            var lbl = new Label
            {
                Text      = label,
                Location  = new Point(6, 2),
                Size      = new Size(28, 16),
                ForeColor = Color.FromArgb(130, 130, 145),
                Font      = new Font("Consolas", 7.5f),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var val = new Label
            {
                Text      = value,
                Location  = new Point(36, 2),
                Size      = new Size(132, 16),
                ForeColor = valColor,
                Font      = new Font("Consolas", 8f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            row.Controls.Add(lbl);
            row.Controls.Add(val);
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

        private void CopySelectedRow()
        {
            if (_dgvResults?.CurrentRow == null) return;
            var sb = new System.Text.StringBuilder();
            foreach (DataGridViewCell cell in _dgvResults.CurrentRow.Cells)
                sb.Append(cell.Value).Append('\t');
            if (sb.Length > 0) sb.Length--;
            try { Clipboard.SetText(sb.ToString()); } catch { }
        }

        private void SetStatus(string text)
        {
            if (InvokeRequired) BeginInvoke((Action)(() => _lblStatus.Text = text));
            else _lblStatus.Text = text;
        }
    }
}
