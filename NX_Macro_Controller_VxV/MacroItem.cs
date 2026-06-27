using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BZComponent;
using HongliangSoft.Utilities;
using NX_Macro_Controller_VxV.Properties;
using NxInterface;

namespace NX_Macro_Controller_VxV;

public class MacroItem : UserControl
{
	private MouseHook mh;

	private string baseName = "";

	private string beforeTex = "1";

	private IContainer components;

	private GroupBoxEx groupBox1;

	private Label label1;

	private ContextMenuStrip contextMenuStrip1;

	private ToolStripMenuItem ラベルの変更ToolStripMenuItem;

	private ToolStripMenuItem 削除ToolStripMenuItem;

	private ButtonEx buttonEx1;

	private ButtonEx buttonEx2;

	private ButtonEx buttonEx3;

	private Label label3;

	private Label label2;

	private MaskedTextBox maskedTextBox1;

	private GhostPanel ghostPanel1;

	public uint LoopCount => uint.Parse(maskedTextBox1.Text);

	public MacroItem(string path)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		InitializeComponent();
		maskedTextBox1.ValidatingType = typeof(uint);
		maskedTextBox1.TypeValidationCompleted += MaskedTextBox1_TypeValidationCompleted;
		((GroupBox)(object)groupBox1).Click += delegate(object sender, EventArgs args)
		{
			OnClick(args);
		};
		baseName = path;
		label1.Text = Path.GetFileName(baseName);
		buttonEx2.Enabled = false;
		ThemeChange(KEYCONFIG.AppConfig.APPTHEME);
		maskedTextBox1.BackColor = BZStyle.NormalColor;
		maskedTextBox1.ForeColor = BZStyle.TextFont;
	}

	private void MaskedTextBox1_TypeValidationCompleted(object sender, TypeValidationEventArgs e)
	{
		string s = maskedTextBox1.Text.Replace(" ", "");
		if (uint.TryParse(s, out var _))
		{
			maskedTextBox1.Text = uint.Parse(s).ToString();
			beforeTex = maskedTextBox1.Text;
		}
		else
		{
			maskedTextBox1.Text = beforeTex;
		}
	}

	public void DeleteTask()
	{
		GlobalVar.MacroList.Remove(baseName);
		if (GlobalVar.MAINFORM.Nmc.SubRunningNmc == baseName)
		{
			GlobalVar.MAINFORM.Nmc.Cancel = true;
		}
		GlobalVar.MAINFORM.MacroShortCutReload();
	}

	public void NameChangeTask(string name)
	{
		int num = GlobalVar.MacroList.IndexOf(baseName);
		if (GlobalVar.MacroList.IndexOf(name) == -1 && num >= 0)
		{
			GlobalVar.MacroList.RemoveAt(num);
			GlobalVar.MacroList.Insert(num, name);
			baseName = name;
			label1.Text = Path.GetFileName(name);
			maskedTextBox1.BackColor = BZStyle.NormalColor;
			maskedTextBox1.ForeColor = BZStyle.TextFont;
		}
	}

	private void pictureBox1_Click(object sender, EventArgs e)
	{
		OnClick(e);
	}

	private void label1_Click(object sender, EventArgs e)
	{
		OnClick(e);
	}

	private void groupBox1_Enter(object sender, EventArgs e)
	{
	}

	private void pictureBox1_DragDrop(object sender, DragEventArgs e)
	{
		OnDragDrop(e);
	}

	private void label1_DragDrop(object sender, DragEventArgs e)
	{
		OnDragDrop(e);
	}

	private void groupBox1_DragDrop(object sender, DragEventArgs e)
	{
		OnDragDrop(e);
	}

	private void textBox1_TextChanged(object sender, EventArgs e)
	{
	}

	private void textBox1_Leave(object sender, EventArgs e)
	{
	}

	private void textBox1_Enter(object sender, EventArgs e)
	{
	}

	private void ラベルの変更ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		((Button)(object)buttonEx3).PerformClick();
	}

	private void 画像の変更ToolStripMenuItem_Click(object sender, EventArgs e)
	{
	}

	private void 削除ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		DeleteTask();
	}

	private void ImgResItem_Paint(object sender, PaintEventArgs e)
	{
	}

	private void buttonEx3_Click(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "NX Macro Controller用マクロファイル(*.nxc;*.nmc)|*.nxc;*.nmc|すべてのファイル(*.*)|*.*";
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			NameChangeTask(openFileDialog.FileName);
		}
	}

	public void ThemeChange(Style style)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		maskedTextBox1.BackColor = BZStyle.NormalColor;
		maskedTextBox1.ForeColor = BZStyle.TextFont;
		if ((int)style == 1)
		{
			buttonEx1.Image = Resources.B1;
		}
		else
		{
			buttonEx1.Image = Resources.B1_L;
		}
	}

	private void buttonEx1_Click(object sender, EventArgs e)
	{
		buttonEx1.Enabled = false;
		buttonEx2.Enabled = true;
		GlobalVar.MAINFORM.MacroActive();
		Task.Factory.StartNew(delegate
		{
			if (GlobalVar.MAINFORM.Nmc.SubRunningNmc == "")
			{
				GlobalVar.MAINFORM.Nmc.SubRunningNmc = baseName;
				GlobalVar.MAINFORM.Nmc.Cancel = true;
			}
			else
			{
				GlobalVar.MAINFORM.Nmc.SubRunningNmc = "";
			}
			while (GlobalVar.MAINFORM.Nmc.Running)
			{
				Thread.Sleep(1);
			}
			GlobalVar.NmcThread = Thread.CurrentThread;
			GlobalVar.MAINFORM.Nmc.BackNmcExecution(baseName, LoopCount);
			while (GlobalVar.MAINFORM.Nmc.Running)
			{
				Thread.Sleep(1);
			}
			Invoke((Action)delegate
			{
				try
				{
					if (!GlobalVar.MAINFORM.Nmc.Running && !(GlobalVar.MAINFORM.Nmc.SubRunningNmc == ""))
					{
						GlobalVar.MAINFORM.MacroDeactive();
					}
					buttonEx1.Enabled = true;
					buttonEx2.Enabled = false;
				}
				catch
				{
				}
			});
		});
	}

	private void buttonEx2_Click(object sender, EventArgs e)
	{
		GlobalVar.MAINFORM.Nmc.Cancel = true;
		while (GlobalVar.MAINFORM.Nmc.Running)
		{
			Application.DoEvents();
			if (!GlobalVar.MAINFORM.Nmc.RunningCsx && !NxCommand.ExitFlag)
			{
				continue;
			}
			if (GlobalVar.NmcThread == null)
			{
				break;
			}
			GlobalVar.NmcThread.Abort();
			GlobalVar.MAINFORM.Nmc.NmcEndSec();
			Invoke((Action)delegate
			{
				try
				{
					if (!GlobalVar.MAINFORM.Nmc.Running && !(GlobalVar.MAINFORM.Nmc.SubRunningNmc == ""))
					{
						GlobalVar.MAINFORM.MacroDeactive();
					}
					buttonEx1.Enabled = true;
					buttonEx2.Enabled = false;
				}
				catch
				{
				}
			});
			break;
		}
	}

	private void mouseHook1_MouseHooked(object sender, MouseHookedEventArgs e)
	{
	}

	private void ghostPanel1_Paint(object sender, PaintEventArgs e)
	{
	}

	public void SuspendLayoutFix()
	{
		SuspendLayout();
		((Control)(object)groupBox1).SuspendLayout();
		((Control)(object)ghostPanel1).SuspendLayout();
		label1.SuspendLayout();
		label2.SuspendLayout();
		label3.SuspendLayout();
		((Control)(object)buttonEx1).SuspendLayout();
		((Control)(object)buttonEx2).SuspendLayout();
		((Control)(object)buttonEx3).SuspendLayout();
		maskedTextBox1.SuspendLayout();
	}

	public void ResumeLayoutFix()
	{
		ResumeLayout(performLayout: false);
		((Control)(object)groupBox1).ResumeLayout(false);
		((Control)(object)ghostPanel1).ResumeLayout(false);
		label1.ResumeLayout(performLayout: false);
		label2.ResumeLayout(performLayout: false);
		label3.ResumeLayout(performLayout: false);
		((Control)(object)buttonEx1).ResumeLayout(false);
		((Control)(object)buttonEx2).ResumeLayout(false);
		((Control)(object)buttonEx3).ResumeLayout(false);
		maskedTextBox1.ResumeLayout(performLayout: false);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		this.components = new System.ComponentModel.Container();
		this.groupBox1 = new GroupBoxEx();
		this.maskedTextBox1 = new System.Windows.Forms.MaskedTextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.buttonEx3 = new ButtonEx();
		this.buttonEx2 = new ButtonEx();
		this.buttonEx1 = new ButtonEx();
		this.label1 = new System.Windows.Forms.Label();
		this.ghostPanel1 = new GhostPanel();
		this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.ラベルの変更ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.削除ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		((System.Windows.Forms.Control)(object)this.groupBox1).SuspendLayout();
		this.contextMenuStrip1.SuspendLayout();
		base.SuspendLayout();
		((System.Windows.Forms.Control)(object)this.groupBox1).Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.groupBox1.BorderColor = System.Drawing.Color.Black;
		((System.Windows.Forms.Control)(object)this.groupBox1).Controls.Add(this.maskedTextBox1);
		((System.Windows.Forms.Control)(object)this.groupBox1).Controls.Add(this.label3);
		((System.Windows.Forms.Control)(object)this.groupBox1).Controls.Add(this.label2);
		((System.Windows.Forms.Control)(object)this.groupBox1).Controls.Add((System.Windows.Forms.Control)(object)this.buttonEx3);
		((System.Windows.Forms.Control)(object)this.groupBox1).Controls.Add((System.Windows.Forms.Control)(object)this.buttonEx2);
		((System.Windows.Forms.Control)(object)this.groupBox1).Controls.Add((System.Windows.Forms.Control)(object)this.buttonEx1);
		((System.Windows.Forms.Control)(object)this.groupBox1).Controls.Add(this.label1);
		((System.Windows.Forms.Control)(object)this.groupBox1).Controls.Add((System.Windows.Forms.Control)(object)this.ghostPanel1);
		((System.Windows.Forms.Control)(object)this.groupBox1).Location = new System.Drawing.Point(3, 3);
		((System.Windows.Forms.Control)(object)this.groupBox1).Name = "groupBox1";
		((System.Windows.Forms.Control)(object)this.groupBox1).Size = new System.Drawing.Size(154, 99);
		((System.Windows.Forms.Control)(object)this.groupBox1).TabIndex = 0;
		((System.Windows.Forms.GroupBox)(object)this.groupBox1).TabStop = false;
		((System.Windows.Forms.Control)(object)this.groupBox1).DragDrop += new System.Windows.Forms.DragEventHandler(groupBox1_DragDrop);
		((System.Windows.Forms.Control)(object)this.groupBox1).Enter += new System.EventHandler(groupBox1_Enter);
		this.maskedTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.maskedTextBox1.ImeMode = System.Windows.Forms.ImeMode.Disable;
		this.maskedTextBox1.Location = new System.Drawing.Point(60, 45);
		this.maskedTextBox1.Mask = "9999999";
		this.maskedTextBox1.Name = "maskedTextBox1";
		this.maskedTextBox1.Size = new System.Drawing.Size(61, 12);
		this.maskedTextBox1.TabIndex = 9;
		this.maskedTextBox1.Text = "1";
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(6, 45);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(48, 12);
		this.label3.TabIndex = 8;
		this.label3.Text = "繰り返し:";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(127, 45);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(17, 12);
		this.label2.TabIndex = 7;
		this.label2.Text = "回";
		((System.Windows.Forms.ButtonBase)(object)this.buttonEx3).FlatAppearance.BorderSize = 0;
		((System.Windows.Forms.Control)(object)this.buttonEx3).ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		this.buttonEx3.Image = null;
		((System.Windows.Forms.Control)(object)this.buttonEx3).Location = new System.Drawing.Point(122, 12);
		((System.Windows.Forms.Control)(object)this.buttonEx3).Name = "buttonEx3";
		((System.Windows.Forms.Control)(object)this.buttonEx3).Size = new System.Drawing.Size(27, 23);
		((System.Windows.Forms.Control)(object)this.buttonEx3).TabIndex = 5;
		((System.Windows.Forms.Control)(object)this.buttonEx3).Text = "...";
		((System.Windows.Forms.ButtonBase)(object)this.buttonEx3).TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
		((System.Windows.Forms.ButtonBase)(object)this.buttonEx3).UseVisualStyleBackColor = true;
		((System.Windows.Forms.Control)(object)this.buttonEx3).Click += new System.EventHandler(buttonEx3_Click);
		((System.Windows.Forms.Control)(object)this.buttonEx2).Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		((System.Windows.Forms.ButtonBase)(object)this.buttonEx2).FlatAppearance.BorderSize = 0;
		((System.Windows.Forms.Control)(object)this.buttonEx2).ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		this.buttonEx2.Image = NX_Macro_Controller_VxV.Properties.Resources.B4;
		((System.Windows.Forms.Control)(object)this.buttonEx2).Location = new System.Drawing.Point(79, 68);
		((System.Windows.Forms.Control)(object)this.buttonEx2).Name = "buttonEx2";
		((System.Windows.Forms.Control)(object)this.buttonEx2).Size = new System.Drawing.Size(70, 23);
		((System.Windows.Forms.Control)(object)this.buttonEx2).TabIndex = 4;
		((System.Windows.Forms.Control)(object)this.buttonEx2).Text = "停止";
		((System.Windows.Forms.ButtonBase)(object)this.buttonEx2).TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
		((System.Windows.Forms.ButtonBase)(object)this.buttonEx2).UseVisualStyleBackColor = true;
		((System.Windows.Forms.Control)(object)this.buttonEx2).Click += new System.EventHandler(buttonEx2_Click);
		((System.Windows.Forms.Control)(object)this.buttonEx1).Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		((System.Windows.Forms.ButtonBase)(object)this.buttonEx1).FlatAppearance.BorderSize = 0;
		((System.Windows.Forms.Control)(object)this.buttonEx1).ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		this.buttonEx1.Image = NX_Macro_Controller_VxV.Properties.Resources.B1;
		((System.Windows.Forms.Control)(object)this.buttonEx1).Location = new System.Drawing.Point(5, 68);
		((System.Windows.Forms.Control)(object)this.buttonEx1).Name = "buttonEx1";
		((System.Windows.Forms.Control)(object)this.buttonEx1).Size = new System.Drawing.Size(70, 23);
		((System.Windows.Forms.Control)(object)this.buttonEx1).TabIndex = 3;
		((System.Windows.Forms.Control)(object)this.buttonEx1).Text = "実行";
		((System.Windows.Forms.ButtonBase)(object)this.buttonEx1).TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
		((System.Windows.Forms.ButtonBase)(object)this.buttonEx1).UseVisualStyleBackColor = true;
		((System.Windows.Forms.Control)(object)this.buttonEx1).Click += new System.EventHandler(buttonEx1_Click);
		this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.label1.AutoEllipsis = true;
		this.label1.Location = new System.Drawing.Point(6, 17);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(110, 12);
		this.label1.TabIndex = 1;
		this.label1.Text = "label1";
		this.label1.Click += new System.EventHandler(label1_Click);
		this.label1.DragDrop += new System.Windows.Forms.DragEventHandler(label1_DragDrop);
		((System.Windows.Forms.Control)(object)this.ghostPanel1).Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		((System.Windows.Forms.Control)(object)this.ghostPanel1).Location = new System.Drawing.Point(1, 1);
		((System.Windows.Forms.Control)(object)this.ghostPanel1).Name = "ghostPanel1";
		((System.Windows.Forms.Control)(object)this.ghostPanel1).Size = new System.Drawing.Size(152, 97);
		((System.Windows.Forms.Control)(object)this.ghostPanel1).TabIndex = 10;
		((System.Windows.Forms.Control)(object)this.ghostPanel1).Paint += new System.Windows.Forms.PaintEventHandler(ghostPanel1_Paint);
		this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.ラベルの変更ToolStripMenuItem, this.削除ToolStripMenuItem });
		this.contextMenuStrip1.Name = "contextMenuStrip1";
		this.contextMenuStrip1.Size = new System.Drawing.Size(143, 48);
		this.ラベルの変更ToolStripMenuItem.Name = "ラベルの変更ToolStripMenuItem";
		this.ラベルの変更ToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
		this.ラベルの変更ToolStripMenuItem.Text = "ファイルの変更";
		this.ラベルの変更ToolStripMenuItem.Click += new System.EventHandler(ラベルの変更ToolStripMenuItem_Click);
		this.削除ToolStripMenuItem.Name = "削除ToolStripMenuItem";
		this.削除ToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
		this.削除ToolStripMenuItem.Text = "削除";
		this.削除ToolStripMenuItem.Click += new System.EventHandler(削除ToolStripMenuItem_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.ContextMenuStrip = this.contextMenuStrip1;
		base.Controls.Add((System.Windows.Forms.Control)(object)this.groupBox1);
		base.Name = "MacroItem";
		base.Size = new System.Drawing.Size(160, 105);
		base.Paint += new System.Windows.Forms.PaintEventHandler(ImgResItem_Paint);
		((System.Windows.Forms.Control)(object)this.groupBox1).ResumeLayout(false);
		((System.Windows.Forms.Control)(object)this.groupBox1).PerformLayout();
		this.contextMenuStrip1.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
