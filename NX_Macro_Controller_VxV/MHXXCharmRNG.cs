// MHXXCharmRNG.cs
// MHXX (Monster Hunter XX / Generations Ultimate) お守り乱数計算ツール
// Python版 (mhxx-rng.ipynb) を C# WinForms に移植

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
        private readonly uint[] _r = new uint[7]; // rolling window

        private static readonly uint[] DefaultSeed = { 0x0194FD72, 0x79E6C985, 0x08DD9701, 0x41CFCE91 };

        // Characteristic polynomial of the XorShift128 used in MHXX
        // 0x100000201a8362f671442057eea368001
        private static readonly BigInteger CharPoly =
            BigInteger.Parse("00100000201a8362f671442057eea368001",
                             NumberStyles.HexNumber);

        // ═══════════════════════════════════════════════════════
        //  Charm Table State
        // ═══════════════════════════════════════════════════════
        private int _kind;   // 0=blue, 1=red, 2=yellow, 3=white
        private int _origin; // 0=マカ, 1=炭鉱

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
            "毒","麻痺","睡眠","気絶","聴覚","風圧","耐震","だる","耐暑","耐寒",        // 0-9
            "寒冷","炎熱","盗み","対防","狂撃","細菌","裂傷","攻撃","防御","体力",       // 10-19
            "火耐","水耐","雷耐","氷耐","龍耐","属耐","火攻","水攻","雷攻","氷攻",       // 20-29
            "龍攻","属攻","特攻","研師","匠","斬味","剣術","研磨","鈍器","抜会",          // 30-39
            "抜減","納刀","納研","刃鱗","装速","反動","精密","通強","貫強","散強",        // 40-49
            "重強","通追","貫追","散追","榴追","拡追","毒追","麻追","睡追","強追",        // 50-59
            "属追","接追","減追","爆追","速射","射法","装数","変則","弾節","達人",        // 60-69
            "痛撃","連撃","特会","属会","会心","裏会","溜短","スタ","体術","気力",        // 70-79
            "走行","回性","回距","泡沫","ガ性","ガ強","ＫＯ","減攻","笛","砲術",         // 80-89
            "重撃","爆弾","本気","闘魂","無傷","チャ","龍気","底力","逆境","逆上",       // 90-99
            "窮地","根性","気配","采配","号令","乗り","跳躍","無心","我慢","ＳＰ",        // 100-109
            "千里","観察","狩人","運搬","加護","英雄","回量","回速","効果","広域",        // 110-119
            "腹減","食い","食事","節食","肉食","茸食","野草","調成","調数","高速",        // 120-129
            "採取","ハチ","護石","気ま","運気","剥取","捕獲","ベル","ここ","ポッ",        // 130-139
            "ユク","龍識","飛行","紅兜","大雪","矛砕","岩穿","紫毒","宝纏","白疾",       // 140-149
            "隻眼","黒炎","金雷","荒鉤","燼滅","朧隠","鎧裂","天眼","青電","銀嶺",       // 150-159
            "鏖魔","真紅","真大","真矛","真岩","真紫","真宝","真白","真隻","真黒",       // 160-169
            "真金","真荒","真燼","真朧","真鎧","真天","真青","真銀","真鏖","北辰",       // 170-179
            "斬術","食欲","職工","剛腕","祈願","裏稼","刀匠","射手","状態","怒",         // 180-189
            "回術","居合","頑強","剛撃","盾持","潔癖","増幅","護収","強欲","対鋼",       // 190-199
            "対霞","対炎","胴倍","秘術","護強"                                            // 200-204
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

        private string[] _skillNames; // currently selected language

        // ═══════════════════════════════════════════════════════
        //  Charm Result Model
        // ═══════════════════════════════════════════════════════
        private struct CharmResult
        {
            public long Frame;
            public string TimeStr;
            public int Skill1Id;
            public int SP1;
            public int Skill2Id;   // -1 = none
            public int SP2;
            public int Slot;
            public int Fill;
            public int Rare;
        }

        // ═══════════════════════════════════════════════════════
        //  UI Controls
        // ═══════════════════════════════════════════════════════
        private ComboBox _cmbLang, _cmbKind, _cmbOrigin;
        private ComboBox _cmbSkill1, _cmbSkill2;
        private NumericUpDown _nudStartFrame, _nudSteps, _nudSP1, _nudSP2, _nudSlot;
        private RadioButton _rbExact, _rbGreater;
        private Button _btnSearch, _btnStop, _btnClear;
        private DataGridView _dgvResults;
        private Label _lblStatus, _lblSearchCount;
        private TextBox _txtFrameConv;
        private Label _lblFrameConvResult;

        private CancellationTokenSource _cts;
        private bool _uiReady;

        // ─────────────────────────────────────────────────────
        public MHXXCharmRNG()
        {
            _skillNames = SkillJA;
            SetBlue();

            BuildUI();
            _uiReady = true;

            // Populate kind/origin combos AFTER _uiReady
            RefreshKindCombo();
            RefreshOriginCombo();
            _cmbKind.SelectedIndex = 0;   // triggers OnKindChanged → RefreshSkillCombos
        }

        // ═══════════════════════════════════════════════════════
        //  UI Construction
        // ═══════════════════════════════════════════════════════
        private void BuildUI()
        {
            BackColor = Color.FromArgb(33, 33, 35);
            ForeColor = Color.White;
            Dock      = DockStyle.Fill;

            // ── Top panel (controls) ──
            var top = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 198,
                BackColor = Color.FromArgb(33, 33, 35),
                Padding   = new Padding(0)
            };
            Controls.Add(top);

            // ── Bottom (results + status) ──
            var bottom = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(28, 28, 28) };
            Controls.Add(bottom);

            // ---- DataGridView ----
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
            _dgvResults.DefaultCellStyle.BackColor      = Color.FromArgb(28, 28, 28);
            _dgvResults.DefaultCellStyle.ForeColor      = Color.White;
            _dgvResults.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 100, 180);
            _dgvResults.DefaultCellStyle.SelectionForeColor = Color.White;
            _dgvResults.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
            _dgvResults.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dgvResults.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            _dgvResults.RowsDefaultCellStyle.BackColor          = Color.FromArgb(28, 28, 28);
            _dgvResults.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(32, 32, 35);

            _dgvResults.Columns.Add("colFrame",  "フレーム");
            _dgvResults.Columns.Add("colTime",   "時間 (d/h/m/s/f)");
            _dgvResults.Columns.Add("colSk1",    "スキル1");
            _dgvResults.Columns.Add("colSP1",    "PT1");
            _dgvResults.Columns.Add("colSk2",    "スキル2");
            _dgvResults.Columns.Add("colSP2",    "PT2");
            _dgvResults.Columns.Add("colSlot",   "スロット");
            _dgvResults.Columns.Add("colFill",   "充填値");
            _dgvResults.Columns.Add("colRare",   "レア度");

            foreach (DataGridViewColumn c in _dgvResults.Columns)
                c.SortMode = DataGridViewColumnSortMode.NotSortable;

            // Context menu for grid
            var cm = new ContextMenuStrip();
            cm.Items.Add("選択行をコピー", null, (s, e) => CopySelectedRow());
            cm.Items.Add("すべてクリア",   null, (s, e) => _dgvResults.Rows.Clear());
            _dgvResults.ContextMenuStrip = cm;
            _dgvResults.BackgroundColor  = Color.FromArgb(28, 28, 28);

            // ---- Status bar ----
            var statusPanel = new Panel { Dock = DockStyle.Bottom, Height = 22, BackColor = Color.FromArgb(40, 40, 42) };
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

            bottom.Controls.Add(_dgvResults);
            bottom.Controls.Add(statusPanel);

            // ── Populate top panel controls ──
            PopulateTopPanel(top);
        }

        private void PopulateTopPanel(Panel top)
        {
            const int L = 6;    // left margin
            const int H = 22;   // row height
            int y = 6;

            // ── Row 1: Language | Kind | Origin ──
            Add(top, DkLabel("言語:", L, y + 3));
            _cmbLang = DkCombo(L + 34, y, 76);
            _cmbLang.Items.AddRange(new object[] { "日本語", "English" });
            _cmbLang.SelectedIndex = 0;
            _cmbLang.SelectedIndexChanged += OnLangChanged;
            Add(top, _cmbLang);

            Add(top, DkLabel("種類:", L + 117, y + 3));
            _cmbKind = DkCombo(L + 151, y, 108);
            _cmbKind.SelectedIndexChanged += OnKindChanged;
            Add(top, _cmbKind);

            Add(top, DkLabel("入手:", L + 267, y + 3));
            _cmbOrigin = DkCombo(L + 298, y, 55);
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
            _nudSteps = DkNUD(L + 197, y, 106, 1, 100_000_000m, 10000);
            _nudSteps.DecimalPlaces = 0;
            Add(top, _nudSteps);
            y += 27;

            // ── Row 3: Skill1 | SP1 ──
            Add(top, DkLabel("スキル1:", L, y + 3));
            _cmbSkill1 = DkCombo(L + 52, y, 200);
            Add(top, _cmbSkill1);

            Add(top, DkLabel("PT:", L + 258, y + 3));
            _nudSP1 = DkNUD(L + 274, y, 55, 1, 20, 5);
            Add(top, _nudSP1);
            y += 27;

            // ── Row 4: Skill2 | SP2 ──
            Add(top, DkLabel("スキル2:", L, y + 3));
            _cmbSkill2 = DkCombo(L + 52, y, 200);
            Add(top, _cmbSkill2);

            Add(top, DkLabel("PT:", L + 258, y + 3));
            _nudSP2 = DkNUD(L + 274, y, 55, -20, 20, 3);
            Add(top, _nudSP2);
            y += 27;

            // ── Row 5: Slot | Search mode ──
            Add(top, DkLabel("スロット≥:", L, y + 3));
            _nudSlot = DkNUD(L + 63, y, 40, 0, 3, 3);
            Add(top, _nudSlot);

            _rbExact = new RadioButton
            {
                Text = "完全一致", Location = new Point(L + 110, y + 2),
                ForeColor = Color.White, AutoSize = true, Checked = true,
                Font = new Font("Segoe UI", 8.25f)
            };
            _rbGreater = new RadioButton
            {
                Text = "以上検索", Location = new Point(L + 210, y + 2),
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

            _btnStop = DkButton("⏹ 停止", L + 98, y, 70);
            _btnStop.Enabled = false;
            _btnStop.Click += (s, e) => _cts?.Cancel();
            Add(top, _btnStop);

            _btnClear = DkButton("🗑 クリア", L + 174, y, 70);
            _btnClear.Click += (s, e) => { _dgvResults.Rows.Clear(); _lblSearchCount.Text = "0 件"; };
            Add(top, _btnClear);

            // Frame→Time utility
            Add(top, DkLabel("F→時:", L + 252, y + 3));
            _txtFrameConv = new TextBox
            {
                Location = new Point(L + 285, y), Width = 60, Height = H,
                BackColor = Color.FromArgb(45, 45, 48), ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 8.5f)
            };
            _txtFrameConv.TextChanged += TxtFrameConv_TextChanged;
            Add(top, _txtFrameConv);
            y += 27;

            // ── Row 7: Frame conv result + hint ──
            _lblFrameConvResult = new Label
            {
                Location = new Point(L, y), Width = 348, Height = 18,
                ForeColor = Color.LightSkyBlue, TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Consolas", 8f), Text = "フレーム数を入力すると時間に変換します"
            };
            Add(top, _lblFrameConvResult);
            y += 20;

            var hint = new Label
            {
                Location = new Point(L, y), Width = 348, Height = 16,
                ForeColor = Color.FromArgb(130, 130, 130),
                Font = new Font("Segoe UI", 7.5f),
                Text = "※1日=2592000F  1時間=108000F  1分=1800F  1秒=30F"
            };
            Add(top, hint);
        }

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
        //  Language / Kind / Origin Refresh
        // ═══════════════════════════════════════════════════════
        private void RefreshKindCombo()
        {
            string[] kinds = (_cmbLang.SelectedIndex == 0)
                ? new[] { "風化したお守り", "古びたお守り", "光るお守り", "なぞのお守り" }
                : new[] { "Enduring Charm", "Timeworn Charm", "Shining Charm", "Mystery Charm" };
            int sel = Math.Max(0, _cmbKind.SelectedIndex);
            _cmbKind.Items.Clear();
            _cmbKind.Items.AddRange(kinds);
            _cmbKind.SelectedIndex = sel < kinds.Length ? sel : 0;
        }

        private void RefreshOriginCombo()
        {
            string[] orgs = (_cmbLang.SelectedIndex == 0)
                ? new[] { "マカフシギ", "炭鉱夫" }
                : new[] { "Melding", "Quest" };
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
                string name = (id < _skillNames.Length) ? _skillNames[id] : $"?{id}";
                _cmbSkill1.Items.Add(name);
            }
            foreach (int id in _skill2Ids)
            {
                string name = (id < _skillNames.Length) ? _skillNames[id] : $"?{id}";
                _cmbSkill2.Items.Add(name);
            }

            _cmbSkill1.SelectedIndex = (prevS1 >= 0 && prevS1 < _cmbSkill1.Items.Count) ? prevS1 : 0;
            _cmbSkill2.SelectedIndex = (prevS2 >= 0 && prevS2 < _cmbSkill2.Items.Count) ? prevS2 : 0;

            _cmbSkill1.EndUpdate();
            _cmbSkill2.EndUpdate();
        }

        private void OnLangChanged(object sender, EventArgs e)
        {
            if (!_uiReady) return;
            _skillNames = (_cmbLang.SelectedIndex == 0) ? SkillJA : SkillEN;
            RefreshKindCombo();
            RefreshOriginCombo();
            RefreshSkillCombos();
        }

        private void OnKindChanged(object sender, EventArgs e)
        {
            if (!_uiReady || _cmbKind.SelectedIndex < 0) return;
            _kind = _cmbKind.SelectedIndex;
            switch (_kind) { case 0: SetBlue(); break; case 1: SetRed(); break; case 2: SetYellow(); break; default: SetWhite(); break; }
            RefreshSkillCombos();
        }

        private void TxtFrameConv_TextChanged(object sender, EventArgs e)
        {
            if (long.TryParse(_txtFrameConv.Text.Trim(), out long f))
                _lblFrameConvResult.Text = $"{f:N0} F  →  {FrameToTimeStr(f)}";
            else
                _lblFrameConvResult.Text = "フレーム数を入力すると時間に変換します";
        }

        // ═══════════════════════════════════════════════════════
        //  Charm Type Tables
        // ═══════════════════════════════════════════════════════
        private void SetBlue()
        {
            _skill1Ids = new[] {
                4,5,10,11,14,15,25,31,32,35,36,37,38,39,40,41,42,44,45,47,
                48,49,50,64,65,66,68,70,71,72,73,76,77,78,79,80,81,82,83,84,
                85,86,87,90,92,93,94,95,97,99,100,101,106,107,108,109,114,115,116,122,123,132 };

            _sp1 = new(int, int)[] {
                (3,7),(5,10),(3,7),(3,7),(3,7),(5,10),(3,7),(3,7),(3,7),(3,7),
                (3,7),(1,5),(2,6),(1,5),(1,5),(5,10),(5,10),(3,7),(2,6),(2,6),
                (2,6),(2,6),(2,6),(1,5),(1,5),(1,5),(3,7),(2,6),(1,5),(2,6),
                (2,6),(2,6),(2,6),(3,7),(3,7),(2,6),(2,6),(2,6),(1,5),(3,7),
                (3,7),(5,10),(5,10),(2,6),(2,6),(1,5),(1,5),(1,5),(2,6),(2,6),
                (2,6),(1,5),(2,6),(1,5),(3,7),(3,7),(3,7),(1,5),(3,7),(2,6),(3,7),(3,7) };

            _skill2Ids = new[] {
                4,5,17,18,25,26,27,28,29,30,32,33,34,35,36,37,39,40,41,43,
                44,45,47,48,49,50,64,65,66,68,69,70,71,74,75,76,77,78,79,80,
                81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,99,100,101,
                105,106,107,108,109,114,115,116,119,122,123,125,132,134,135,136,
                161,162,163,164,165,166,167,168,169,170,171,172,173,174,175,176,177,178 };

            _sp2 = new(int, int)[] {
                (3,5),(5,7),(7,10),(5,13),(5,7),(5,13),(5,13),(5,13),(5,13),(5,13),
                (5,7),(7,10),(3,5),(5,7),(5,7),(3,5),(5,5),(2,8),(5,7),(3,3),
                (5,7),(5,7),(3,5),(3,5),(3,5),(3,5),(3,5),(3,5),(3,5),(5,7),
                (7,10),(3,5),(1,3),(3,5),(3,3),(3,5),(3,5),(3,5),(3,5),(3,5),
                (3,5),(3,5),(1,3),(3,5),(3,5),(7,10),(7,10),(5,10),(5,10),(3,5),
                (5,10),(3,5),(1,3),(1,3),(1,3),(3,3),(3,5),(3,5),(3,5),(1,3),
                (7,10),(3,5),(1,3),(5,7),(5,7),(7,10),(1,3),(3,5),(5,12),(3,5),
                (5,7),(3,5),(7,10),(5,7),(3,5),(5,7),
                (3,3),(3,3),(3,3),(3,3),(3,3),(3,3),(3,3),(3,3),(3,3),(3,3),
                (3,3),(3,3),(3,3),(3,3),(3,3),(3,3),(3,3),(3,3) };

            _slotValue = new(int, int, int)[] {
                (100,100,100),(3,53,88),(5,55,89),(7,57,89),(13,58,89),
                (16,60,90),(22,62,90),(30,66,90),(38,68,91),(50,72,91),
                (55,75,92),(59,77,92),(64,81,94),(67,83,94),(71,86,96),
                (74,88,96),(79,91,98),(82,92,98),(86,94,99),(90,96,99) };
            _th = 15;
        }

        private void SetRed()
        {
            _skill1Ids = new[] {
                4,5,10,11,14,15,25,26,27,28,29,30,31,32,35,36,38,41,42,44,
                45,47,48,49,50,65,68,70,72,73,76,77,78,79,81,82,84,85,86,87,
                90,92,97,99,100,103,104,106,108,109,114,116,122,123,124,132 };

            _sp1 = new(int, int)[] {
                (1,5),(1,5),(1,5),(1,5),(1,5),(1,8),(1,5),(1,7),(1,7),(1,7),
                (1,7),(1,7),(1,5),(1,6),(1,5),(1,5),(1,6),(1,6),(1,6),(1,6),
                (1,5),(1,5),(1,5),(1,5),(1,5),(1,3),(1,5),(1,5),(1,5),(1,5),
                (1,5),(1,5),(1,6),(1,6),(1,6),(1,6),(1,6),(1,6),(1,6),(1,6),
                (1,5),(1,6),(1,6),(1,5),(1,5),(1,7),(1,7),(1,5),(1,5),(1,6),
                (1,7),(1,6),(1,5),(1,5),(1,5),(1,7) };

            _skill2Ids = new[] {
                3,4,5,17,18,19,20,21,22,23,24,25,26,27,28,29,30,32,33,34,
                35,36,37,39,40,41,42,44,45,47,48,49,50,64,65,66,68,69,70,71,
                74,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,
                95,97,99,100,101,103,104,105,106,107,108,109,110,114,115,116,117,119,120,122,
                123,124,125,132,134,135,136,143,144,145,146,147,148,149,150,151,152,153,154,155,
                156,157,158,159,160 };

            _sp2 = new(int, int)[] {
                (10,13),(3,3),(10,3),(10,10),(10,10),(10,13),(10,13),(10,13),(10,13),(10,13),
                (10,13),(3,3),(10,13),(10,13),(10,13),(10,13),(10,13),(10,4),(10,8),(5,5),
                (3,3),(3,3),(3,3),(3,3),(5,8),(10,4),(3,3),(3,4),(3,3),(3,3),
                (3,3),(3,3),(3,3),(3,3),(5,5),(3,3),(5,5),(10,10),(3,3),(3,3),
                (3,3),(3,3),(3,3),(3,4),(3,4),(5,5),(3,4),(3,4),(3,3),(3,4),
                (3,4),(3,4),(3,4),(10,10),(10,10),(3,3),(10,10),(3,4),(3,3),(3,3),
                (3,3),(3,4),(5,5),(5,5),(3,3),(10,10),(10,10),(5,5),(5,5),(3,3),
                (5,5),(3,3),(10,12),(10,9),(3,3),(3,4),(10,12),(10,12),(10,10),(3,3),
                (5,5),(5,5),(3,3),(8,10),(3,3),(3,3),(3,3),(3,3),(3,3),(3,3),
                (3,3),(3,3),(3,3),(3,3),(3,3),(3,3),(3,3),(3,3),(3,3),(3,3),(3,3),(3,3),(3,3),(3,3),(3,3) };

            _slotValue = new(int, int, int)[] {
                (8,58,88),(9,59,88),(16,61,89),(17,62,89),(23,63,89),
                (25,65,90),(31,66,90),(38,68,90),(45,71,91),(58,76,91),
                (63,79,92),(66,80,92),(71,83,94),(74,84,94),(78,87,96),
                (82,90,96),(86,93,98),(88,94,98),(91,96,99),(94,97,99) };
            _th = 25;
        }

        private void SetYellow()
        {
            _skill1Ids = new[] {
                0,1,2,3,5,6,7,13,17,18,19,20,21,22,23,24,26,27,28,29,
                30,32,33,38,41,44,46,51,52,53,54,55,62,63,68,69,72,73,78,79,
                81,84,85,86,87,88,89,91,97,98,99,100,103,104,106,108,109,110,113,114,
                116,117,119,120,122,123,124,126,129,131,132 };

            _sp1 = new(int, int)[] {
                (1,5),(1,5),(1,5),(1,8),(1,4),(1,7),(1,7),(1,7),(1,4),(1,4),
                (1,8),(1,6),(1,6),(1,6),(1,6),(1,6),(1,7),(1,7),(1,7),(1,7),
                (1,7),(1,4),(1,4),(1,4),(1,6),(1,4),(1,6),(1,6),(1,10),(1,10),
                (1,10),(1,10),(1,10),(1,10),(1,3),(1,4),(1,3),(1,3),(1,6),(1,6),
                (1,6),(1,6),(1,4),(1,6),(1,6),(1,6),(1,6),(1,6),(1,6),(1,5),
                (1,3),(1,3),(1,5),(1,5),(1,3),(1,3),(1,6),(1,8),(1,8),(1,7),
                (1,6),(1,7),(1,8),(1,8),(1,4),(1,3),(1,3),(1,8),(1,8),(1,8),(1,3) };

            _skill2Ids = new[] {
                0,1,2,3,6,7,8,9,12,13,14,15,16,17,18,19,20,21,22,23,
                24,32,40,46,51,52,53,54,55,56,57,58,59,60,61,62,63,65,67,68,
                69,72,73,88,89,91,98,99,100,102,103,104,105,106,108,110,111,112,113,117,
                118,119,120,121,123,124,126,127,128,129,130,131,132,133 };

            _sp2 = new(int, int)[] {
                (10,7),(10,7),(10,7),(10,10),(10,8),(10,8),(10,10),(10,10),(10,10),(10,8),
                (5,5),(5,5),(10,10),(7,7),(7,7),(10,10),(10,10),(10,10),(10,10),(10,10),
                (10,10),(4,4),(5,5),(10,10),(8,8),(10,10),(10,10),(10,10),(10,10),(10,10),
                (10,10),(10,10),(10,12),(10,12),(10,10),(10,10),(10,10),(3,3),(10,10),(5,5),
                (7,7),(5,5),(5,5),(8,8),(8,8),(8,8),(5,5),(5,5),(5,5),(10,10),
                (7,7),(7,7),(8,8),(5,5),(5,5),(10,10),(10,10),(10,10),(10,10),(4,4),
                (10,10),(10,10),(10,10),(10,13),(5,5),(5,5),(10,10),(10,13),(10,10),(10,10),
                (10,13),(10,10),(5,5),(10,13) };

            _slotValue = new(int, int, int)[] {
                (2,72,100),(9,74,100),(16,76,100),(23,78,100),(30,80,100),
                (37,82,100),(44,84,100),(51,86,100),(58,88,100),(75,90,100),
                (83,92,100),(87,95,100),(90,97,100),(92,98,100),(94,99,100),
                (95,99,100),(97,100,100),(98,100,100),(99,100,100),(99,100,100) };
            _th = 35;
        }

        private void SetWhite()
        {
            _skill1Ids = new[] {
                0,1,2,3,6,7,8,9,12,13,14,16,17,18,19,20,21,22,23,24,
                46,51,52,53,54,55,56,57,58,59,60,61,62,63,67,69,88,89,91,98,
                102,103,104,105,110,111,112,113,118,119,120,121,126,127,128,129,130,131,133 };

            _sp1 = new(int, int)[] {
                (1,5),(1,5),(1,5),(1,8),(1,7),(1,7),(1,10),(1,10),(1,10),(1,7),
                (1,3),(1,5),(1,4),(1,4),(1,8),(1,6),(1,6),(1,6),(1,6),(1,6),
                (1,6),(1,8),(1,8),(1,8),(1,8),(1,8),(1,8),(1,8),(1,8),(1,8),
                (1,8),(1,8),(1,8),(1,8),(1,5),(1,4),(1,6),(1,6),(1,6),(1,4),
                (1,8),(1,3),(1,3),(1,10),(1,8),(1,8),(1,8),(1,8),(1,8),(1,8),
                (1,8),(1,10),(1,8),(1,10),(1,8),(1,8),(1,10),(1,8),(1,10) };

            _skill2Ids = new[] { 0 };
            _sp2 = new(int, int)[] { (10, 7) };

            _slotValue = new(int, int, int)[] {
                (55,100,100),(60,100,100),(65,100,100),(70,100,100),(75,100,100),
                (80,100,100),(85,100,100),(90,100,100),(95,100,100),(99,100,100),
                (100,100,100),(100,100,100),(100,100,100),(100,100,100),(100,100,100),
                (100,100,100),(100,100,100),(100,100,100),(100,100,100),(100,100,100) };
            _th = 100;
        }

        // ═══════════════════════════════════════════════════════
        //  XorShift128 Core
        // ═══════════════════════════════════════════════════════
        private void Init()
        {
            _x = DefaultSeed[0]; _y = DefaultSeed[1]; _z = DefaultSeed[2]; _w = DefaultSeed[3];
            _t = 0; _f = 0;
        }

        private void Ascend()
        {
            _t = (_x ^ (_x << 15)) & 0xFFFFFFFF;
            _x = _y; _y = _z; _z = _w;
            _w = (_w ^ (_w >> 21) ^ _t ^ (_t >> 4)) & 0xFFFFFFFF;
            _f++;
        }

        private void Roll()
        {
            _r[0] = _r[1]; _r[1] = _r[2]; _r[2] = _r[3];
            _r[3] = _r[4]; _r[4] = _r[5]; _r[5] = _r[6];
            _r[6] = _w;
            Ascend();
        }

        // ─────────────────────────────────────────────────────
        //  Polynomial arithmetic over GF(2)
        // ─────────────────────────────────────────────────────
        private static int BigIntBitLen(BigInteger n)
        {
            if (n.IsZero) return 0;
            byte[] bytes = n.ToByteArray(); // little-endian, sign-extended
            int top = bytes.Length - 1;
            while (top > 0 && bytes[top] == 0) top--;
            byte b = bytes[top];
            int bits = top * 8;
            while (b != 0) { bits++; b >>= 1; }
            return bits;
        }

        private static BigInteger PolyMul(BigInteger p1, BigInteger p2)
        {
            BigInteger res = BigInteger.Zero;
            while (p2 > BigInteger.Zero)
            {
                if ((p2 & BigInteger.One) == BigInteger.One) res ^= p1;
                p1 <<= 1;
                p2 >>= 1;
            }
            return res;
        }

        private static BigInteger PolyMod(BigInteger p, BigInteger m)
        {
            int mLen = BigIntBitLen(m);
            while (true)
            {
                int pLen = BigIntBitLen(p);
                int delta = pLen - mLen;
                if (delta < 0) break;
                p ^= (m << delta);
            }
            return p;
        }

        private static BigInteger PolyPowMod(BigInteger baseP, BigInteger exp, BigInteger mod)
        {
            BigInteger res = BigInteger.One;
            baseP = PolyMod(baseP, mod);
            while (exp > BigInteger.Zero)
            {
                if ((exp & BigInteger.One) == BigInteger.One)
                    res = PolyMod(PolyMul(res, baseP), mod);
                baseP = PolyMod(PolyMul(baseP, baseP), mod);
                exp >>= 1;
            }
            return res;
        }

        private void Jump(long frame)
        {
            Init();
            if (frame == 0) { for (int i = 0; i < 7; i++) Roll(); return; }

            BigInteger period = (BigInteger.One << 128) - BigInteger.One;
            BigInteger frameB = new BigInteger(frame);
            frameB = ((frameB % period) + period) % period;

            BigInteger rPoly = PolyPowMod(new BigInteger(2), frameB, CharPoly);

            uint sx = 0, sy = 0, sz = 0, sw = 0;
            while (rPoly > BigInteger.Zero)
            {
                if ((rPoly & BigInteger.One) == BigInteger.One)
                { sx ^= _x; sy ^= _y; sz ^= _z; sw ^= _w; }
                rPoly >>= 1;
                Ascend();
            }
            _x = sx; _y = sy; _z = sz; _w = sw;
            _f = frame;
            for (int i = 0; i < 7; i++) Roll();
        }

        // ═══════════════════════════════════════════════════════
        //  Charm Generation
        // ═══════════════════════════════════════════════════════
        private int CalcSlot(int fill, int q5mod)
        {
            if (fill < 1 || fill > _slotValue.Length) return 0;
            var sv = _slotValue[fill - 1];
            if (q5mod >= sv.T3) return 3;
            if (q5mod >= sv.T2) return 2;
            if (q5mod >= sv.T1) return 1;
            return 0;
        }

        private int CalcRare(int slot, int fill)
        {
            int n = slot * 2 + fill;
            return _kind switch
            {
                0 => n >= 13 ? 10 : n >= 8 ? 9 : 8,
                1 => n >= 13 ? 7  : n >= 8 ? 6 : 5,
                2 => n >= 8  ? 4  : 3,
                _ => n >= 8  ? 2  : 1
            };
        }

        private CharmResult GetCharm(int origin)
        {
            int len1 = _skill1Ids.Length, len2 = _skill2Ids.Length;
            int id1 = (int)(_r[0] % (uint)len1);
            int id2 = (int)(_r[3] % (uint)len2);
            int s1  = _sp1[id1].Max, s2 = _sp2[id2].Max;

            int tmp1 = (int)(_r[1] % (uint)(_sp1[id1].Max - _sp1[id1].Min + 1)) + _sp1[id1].Min;

            int sk2Id = -1, tmp2 = 0;
            uint q5;

            if (_r[2] % 100 >= (uint)_th)
            {
                sk2Id = _skill2Ids[id2];
                uint q4;
                if (origin == 1 && _r[4] % 2 == 0)
                {
                    q4 = _r[5]; q5 = _r[6];
                    tmp2 = (int)(q4 % (uint)(_sp2[id2].Min + 1)) - _sp2[id2].Min;
                }
                else
                {
                    if (origin == 1) { q4 = _r[5]; q5 = _r[6]; }
                    else             { q4 = _r[4]; q5 = _r[5]; }
                    tmp2 = (int)(q4 % (uint)_sp2[id2].Max) + 1;
                }
            }
            else { q5 = _r[3]; }

            int sp2raw = tmp2;
            if (sk2Id == _skill1Ids[id1] || tmp2 < 0) tmp2 = 0;

            int fillVal = (tmp1 * s2 + tmp2 * s1) * 10 / (s1 * s2);
            int slotNum = CalcSlot(fillVal, (int)(q5 % 100));

            return new CharmResult
            {
                Frame    = _f - 7,
                TimeStr  = FrameToTimeStr(_f - 7),
                Skill1Id = _skill1Ids[id1],
                SP1      = tmp1,
                Skill2Id = sk2Id,
                SP2      = (sk2Id < 0) ? 0 : sp2raw,
                Slot     = slotNum,
                Fill     = fillVal,
                Rare     = CalcRare(slotNum, fillVal)
            };
        }

        // ═══════════════════════════════════════════════════════
        //  Frame → Time
        // ═══════════════════════════════════════════════════════
        private static string FrameToTimeStr(long frame)
        {
            if (frame < 0) return $"({frame})";
            long f  = frame;
            long d  = f / 2592000; f %= 2592000;
            long h  = f / 108000;  f %= 108000;
            long m  = f / 1800;    f %= 1800;
            long s  = f / 30;
            long fr = f % 30;
            return $"{d}d {h}h {m}m {s}s {fr}f";
        }

        // ═══════════════════════════════════════════════════════
        //  Search
        // ═══════════════════════════════════════════════════════
        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            if (_btnStop.Enabled) { _cts?.Cancel(); return; }

            // ── Snapshot all params before going async ──
            long  startFrame  = (long)_nudStartFrame.Value;
            int   steps       = (int)_nudSteps.Value;
            int   skill1Sel   = _cmbSkill1.SelectedIndex;   // index into _skill1Ids[]
            int   sp1Target   = (int)_nudSP1.Value;
            int   skill2Sel   = _cmbSkill2.SelectedIndex;   // 0=none, else [sel-1] into _skill2Ids[]
            int   sp2Target   = (int)_nudSP2.Value;
            int   slotTarget  = (int)_nudSlot.Value;
            bool  exact       = _rbExact.Checked;
            int   origin      = _origin;
            int   tgtId1      = skill1Sel;
            int   tgtId2      = (skill2Sel <= 0) ? -1 : (skill2Sel - 1);

            // Table snapshots
            int[] sk1 = (int[])_skill1Ids.Clone(), sk2 = (int[])_skill2Ids.Clone();
            var   p1  = ((int, int)[])_sp1.Clone(), p2 = ((int, int)[])_sp2.Clone();
            var   sv  = ((int, int, int)[])_slotValue.Clone();
            int   th  = _th, kind = _kind;
            string[] skNames = (string[])_skillNames.Clone();

            _btnSearch.Enabled = false;
            _btnStop.Enabled   = true;
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            SetStatus("検索中...");
            int totalFound = _dgvResults.Rows.Count;

            try
            {
                await Task.Run(() =>
                {
                    // Local RNG state
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

                    // Jump to startFrame
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

                        // Pre-filter by skill1 index
                        if (r[0] % (uint)len1 != (uint)tgtId1) continue;

                        // Pre-filter by skill2 index (if required)
                        bool hasSkill2 = (tgtId2 >= 0);
                        if (hasSkill2)
                        {
                            if (r[2] % 100 < (uint)th) continue;
                            if (r[3] % (uint)len2 != (uint)tgtId2) continue;
                        }

                        // Generate charm
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

                        // Slot
                        int slotNum;
                        if (fillVal < 1 || fillVal > sv.Length) slotNum = 0;
                        else
                        {
                            var sv_ = sv[fillVal - 1];
                            slotNum = q5m >= sv_.Item3 ? 3 : q5m >= sv_.Item2 ? 2 : q5m >= sv_.Item1 ? 1 : 0;
                        }

                        // Rare
                        int rn  = slotNum * 2 + fillVal;
                        int rare = kind switch
                        {
                            0 => rn >= 13 ? 10 : rn >= 8 ? 9 : 8,
                            1 => rn >= 13 ? 7  : rn >= 8 ? 6 : 5,
                            2 => rn >= 8  ? 4  : 3,
                            _ => rn >= 8  ? 2  : 1
                        };

                        // Apply search filters
                        if (exact)
                        {
                            if (tmp1 != sp1Target || slotNum != slotTarget) continue;
                            if (hasSkill2) { if (sp2raw != sp2Target) continue; }
                            else
                            {
                                if (sk2id != -1 && sk2id != sk1[id1] && sp2raw != 0) continue;
                            }
                        }
                        else // >= search
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

                        string sk1Name = (sk1[id1] < skNames.Length) ? skNames[sk1[id1]] : $"#{sk1[id1]}";
                        string sk2Name = (sk2id < 0) ? "---" : ((sk2id < skNames.Length) ? skNames[sk2id] : $"#{sk2id}");

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

                        if (i % 200000 == 0 && i > 0)
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
                int cnt = _dgvResults.Rows.Count;
                if (!ct.IsCancellationRequested)
                    SetStatus($"完了 — {cnt} 件ヒット");
                _lblSearchCount.Text = $"{cnt} 件";
                _btnSearch.Enabled  = true;
                _btnStop.Enabled    = false;
            }
        }

        private void FlushBatch(List<CharmResult> batch, string[] skNames)
        {
            _dgvResults.SuspendLayout();
            foreach (var c in batch)
            {
                string sk1Name = (c.Skill1Id >= 0 && c.Skill1Id < skNames.Length) ? skNames[c.Skill1Id] : $"#{c.Skill1Id}";
                string sk2Name = (c.Skill2Id < 0) ? "---"
                               : (c.Skill2Id < skNames.Length ? skNames[c.Skill2Id] : $"#{c.Skill2Id}");
                string sp2Str  = (c.Skill2Id < 0 || c.SP2 == 0) ? "---" : c.SP2.ToString();

                int rowIdx = _dgvResults.Rows.Add(
                    c.Frame, c.TimeStr,
                    sk1Name, c.SP1,
                    sk2Name, sp2Str,
                    "S" + c.Slot, c.Fill,
                    "RARE" + c.Rare);

                // Colour by rarity
                Color fc = c.Rare switch
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
                _dgvResults.Rows[rowIdx].Cells["colRare"].Style.ForeColor = fc;
                _dgvResults.Rows[rowIdx].Cells["colRare"].Style.Font = new Font("Consolas", 8.5f, FontStyle.Bold);

                if (c.Slot >= 2)
                    _dgvResults.Rows[rowIdx].Cells["colSlot"].Style.ForeColor = Color.LightGreen;
            }
            _dgvResults.ResumeLayout();
            _lblSearchCount.Text = $"{_dgvResults.Rows.Count} 件";
        }

        private void CopySelectedRow()
        {
            if (_dgvResults.CurrentRow == null) return;
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
