using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Xml;
using BZComponent;
using CustomScrollBar;
using DirectShowLib;
using DiscordRPC;
using DiscordRPC.Events;
using DiscordRPC.Message;
using HongliangSoft.Utilities;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Rendering;
using IronPython.Hosting;
using NX_Macro_Controller_VxV.Properties;
using NxInterface;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using PSTaskDialog;
using RJCP.IO.Ports;

namespace NX_Macro_Controller_VxV;

public class NXMC_VxV : FormEx
{
	public struct nxSelection
	{
		public int X1;

		public int X2;

		public int Y1;

		public int Y2;

		public int PicW;

		public int PicH;

		public double PicD;

		public bool Start;
	}

	public enum CaptureStyle
	{
		None,
		DirectShow,
		OpenCV
	}

	private class USBDeviceInfo
	{
		public string DeviceID { get; private set; }

		public string PnpDeviceID { get; private set; }

		public string Description { get; private set; }

		public USBDeviceInfo(string deviceID, string pnpDeviceID, string description)
		{
			DeviceID = deviceID;
			PnpDeviceID = pnpDeviceID;
			Description = description;
		}
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static OnReadyEvent _003C_003E9__36_1;

		public static OnPresenceUpdateEvent _003C_003E9__36_2;

		public static Func<string[], int> _003C_003E9__36_21;

		public static Func<ResourcesImage, string> _003C_003E9__37_0;

		public static Func<ResourcesImage, string> _003C_003E9__37_1;

		public static Func<ResourcesImage, string> _003C_003E9__39_0;

		public static Action _003C_003E9__83_2;

		public static Func<ResourcesImage, string> _003C_003E9__88_0;

		public static Action _003C_003E9__162_0;

		internal void _003C_002Ector_003Eb__36_1(object sender, ReadyMessage e)
		{
		}

		internal void _003C_002Ector_003Eb__36_2(object sender, PresenceMessage e)
		{
		}

		internal int _003C_002Ector_003Eb__36_21(string[] _)
		{
			return _[0].Length;
		}

		internal string _003CTextAreaOnKeyUp_003Eb__37_0(ResourcesImage _)
		{
			return _.label;
		}

		internal string _003CTextAreaOnKeyUp_003Eb__37_1(ResourcesImage _)
		{
			return _.label;
		}

		internal string _003CCodeEditOnMouseHover_003Eb__39_0(ResourcesImage _)
		{
			return _.label;
		}

		internal void _003C設定ToolStripMenuItem_DropDownOpened_003Eb__83_2()
		{
			NxControllerInterface.StartedBluetooth = true;
			NxControllerInterface.StartGamepad();
		}

		internal string _003CflowLayoutPanel1_DragDrop_003Eb__88_0(ResourcesImage _)
		{
			return _.label;
		}

		internal void _003CComConnect_Click_003Eb__162_0()
		{
			NxControllerInterface.StartedBluetooth = true;
			NxControllerInterface.StartGamepad();
		}
	}

	private System.Drawing.Image _capturedImage = new Bitmap(10, 10);

	public DiscordRpcClient DiscordRpcClient;

	public TextEditor CodeEdit;

	public System.Windows.Controls.ListBox LsBox;

	private Matsub _popUpWindow;

	public nxSelection NxSel;

	public NMC Nmc = new NMC();

	private KeyMessageFilter _mFilter = new KeyMessageFilter();

	private string _selectedPort = "";

	public bool _captureNow;

	private int _captureMode;

	private SerialPortStream _serialPort = new SerialPortStream();

	public bool KeyRecoding;

	private bool _vScrollF;

	private string macroSelCmbText = "";

	private string _amiibo = "";

	private Stopwatch _lastTaskView = new Stopwatch();

	private Stopwatch _lastHighlight = new Stopwatch();

	private string[] _portsBuffer = SerialPort.GetPortNames();

	public DSHDMICapture DsCapture;

	public VideoCapture CvCapture;

	public CaptureStyle CurrentCaptureFormat;

	private KeyboardHook kbh = new KeyboardHook();

	private bool captureRun;

	public string CurrentDirectory = "";

	public string MacroDirectory = "";

	private Bitmap captureScreenBuffer;

	private Process _pokeconProcess;

	private bool _pokeconRnnning;

	private const uint MSGFLT_ALLOW = 1u;

	private const uint WM_DROPFILES = 563u;

	private const uint WM_COPYDATA = 74u;

	private const uint WM_COPYGLOBALDATA = 73u;

	private CompletionWindow completionWindow;

	private string[][] commandlist = (from _ in new string[77][]
		{
			new string[2] { "Press", "Command" },
			new string[2] { "Hold", "Command" },
			new string[2] { "HoldRelease", "Command" },
			new string[2] { "Continue", "NArgsCommand" },
			new string[2] { "Break", "NArgsCommand" },
			new string[2] { "Count", "NArgsCommand" },
			new string[2] { "Call", "Command" },
			new string[2] { "Wait", "Command" },
			new string[2] { "Loop", "Block" },
			new string[2] { "ImgCmp", "Block" },
			new string[2] { "Snipping", "Command" },
			new string[2] { "Stop", "NArgsCommand" },
			new string[2] { "Not", "NArgsBlock" },
			new string[2] { "Notification", "Command" },
			new string[2] { "LineNotifyWithImage", "Command" },
			new string[2] { "LineNotify", "Command" },
			new string[2] { "Amiibo", "Command" },
			new string[2] { "Func", "Block" },
			new string[2] { "Exec", "Command" },
			new string[2] { "Rumble", "Block" },
			new string[2] { "A", "key" },
			new string[2] { "B", "Key" },
			new string[2] { "X", "Key" },
			new string[2] { "Y", "Key" },
			new string[2] { "L", "Key" },
			new string[2] { "R", "Key" },
			new string[2] { "ZL", "Key" },
			new string[2] { "ZR", "Key" },
			new string[2] { "START", "Key" },
			new string[2] { "SELECT", "Key" },
			new string[2] { "HOME", "Key" },
			new string[2] { "CAPTURE", "Key" },
			new string[2] { "UP", "Key" },
			new string[2] { "DOWN", "Key" },
			new string[2] { "RIGHT", "Key" },
			new string[2] { "LEFT", "Key" },
			new string[2] { "UPRIGHT", "Key" },
			new string[2] { "UPLEFT", "Key" },
			new string[2] { "DOWNRIGHT", "Key" },
			new string[2] { "DOWNLEFT", "Key" },
			new string[2] { "UP_L", "Key" },
			new string[2] { "DOWN_L", "Key" },
			new string[2] { "RIGHT_L", "Key" },
			new string[2] { "LEFT_L", "Key" },
			new string[2] { "UPRIGHT_L", "Key" },
			new string[2] { "UPLEFT_L", "Key" },
			new string[2] { "DOWNRIGHT_L", "Key" },
			new string[2] { "DOWNLEFT_L", "Key" },
			new string[2] { "UP_R", "Key" },
			new string[2] { "DOWN_R", "Key" },
			new string[2] { "RIGHT_R", "Key" },
			new string[2] { "LEFT_R", "Key" },
			new string[2] { "UPRIGHT_R", "Key" },
			new string[2] { "UPLEFT_R", "Key" },
			new string[2] { "DOWNRIGHT_R", "Key" },
			new string[2] { "DOWNLEFT_R", "Key" },
			new string[2] { "CLICK_R", "Key" },
			new string[2] { "CLICK_L", "Key" },
			new string[2] { "HIRAGANA", "Key" },
			new string[2] { "KATAKANA", "Key" },
			new string[2] { "ALPHANUMERIC", "Key" },
			new string[2] { "Keyboard", "Command" },
			new string[2] { "KeyboardMode", "Command" },
			new string[2] { "If", "Block" },
			new string[2] { "Else", "NArgsBlock" },
			new string[2] { "ElseIf", "Block" },
			new string[2] { "Var", "NArgsCommand" },
			new string[2] { "CallCsx", "Command" },
			new string[2] { "Print", "Command" },
			new string[2] { "ImgCmp720p", "Block" },
			new string[2] { "ImgCmpRect", "Block" },
			new string[2] { "ImgCmpRect720p", "Block" },
			new string[2] { "ImgCmpGray", "Block" },
			new string[2] { "ImgCmpGray720p", "Block" },
			new string[2] { "ImgCmpRectGray", "Block" },
			new string[2] { "ImgCmpRectGray720p", "Block" },
			new string[2] { "While", "Block" }
		}.ToList()
		orderby _[0].Length
		select _).ToArray();

	private MultiTextWriter mtw;

	private bool isPressCtrl;

	private int[] CRC_TABLE = new int[256]
	{
		0, 7, 14, 9, 28, 27, 18, 21, 56, 63,
		54, 49, 36, 35, 42, 45, 112, 119, 126, 121,
		108, 107, 98, 101, 72, 79, 70, 65, 84, 83,
		90, 93, 224, 231, 238, 233, 252, 251, 242, 245,
		216, 223, 214, 209, 196, 195, 202, 205, 144, 151,
		158, 153, 140, 139, 130, 133, 168, 175, 166, 161,
		180, 179, 186, 189, 199, 192, 201, 206, 219, 220,
		213, 210, 255, 248, 241, 246, 227, 228, 237, 234,
		183, 176, 185, 190, 171, 172, 165, 162, 143, 136,
		129, 134, 147, 148, 157, 154, 39, 32, 41, 46,
		59, 60, 53, 50, 31, 24, 17, 22, 3, 4,
		13, 10, 87, 80, 89, 94, 75, 76, 69, 66,
		111, 104, 97, 102, 115, 116, 125, 122, 137, 142,
		135, 128, 149, 146, 155, 156, 177, 182, 191, 184,
		173, 170, 163, 164, 249, 254, 247, 240, 229, 226,
		235, 236, 193, 198, 207, 200, 221, 218, 211, 212,
		105, 110, 103, 96, 117, 114, 123, 124, 81, 86,
		95, 88, 77, 74, 67, 68, 25, 30, 23, 16,
		5, 2, 11, 12, 33, 38, 47, 40, 61, 58,
		51, 52, 78, 73, 64, 71, 82, 85, 92, 91,
		118, 113, 120, 127, 106, 109, 100, 99, 62, 57,
		48, 55, 34, 37, 44, 43, 6, 1, 8, 15,
		26, 29, 20, 19, 174, 169, 160, 167, 178, 181,
		188, 187, 150, 145, 152, 159, 138, 141, 132, 131,
		222, 217, 208, 215, 194, 197, 204, 203, 230, 225,
		232, 239, 250, 253, 244, 243
	};

	private int _highLightLine = -1;

	private bool macroDataChanged;

	private List<string> pokeconScriptFiles = new List<string>();

	private IContainer components;

	private PictureBox CaptureScreen;

	private BackgroundWorker CaptureBGW;

	private GroupBoxEx groupBox1;

	private ButtonEx CapConnect;

	private ComboBoxEx CapDeviceList;

	private MenuStrip menuStrip1;

	private ToolStripMenuItem ファイルToolStripMenuItem;

	private ToolStripMenuItem aboutToolStripMenuItem;

	private ElementHost elementHost1;

	private System.Windows.Forms.Panel panel1;

	private System.Windows.Forms.ToolTip toolTip1;

	private ButtonEx button3;

	private ButtonEx button4;

	private ButtonEx button6;

	private ToolStripMenuItem マクロの読み込みToolStripMenuItem;

	private ToolStripMenuItem マクロの保存ToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem1;

	private ToolStripMenuItem 終了ToolStripMenuItem;

	private ToolStripMenuItem 設定ToolStripMenuItem;

	private TabControlEx tabControl1;

	private TabPage tabPage2;

	private TabPage tabPage3;

	private ToolStripMenuItem 接続ToolStripMenuItem;

	private FlowLayoutPanel flowLayoutPanel1;

	private ToolStripMenuItem BTSetUpToolStripMenuItem;

	private ToolStripMenuItem 環境設定ToolStripMenuItem1;

	private ContextMenuStrip CaptureContext;

	private ToolStripMenuItem 画面をキャプチャToolStripMenuItem;

	private ToolStripMenuItem バージョン情報ToolStripMenuItem;

	private System.Windows.Forms.Label label1;

	private System.Windows.Forms.Panel panel2;

	private System.Windows.Forms.Panel panel3;

	private ButtonEx buttonEx1;

	private ScrollBarEx vScrollBar1;

	private ScrollBarEx hScrollBar1;

	private System.Windows.Forms.Label label2;

	private KeyboardHook keyboardHook1;

	private ToolStripMenuItem 共有ToolStripMenuItem;

	private ToolStripMenuItem マクロ共有サーバーに接続ToolStripMenuItem;

	private System.Windows.Forms.Timer timer1;

	private TabPage tabPage1;

	private FlowLayoutPanel flowLayoutPanel2;

	private ToolStripMenuItem readmeToolStripMenuItem;

	private ToolStripMenuItem 全画面キャプチャToolStripMenuItem;

	private ToolStripMenuItem ヘルプToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem3;

	private ToolStripMenuItem amiiboの読み込みToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem5;

	private System.Windows.Forms.Panel panel4;

	private GhostPanel ghostPanel4;

	private Splitter splitter1;

	private TabPage tabPage4;

	private TabPage tabPageMHXX;

	private MHXXCharmRNG mhxxCharmRNG1;

	private GhostPanel ghostPanel6;

	private MouseHook mouseHook1;

	private System.Windows.Forms.Button button1;

	private System.Windows.Forms.TextBox textBox1;

	private GroupBoxEx groupBoxEx1;

	private ScrollBarEx scrollBarEx1;

	private System.Windows.Forms.Button button2;

	private ButtonEx ComConnect;

	private ComboBoxEx macroSelCmb;

	private GhostPanel ghostPanel5;

	private ButtonEx buttonEx2;

	private System.Windows.Forms.Label label3;

	private ComboBoxEx ComPortList;

	private ButtonEx buttonEx3;

	private ToolStripMenuItem マクロを上書き保存ToolStripMenuItem;

	private TabControlEx tabControlEx1;

	private TabPage tabPage5;

	private TabPage tabPage6;

	private ScrollBarEx scrollBarEx2;

	private ScrollBarEx scrollBarEx3;

	private ScrollBarEx scrollBarEx4;

	private FlowLayoutPanel flowLayoutPanel3;

	private ComboBoxEx macroSubDirCmb;

	private System.Windows.Forms.Label label4;

	private FileSystemWatcher fileSystemWatcher1;

	private FileSystemWatcher fileSystemWatcher2;

	private FileSystemWatcher fileSystemWatcher3;

	private GhostPanel ghostPanel7;

	private ButtonEx buttonEx4;

	private ToolStripMenuItem cH552SERIALセットアップToolStripMenuItem;

	private ToolStripMenuItem cH552へ書き込みToolStripMenuItem;

	private ToolStripMenuItem マクロの新規作成ToolStripMenuItem;

	private ButtonEx buttonEx5;

	private ToolStripMenuItem discordサーバーToolStripMenuItem;

	private FileSystemWatcher fileSystemWatcher4;

	private FileSystemWatcher fileSystemWatcher5;

	[DllImport("user32", SetLastError = true)]
	private static extern bool ChangeWindowMessageFilterEx(IntPtr hWnd, uint msg, uint action, IntPtr unused);

	public NXMC_VxV()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_09f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a01: Expected O, but got Unknown
		//IL_0a1b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a20: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a26: Expected O, but got Unknown
		//IL_0a67: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a6c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a77: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a82: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a83: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a88: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a93: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a9e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab9: Expected O, but got Unknown
		//IL_0abe: Expected O, but got Unknown
		//IL_0acb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ad5: Expected O, but got Unknown
		//IL_0af2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0afc: Expected O, but got Unknown
		//IL_0a45: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a4a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a50: Expected O, but got Unknown
		InitializeComponent();
		ChangeWindowMessageFilterEx(((System.Windows.Forms.Control)this).Handle, 563u, 1u, (IntPtr)0);
		ChangeWindowMessageFilterEx(((System.Windows.Forms.Control)this).Handle, 74u, 1u, (IntPtr)0);
		ChangeWindowMessageFilterEx(((System.Windows.Forms.Control)this).Handle, 73u, 1u, (IntPtr)0);
		if (((Component)this).DesignMode)
		{
			return;
		}
		try
		{
			Process process = new Process();
			process.StartInfo.FileName = "regsvr32.exe";
			process.StartInfo.Arguments = "/s \"" + Path.GetFullPath(GlobalVar.BasePath + "NX2VCam.dll") + "\"";
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.Verb = "RunAs";
			process.Start();
			process.WaitForExit();
			process.Close();
		}
		catch
		{
		}
		GlobalVar.ShareVideoRam = MemoryMappedFile.CreateOrOpen("nx_video_memory", 2765312L).CreateViewAccessor();
		for (int num = 0; num < 2765312; num++)
		{
			GlobalVar.ShareVideoRam.Write(num, 128);
		}
		Task.Factory.StartNew(delegate
		{
			try
			{
				_ = new byte[32];
				NamedPipeServerStream namedPipeServerStream = new NamedPipeServerStream("NxConPipe", PipeDirection.InOut);
				namedPipeServerStream.WaitForConnection();
				BinaryReader binaryReader = new BinaryReader(namedPipeServerStream);
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Stop();
				while (true)
				{
					try
					{
						int count = (int)binaryReader.ReadUInt32();
						string str = new string(binaryReader.ReadChars(count));
						Nmc.SendPythonSerial(str);
						stopwatch.Restart();
					}
					catch (Exception)
					{
						namedPipeServerStream.Close();
						Nmc.PythonKeyFlag = 9259542121117908992uL;
						namedPipeServerStream = new NamedPipeServerStream("NxConPipe", PipeDirection.InOut);
						namedPipeServerStream.WaitForConnection();
						binaryReader = new BinaryReader(namedPipeServerStream);
					}
				}
			}
			catch (Exception value)
			{
				Console.WriteLine(value);
			}
		});
		DiscordRpcClient = new DiscordRpcClient("785369550442201088");
		DiscordRpcClient discordRpcClient = DiscordRpcClient;
		object obj2 = _003C_003Ec._003C_003E9__36_1;
		if (obj2 == null)
		{
			OnReadyEvent val = delegate
			{
			};
			_003C_003Ec._003C_003E9__36_1 = val;
			obj2 = (object)val;
		}
		discordRpcClient.OnReady += (OnReadyEvent)obj2;
		DiscordRpcClient discordRpcClient2 = DiscordRpcClient;
		object obj3 = _003C_003Ec._003C_003E9__36_2;
		if (obj3 == null)
		{
			OnPresenceUpdateEvent val2 = delegate
			{
			};
			_003C_003Ec._003C_003E9__36_2 = val2;
			obj3 = (object)val2;
		}
		discordRpcClient2.OnPresenceUpdate += (OnPresenceUpdateEvent)obj3;
		DiscordRpcClient.Initialize();
		DiscordRpcClient.SetPresence(new RichPresence
		{
			Details = "",
			State = "",
			Assets = new Assets
			{
				LargeImageKey = "icon22222_512",
				LargeImageText = "",
				SmallImageKey = "idle",
				SmallImageText = "停止中"
			}
		});
		kbh.KeyboardHooked += new KeyboardHookedEventHandler(keyboardHook1_KeyboardHooked);
		((System.Windows.Forms.Control)(object)this).Text = GlobalVar.AppName;
		GlobalVar.MAINFORM = this;
		System.Windows.Forms.Application.AddMessageFilter(_mFilter);
		CodeEdit = new TextEditor();
		LsBox = new System.Windows.Controls.ListBox();
		elementHost1.Child = (UIElement)(object)CodeEdit;
		IHighlightingDefinition syntaxHighlighting = HighlightingLoader.Load((XmlReader)new XmlTextReader(new MemoryStream(Encoding.UTF8.GetBytes(Resources.NX))), (IHighlightingDefinitionReferenceResolver)(object)HighlightingManager.Instance);
		CodeEdit.SyntaxHighlighting = syntaxHighlighting;
		((System.Windows.Controls.Control)(object)CodeEdit).FontFamily = new System.Windows.Media.FontFamily("Consola");
		CodeEdit.Text = "//ここにマクロを記述する\r\n";
		CodeEdit.CaretOffset = CodeEdit.Text.Length;
		CodeEdit.ShowLineNumbers = true;
		CodeEdit.Options.ShowEndOfLine = true;
		CodeEdit.Options.AllowScrollBelowDocument = false;
		CodeEdit.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
		CodeEdit.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
		((UIElement)(object)CodeEdit).Drop += delegate(object sender, System.Windows.DragEventArgs args)
		{
			string[] array = (string[])args.Data.GetData(System.Windows.Forms.DataFormats.FileDrop, autoConvert: false);
			try
			{
				string text = Path.GetExtension(array[0]).ToLower();
				if (text == ".nxc" || text == ".nmc")
				{
					Nmc.NMCRead(array[0]);
					((System.Windows.Forms.Control)(object)this).Text = GlobalVar.AppName + " - " + Path.GetFileName(array[0]);
					CodeEdit.TextArea.Document.BeginUpdate();
					CodeEdit.TextArea.Document.Text = Nmc.Code;
					CodeEdit.TextArea.Document.EndUpdate();
					マクロを上書き保存ToolStripMenuItem.Enabled = true;
					flowLayoutPanel3.Enabled = true;
					flowLayoutPanel3.Visible = true;
					SetMacroDirectory(array[0]);
					ImageReload();
					DataFileReload();
				}
			}
			catch
			{
			}
		};
		((UIElement)(object)CodeEdit).LayoutUpdated += delegate
		{
			_vScrollF = true;
			vScrollBar1.Maximum = (int)Math.Max(0.0, (CodeEdit.ExtentHeight - CodeEdit.ViewportHeight) * 100.0);
			((System.Windows.Forms.Control)(object)vScrollBar1).Visible = vScrollBar1.Maximum != 0;
			((System.Windows.Forms.Control)(object)vScrollBar1).Visible = true;
			vScrollBar1.Value = (int)Math.Max(0.0, CodeEdit.VerticalOffset * 100.0);
			hScrollBar1.Maximum = (int)Math.Max(0.0, (CodeEdit.ExtentWidth - CodeEdit.ViewportWidth) * 100.0);
			((System.Windows.Forms.Control)(object)hScrollBar1).Visible = hScrollBar1.Maximum != 0;
			((System.Windows.Forms.Control)(object)hScrollBar1).Visible = true;
			hScrollBar1.Value = (int)Math.Max(0.0, CodeEdit.HorizontalOffset * 100.0);
			((System.Windows.Forms.Control)(object)vScrollBar1).Top = elementHost1.Top;
			((System.Windows.Forms.Control)(object)hScrollBar1).Left = elementHost1.Left;
			((System.Windows.Forms.Control)(object)hScrollBar1).Top = panel1.Height - ((System.Windows.Forms.Control)(object)hScrollBar1).Height;
			((System.Windows.Forms.Control)(object)vScrollBar1).Left = panel1.Width - ((System.Windows.Forms.Control)(object)vScrollBar1).Width;
			elementHost1.Height = panel1.Height - 1 - ((!((System.Windows.Forms.Control)(object)hScrollBar1).Visible) ? 1 : ((System.Windows.Forms.Control)(object)hScrollBar1).Height);
			if (!((System.Windows.Forms.Control)(object)hScrollBar1).Visible)
			{
				((System.Windows.Forms.Control)(object)vScrollBar1).Height = panel1.Height;
			}
			else
			{
				((System.Windows.Forms.Control)(object)vScrollBar1).Height = panel1.Height - ((System.Windows.Forms.Control)(object)hScrollBar1).Height;
			}
			if (!((System.Windows.Forms.Control)(object)vScrollBar1).Visible)
			{
				((System.Windows.Forms.Control)(object)hScrollBar1).Width = panel1.Width;
				label1.Visible = false;
			}
			else
			{
				((System.Windows.Forms.Control)(object)hScrollBar1).Width = panel1.Width - ((System.Windows.Forms.Control)(object)vScrollBar1).Width;
				if (((System.Windows.Forms.Control)(object)hScrollBar1).Visible)
				{
					label1.BackColor = BZStyle.NormalColor;
					label1.Visible = true;
					label1.Left = panel1.Width - ((System.Windows.Forms.Control)(object)vScrollBar1).Width;
					label1.Top = panel1.Height - ((System.Windows.Forms.Control)(object)hScrollBar1).Height;
					label1.Width = ((System.Windows.Forms.Control)(object)vScrollBar1).Width;
					label1.Height = ((System.Windows.Forms.Control)(object)hScrollBar1).Height;
				}
				else
				{
					label1.Visible = false;
				}
			}
			_vScrollF = false;
		};
		vScrollBar1.ValueChanged += delegate
		{
			if (!_vScrollF)
			{
				CodeEdit.ScrollToVerticalOffset((double)vScrollBar1.Value / 100.0);
			}
		};
		hScrollBar1.ValueChanged += delegate
		{
			if (!_vScrollF)
			{
				CodeEdit.ScrollToHorizontalOffset((double)hScrollBar1.Value / 100.0);
			}
		};
		((UIElement)(object)CodeEdit).PreviewKeyUp += delegate(object sender, System.Windows.Input.KeyEventArgs args)
		{
			if (_popUpWindow != null && (args.Key == Key.Up || args.Key == Key.Down || args.Key == Key.Return))
			{
				args.Handled = true;
			}
		};
		((UIElement)(object)CodeEdit).PreviewKeyDown += delegate(object sender, System.Windows.Input.KeyEventArgs args)
		{
			if (args.Key == Key.S && isPressCtrl)
			{
				if (マクロを上書き保存ToolStripMenuItem.Enabled)
				{
					マクロを上書き保存ToolStripMenuItem.PerformClick();
				}
				else
				{
					マクロの保存ToolStripMenuItem.PerformClick();
				}
				args.Handled = true;
			}
			if (_popUpWindow != null && (args.Key == Key.Up || args.Key == Key.Down || args.Key == Key.Return))
			{
				if (_popUpWindow.listBox1.Items.Count > 0)
				{
					if (args.Key == Key.Up && _popUpWindow.listBox1.SelectedIndex > 0)
					{
						_popUpWindow.listBox1.SelectedIndex--;
					}
					if (args.Key == Key.Down && _popUpWindow.listBox1.SelectedIndex < _popUpWindow.listBox1.Items.Count - 1)
					{
						_popUpWindow.listBox1.SelectedIndex++;
					}
					if (args.Key == Key.Return)
					{
						_popUpWindow.ReplaceLabel();
						_popUpWindow.Close();
					}
				}
				args.Handled = true;
			}
			else
			{
				if (_popUpWindow == null && args.Key == Key.Up && CodeEdit.TextArea.Caret.Line >= 2)
				{
					Caret caret = CodeEdit.TextArea.Caret;
					int line = caret.Line;
					caret.Line = line - 1;
					args.Handled = true;
				}
				if (_popUpWindow == null && args.Key == Key.Down && CodeEdit.TextArea.Caret.Line < CodeEdit.Document.LineCount)
				{
					Caret caret2 = CodeEdit.TextArea.Caret;
					int line = caret2.Line;
					caret2.Line = line + 1;
					args.Handled = true;
				}
			}
		};
		CodeEdit.TextArea.TextEntered += textEditor_TextArea_TextEntered;
		((UIElement)(object)CodeEdit).PreviewKeyUp += TextAreaOnKeyUp;
		CodeEdit.TextArea.TextEntering += TextArea_TextEntering;
		CodeEdit.MouseHover += CodeEditOnMouseHover;
		((FrameworkElement)(object)CodeEdit).Loaded += delegate
		{
			if (PresentationSource.FromVisual((Visual)(object)CodeEdit).CompositionTarget is HwndTarget hwndTarget)
			{
				hwndTarget.RenderMode = RenderMode.Default;
			}
		};
		System.Windows.Forms.ContextMenu elctm = new System.Windows.Forms.ContextMenu();
		elctm.MenuItems.Add("実行(&E)");
		elctm.MenuItems[elctm.MenuItems.Count - 1].Click += delegate
		{
			((System.Windows.Forms.Button)(object)button6).PerformClick();
		};
		elctm.MenuItems.Add("この行から実行(&S)");
		elctm.MenuItems[elctm.MenuItems.Count - 1].Click += delegate
		{
			macroStartButtonFunc(CodeEdit.TextArea.Caret.Line - 1);
		};
		elctm.MenuItems.Add("-");
		elctm.MenuItems.Add("元に戻す(&U)");
		elctm.MenuItems[elctm.MenuItems.Count - 1].Click += delegate
		{
			CodeEdit.Undo();
		};
		elctm.MenuItems.Add("やり直し(&R)");
		elctm.MenuItems[elctm.MenuItems.Count - 1].Click += delegate
		{
			CodeEdit.Redo();
		};
		elctm.MenuItems.Add("-");
		elctm.MenuItems.Add("切り取り(&T)");
		elctm.MenuItems[elctm.MenuItems.Count - 1].Click += delegate
		{
			CodeEdit.Cut();
		};
		elctm.MenuItems.Add("コピー(&C)");
		elctm.MenuItems[elctm.MenuItems.Count - 1].Click += delegate
		{
			CodeEdit.Copy();
		};
		elctm.MenuItems.Add("貼り付け(&P)");
		elctm.MenuItems[elctm.MenuItems.Count - 1].Click += delegate
		{
			CodeEdit.Paste();
		};
		elctm.MenuItems.Add("削除(&D)");
		elctm.MenuItems[elctm.MenuItems.Count - 1].Click += delegate
		{
			CodeEdit.Delete();
		};
		elctm.MenuItems.Add("-");
		elctm.MenuItems.Add("すべて選択(&A)");
		elctm.MenuItems[elctm.MenuItems.Count - 1].Click += delegate
		{
			CodeEdit.SelectAll();
		};
		elctm.MenuItems.Add("-");
		elctm.MenuItems.Add("ヘルプ(&H)");
		elctm.MenuItems[elctm.MenuItems.Count - 1].Click += delegate
		{
			ヘルプToolStripMenuItem.PerformClick();
		};
		elctm.Popup += delegate
		{
			elctm.MenuItems[3].Enabled = CodeEdit.CanUndo;
			elctm.MenuItems[4].Enabled = CodeEdit.CanRedo;
			elctm.MenuItems[0].Enabled = button4.Enabled;
			elctm.MenuItems[1].Enabled = button4.Enabled;
		};
		elementHost1.ContextMenu = elctm;
		toolTip1.SetToolTip(elementHost1, "Test");
	}

	private void TextAreaOnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
	{
		System.Drawing.Point lpPoint = new System.Drawing.Point(((System.Windows.Forms.Control)this).Left, ((System.Windows.Forms.Control)this).Top);
		GetCaretPos(out lpPoint);
		int num = IsInMat(CodeEdit.TextArea.Caret.Line, CodeEdit.TextArea.Caret.Column);
		if (num == 1)
		{
			if (_popUpWindow == null)
			{
				if (completionWindow != null)
				{
					((Window)(object)completionWindow).Close();
				}
				_popUpWindow = new Matsub(0);
				_popUpWindow.nxmc = this;
				_popUpWindow.line = CodeEdit.TextArea.Caret.Line - 1;
				_popUpWindow.Caretflg = true;
				string matImage = GetMatImage(CodeEdit.TextArea.Caret.Line);
				_popUpWindow.SelectedIndex = Nmc.ResourcesImages.Select((ResourcesImage _) => _.label).ToList().IndexOf(matImage);
				_popUpWindow.Left = lpPoint.X + elementHost1.PointToScreen(new System.Drawing.Point(0, 0)).X;
				_popUpWindow.Top = lpPoint.Y + elementHost1.PointToScreen(new System.Drawing.Point(0, 0)).Y + 15;
				_popUpWindow.StartPosition = FormStartPosition.Manual;
				_popUpWindow.Show();
			}
		}
		else if (num >= 2)
		{
			if (_popUpWindow == null)
			{
				if (completionWindow != null)
				{
					((Window)(object)completionWindow).Close();
				}
				_popUpWindow = new Matsub(num - 1);
				_popUpWindow.nxmc = this;
				_popUpWindow.line = CodeEdit.TextArea.Caret.Line - 1;
				_popUpWindow.Caretflg = true;
				string matImage2 = GetMatImage(CodeEdit.TextArea.Caret.Line);
				_popUpWindow.SelectedIndex = Nmc.ResourcesImages.Select((ResourcesImage _) => _.label).ToList().IndexOf(matImage2);
				_popUpWindow.Left = lpPoint.X + elementHost1.PointToScreen(new System.Drawing.Point(0, 0)).X;
				_popUpWindow.Top = lpPoint.Y + elementHost1.PointToScreen(new System.Drawing.Point(0, 0)).Y + 15;
				_popUpWindow.StartPosition = FormStartPosition.Manual;
				_popUpWindow.Show();
			}
		}
		else if (_popUpWindow != null)
		{
			_popUpWindow.Close();
			_popUpWindow = null;
		}
	}

	private void mouseHook1_MouseHooked(object sender, MouseHookedEventArgs e)
	{
		try
		{
			if (_popUpWindow != null && e.Message == MouseMessage.Move)
			{
				_ = _popUpWindow.Caretflg;
			}
		}
		catch (Exception)
		{
		}
	}

	private void CodeEditOnMouseHover(object sender, System.Windows.Input.MouseEventArgs e)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		TextViewPosition? positionFromPoint = CodeEdit.GetPositionFromPoint(e.GetPosition((IInputElement)CodeEdit));
		if (!positionFromPoint.HasValue)
		{
			return;
		}
		TextViewPosition value = positionFromPoint.Value;
		int line = value.Line;
		value = positionFromPoint.Value;
		int num = IsInMat(line, value.Column);
		if (num == 1)
		{
			if (_popUpWindow == null)
			{
				_popUpWindow = new Matsub(0);
				_popUpWindow.nxmc = this;
				Matsub popUpWindow = _popUpWindow;
				value = positionFromPoint.Value;
				popUpWindow.line = value.Line - 1;
				value = positionFromPoint.Value;
				string matImage = GetMatImage(value.Line);
				_popUpWindow.SelectedIndex = Nmc.ResourcesImages.Select((ResourcesImage _) => _.label).ToList().IndexOf(matImage);
				_popUpWindow.Left = System.Windows.Forms.Control.MousePosition.X - 14;
				_popUpWindow.Top = System.Windows.Forms.Control.MousePosition.Y;
				_popUpWindow.StartPosition = FormStartPosition.Manual;
				_popUpWindow.Show();
				_popUpWindow.Focus();
				e.Handled = true;
			}
		}
		else if (num >= 2 && _popUpWindow == null)
		{
			_popUpWindow = new Matsub(num - 1);
			_popUpWindow.nxmc = this;
			Matsub popUpWindow2 = _popUpWindow;
			value = positionFromPoint.Value;
			popUpWindow2.line = value.Line - 1;
			value = positionFromPoint.Value;
			GetMatImage(value.Line);
			_popUpWindow.Left = System.Windows.Forms.Control.MousePosition.X - 14;
			_popUpWindow.Top = System.Windows.Forms.Control.MousePosition.Y;
			_popUpWindow.StartPosition = FormStartPosition.Manual;
			_popUpWindow.Show();
			e.Handled = true;
		}
	}

	private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
	{
	}

	public void Closepopup()
	{
		if (_popUpWindow != null)
		{
			try
			{
				_popUpWindow.Dispose();
			}
			catch (Exception)
			{
			}
			_popUpWindow = null;
		}
	}

	public void KeyInputSet(string key, decimal time, decimal waitTime = 0m, bool plF = false)
	{
		string[] array = new string[2]
		{
			"Press(" + key + ", " + time.ToString("F2") + ")",
			""
		};
		if (waitTime != 0m)
		{
			array = new string[2]
			{
				"Press(" + key + ", " + time.ToString("F2") + ", " + waitTime.ToString("F2") + ")",
				""
			};
		}
		if (plF)
		{
			array = key.Split(new string[1] { "\r\n" }, StringSplitOptions.None);
			array.Append("");
		}
		TextArea textArea = CodeEdit.TextArea;
		string text = "";
		bool flag = false;
		for (int i = 0; i < textArea.Document.Lines[textArea.Caret.Line - 1].Length; i++)
		{
			if (textArea.Document.Text[textArea.Document.Lines[textArea.Caret.Line - 1].Offset + i].ToString() == " ")
			{
				text += " ";
			}
			if (textArea.Document.Text[textArea.Document.Lines[textArea.Caret.Line - 1].Offset + i].ToString() == "\t")
			{
				text += "\t";
			}
			if (textArea.Document.Text[textArea.Document.Lines[textArea.Caret.Line - 1].Offset + i].ToString() == "\u3000")
			{
				text += "\u3000";
			}
			if (textArea.Document.Text[textArea.Document.Lines[textArea.Caret.Line - 1].Offset + i].ToString() != "\t" && textArea.Document.Text[textArea.Document.Lines[textArea.Caret.Line - 1].Offset + i].ToString() != " " && textArea.Document.Text[textArea.Document.Lines[textArea.Caret.Line - 1].Offset + i].ToString() != "\u3000")
			{
				flag = true;
				break;
			}
		}
		string text2 = "";
		for (int j = 0; j < array.Length; j++)
		{
			if (j != 0)
			{
				text2 += text;
			}
			else if (flag)
			{
				text2 = text2 + "\r\n" + text;
			}
			text2 += array[j];
			if (j < array.Length - 1)
			{
				text2 += "\r\n";
			}
		}
		textArea.Document.Insert(textArea.Caret.Offset, text2);
	}

	[DllImport("user32.dll")]
	private static extern bool GetCaretPos(out System.Drawing.Point lpPoint);

	private void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
	{
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Expected O, but got Unknown
		if (e.Text == "{")
		{
			CodeEdit.TextArea.Document.Replace(CodeEdit.TextArea.Caret.Offset - 1, 1, "{}");
			TextEditor codeEdit = CodeEdit;
			int selectionStart = codeEdit.SelectionStart;
			codeEdit.SelectionStart = selectionStart - 1;
		}
		if (e.Text == "(")
		{
			CodeEdit.TextArea.Document.Replace(CodeEdit.TextArea.Caret.Offset - 1, 1, "()");
			TextEditor codeEdit2 = CodeEdit;
			int selectionStart = codeEdit2.SelectionStart;
			codeEdit2.SelectionStart = selectionStart - 1;
		}
		if (CodeEdit.SelectionStart >= 2 && CodeEdit.Text[CodeEdit.SelectionStart - 2] != ' ' && CodeEdit.Text[CodeEdit.SelectionStart - 2] != '\n' && CodeEdit.Text[CodeEdit.SelectionStart - 2] != '\t' && CodeEdit.Text[CodeEdit.SelectionStart - 2] != '(' && CodeEdit.Text[CodeEdit.SelectionStart - 2] != '{')
		{
			return;
		}
		completionWindow = new CompletionWindow(CodeEdit.TextArea);
		IList<ICompletionData> completionData = completionWindow.CompletionList.CompletionData;
		string text = e.Text.ToLower();
		string text2 = "";
		for (int i = 0; i < text.Length; i++)
		{
			text2 += text[i];
			text2 += ".*?";
		}
		string[][] array = commandlist;
		foreach (string[] array2 in array)
		{
			if (text2[0] == array2[0][0].ToString().ToLower()[0] && Regex.Match(array2[0].ToLower(), text2).Success)
			{
				completionData.Add((ICompletionData)(object)new CompletionData(array2));
			}
		}
		if (completionData.Count > 0)
		{
			((Window)(object)completionWindow).Show();
			((Window)(object)completionWindow).Closed += delegate
			{
				completionWindow = null;
			};
		}
	}

	private bool IsInComment(int line, int column)
	{
		object service = CodeEdit.TextArea.GetService(typeof(IHighlighter));
		IHighlighter val = (IHighlighter)((service is IHighlighter) ? service : null);
		if (val == null)
		{
			return false;
		}
		int off = CodeEdit.Document.GetOffset(line, column);
		HighlightedLine val2 = val.HighlightLine(line);
		if (val2.Sections.Count == 0)
		{
			return false;
		}
		return val2.Sections.Any((HighlightedSection s) => s.Offset <= off && s.Offset + s.Length >= off && ((object)s.Color.Foreground).ToString() == "#" + System.Drawing.Color.Green.ToArgb().ToString("X8"));
	}

	private int IsInMat(int line, int column)
	{
		DocumentLine val = CodeEdit.Document.Lines[line - 1];
		try
		{
			string[] array = CodeEdit.Document.GetText(val.Offset, column - 1).Split();
			List<string> list = new List<string>();
			string[] array2 = array;
			foreach (string text in array2)
			{
				list.AddRange(text.Split(')'));
			}
			list[list.Count - 1] = list[list.Count - 1].TrimStart();
			if (list[list.Count - 1].Substring(0, 5) == "Call(")
			{
				return 2;
			}
			if (list[list.Count - 1].Substring(0, 7) == "ImgCmp(")
			{
				return 1;
			}
			if (list[list.Count - 1].Substring(0, 8) == "CallCsx(")
			{
				return 3;
			}
			if (list[list.Count - 1].Substring(0, 11) == "ImgCmpRect(")
			{
				return 1;
			}
			if (list[list.Count - 1].Substring(0, 11) == "ImgCmpGray(")
			{
				return 1;
			}
			if (list[list.Count - 1].Substring(0, 11) == "ImgCmp720p(")
			{
				return 1;
			}
			if (list[list.Count - 1].Substring(0, 15) == "ImgCmpRect720p(")
			{
				return 1;
			}
			if (list[list.Count - 1].Substring(0, 15) == "ImgCmpRectGray(")
			{
				return 1;
			}
			if (list[list.Count - 1].Substring(0, 15) == "ImgCmpGray720p(")
			{
				return 1;
			}
			if (list[list.Count - 1].Substring(0, 19) == "ImgCmpRectGray720p(")
			{
				return 1;
			}
		}
		catch (Exception)
		{
		}
		return 0;
	}

	private string GetMatImage(int line)
	{
		DocumentLine val = CodeEdit.Document.Lines[line - 1];
		try
		{
			string[] array = CodeEdit.Document.GetText(val.Offset, val.Length).Split('(', ')');
			for (int i = 0; i < array.Length - 1; i++)
			{
				if (array[i].TrimStart() == "ImgCmp")
				{
					return array[i + 1];
				}
				if (array[i].TrimStart() == "ImgCmpRect")
				{
					return array[i + 1];
				}
				if (array[i].TrimStart() == "ImgCmpGray")
				{
					return array[i + 1];
				}
				if (array[i].TrimStart() == "ImgCmp720p")
				{
					return array[i + 1];
				}
				if (array[i].TrimStart() == "ImgCmpRect720p")
				{
					return array[i + 1];
				}
				if (array[i].TrimStart() == "ImgCmpGray720p")
				{
					return array[i + 1];
				}
				if (array[i].TrimStart() == "ImgCmpRectGray")
				{
					return array[i + 1];
				}
				if (array[i].TrimStart() == "ImgCmpRectGray720p")
				{
					return array[i + 1];
				}
				if (array[i].TrimStart() == "CallCsx")
				{
					return array[i + 1];
				}
				if (array[i].TrimStart() == "Call")
				{
					return array[i + 1];
				}
			}
		}
		catch (Exception)
		{
		}
		return "";
	}

	public void ImageReload(bool isDataOnly = false)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		flowLayoutPanel1.Controls.Clear();
		for (int i = 0; i < Nmc.ResourcesImages.Count; i++)
		{
			ImgResItem imgResItem = new ImgResItem(Nmc.ResourcesImages[i]);
			imgResItem.Width = 155;
			imgResItem.Height = 100;
			flowLayoutPanel1.Controls.Add(imgResItem);
		}
		DropIcon dropIcon = new DropIcon(isFile: false);
		dropIcon.Width = 155;
		dropIcon.Height = 100;
		flowLayoutPanel1.Controls.Add(dropIcon);
		flowLayoutPanel1.ContextMenuStrip = dropIcon.ContextMenuStrip;
		FileItemTheme(KEYCONFIG.AppConfig.APPTHEME);
		flowLayoutPanel1_Resize(null, null);
	}

	public void DataFileReload()
	{
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		flowLayoutPanel3.Controls.Clear();
		new List<string>();
		if (MacroDirectory != "" && Directory.Exists(MacroDirectory))
		{
			if (CurrentDirectory != "")
			{
				FileResItem fileResItem = new FileResItem(null);
				fileResItem.SetFolder("..\\");
				fileResItem.Width = 155;
				fileResItem.Height = 100;
				flowLayoutPanel3.Controls.Add(fileResItem);
			}
			string[] directories = Directory.GetDirectories(MacroDirectory + CurrentDirectory, "*", SearchOption.TopDirectoryOnly);
			for (int i = 0; i < directories.Length; i++)
			{
				directories[i] = Util.GetRelativePath(MacroDirectory, directories[i]).Substring(2);
				FileResItem fileResItem2 = new FileResItem(null);
				fileResItem2.SetFolder(directories[i]);
				fileResItem2.Width = 155;
				fileResItem2.Height = 100;
				flowLayoutPanel3.Controls.Add(fileResItem2);
			}
			directories = Directory.GetFiles(MacroDirectory + CurrentDirectory, "*", SearchOption.TopDirectoryOnly);
			for (int j = 0; j < directories.Length; j++)
			{
				directories[j] = Util.GetRelativePath(MacroDirectory, directories[j]).Substring(2);
				FileResItem fileResItem3 = new FileResItem(directories[j]);
				fileResItem3.Width = 155;
				fileResItem3.Height = 100;
				flowLayoutPanel3.Controls.Add(fileResItem3);
			}
		}
		DropIcon dropIcon = new DropIcon(isFile: true);
		dropIcon.Width = 155;
		dropIcon.Height = 100;
		dropIcon.isFile = true;
		flowLayoutPanel3.Controls.Add(dropIcon);
		flowLayoutPanel3.ContextMenuStrip = dropIcon.ContextMenuStrip;
		FileItemTheme(KEYCONFIG.AppConfig.APPTHEME);
	}

	public void MacroShortCutReload()
	{
		flowLayoutPanel2.Controls.Clear();
		for (int i = 0; i < GlobalVar.MacroList.Count; i++)
		{
			if (!string.IsNullOrWhiteSpace(GlobalVar.MacroList[i]))
			{
				MacroItem macroItem = new MacroItem(GlobalVar.MacroList[i]);
				macroItem.Width = 160;
				macroItem.Height = 105;
				Util.EnableDoubleBuffering(macroItem);
				flowLayoutPanel2.Controls.Add(macroItem);
			}
		}
		DropMacro dropMacro = new DropMacro();
		dropMacro.Width = 160;
		dropMacro.Height = 105;
		dropMacro.AllowDrop = true;
		Util.EnableDoubleBuffering(dropMacro);
		flowLayoutPanel2.Controls.Add(dropMacro);
		flowLayoutPanel2_Resize(null, null);
	}

	private void Press(string x = "", int y = 0)
	{
	}

	private async void Form1_Load(object sender, EventArgs e)
	{
		((System.Windows.Forms.Control)(object)macroSubDirCmb).Tag = null;
		_lastHighlight.Start();
		_lastTaskView.Start();
		if (((Component)this).DesignMode)
		{
			return;
		}
		if (!Directory.Exists(GlobalVar.BasePath + "Poke-Controller"))
		{
			File.WriteAllBytes(GlobalVar.BasePath + "Poke-Controller.zip", Resources.Poke_Controller);
			ZipFile.ExtractToDirectory(GlobalVar.BasePath + "Poke-Controller.zip", GlobalVar.BasePath ?? "");
			File.Delete(GlobalVar.BasePath + "Poke-Controller.zip");
		}
		if (!Directory.Exists(GlobalVar.BasePath + "CH552"))
		{
			File.WriteAllBytes(GlobalVar.BasePath + "CH552.zip", Resources.CH552);
			ZipFile.ExtractToDirectory(GlobalVar.BasePath + "CH552.zip", GlobalVar.BasePath ?? "");
			File.Delete(GlobalVar.BasePath + "CH552.zip");
			"https://bzl-web.com/file/sdcc/sdcc.zip".ToDownload(GlobalVar.BasePath + "CH552\\sdcc.zip");
			ZipFile.ExtractToDirectory(GlobalVar.BasePath + "CH552\\sdcc.zip", GlobalVar.BasePath + "CH552");
			File.Delete(GlobalVar.BasePath + "CH552\\sdcc.zip");
		}
		mtw = new MultiTextWriter(new ControlWriter(textBox1), Console.Out);
		Console.SetOut(mtw);
		comboBoxEx2_Enter(null, null);
		((System.Windows.Controls.Control)(object)CodeEdit).FontFamily = new System.Windows.Media.FontFamily("Consola");
		((System.Windows.Forms.Control)(object)this).ForeColor = BZStyle.TextFont;
		((System.Windows.Forms.Control)(object)this).BackColor = BZStyle.GrayColor;
		menuStrip1.Renderer = new ToolStripProfessionalRenderer((ProfessionalColorTable)new CustomColorTable());
		menuStrip1.ForeColor = BZStyle.TextFont;
		Util.EnableDoubleBuffering(CaptureScreen);
		tabControlEx1.Rank = 1;
		((System.Windows.Forms.Control)(object)groupBoxEx1).BackColor = BZStyle.BackColor;
		textBox1.BackColor = BZStyle.BackColor;
		textBox1.ForeColor = BZStyle.TextFont;
		scrollBarEx1.Maximum = 0;
		((System.Windows.Forms.Control)(object)groupBox1).Refresh();
		foreach (ToolStripMenuItem item in menuStrip1.Items)
		{
			item.ForeColor = BZStyle.TextFont;
			foreach (ToolStripItem dropDownItem in item.DropDownItems)
			{
				dropDownItem.ForeColor = BZStyle.TextFont;
			}
		}
		panel2.Location = new System.Drawing.Point(((FormEx)this).ActualLeft, ((FormEx)this).ActualTop);
		panel2.Size = new System.Drawing.Size(((FormEx)this).ActualWidth, ((FormEx)this).ActualHeight);
		panel3.BackColor = BZStyle.HighlightColor;
		((System.Windows.Forms.GroupBox)(object)groupBox1).Click += delegate
		{
			((System.Windows.Forms.Control)this).Focus();
			((ContainerControl)this).ActiveControl = null;
		};
		macroDirReload();
		fileSystemWatcher4.EnableRaisingEvents = false;
		fileSystemWatcher4.Dispose();
		fileSystemWatcher4 = new FileSystemWatcher(GlobalVar.BasePath + "Macro");
		fileSystemWatcher4.Filter = "*";
		fileSystemWatcher4.IncludeSubdirectories = false;
		fileSystemWatcher4.EnableRaisingEvents = true;
		fileSystemWatcher4.Created += delegate
		{
			macroDirReload();
		};
		fileSystemWatcher4.Deleted += delegate
		{
			macroDirReload();
		};
		fileSystemWatcher4.Renamed += delegate
		{
			macroDirReload();
		};
		_ = GlobalVar.BasePath + "config.ini";
		if (!File.Exists(GlobalVar.BasePath + "config.ini"))
		{
			Util.SaveConfig();
		}
		ReadConfig();
		GamePadInput.Start();
		NxControllerInterface.SerialPort = _serialPort;
		((System.Windows.Forms.ComboBox)(object)CapDeviceList).Items.Clear();
		DsDevice[] devicesOfCat = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
		foreach (DsDevice val in devicesOfCat)
		{
			((System.Windows.Forms.ComboBox)(object)CapDeviceList).Items.Add(val.Name);
		}
		if (((System.Windows.Forms.ComboBox)(object)CapDeviceList).Items.Count > 0)
		{
			((ListControl)(object)CapDeviceList).SelectedIndex = 0;
		}
		ImageReload();
		DataFileReload();
		MacroShortCutReload();
		GlobalVar.TaskName[0] = "準備完了";
	}

	private void ReadConfig()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Util.ReadConfig();
		SetTheme(KEYCONFIG.AppConfig.APPTHEME);
	}

	private static List<USBDeviceInfo> GetUSBDevices()
	{
		List<USBDeviceInfo> list = new List<USBDeviceInfo>();
		ManagementObjectCollection managementObjectCollection;
		using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("Select * From Win32_PnPEntity"))
		{
			managementObjectCollection = managementObjectSearcher.Get();
		}
		foreach (ManagementBaseObject item in managementObjectCollection)
		{
			if ((string)item.GetPropertyValue("Description") == "Generic Bluetooth Adapter" || (string)item.GetPropertyValue("Description") == "Generic Bluetooth Radio")
			{
				list.Add(new USBDeviceInfo((string)item.GetPropertyValue("DeviceID"), (string)item.GetPropertyValue("PNPDeviceID"), (string)item.GetPropertyValue("Description")));
			}
		}
		managementObjectCollection.Dispose();
		return list;
	}

	private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
	{
	}

	private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
	}

	private void Form1_FormClosing(object sender, FormClosingEventArgs e)
	{
		Util.SaveConfig();
		DiscordRpcClient.Dispose();
		CaptureBGW.CancelAsync();
		while (CaptureBGW.IsBusy)
		{
			System.Windows.Forms.Application.DoEvents();
		}
		if (NxControllerInterface.StartedBluetooth)
		{
			NxControllerInterface.ShutdownGamepad();
		}
		if (_serialPort.IsOpen)
		{
			_serialPort.Close();
		}
		try
		{
			Process process = new Process();
			process.StartInfo.FileName = "regsvr32.exe";
			process.StartInfo.Arguments = "/s /u \"" + Path.GetFullPath(GlobalVar.BasePath + "NX2VCam.dll") + "\"";
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.Verb = "RunAs";
			process.Start();
			process.WaitForExit();
			process.Close();
		}
		catch
		{
		}
	}

	private unsafe void CapConnect_Click(object sender, EventArgs e)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Invalid comparison between Unknown and I4
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Expected O, but got Unknown
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Invalid comparison between Unknown and I4
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Expected O, but got Unknown
		int capIndex = ((ListControl)(object)CapDeviceList).SelectedIndex;
		if (DsCapture != null)
		{
			DsCapture.Stop();
			DsCapture.Dispose();
			DsCapture = null;
			if ((int)KEYCONFIG.AppConfig.APPTHEME == 1)
			{
				CapConnect.Image = Resources.B3;
			}
			else
			{
				CapConnect.Image = Resources.B3_L;
			}
			((System.Windows.Forms.Control)(object)CapDeviceList).Enabled = true;
			CurrentCaptureFormat = CaptureStyle.None;
			GlobalVar.TaskName[1] = "";
			GlobalVar.MAINFORM.TaskView();
			return;
		}
		if (CvCapture != null)
		{
			((DisposableObject)CvCapture).Dispose();
			CvCapture = null;
			if ((int)KEYCONFIG.AppConfig.APPTHEME == 1)
			{
				CapConnect.Image = Resources.B3;
			}
			else
			{
				CapConnect.Image = Resources.B3_L;
			}
			((System.Windows.Forms.Control)(object)CapDeviceList).Enabled = true;
			CurrentCaptureFormat = CaptureStyle.None;
			GlobalVar.TaskName[1] = "";
			GlobalVar.MAINFORM.TaskView();
			return;
		}
		GlobalVar.TaskName[1] = $"映像デバイス({((System.Windows.Forms.ComboBox)(object)CapDeviceList).Items[capIndex]}) : 接続試行中";
		GlobalVar.MAINFORM.TaskView();
		CapConnect.Image = Resources.B3_LINK;
		((System.Windows.Forms.Control)(object)CapDeviceList).Enabled = false;
		Mat _frame = new Mat();
		if (KEYCONFIG.AppConfig.CAPTURESTYLE == CaptureStyle.DirectShow)
		{
			DsCapture = new DSHDMICapture(capIndex, CaptureScreen.Handle);
			DsCapture.renderingSize = CaptureScreen.ClientSize;
			DsCapture.Play();
			CurrentCaptureFormat = CaptureStyle.DirectShow;
		}
		else
		{
			CvCapture = new VideoCapture();
			CvCapture.Open(capIndex);
			CvCapture.Set((CaptureProperty)3, 1920.0);
			CvCapture.Set((CaptureProperty)4, 1080.0);
			CvCapture.Set((CaptureProperty)5, 29.97);
			CurrentCaptureFormat = CaptureStyle.OpenCV;
		}
		Bitmap image = new Bitmap(1920, 1080);
		CaptureScreen.Image = image;
		Task.Factory.StartNew(delegate
		{
			GlobalVar.TaskName[1] = $"映像デバイス({((System.Windows.Forms.ComboBox)(object)CapDeviceList).Items[capIndex]}) : 接続中";
			GlobalVar.MAINFORM.TaskView();
			if (captureRun)
			{
				captureRun = false;
				return;
			}
			captureRun = true;
			new Stopwatch().Stop();
			byte[] array = new byte[2764800];
			Task.Factory.StartNew(delegate
			{
				//IL_003f: Unknown result type (might be due to invalid IL or missing references)
				Bitmap bitmap3 = new Bitmap(1920, 1080);
				while (true)
				{
					try
					{
						if (CurrentCaptureFormat == CaptureStyle.OpenCV)
						{
							CvCapture.Read(_frame);
							if (_frame.Size().Width > 0)
							{
								bitmap3 = BitmapConverter.ToBitmap(_frame);
								double num6 = 0.0;
								Bitmap bitmap4;
								lock (NxCommand.lockObject)
								{
									NxCommand.CurrentFrame = bitmap3;
									int width = CaptureScreen.Width;
									int height = CaptureScreen.Height;
									double val = (double)width / (double)bitmap3.Width;
									double val2 = (double)height / (double)bitmap3.Height;
									num6 = Math.Min(val, val2);
									int width2 = (int)((double)bitmap3.Width * num6);
									int height2 = (int)((double)bitmap3.Height * num6);
									bitmap4 = (Bitmap)bitmap3.ImageResize(width2, height2);
									if (_captureNow)
									{
										int num7 = (int)((double)Math.Min(NxSel.X1, NxSel.X2) * num6);
										int num8 = (int)((double)Math.Max(NxSel.X1, NxSel.X2) * num6);
										int num9 = (int)((double)Math.Min(NxSel.Y1, NxSel.Y2) * num6);
										int num10 = (int)((double)Math.Max(NxSel.Y1, NxSel.Y2) * num6);
										Bitmap bitmap5 = Util.AdjustBrightness(bitmap4, -30);
										if (Math.Min(NxSel.Y1, NxSel.Y2) != -1)
										{
											Graphics graphics = Graphics.FromImage(bitmap5);
											graphics.DrawImage(bitmap4, num7, num9, new Rectangle(num7, num9, num8 - num7, num10 - num9), GraphicsUnit.Pixel);
											graphics.Dispose();
										}
										bitmap4 = bitmap5;
									}
								}
								captureScreenBuffer = bitmap4;
								((System.Windows.Forms.Control)this).Invoke((Delegate)(Action)delegate
								{
									try
									{
										CaptureScreen.Invalidate();
									}
									catch (Exception)
									{
									}
								});
							}
							Thread.Sleep(30);
						}
						else
						{
							Thread.Sleep(1000);
						}
					}
					catch
					{
						Thread.Sleep(1000);
					}
				}
			});
			while (true)
			{
				if (!captureRun)
				{
					captureRun = true;
				}
				try
				{
					_ = CurrentCaptureFormat;
					_ = 2;
					Bitmap bitmap = CaptureImage();
					if (bitmap != null)
					{
						Bitmap bitmap2 = (Bitmap)bitmap.ImageResize(1280, 720);
						BitmapData bitmapData = bitmap2.LockBits(new Rectangle(0, 0, bitmap2.Width, bitmap2.Height), ImageLockMode.ReadWrite, bitmap2.PixelFormat);
						byte* ptr = (byte*)(void*)bitmapData.Scan0;
						if (ptr != null)
						{
							GlobalVar.ShareVideoRam = MemoryMappedFile.CreateOrOpen("nx_video_memory", 2765312L).CreateViewAccessor();
							fixed (byte* ptr2 = array)
							{
								_ = bitmap2.Height;
								_ = bitmap2.Width;
								for (int num = 719; num >= 0; num--)
								{
									int num2 = 720 - num - 1;
									int num3 = num * 3840;
									int num4 = num2 * 3840;
									for (int num5 = 0; num5 < 3840; num5++)
									{
										ptr2[num3 + num5] = ptr[num4 + num5];
									}
								}
							}
							GlobalVar.ShareVideoRam.WriteArray(0L, array, 0, array.Length);
						}
						bitmap2.UnlockBits(bitmapData);
					}
					Thread.Sleep(8);
					GC.Collect(0);
					GC.WaitForPendingFinalizers();
					GC.Collect(1);
					GC.WaitForPendingFinalizers();
					GC.Collect(2);
					GC.WaitForPendingFinalizers();
				}
				catch
				{
					Thread.Sleep(100);
				}
			}
		});
	}

	private void button1_Click(object sender, EventArgs e)
	{
	}

	private void button3_Click(object sender, EventArgs e)
	{
		if (!InputDialog.Opening)
		{
			InputDialog.Opening = true;
			new InputDialog().Show();
		}
	}

	private void pictureBox1_Click(object sender, EventArgs e)
	{
	}

	private void button1_Click_1(object sender, EventArgs e)
	{
	}

	private void 終了ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		((Form)this).Close();
	}

	private void マクロの読み込みToolStripMenuItem_Click(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "NX Macro Controller用マクロファイル(*.nxc;*.nmc)|*.nxc;*.nmc|すべてのファイル(*.*)|*.*";
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			LoadMacro(openFileDialog.FileName);
		}
	}

	private void LoadMacro(string path)
	{
		Nmc.NMCRead(path);
		((System.Windows.Forms.Control)(object)this).Text = GlobalVar.AppName + " - " + Path.GetFileName(path);
		CodeEdit.TextArea.Document.BeginUpdate();
		CodeEdit.TextArea.Document.Text = Nmc.Code;
		CodeEdit.TextArea.Document.EndUpdate();
		マクロを上書き保存ToolStripMenuItem.Enabled = true;
		flowLayoutPanel3.Enabled = true;
		flowLayoutPanel3.Visible = true;
		SetMacroDirectory(path);
		ImageReload();
		DataFileReload();
	}

	public void SetMacroDirectory(string path)
	{
		CurrentDirectory = "";
		MacroDirectory = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + "\\";
		if (Directory.Exists(MacroDirectory))
		{
			FSWReload();
		}
	}

	public void FSWReload()
	{
		fileSystemWatcher1.EnableRaisingEvents = false;
		fileSystemWatcher1.Dispose();
		fileSystemWatcher1 = new FileSystemWatcher(MacroDirectory + CurrentDirectory);
		fileSystemWatcher1.Filter = "*";
		fileSystemWatcher1.IncludeSubdirectories = false;
		fileSystemWatcher1.EnableRaisingEvents = true;
		fileSystemWatcher1.Created += fileSystemWatcher1_Created;
		fileSystemWatcher1.Deleted += fileSystemWatcher1_Deleted;
		fileSystemWatcher1.Renamed += fileSystemWatcher1_Renamed;
		fileSystemWatcher2.EnableRaisingEvents = false;
		fileSystemWatcher2.Dispose();
		fileSystemWatcher2 = new FileSystemWatcher(Path.GetDirectoryName(Path.GetDirectoryName(MacroDirectory)));
		fileSystemWatcher2.Filter = "*";
		fileSystemWatcher2.IncludeSubdirectories = false;
		fileSystemWatcher2.EnableRaisingEvents = true;
		fileSystemWatcher2.Deleted += fileSystemWatcher2_Deleted;
		fileSystemWatcher2.Renamed += fileSystemWatcher2_Renamed;
		fileSystemWatcher3.EnableRaisingEvents = false;
		fileSystemWatcher3.Dispose();
		fileSystemWatcher3 = new FileSystemWatcher(Path.GetDirectoryName(Path.GetDirectoryName(MacroDirectory + CurrentDirectory)));
		fileSystemWatcher3.Filter = "*";
		fileSystemWatcher3.IncludeSubdirectories = false;
		fileSystemWatcher3.EnableRaisingEvents = true;
		fileSystemWatcher3.Deleted += fileSystemWatcher3_Deleted;
		fileSystemWatcher3.Renamed += fileSystemWatcher3_Renamed;
	}

	private void リソース管理ToolStripMenuItem_Click(object sender, EventArgs e)
	{
	}

	private void button4_Click(object sender, EventArgs e)
	{
		if (KeyRecoding)
		{
			KeyRecoding = false;
			return;
		}
		((System.Windows.Forms.Control)(object)button4).Text = "停止";
		KeyRecoding = true;
		button6.Enabled = false;
		button4.Image = Resources.B4;
		buttonEx2.Enabled = false;
		マクロの読み込みToolStripMenuItem.Enabled = false;
		cH552へ書き込みToolStripMenuItem.Enabled = false;
		cH552SERIALセットアップToolStripMenuItem.Enabled = false;
		Task.Factory.StartNew(delegate
		{
			Stopwatch stopwatch = new Stopwatch();
			ulong num = 9259542121117908992uL;
			stopwatch.Start();
			while (KeyRecoding)
			{
				ulong padAndKeyboardFlag = Nmc.GetPadAndKeyboardFlag();
				if (padAndKeyboardFlag != num)
				{
					long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
					stopwatch.Restart();
					if (elapsedMilliseconds != 0L)
					{
						string text = ((decimal)elapsedMilliseconds / 1000m).ToString("F2");
						string[] keyList = NMC.GetKeyList(num);
						if (keyList.Length == 0)
						{
							string wait = "Wait(" + text + ")";
							((System.Windows.Forms.Control)this).Invoke((Delegate)(Action)delegate
							{
								KeyInputSet(wait, 0m, 0m, plF: true);
							});
						}
						else
						{
							string press = "Press(" + string.Join(", ", keyList) + ", " + text + ")";
							((System.Windows.Forms.Control)this).Invoke((Delegate)(Action)delegate
							{
								KeyInputSet(press, 0m, 0m, plF: true);
							});
						}
					}
					num = padAndKeyboardFlag;
				}
				Thread.Sleep(1);
			}
			((System.Windows.Forms.Control)this).Invoke((Delegate)(Action)delegate
			{
				((System.Windows.Forms.Control)(object)button4).Text = "記録";
				button6.Enabled = true;
				buttonEx2.Enabled = true;
				マクロの読み込みToolStripMenuItem.Enabled = true;
				cH552へ書き込みToolStripMenuItem.Enabled = true;
				cH552SERIALセットアップToolStripMenuItem.Enabled = true;
				button4.Image = Resources.B2;
			});
		});
	}

	private void 環境設定ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Invalid comparison between Unknown and I4
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Invalid comparison between Unknown and I4
		CaptureStyle cAPTURESTYLE = KEYCONFIG.AppConfig.CAPTURESTYLE;
		SettingDialog settingDialog = new SettingDialog();
		settingDialog.StartPosition = FormStartPosition.CenterParent;
		settingDialog.ShowDialog();
		if (!settingDialog.SettingChanged)
		{
			return;
		}
		ReadConfig();
		GlobalVar.TaskName[0] = "設定が変更されました";
		GlobalVar.MAINFORM.TaskView();
		if (cAPTURESTYLE == KEYCONFIG.AppConfig.CAPTURESTYLE || ((System.Windows.Forms.Control)(object)CapDeviceList).Enabled)
		{
			return;
		}
		if (DsCapture != null)
		{
			DsCapture.Stop();
			DsCapture.Dispose();
			DsCapture = null;
			if ((int)KEYCONFIG.AppConfig.APPTHEME == 1)
			{
				CapConnect.Image = Resources.B3;
			}
			else
			{
				CapConnect.Image = Resources.B3_L;
			}
			CurrentCaptureFormat = CaptureStyle.None;
		}
		else if (CvCapture != null)
		{
			((DisposableObject)CvCapture).Dispose();
			CvCapture = null;
			if ((int)KEYCONFIG.AppConfig.APPTHEME == 1)
			{
				CapConnect.Image = Resources.B3;
			}
			else
			{
				CapConnect.Image = Resources.B3_L;
			}
			CurrentCaptureFormat = CaptureStyle.None;
		}
		((System.Windows.Forms.Button)(object)CapConnect).PerformClick();
	}

	public void MacroActive()
	{
		((System.Windows.Forms.Control)this).Invoke((Delegate)(Action)delegate
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Expected O, but got Unknown
			//IL_0068: Expected O, but got Unknown
			DiscordRpcClient.SetPresence(new RichPresence
			{
				Details = "",
				State = "",
				Timestamps = Timestamps.Now,
				Assets = new Assets
				{
					LargeImageKey = "icon22222_512",
					LargeImageText = "",
					SmallImageKey = "online",
					SmallImageText = "動作中"
				}
			});
			panel3.BackColor = System.Drawing.Color.FromArgb(202, 81, 0);
			base.BorderColor = System.Drawing.Color.FromArgb(202, 81, 0);
			((System.Windows.Forms.Control)(object)this).Refresh();
		});
	}

	public void MacroDeactive()
	{
		if (!Nmc.Running)
		{
			((System.Windows.Forms.Control)this).Invoke((Delegate)(Action)delegate
			{
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				//IL_0026: Unknown result type (might be due to invalid IL or missing references)
				//IL_0031: Unknown result type (might be due to invalid IL or missing references)
				//IL_003c: Unknown result type (might be due to invalid IL or missing references)
				//IL_003d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0042: Unknown result type (might be due to invalid IL or missing references)
				//IL_004d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0058: Unknown result type (might be due to invalid IL or missing references)
				//IL_0063: Unknown result type (might be due to invalid IL or missing references)
				//IL_0073: Expected O, but got Unknown
				//IL_0078: Expected O, but got Unknown
				panel3.BackColor = BZStyle.HighlightColor;
				base.BorderColor = BZStyle.HighlightColor;
				DiscordRpcClient.SetPresence(new RichPresence
				{
					Details = "",
					State = "",
					Assets = new Assets
					{
						LargeImageKey = "icon22222_512",
						LargeImageText = "",
						SmallImageKey = "idle",
						SmallImageText = "停止中"
					}
				});
				((System.Windows.Forms.Control)(object)this).Refresh();
			});
		}
	}

	private void button6_Click(object sender, EventArgs e)
	{
		macroStartButtonFunc();
	}

	private void macroStartButtonFunc(int startPos = 0)
	{
		if (Nmc.SubRunningNmc != "")
		{
			Nmc.Cancel = true;
			while (Nmc.Running)
			{
				System.Windows.Forms.Application.DoEvents();
				if (GlobalVar.MAINFORM.Nmc.RunningCsx || NxCommand.ExitFlag)
				{
					if (GlobalVar.NmcThread != null)
					{
						GlobalVar.NmcThread.Abort();
						Nmc.NmcEndSec();
					}
					return;
				}
			}
		}
		if (Nmc.Running)
		{
			Nmc.Cancel = true;
			while (Nmc.Running)
			{
				System.Windows.Forms.Application.DoEvents();
				if (GlobalVar.MAINFORM.Nmc.RunningCsx || NxCommand.ExitFlag)
				{
					if (GlobalVar.NmcThread != null)
					{
						GlobalVar.NmcThread.Abort();
						Nmc.NmcEndSec();
					}
					break;
				}
			}
			return;
		}
		ButtonEx obj = button6;
		string text = (((System.Windows.Forms.Control)(object)buttonEx5).Text = "停止");
		((System.Windows.Forms.Control)(object)obj).Text = text;
		ButtonEx obj2 = button6;
		System.Drawing.Image image = (buttonEx5.Image = Resources.B4);
		obj2.Image = image;
		button4.Enabled = false;
		buttonEx2.Enabled = false;
		cH552へ書き込みToolStripMenuItem.Enabled = false;
		cH552SERIALセットアップToolStripMenuItem.Enabled = false;
		マクロの読み込みToolStripMenuItem.Enabled = false;
		Nmc.Code = CodeEdit.Text;
		MacroActive();
		CodeEdit.IsReadOnly = true;
		Nmc.IsMain = true;
		Nmc.NmcExecution(startPos);
		Task.Factory.StartNew(delegate
		{
			while (Nmc.Running)
			{
				Thread.Sleep(16);
				if (NxCommand.ExitFlag && GlobalVar.NmcThread != null)
				{
					GlobalVar.NmcThread.Abort();
					Nmc.NmcEndSec();
				}
			}
			((System.Windows.Forms.Control)this).Invoke((Delegate)(Action)delegate
			{
				//IL_008d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0093: Invalid comparison between Unknown and I4
				ButtonEx obj3 = button6;
				string text3 = (((System.Windows.Forms.Control)(object)buttonEx5).Text = "実行");
				((System.Windows.Forms.Control)(object)obj3).Text = text3;
				CodeEdit.IsReadOnly = false;
				button4.Enabled = true;
				buttonEx2.Enabled = true;
				cH552へ書き込みToolStripMenuItem.Enabled = true;
				cH552SERIALセットアップToolStripMenuItem.Enabled = true;
				マクロの読み込みToolStripMenuItem.Enabled = true;
				Environment.CurrentDirectory = GlobalVar.BasePath;
				if (_amiibo != "")
				{
					NxControllerInterface.SendAmiibo(_amiibo);
				}
				if ((int)KEYCONFIG.AppConfig.APPTHEME == 1)
				{
					ButtonEx obj4 = button6;
					System.Drawing.Image image2 = (buttonEx5.Image = Resources.B1);
					obj4.Image = image2;
				}
				else
				{
					ButtonEx obj5 = button6;
					System.Drawing.Image image2 = (buttonEx5.Image = Resources.B1_L);
					obj5.Image = image2;
				}
				if (Nmc.SubRunningNmc == "")
				{
					MacroDeactive();
				}
			});
		});
	}

	private void keyboardHook1_KeyboardHooked(object sender, KeyboardHookedEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Invalid comparison between Unknown and I4
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Invalid comparison between Unknown and I4
		if ((int)e.UpDown == 0)
		{
			if (e.KeyCode == Keys.RControlKey || e.KeyCode == Keys.LControlKey)
			{
				isPressCtrl = true;
			}
		}
		else if ((int)e.UpDown == 1 && (e.KeyCode == Keys.RControlKey || e.KeyCode == Keys.LControlKey))
		{
			isPressCtrl = false;
		}
		if (!KEYCONFIG.ControlConfig.USEKEYBOARD || (KEYCONFIG.ControlConfig.NOTUSEDEACTIVATE && (object)Form.ActiveForm != this) || (KEYCONFIG.ControlConfig.NOTUSERUNNINGMACRO && Nmc.Running) || (KEYCONFIG.ControlConfig.GAMEPADONLY && GamePadInput.Connected) || ((ContainerControl)this).ActiveControl == elementHost1)
		{
			Nmc.KeyBoardKeyFlag = 9259542121117908992uL;
			return;
		}
		ulong num = Nmc.KeyBoardKeyFlag;
		if ((int)e.UpDown == 0)
		{
			if (KEYCONFIG.Button.A == e.KeyCode)
			{
				num |= 8;
			}
			if (KEYCONFIG.Button.B == e.KeyCode)
			{
				num |= 4;
			}
			if (KEYCONFIG.Button.X == e.KeyCode)
			{
				num |= 2;
			}
			if (KEYCONFIG.Button.Y == e.KeyCode)
			{
				num |= 1;
			}
			if (KEYCONFIG.Button.ZL == e.KeyCode)
			{
				num |= 0x800000;
			}
			if (KEYCONFIG.Button.ZR == e.KeyCode)
			{
				num |= 0x80;
			}
			if (KEYCONFIG.Button.L == e.KeyCode)
			{
				num |= 0x400000;
			}
			if (KEYCONFIG.Button.R == e.KeyCode)
			{
				num |= 0x40;
			}
			if (KEYCONFIG.DPad.UP == e.KeyCode)
			{
				num |= 0x20000;
			}
			if (KEYCONFIG.DPad.RIGHT == e.KeyCode)
			{
				num |= 0x40000;
			}
			if (KEYCONFIG.DPad.LEFT == e.KeyCode)
			{
				num |= 0x80000;
			}
			if (KEYCONFIG.DPad.DOWN == e.KeyCode)
			{
				num |= 0x10000;
			}
			if (KEYCONFIG.Button.START == e.KeyCode)
			{
				num |= 0x200;
			}
			if (KEYCONFIG.Button.SELECT == e.KeyCode)
			{
				num |= 0x100;
			}
			if (KEYCONFIG.Button.HOME == e.KeyCode)
			{
				num |= 0x1000;
			}
			if (KEYCONFIG.Button.CAPTURE == e.KeyCode)
			{
				num |= 0x2000;
			}
			if (KEYCONFIG.Button.CLICKL == e.KeyCode)
			{
				num |= 0x800;
			}
			if (KEYCONFIG.Button.CLICKR == e.KeyCode)
			{
				num |= 0x400;
			}
			if (KEYCONFIG.AnalogL.UP == e.KeyCode)
			{
				num &= 0xFFFFFF00FFFFFFFFuL;
				num |= 0;
			}
			if (KEYCONFIG.AnalogL.DOWN == e.KeyCode)
			{
				num &= 0xFFFFFF00FFFFFFFFuL;
				num |= 0xFF00000000L;
			}
			if (KEYCONFIG.AnalogL.LEFT == e.KeyCode)
			{
				num &= 0xFFFF00FFFFFFFFFFuL;
				num |= 0;
			}
			if (KEYCONFIG.AnalogL.RIGHT == e.KeyCode)
			{
				num &= 0xFFFF00FFFFFFFFFFuL;
				num |= 0xFF0000000000L;
			}
			if (KEYCONFIG.AnalogR.UP == e.KeyCode)
			{
				num &= 0xFF00FFFFFFFFFFFFuL;
				num |= 0;
			}
			if (KEYCONFIG.AnalogR.DOWN == e.KeyCode)
			{
				num &= 0xFF00FFFFFFFFFFFFuL;
				num |= 0xFF000000000000L;
			}
			if (KEYCONFIG.AnalogR.LEFT == e.KeyCode)
			{
				num &= 0xFFFFFFFFFFFFFFL;
				num |= 0;
			}
			if (KEYCONFIG.AnalogR.RIGHT == e.KeyCode)
			{
				num &= 0xFFFFFFFFFFFFFFL;
				num |= 0xFF00000000000000uL;
			}
		}
		if ((int)e.UpDown == 1)
		{
			if (KEYCONFIG.Button.A == e.KeyCode)
			{
				num &= 0xFFFFFFFFFFFFFFF7uL;
			}
			if (KEYCONFIG.Button.B == e.KeyCode)
			{
				num &= 0xFFFFFFFFFFFFFFFBuL;
			}
			if (KEYCONFIG.Button.X == e.KeyCode)
			{
				num &= 0xFFFFFFFFFFFFFFFDuL;
			}
			if (KEYCONFIG.Button.Y == e.KeyCode)
			{
				num &= 0xFFFFFFFFFFFFFFFEuL;
			}
			if (KEYCONFIG.Button.ZL == e.KeyCode)
			{
				num &= 0xFFFFFFFFFF7FFFFFuL;
			}
			if (KEYCONFIG.Button.ZR == e.KeyCode)
			{
				num &= 0xFFFFFFFFFFFFFF7FuL;
			}
			if (KEYCONFIG.Button.L == e.KeyCode)
			{
				num &= 0xFFFFFFFFFFBFFFFFuL;
			}
			if (KEYCONFIG.Button.R == e.KeyCode)
			{
				num &= 0xFFFFFFFFFFFFFFBFuL;
			}
			if (KEYCONFIG.DPad.UP == e.KeyCode)
			{
				num &= 0xFFFFFFFFFFFDFFFFuL;
			}
			if (KEYCONFIG.DPad.RIGHT == e.KeyCode)
			{
				num &= 0xFFFFFFFFFFFBFFFFuL;
			}
			if (KEYCONFIG.DPad.LEFT == e.KeyCode)
			{
				num &= 0xFFFFFFFFFFF7FFFFuL;
			}
			if (KEYCONFIG.DPad.DOWN == e.KeyCode)
			{
				num &= 0xFFFFFFFFFFFEFFFFuL;
			}
			if (KEYCONFIG.Button.START == e.KeyCode)
			{
				num &= 0xFFFFFFFFFFFFFDFFuL;
			}
			if (KEYCONFIG.Button.SELECT == e.KeyCode)
			{
				num &= 0xFFFFFFFFFFFFFEFFuL;
			}
			if (KEYCONFIG.Button.HOME == e.KeyCode)
			{
				num &= 0xFFFFFFFFFFFFEFFFuL;
			}
			if (KEYCONFIG.Button.CAPTURE == e.KeyCode)
			{
				num &= 0xFFFFFFFFFFFFDFFFuL;
			}
			if (KEYCONFIG.Button.CLICKL == e.KeyCode)
			{
				num &= 0xFFFFFFFFFFFFF7FFuL;
			}
			if (KEYCONFIG.Button.CLICKR == e.KeyCode)
			{
				num &= 0xFFFFFFFFFFFFFBFFuL;
			}
			if (KEYCONFIG.AnalogL.UP == e.KeyCode)
			{
				num &= 0xFFFFFF00FFFFFFFFuL;
				num |= 0x8000000000L;
			}
			if (KEYCONFIG.AnalogL.DOWN == e.KeyCode)
			{
				num &= 0xFFFFFF00FFFFFFFFuL;
				num |= 0x8000000000L;
			}
			if (KEYCONFIG.AnalogL.LEFT == e.KeyCode)
			{
				num &= 0xFFFF00FFFFFFFFFFuL;
				num |= 0x800000000000L;
			}
			if (KEYCONFIG.AnalogL.RIGHT == e.KeyCode)
			{
				num &= 0xFFFF00FFFFFFFFFFuL;
				num |= 0x800000000000L;
			}
			if (KEYCONFIG.AnalogR.UP == e.KeyCode)
			{
				num &= 0xFF00FFFFFFFFFFFFuL;
				num |= 0x80000000000000L;
			}
			if (KEYCONFIG.AnalogR.DOWN == e.KeyCode)
			{
				num &= 0xFF00FFFFFFFFFFFFuL;
				num |= 0x80000000000000L;
			}
			if (KEYCONFIG.AnalogR.LEFT == e.KeyCode)
			{
				num &= 0xFFFFFFFFFFFFFFL;
				num |= 0x8000000000000000uL;
			}
			if (KEYCONFIG.AnalogR.RIGHT == e.KeyCode)
			{
				num &= 0xFFFFFFFFFFFFFFL;
				num |= 0x8000000000000000uL;
			}
		}
		Nmc.KeyBoardKeyFlag = num;
	}

	private void mouseHook1_MouseHooked_1(object sender, MouseHookedEventArgs e)
	{
	}

	private void 接続ToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
	{
	}

	private void 設定ToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
	{
		接続ToolStripMenuItem.DropDownItems.Clear();
		接続ToolStripMenuItem.Text = "接続";
		if (_selectedPort != "")
		{
			接続ToolStripMenuItem.Text = "接続(" + _selectedPort + ")";
		}
		string[] portNames = SerialPort.GetPortNames();
		foreach (string text in portNames)
		{
			接続ToolStripMenuItem.DropDownItems.Add(text);
			接続ToolStripMenuItem.DropDownItems[接続ToolStripMenuItem.DropDownItems.Count - 1].ForeColor = BZStyle.TextFont;
			接続ToolStripMenuItem.DropDownItems[接続ToolStripMenuItem.DropDownItems.Count - 1].Name = text;
			if (_selectedPort == text)
			{
				((ToolStripMenuItem)接続ToolStripMenuItem.DropDownItems[接続ToolStripMenuItem.DropDownItems.Count - 1]).Checked = true;
			}
			接続ToolStripMenuItem.DropDownItems[接続ToolStripMenuItem.DropDownItems.Count - 1].Click += delegate(object o, EventArgs args)
			{
				//IL_0072: Unknown result type (might be due to invalid IL or missing references)
				//IL_007c: Expected O, but got Unknown
				//IL_00da: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e4: Expected O, but got Unknown
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				//IL_0027: Invalid comparison between Unknown and I4
				if (NxControllerInterface.StartedBluetooth)
				{
					NxControllerInterface.StartedBluetooth = false;
					NxControllerInterface.ShutdownGamepad();
				}
				ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)o;
				if (toolStripMenuItem.Checked)
				{
					if ((int)KEYCONFIG.AppConfig.APPTHEME == 1)
					{
						ComConnect.Image = Resources.B5;
					}
					else
					{
						ComConnect.Image = Resources.B5_L;
					}
					_selectedPort = "";
					try
					{
						if (_serialPort.IsOpen)
						{
							_serialPort.Close();
						}
						return;
					}
					catch (Exception)
					{
						_serialPort = new SerialPortStream();
						NxControllerInterface.SerialPort = _serialPort;
						return;
					}
				}
				toolStripMenuItem.Checked = true;
				ComConnect.Image = Resources.B5_LINK;
				_selectedPort = toolStripMenuItem.Name;
				((System.Windows.Forms.Control)(object)ComPortList).Text = _selectedPort;
				try
				{
					if (_serialPort.IsOpen)
					{
						_serialPort.Close();
					}
				}
				catch (Exception)
				{
					_serialPort = new SerialPortStream();
					NxControllerInterface.SerialPort = _serialPort;
				}
				try
				{
					NxControllerInterface.OpenSerial(toolStripMenuItem.Name);
				}
				catch (Exception)
				{
				}
			};
		}
		接続ToolStripMenuItem.DropDownItems.Add("-");
		接続ToolStripMenuItem.DropDownItems.Add("Bluetooth無線接続");
		接続ToolStripMenuItem.DropDownItems[接続ToolStripMenuItem.DropDownItems.Count - 1].ForeColor = BZStyle.TextFont;
		接続ToolStripMenuItem.DropDownItems[接続ToolStripMenuItem.DropDownItems.Count - 1].Name = "Bluetooth";
		if (_selectedPort == "Bluetooth")
		{
			((ToolStripMenuItem)接続ToolStripMenuItem.DropDownItems[接続ToolStripMenuItem.DropDownItems.Count - 1]).Checked = true;
		}
		接続ToolStripMenuItem.DropDownItems[接続ToolStripMenuItem.DropDownItems.Count - 1].Click += delegate(object o, EventArgs args)
		{
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Expected O, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Invalid comparison between Unknown and I4
			if (NxControllerInterface.StartedBluetooth)
			{
				NxControllerInterface.StartedBluetooth = false;
				NxControllerInterface.ShutdownGamepad();
			}
			ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)o;
			if (toolStripMenuItem.Checked)
			{
				if ((int)KEYCONFIG.AppConfig.APPTHEME == 1)
				{
					ComConnect.Image = Resources.B5;
				}
				else
				{
					ComConnect.Image = Resources.B5_L;
				}
				_selectedPort = "";
			}
			else
			{
				try
				{
					if (_serialPort.IsOpen)
					{
						_serialPort.Close();
					}
				}
				catch (Exception)
				{
					_serialPort = new SerialPortStream();
					NxControllerInterface.SerialPort = _serialPort;
				}
				Task.Factory.StartNew(delegate
				{
					NxControllerInterface.StartedBluetooth = true;
					NxControllerInterface.StartGamepad();
				});
				toolStripMenuItem.Checked = true;
				ComConnect.Image = Resources.B5_LINK;
				_selectedPort = toolStripMenuItem.Name;
				((System.Windows.Forms.Control)(object)ComPortList).Text = "Bluetooth無線接続";
			}
		};
	}

	private void dropIcon1_Click(object sender, EventArgs e)
	{
	}

	private void tabPage3_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
	{
		e.Effect = System.Windows.Forms.DragDropEffects.All;
	}

	private void flowLayoutPanel1_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
	{
		e.Effect = System.Windows.Forms.DragDropEffects.All;
	}

	private void dropIcon1_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
	{
		e.Effect = System.Windows.Forms.DragDropEffects.All;
	}

	private void flowLayoutPanel1_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
	{
		string[] array = (string[])e.Data.GetData(System.Windows.Forms.DataFormats.FileDrop, autoConvert: false);
		foreach (string text in array)
		{
			try
			{
				switch (Path.GetExtension(text).ToLower())
				{
				case ".bmp":
				case ".jpg":
				case ".png":
				case ".tif":
				{
					Bitmap im = new Bitmap(text);
					List<string> list = GlobalVar.MAINFORM.Nmc.ResourcesImages.Select((ResourcesImage _) => _.label).ToList();
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
					bool flag = false;
					for (int num = 0; num < list.Count; num++)
					{
						if (list[num] == fileNameWithoutExtension)
						{
							flag = true;
							ResourcesImage item = new ResourcesImage(im, fileNameWithoutExtension);
							GlobalVar.MAINFORM.Nmc.ResourcesImages.RemoveAt(num);
							GlobalVar.MAINFORM.Nmc.ResourcesImages.Insert(num, item);
						}
					}
					if (!flag)
					{
						ResourcesImage item2 = new ResourcesImage(im, fileNameWithoutExtension);
						GlobalVar.MAINFORM.Nmc.ResourcesImages.Add(item2);
					}
					break;
				}
				}
			}
			catch
			{
			}
		}
		ImageReload();
	}

	private void 画面をキャプチャToolStripMenuItem_Click(object sender, EventArgs e)
	{
		NxSel.X1 = -1;
		NxSel.X2 = -1;
		NxSel.Y1 = -1;
		NxSel.Y2 = -1;
		NxSel.Start = false;
		_captureMode = 0;
		_captureNow = true;
	}

	public void StartSnippingSetting()
	{
		if (CvCapture != null || DsCapture != null)
		{
			NxSel.X1 = -1;
			NxSel.X2 = -1;
			NxSel.Y1 = -1;
			NxSel.Y2 = -1;
			NxSel.Start = false;
			_captureMode = 1;
			_captureNow = true;
		}
	}

	private void CaptureScreen_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
	{
		if (CvCapture == null && DsCapture == null)
		{
			return;
		}
		int num = 1920;
		int num2 = 1080;
		if (DsCapture != null)
		{
			num = DsCapture.Width;
			num2 = DsCapture.Height;
		}
		int width = CaptureScreen.Width;
		int height = CaptureScreen.Height;
		double num3 = (double)width / (double)num;
		double num4 = (double)height / (double)num2;
		int num5 = 0;
		int num6 = 0;
		if (num4 > num3)
		{
			num6 = (int)(((double)height - (double)num2 * num3) / 2.0);
		}
		else
		{
			num5 = (int)(((double)width - (double)num * num4) / 2.0);
			num3 = num4;
		}
		if (DsCapture != null)
		{
			NxSel.X1 = Math.Min(Math.Max((int)((double)e.X / num3), 0), num - 1);
			NxSel.Y1 = Math.Min(Math.Max((int)((double)e.Y / num3), 0), num2 - 1);
			if ((int)((double)e.X / num3) < num && (int)((double)e.Y / num3) < num2)
			{
				Console.WriteLine($"MouseDown : X - {NxSel.X1} / Y - {NxSel.Y1}");
			}
		}
		else
		{
			NxSel.X1 = Math.Min(Math.Max((int)((double)(e.X - num5) / num3), 0), num - 1);
			NxSel.Y1 = Math.Min(Math.Max((int)((double)(e.Y - num6) / num3), 0), num2 - 1);
			if (e.X >= num5 && (int)((double)(e.X - num5) / num3) < num && e.Y >= num6 && (int)((double)(e.Y - num6) / num3) < num2)
			{
				Console.WriteLine($"MouseDown : X - {NxSel.X1} / Y - {NxSel.Y1}");
			}
		}
		NxSel.PicD = num3;
		NxSel.PicW = num5;
		NxSel.PicH = num6;
		NxSel.Start = true;
	}

	public Bitmap CaptureImage()
	{
		while (true)
		{
			try
			{
				if (NxCommand.CurrentFrame == null)
				{
					return null;
				}
				return (Bitmap)NxCommand.CurrentFrame.Clone();
			}
			catch (Exception)
			{
			}
		}
	}

	public async void SetCapImage()
	{
	}

	private byte crc8(byte[] data)
	{
		byte b = 0;
		for (int i = 0; i < data.Length; i++)
		{
			b = (byte)CRC_TABLE[data[i] ^ b];
		}
		return b;
	}

	public void Snipping(int x, int y, int width, int height)
	{
		if (DsCapture != null || CvCapture != null)
		{
			Bitmap bitmap;
			lock (NxCommand.lockObject)
			{
				bitmap = CaptureImage();
			}
			Rectangle rect = new Rectangle(x, y, width, height);
			Bitmap bitmap2 = bitmap.Clone(rect, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			if (!Directory.Exists(GlobalVar.CaptureOutput))
			{
				Directory.CreateDirectory(GlobalVar.BasePath + "Captures");
				GlobalVar.CaptureOutput = Path.GetFullPath(GlobalVar.BasePath + "Captures");
				Util.SaveConfig();
			}
			bitmap2.Save(GlobalVar.CaptureOutput + "/" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-ffffff") + ".png", ImageFormat.Png);
		}
	}

	private void CaptureScreen_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
	{
		if (NxSel.X1 == -1 || !_captureNow)
		{
			return;
		}
		_captureNow = false;
		try
		{
			if (_captureMode == 0)
			{
				Snipping(Math.Min(NxSel.X1, NxSel.X2), Math.Min(NxSel.Y1, NxSel.Y2), Math.Abs(NxSel.X2 - NxSel.X1) + 1, Math.Abs(NxSel.Y2 - NxSel.Y1) + 1);
				return;
			}
			KeyInputSet("Snipping(" + Math.Min(NxSel.X1, NxSel.X2) + ", " + Math.Min(NxSel.Y1, NxSel.Y2) + ", " + (Math.Abs(NxSel.X2 - NxSel.X1) + 1) + ", " + (Math.Abs(NxSel.Y2 - NxSel.Y1) + 1) + ")", 0m, 0m, plF: true);
		}
		catch
		{
		}
	}

	private void CaptureContext_Opening(object sender, CancelEventArgs e)
	{
		if (_captureNow || (CvCapture == null && DsCapture == null))
		{
			画面をキャプチャToolStripMenuItem.Enabled = false;
		}
		else
		{
			画面をキャプチャToolStripMenuItem.Enabled = true;
		}
	}

	private void CaptureScreen_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
	{
		if (_captureNow && (CvCapture != null || DsCapture != null))
		{
			int num = 1920;
			int num2 = 1080;
			if (DsCapture != null)
			{
				num = DsCapture.Width;
				num2 = DsCapture.Height;
			}
			int width = CaptureScreen.Width;
			int height = CaptureScreen.Height;
			double num3 = (double)width / (double)num;
			double num4 = (double)height / (double)num2;
			int num5 = 0;
			int num6 = 0;
			if (num4 > num3)
			{
				num6 = (int)(((double)height - (double)num2 * num3) / 2.0);
			}
			else
			{
				num5 = (int)(((double)width - (double)num * num4) / 2.0);
				num3 = num4;
			}
			NxSel.X2 = Math.Min(Math.Max((int)((double)(e.X - num5) / num3), 0), num - 1);
			NxSel.Y2 = Math.Min(Math.Max((int)((double)(e.Y - num6) / num3), 0), num2 - 1);
		}
	}

	private void BTSetUpToolStripMenuItem_Click(object sender, EventArgs e)
	{
		Bluetooth制御セットアップ bluetooth制御セットアップ = new Bluetooth制御セットアップ();
		bluetooth制御セットアップ.StartPosition = FormStartPosition.CenterParent;
		bluetooth制御セットアップ.ShowDialog();
	}

	private void 接続ToolStripMenuItem_Click(object sender, EventArgs e)
	{
	}

	private void NXMC_VxV_KeyPress(object sender, KeyPressEventArgs e)
	{
	}

	private void NXMC_VxV_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		Keys keyCode = e.KeyCode;
		if ((uint)(keyCode - 37) <= 3u)
		{
			e.IsInputKey = true;
		}
	}

	private void CapDeviceList_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
	}

	private void CapDeviceList_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
	{
	}

	private void tabControl1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
	{
		Keys keyCode = e.KeyCode;
		if ((uint)(keyCode - 37) <= 3u)
		{
			e.Handled = true;
		}
	}

	private void button6_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		Keys keyCode = e.KeyCode;
		if ((uint)(keyCode - 37) <= 3u)
		{
			e.IsInputKey = true;
		}
	}

	private void button1_Click_2(object sender, EventArgs e)
	{
	}

	public void TaskView()
	{
		Task.Factory.StartNew(delegate
		{
			string taskname = "";
			string[] taskName = GlobalVar.TaskName;
			foreach (string text in taskName)
			{
				if (!string.IsNullOrEmpty(text))
				{
					if (taskname == "")
					{
						taskname = text;
					}
					else
					{
						taskname = taskname + "   /   " + text;
					}
				}
			}
			((System.Windows.Forms.Control)this).Invoke((Delegate)(Action)delegate
			{
				label2.Text = taskname;
			});
		});
	}

	public void TaskViewLite()
	{
		if (_lastTaskView.ElapsedMilliseconds >= 32)
		{
			_lastTaskView.Restart();
			TaskView();
		}
	}

	private void マクロの保存ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.InitialDirectory = Path.GetFullPath(GlobalVar.BasePath + "Macro\\" + ((System.Windows.Forms.Control)(object)macroSubDirCmb).Text);
		saveFileDialog.Filter = "NX Macro Controller用マクロファイル(*.nxc)|*.nxc|すべてのファイル(*.*)|*.*";
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			Nmc.Code = CodeEdit.Text;
			byte[] fileData = Nmc.GetFileData();
			if (File.Exists(saveFileDialog.FileName))
			{
				File.Delete(saveFileDialog.FileName);
			}
			File.WriteAllBytes(saveFileDialog.FileName, fileData);
			Nmc.FilePath = Path.GetFullPath(Path.GetDirectoryName(saveFileDialog.FileName)) + "\\";
			Nmc.AllPath = saveFileDialog.FileName;
			((System.Windows.Forms.Control)(object)this).Text = GlobalVar.AppName + " - " + Path.GetFileName(saveFileDialog.FileName);
			マクロを上書き保存ToolStripMenuItem.Enabled = true;
			flowLayoutPanel3.Enabled = true;
			flowLayoutPanel3.Visible = true;
			SetMacroDirectory(saveFileDialog.FileName);
			GlobalVar.TaskName[0] = "ファイルが保存されました";
			GlobalVar.MAINFORM.TaskView();
			macroDirReload();
		}
	}

	private void バージョン情報ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		AppInfoDialog appInfoDialog = new AppInfoDialog();
		appInfoDialog.StartPosition = FormStartPosition.CenterParent;
		appInfoDialog.ShowDialog();
	}

	private void button1_Click_3(object sender, EventArgs e)
	{
		panel2.Location = new System.Drawing.Point(((FormEx)this).ActualLeft, ((FormEx)this).ActualTop);
		panel2.Size = new System.Drawing.Size(((FormEx)this).ActualWidth, ((FormEx)this).ActualHeight);
	}

	private void NXMC_VxV_Resize(object sender, EventArgs e)
	{
		panel2.Location = new System.Drawing.Point(((FormEx)this).ActualLeft, ((FormEx)this).ActualTop);
		panel2.Size = new System.Drawing.Size(((FormEx)this).ActualWidth, ((FormEx)this).ActualHeight);
	}

	private void NXMC_VxV_SizeChanged(object sender, EventArgs e)
	{
		panel2.Location = new System.Drawing.Point(((FormEx)this).ActualLeft, ((FormEx)this).ActualTop);
		panel2.Size = new System.Drawing.Size(((FormEx)this).ActualWidth, ((FormEx)this).ActualHeight);
	}

	private async void buttonEx1_Click(object sender, EventArgs e)
	{
		Python.CreateEngine().CreateScriptSourceFromString("print(\"Hello World!\")").Execute();
	}

	private void SetTheme(BZComponent.Style style)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Invalid comparison between Unknown and I4
		BZStyle.SetStyle(style);
		foreach (ToolStripMenuItem item in menuStrip1.Items)
		{
			item.ForeColor = BZStyle.TextFont;
			foreach (ToolStripItem dropDownItem in item.DropDownItems)
			{
				dropDownItem.ForeColor = BZStyle.TextFont;
			}
		}
		elementHost1.BackColor = BZStyle.NormalColor;
		panel1.BackColor = BZStyle.NormalColor;
		textBox1.BackColor = BZStyle.BackColor;
		textBox1.ForeColor = BZStyle.TextFont;
		((System.Windows.Forms.Control)(object)groupBoxEx1).BackColor = BZStyle.BackColor;
		((System.Windows.Forms.Control)(object)ghostPanel7).BackColor = BZStyle.NormalColor;
		CaptureScreen.BackColor = BZStyle.BackColor;
		((System.Windows.Forms.Control)(object)CapConnect).ForeColor = BZStyle.TextFont;
		((System.Windows.Forms.Control)(object)ComConnect).ForeColor = BZStyle.TextFont;
		((System.Windows.Forms.Control)(object)buttonEx2).ForeColor = BZStyle.TextFont;
		((System.Windows.Forms.Control)(object)button3).ForeColor = BZStyle.TextFont;
		((System.Windows.Forms.Control)(object)button4).ForeColor = BZStyle.TextFont;
		((System.Windows.Forms.Control)(object)button6).ForeColor = BZStyle.TextFont;
		((System.Windows.Controls.Control)(object)CodeEdit).Background = BZStyle.BackColor.ToBrush();
		((System.Windows.Controls.Control)(object)CodeEdit).Foreground = BZStyle.TextFont.ToBrush();
		((System.Windows.Controls.Control)(object)CodeEdit).BorderBrush = BZStyle.NormalColor.ToBrush();
		CodeEdit.TextArea.TextView.BackgroundRenderers.Add((IBackgroundRenderer)(object)new HighLightLine(System.Drawing.Color.Khaki, BZStyle.NormalColor));
		CodeEdit.Options.HighlightCurrentLine = true;
		CodeEdit.Options.ShowEndOfLine = false;
		CodeEdit.TextArea.TextView.CurrentLineBackground = BZStyle.DarkColor.ToBrush();
		CodeEdit.TextArea.TextView.CurrentLineBorder = new System.Windows.Media.Pen(BZStyle.ForeColor.ToBrush(), 1.0);
		MacroItemTheme(style);
		if ((int)style == 1)
		{
			IHighlightingDefinition syntaxHighlighting = HighlightingLoader.Load((XmlReader)new XmlTextReader(new MemoryStream(Encoding.UTF8.GetBytes(Resources.NX_D))), (IHighlightingDefinitionReferenceResolver)(object)HighlightingManager.Instance);
			CodeEdit.SyntaxHighlighting = syntaxHighlighting;
			CodeEdit.LineNumbersForeground = System.Drawing.Color.SteelBlue.ToBrush();
			HighLightLine.HighLightSet(BZStyle.NormalColor, BZStyle.NormalColor);
			if (!Nmc.Running)
			{
				ButtonEx obj2 = button6;
				System.Drawing.Image image = (buttonEx5.Image = Resources.B1);
				obj2.Image = image;
			}
			if (((System.Windows.Forms.Control)(object)CapDeviceList).Enabled)
			{
				CapConnect.Image = Resources.B3;
			}
			if (((System.Windows.Forms.Control)(object)ComPortList).Enabled)
			{
				ComConnect.Image = Resources.B5;
			}
			if (((System.Windows.Forms.Control)(object)buttonEx2).Text != "読込")
			{
				if (((System.Windows.Forms.Control)(object)buttonEx2).Text != "実行")
				{
					buttonEx2.Image = Resources.B1;
				}
			}
			else
			{
				buttonEx2.Image = Resources.B6;
			}
			buttonEx3.Image = Resources.B7;
			((FormEx)this).IconSet((System.Drawing.Image)Resources.iconD);
			System.Drawing.Color color = System.Drawing.Color.FromArgb(37, 37, 38);
			NxControllerInterface.SendPadcolor(System.Drawing.Color.FromArgb(65, 65, 67), System.Drawing.Color.FromArgb(28, 151, 234), color, color);
		}
		else
		{
			IHighlightingDefinition syntaxHighlighting2 = HighlightingLoader.Load((XmlReader)new XmlTextReader(new MemoryStream(Encoding.UTF8.GetBytes(Resources.NX))), (IHighlightingDefinitionReferenceResolver)(object)HighlightingManager.Instance);
			CodeEdit.SyntaxHighlighting = syntaxHighlighting2;
			CodeEdit.LineNumbersForeground = System.Drawing.Color.SteelBlue.ToBrush();
			HighLightLine.HighLightSet(BZStyle.NormalColor, BZStyle.NormalColor);
			if (!Nmc.Running)
			{
				ButtonEx obj3 = button6;
				System.Drawing.Image image = (buttonEx5.Image = Resources.B1_L);
				obj3.Image = image;
			}
			if (((System.Windows.Forms.Control)(object)CapDeviceList).Enabled)
			{
				CapConnect.Image = Resources.B3_L;
			}
			if (((System.Windows.Forms.Control)(object)ComPortList).Enabled)
			{
				ComConnect.Image = Resources.B5_L;
			}
			if (((System.Windows.Forms.Control)(object)buttonEx2).Text != "読込")
			{
				if (((System.Windows.Forms.Control)(object)buttonEx2).Text != "実行")
				{
					buttonEx2.Image = Resources.B1_L;
				}
			}
			else
			{
				buttonEx2.Image = Resources.B6_L;
			}
			buttonEx3.Image = Resources.B7_L;
			((FormEx)this).IconSet((System.Drawing.Image)Resources.iconL);
			System.Drawing.Color color2 = System.Drawing.Color.FromArgb(167, 167, 179);
			NxControllerInterface.SendPadcolor(System.Drawing.Color.FromArgb(222, 222, 235), System.Drawing.Color.FromArgb(0, 122, 204), color2, color2);
		}
		MacroShortCutReload();
		ImageReload();
		DataFileReload();
		((System.Windows.Forms.Control)(object)this).Refresh();
	}

	private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
	{
	}

	private void tabPage2_Click(object sender, EventArgs e)
	{
		((System.Windows.Forms.Control)this).Focus();
		((ContainerControl)this).ActiveControl = null;
	}

	private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
	{
	}

	private void flowLayoutPanel1_Click(object sender, EventArgs e)
	{
		((System.Windows.Forms.Control)this).Focus();
		((ContainerControl)this).ActiveControl = null;
	}

	private void menuStrip1_Click(object sender, EventArgs e)
	{
		((System.Windows.Forms.Control)this).Focus();
		((ContainerControl)this).ActiveControl = null;
	}

	private void CaptureScreen_Click(object sender, EventArgs e)
	{
		((System.Windows.Forms.Control)this).Focus();
		((ContainerControl)this).ActiveControl = null;
	}

	private void label2_Click(object sender, EventArgs e)
	{
		((System.Windows.Forms.Control)this).Focus();
		((ContainerControl)this).ActiveControl = null;
	}

	private void マクロ共有サーバーに接続ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (!MacroShare.Opened)
		{
			MacroShare obj = new MacroShare
			{
				StartPosition = FormStartPosition.CenterParent
			};
			MacroShare.Opened = true;
			obj.Show();
		}
	}

	private void keyboardHook2_KeyboardHooked(object sender, KeyboardHookedEventArgs e)
	{
	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		if (!textBox1.IsDisposed && textBox1 != null && mtw != null)
		{
			string allText = mtw.allText;
			mtw.allText = "";
			if (allText != "")
			{
				textBox1.AppendText(allText);
			}
			if (!((System.Windows.Forms.Control)(object)scrollBarEx1).IsDisposed && scrollBarEx1 != null)
			{
				int num = (int)((double)scrollBarEx1.Value / 100.0 / (double)TextBoxUTL.GetLineHeight(textBox1));
				int ypos = TextBoxUTL.GetYpos(textBox1);
				if (num != ypos)
				{
					scrollBarEx1.Value = ypos * 100 * TextBoxUTL.GetLineHeight(textBox1);
				}
			}
			((System.Windows.Forms.Control)(object)scrollBarEx2).Visible = flowLayoutPanel2.VerticalScroll.Visible;
			if (flowLayoutPanel2.VerticalScroll.Maximum >= flowLayoutPanel2.Height)
			{
				((System.Windows.Forms.Control)(object)scrollBarEx2).Tag = "update";
				scrollBarEx2.Value = flowLayoutPanel2.VerticalScroll.Value * scrollBarEx2.Maximum / (flowLayoutPanel2.VerticalScroll.Maximum - flowLayoutPanel2.Height);
				((System.Windows.Forms.Control)(object)scrollBarEx2).Tag = "";
			}
			((System.Windows.Forms.Control)(object)scrollBarEx3).Visible = flowLayoutPanel1.VerticalScroll.Visible;
			if (flowLayoutPanel1.VerticalScroll.Maximum >= flowLayoutPanel1.Height)
			{
				((System.Windows.Forms.Control)(object)scrollBarEx3).Tag = "update";
				scrollBarEx3.Value = flowLayoutPanel1.VerticalScroll.Value * scrollBarEx3.Maximum / (flowLayoutPanel1.VerticalScroll.Maximum - flowLayoutPanel1.Height);
				((System.Windows.Forms.Control)(object)scrollBarEx3).Tag = "";
			}
			((System.Windows.Forms.Control)(object)scrollBarEx4).Visible = flowLayoutPanel3.VerticalScroll.Visible;
			if (flowLayoutPanel3.VerticalScroll.Maximum >= flowLayoutPanel3.Height)
			{
				((System.Windows.Forms.Control)(object)scrollBarEx4).Tag = "update";
				scrollBarEx4.Value = flowLayoutPanel3.VerticalScroll.Value * scrollBarEx4.Maximum / (flowLayoutPanel3.VerticalScroll.Maximum - flowLayoutPanel3.Height);
				((System.Windows.Forms.Control)(object)scrollBarEx4).Tag = "";
			}
			tabPage1.PerformLayout();
			tabPage3.PerformLayout();
		}
		string[] portNames = SerialPort.GetPortNames();
		if (portNames.Length != _portsBuffer.Length)
		{
			comboBoxEx2_Enter(null, null);
			_portsBuffer = portNames;
		}
		else
		{
			for (int i = 0; i < portNames.Length; i++)
			{
				if (portNames[i] != _portsBuffer[i])
				{
					comboBoxEx2_Enter(null, null);
					_portsBuffer = portNames;
					break;
				}
			}
		}
		if (macroDataChanged)
		{
			macroDataChanged = false;
			DataFileReload();
		}
	}

	public void UpdateHighLightLite()
	{
		if (_lastHighlight.ElapsedMilliseconds >= 16)
		{
			_lastHighlight.Restart();
			UpdateHighLight();
		}
	}

	public void UpdateHighLight()
	{
		Task.Factory.StartNew(delegate
		{
			if (GlobalVar.HighLightLine != _highLightLine)
			{
				((System.Windows.Forms.Control)this).Invoke((Delegate)(Action)delegate
				{
					HighLightLine.HighLightLineSet(GlobalVar.HighLightLine);
					_highLightLine = GlobalVar.HighLightLine;
					if (KEYCONFIG.EditorConfig.RUNNINGFOCUS)
					{
						CodeEdit.ScrollToLine(GlobalVar.HighLightLine);
					}
					CodeEdit.TextArea.TextView.Redraw();
				});
			}
		});
	}

	private void flowLayoutPanel2_MouseEnter(object sender, EventArgs e)
	{
	}

	private void flowLayoutPanel2_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
	{
		Util.WriteLine("DragEnter");
		e.Effect = System.Windows.Forms.DragDropEffects.All;
	}

	private void flowLayoutPanel2_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
	{
		string[] array = (string[])e.Data.GetData(System.Windows.Forms.DataFormats.FileDrop, autoConvert: false);
		foreach (string text in array)
		{
			try
			{
				if (Util.MacroDataCheckOffline(File.ReadAllBytes(text)) && !GlobalVar.MacroList.Contains(text))
				{
					GlobalVar.MacroList.Add(text);
				}
			}
			catch
			{
			}
		}
		MacroShortCutReload();
	}

	private void MacroItemTheme(BZComponent.Style style)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		foreach (object control in flowLayoutPanel2.Controls)
		{
			try
			{
				((MacroItem)control).ThemeChange(style);
			}
			catch (Exception)
			{
			}
		}
	}

	private void FileItemTheme(BZComponent.Style style)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		foreach (object control in flowLayoutPanel3.Controls)
		{
			try
			{
				((FileResItem)control).SetTheme(style);
			}
			catch (Exception)
			{
			}
		}
	}

	private void readmeToolStripMenuItem_Click(object sender, EventArgs e)
	{
		Process.Start(GlobalVar.BasePath + "Readme.txt");
	}

	private void 全画面キャプチャToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (CurrentCaptureFormat == CaptureStyle.DirectShow)
		{
			Snipping(0, 0, DsCapture.Width, DsCapture.Height);
		}
		else
		{
			Snipping(0, 0, 1920, 1080);
		}
	}

	private void ヘルプToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (!HelpDialog.Opening)
		{
			new HelpDialog().Show((System.Windows.Forms.IWin32Window)this);
		}
	}

	private DialogResult UpdateCheckDialog()
	{
		return cTaskDialog.ShowTaskDialogBox((System.Windows.Forms.IWin32Window)this, "更新の確認", "", "新しいバージョンが公開されています。\r\nアプリケーションを終了して公開元のページを開きますか？", "", "", "今後、このメッセージを表示しない", "", "", (eTaskDialogButtons)0, (eSysIcons)0, (eSysIcons)0);
	}

	private void NXMC_VxV_Shown(object sender, EventArgs e)
	{
		if (KEYCONFIG.AppConfig.UPDATECHECK && Util.UpdateCheck())
		{
			((System.Windows.Forms.Control)this).Invoke((Delegate)(Action)delegate
			{
				DialogResult num2 = UpdateCheckDialog();
				KEYCONFIG.AppConfig.UPDATECHECK = !cTaskDialog.VerificationChecked;
				if (num2 == DialogResult.Yes)
				{
					Process.Start("https://blog.bzl-web.com/entry/2090/11/11/000000#NX-Macro-Controller");
					((Form)this).Close();
				}
			});
		}
		try
		{
			if (GlobalVar.Ver <= GlobalVar.LastVer)
			{
				return;
			}
			string[] array = File.ReadAllLines(GlobalVar.BasePath + "Readme.txt");
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (text == "■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■")
				{
					flag = false;
				}
				if (text == "●更新履歴" || flag)
				{
					stringBuilder.AppendLine(text);
					flag = true;
				}
			}
			ChLog chLog = new ChLog();
			chLog.Log = stringBuilder.ToString().Trim();
			chLog.StartPosition = FormStartPosition.CenterParent;
			chLog.ShowDialog();
		}
		catch
		{
		}
	}

	private void NXMC_VxV_Activated(object sender, EventArgs e)
	{
	}

	private void amiibo読込ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "Amiiboデータ(*.bin)|*.bin|すべてのファイル(*.*)|*.*";
		if (openFileDialog.ShowDialog() == DialogResult.OK && File.ReadAllBytes(openFileDialog.FileName).Length <= 540)
		{
			_amiibo = openFileDialog.FileName;
			NxControllerInterface.SendAmiibo(_amiibo);
		}
	}

	private void CaptureScreen_SizeChanged(object sender, EventArgs e)
	{
		captureRun = false;
		if (DsCapture != null)
		{
			DsCapture.renderingSize = CaptureScreen.ClientSize;
		}
	}

	private void pictureBox2_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
	{
	}

	private void groupBox1_Enter(object sender, EventArgs e)
	{
	}

	private void ghostPanel5_Click(object sender, EventArgs e)
	{
		((System.Windows.Forms.Control)this).Focus();
		((ContainerControl)this).ActiveControl = null;
	}

	private void NXMC_VxV_Click(object sender, EventArgs e)
	{
		((System.Windows.Forms.Control)this).Focus();
		((ContainerControl)this).ActiveControl = null;
	}

	private void panel2_Click(object sender, EventArgs e)
	{
		((System.Windows.Forms.Control)this).Focus();
		((ContainerControl)this).ActiveControl = null;
	}

	private void mouseHook1_MouseHooked_2(object sender, MouseHookedEventArgs e)
	{
		if ((object)Form.ActiveForm != this || (e.Message != MouseMessage.LDown && e.Message != MouseMessage.RDown && e.Message != MouseMessage.MDown))
		{
			return;
		}
		System.Drawing.Point point = CaptureScreen.PointToClient(e.Point);
		if (point.X >= 0 && point.Y >= 0 && point.X <= CaptureScreen.Width && point.Y <= CaptureScreen.Height)
		{
			if (DsCapture != null)
			{
				double num = (double)DsCapture.Width / (double)CaptureScreen.Width;
				double num2 = (double)DsCapture.Height / (double)CaptureScreen.Height;
				_ = point.X;
				_ = point.Y;
			}
			if ((object)((ContainerControl)this).ActiveControl != CapDeviceList && (object)((ContainerControl)this).ActiveControl != ComPortList && (object)((ContainerControl)this).ActiveControl != macroSelCmb && (object)((ContainerControl)this).ActiveControl != macroSubDirCmb)
			{
				((System.Windows.Forms.Control)this).Focus();
				((ContainerControl)this).ActiveControl = null;
			}
		}
	}

	private void CapDeviceList_SelectedIndexChanged(object sender, EventArgs e)
	{
		((System.Windows.Forms.Control)this).Focus();
		((ContainerControl)this).ActiveControl = null;
	}

	private void button1_Click_4(object sender, EventArgs e)
	{
	}

	private void ghostPanel6_Paint(object sender, PaintEventArgs e)
	{
	}

	private void textBox1_TextChanged(object sender, EventArgs e)
	{
		int textSize = TextBoxUTL.GetTextSize(textBox1);
		if (textBox1.Height >= textSize)
		{
			scrollBarEx1.Maximum = 0;
		}
		else
		{
			scrollBarEx1.Maximum = (textSize - textBox1.Height) * 100;
		}
		int num = (int)((double)scrollBarEx1.Value / 100.0 / (double)TextBoxUTL.GetLineHeight(textBox1));
		int ypos = TextBoxUTL.GetYpos(textBox1);
		if (num != ypos)
		{
			scrollBarEx1.Value = ypos * 100 * TextBoxUTL.GetLineHeight(textBox1);
		}
	}

	private void button2_Click(object sender, EventArgs e)
	{
	}

	private void scrollBarEx1_Scroll(object sender, ScrollEventArgs e)
	{
		if (scrollBarEx1.Value == scrollBarEx1.Maximum)
		{
			TextBoxUTL.SetYpos(textBox1, scrollBarEx1.Value / 100 / TextBoxUTL.GetLineHeight(textBox1) + 1);
		}
		else
		{
			TextBoxUTL.SetYpos(textBox1, scrollBarEx1.Value / 100 / TextBoxUTL.GetLineHeight(textBox1));
		}
	}

	private void textBox1_Layout(object sender, LayoutEventArgs e)
	{
	}

	private void tabControl1_SizeChanged(object sender, EventArgs e)
	{
	}

	private void 設定ToolStripMenuItem_Click(object sender, EventArgs e)
	{
	}

	private void comboBoxEx2_Click(object sender, EventArgs e)
	{
	}

	private void comboBoxEx2_Enter(object sender, EventArgs e)
	{
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Expected O, but got Unknown
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Invalid comparison between Unknown and I4
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Expected O, but got Unknown
		string text = ((System.Windows.Forms.Control)(object)ComPortList).Text;
		bool flag = false;
		((System.Windows.Forms.ComboBox)(object)ComPortList).BeginUpdate();
		((System.Windows.Forms.ComboBox)(object)ComPortList).Items.Clear();
		((System.Windows.Forms.ComboBox)(object)ComPortList).Items.Add("接続先ポートを選択");
		string[] portNames = SerialPort.GetPortNames();
		for (int i = 0; i < portNames.Length; i++)
		{
			((System.Windows.Forms.ComboBox)(object)ComPortList).Items.Add(portNames[i]);
			if (text == portNames[i])
			{
				flag = true;
			}
		}
		((System.Windows.Forms.ComboBox)(object)ComPortList).Items.Add("Bluetooth無線接続");
		((System.Windows.Forms.ComboBox)(object)ComPortList).EndUpdate();
		((System.Windows.Forms.Control)(object)ComPortList).Text = text;
		if (((System.Windows.Forms.Control)(object)ComPortList).Enabled || flag)
		{
			return;
		}
		if (NxControllerInterface.StartedBluetooth)
		{
			NxControllerInterface.StartedBluetooth = false;
			NxControllerInterface.ShutdownGamepad();
		}
		try
		{
			if (_serialPort.IsOpen)
			{
				_serialPort.Close();
			}
			_serialPort = new SerialPortStream();
			NxControllerInterface.SerialPort = _serialPort;
		}
		catch (Exception)
		{
			_serialPort = new SerialPortStream();
			NxControllerInterface.SerialPort = _serialPort;
		}
		if ((int)KEYCONFIG.AppConfig.APPTHEME == 1)
		{
			ComConnect.Image = Resources.B5;
		}
		else
		{
			ComConnect.Image = Resources.B5_L;
		}
		_selectedPort = "";
		((System.Windows.Forms.Control)(object)ComPortList).Enabled = true;
	}

	private void CapDeviceList_Enter(object sender, EventArgs e)
	{
	}

	private void ComConnect_Click(object sender, EventArgs e)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Expected O, but got Unknown
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Expected O, but got Unknown
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Expected O, but got Unknown
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Invalid comparison between Unknown and I4
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Invalid comparison between Unknown and I4
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Invalid comparison between Unknown and I4
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Expected O, but got Unknown
		string text = ((System.Windows.Forms.Control)(object)ComPortList).Text;
		if (text == "Bluetooth無線接続")
		{
			if (NxControllerInterface.StartedBluetooth)
			{
				NxControllerInterface.StartedBluetooth = false;
				NxControllerInterface.ShutdownGamepad();
				if ((int)KEYCONFIG.AppConfig.APPTHEME == 1)
				{
					ComConnect.Image = Resources.B5;
				}
				else
				{
					ComConnect.Image = Resources.B5_L;
				}
				_selectedPort = "";
				((System.Windows.Forms.Control)(object)ComPortList).Enabled = true;
				return;
			}
			try
			{
				if (_serialPort.IsOpen)
				{
					_serialPort.Close();
				}
			}
			catch (Exception)
			{
				_serialPort = new SerialPortStream();
				NxControllerInterface.SerialPort = _serialPort;
			}
			Task.Factory.StartNew(delegate
			{
				NxControllerInterface.StartedBluetooth = true;
				NxControllerInterface.StartGamepad();
			});
			ComConnect.Image = Resources.B5_LINK;
			_selectedPort = "Bluetooth";
			((System.Windows.Forms.Control)(object)ComPortList).Enabled = false;
			return;
		}
		if (((ListControl)(object)ComPortList).SelectedIndex > 0)
		{
			if (NxControllerInterface.StartedBluetooth)
			{
				NxControllerInterface.StartedBluetooth = false;
				NxControllerInterface.ShutdownGamepad();
			}
			if (text == _selectedPort)
			{
				if ((int)KEYCONFIG.AppConfig.APPTHEME == 1)
				{
					ComConnect.Image = Resources.B5;
				}
				else
				{
					ComConnect.Image = Resources.B5_L;
				}
				_selectedPort = "";
				((System.Windows.Forms.Control)(object)ComPortList).Enabled = true;
				try
				{
					if (_serialPort.IsOpen)
					{
						_serialPort.Close();
					}
					_serialPort = new SerialPortStream();
					NxControllerInterface.SerialPort = _serialPort;
					return;
				}
				catch (Exception)
				{
					_serialPort = new SerialPortStream();
					NxControllerInterface.SerialPort = _serialPort;
					return;
				}
			}
			ComConnect.Image = Resources.B5_LINK;
			_selectedPort = text;
			((System.Windows.Forms.Control)(object)ComPortList).Enabled = false;
			try
			{
				if (_serialPort.IsOpen)
				{
					_serialPort.Close();
				}
			}
			catch (Exception)
			{
				_serialPort = new SerialPortStream();
				NxControllerInterface.SerialPort = _serialPort;
			}
			try
			{
				NxControllerInterface.OpenSerial(text);
				return;
			}
			catch (Exception)
			{
				return;
			}
		}
		if (NxControllerInterface.StartedBluetooth)
		{
			NxControllerInterface.StartedBluetooth = false;
			NxControllerInterface.ShutdownGamepad();
		}
		try
		{
			if (_serialPort.IsOpen)
			{
				_serialPort.Close();
			}
		}
		catch (Exception)
		{
			_serialPort = new SerialPortStream();
			NxControllerInterface.SerialPort = _serialPort;
		}
		if ((int)KEYCONFIG.AppConfig.APPTHEME == 1)
		{
			ComConnect.Image = Resources.B5;
		}
		else
		{
			ComConnect.Image = Resources.B5_L;
		}
		_selectedPort = "";
		((System.Windows.Forms.Control)(object)ComPortList).Enabled = true;
	}

	private void ComPortList_DropDown(object sender, EventArgs e)
	{
	}

	private void buttonEx3_Click(object sender, EventArgs e)
	{
		Process.Start("EXPLORER.EXE", GlobalVar.BasePath + "Macro\\" + ((System.Windows.Forms.Control)(object)macroSubDirCmb).Text);
	}

	private void NXMC_VxV_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
	{
		e.Effect = System.Windows.Forms.DragDropEffects.All;
	}

	private void ghostPanel5_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
	{
	}

	private void panel5_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
	{
	}

	private void flowLayoutPanel2_Scroll(object sender, ScrollEventArgs e)
	{
	}

	private void scrollBarEx2_Scroll(object sender, ScrollEventArgs e)
	{
		if ((((System.Windows.Forms.Control)(object)scrollBarEx2).Tag == null || !(((System.Windows.Forms.Control)(object)scrollBarEx2).Tag.ToString() == "update")) && flowLayoutPanel2.VerticalScroll.Maximum >= flowLayoutPanel2.Height)
		{
			flowLayoutPanel2.VerticalScroll.Value = (int)((double)scrollBarEx2.Value / (double)scrollBarEx2.Maximum * (double)(flowLayoutPanel2.VerticalScroll.Maximum - flowLayoutPanel2.Height));
		}
	}

	private void flowLayoutPanel2_Resize(object sender, EventArgs e)
	{
		scrollBarEx2.Maximum = Math.Max(0, (flowLayoutPanel2.VerticalScroll.Maximum - flowLayoutPanel2.Height) * 100);
		((System.Windows.Forms.Control)(object)scrollBarEx2).Visible = flowLayoutPanel2.VerticalScroll.Visible;
	}

	private void flowLayoutPanel1_Resize(object sender, EventArgs e)
	{
		scrollBarEx3.Maximum = Math.Max(0, (flowLayoutPanel1.VerticalScroll.Maximum - flowLayoutPanel1.Height) * 100);
		((System.Windows.Forms.Control)(object)scrollBarEx3).Visible = flowLayoutPanel1.VerticalScroll.Visible;
	}

	private void scrollBarEx3_Scroll(object sender, ScrollEventArgs e)
	{
		if ((((System.Windows.Forms.Control)(object)scrollBarEx3).Tag == null || !(((System.Windows.Forms.Control)(object)scrollBarEx3).Tag.ToString() == "update")) && flowLayoutPanel1.VerticalScroll.Maximum >= flowLayoutPanel1.Height)
		{
			flowLayoutPanel1.VerticalScroll.Value = (int)((double)scrollBarEx3.Value / (double)scrollBarEx3.Maximum * (double)(flowLayoutPanel1.VerticalScroll.Maximum - flowLayoutPanel1.Height));
		}
	}

	private void scrollBarEx4_Scroll(object sender, ScrollEventArgs e)
	{
		if ((((System.Windows.Forms.Control)(object)scrollBarEx4).Tag == null || !(((System.Windows.Forms.Control)(object)scrollBarEx4).Tag.ToString() == "update")) && flowLayoutPanel3.VerticalScroll.Maximum >= flowLayoutPanel3.Height)
		{
			flowLayoutPanel3.VerticalScroll.Value = (int)((double)scrollBarEx4.Value / (double)scrollBarEx4.Maximum * (double)(flowLayoutPanel3.VerticalScroll.Maximum - flowLayoutPanel3.Height));
		}
	}

	private void flowLayoutPanel3_Resize(object sender, EventArgs e)
	{
		scrollBarEx4.Maximum = Math.Max(0, (flowLayoutPanel3.VerticalScroll.Maximum - flowLayoutPanel3.Height) * 100);
		((System.Windows.Forms.Control)(object)scrollBarEx4).Visible = flowLayoutPanel3.VerticalScroll.Visible;
	}

	private void マクロを上書き保存ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		Nmc.Code = CodeEdit.Text;
		byte[] fileData = Nmc.GetFileData();
		File.WriteAllBytes(Nmc.AllPath, fileData);
		GlobalVar.TaskName[0] = "ファイルが保存されました";
		GlobalVar.MAINFORM.TaskView();
	}

	private void NXMC_VxV_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
	{
		string[] array = (string[])e.Data.GetData(System.Windows.Forms.DataFormats.FileDrop, autoConvert: false);
		try
		{
			string text = Path.GetExtension(array[0]).ToLower();
			if (text == ".nxc" || text == ".nmc")
			{
				Nmc.NMCRead(array[0]);
				((System.Windows.Forms.Control)(object)this).Text = GlobalVar.AppName + " - " + Path.GetFileName(array[0]);
				CodeEdit.TextArea.Document.BeginUpdate();
				CodeEdit.TextArea.Document.Text = Nmc.Code;
				CodeEdit.TextArea.Document.EndUpdate();
				マクロを上書き保存ToolStripMenuItem.Enabled = true;
				flowLayoutPanel3.Enabled = true;
				flowLayoutPanel3.Visible = true;
				SetMacroDirectory(array[0]);
				ImageReload();
				DataFileReload();
			}
		}
		catch
		{
		}
	}

	private void flowLayoutPanel3_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
	{
		string[] array = (string[])e.Data.GetData(System.Windows.Forms.DataFormats.FileDrop, autoConvert: false);
		foreach (string path in array)
		{
			try
			{
				if (Util.GetFilePathType(path) == Util.FilePathType.File)
				{
					string text = Path.GetFileName(path);
					if (CurrentDirectory != "")
					{
						text = CurrentDirectory + text;
					}
					text = Nmc.GetDataPath(text);
					if (!Directory.Exists(GlobalVar.MAINFORM.MacroDirectory))
					{
						Directory.CreateDirectory(GlobalVar.MAINFORM.MacroDirectory);
						GlobalVar.MAINFORM.FSWReload();
					}
					Directory.CreateDirectory(Path.GetDirectoryName(text));
					File.WriteAllBytes(text, File.ReadAllBytes(path));
				}
				else
				{
					if (Util.GetFilePathType(path) != Util.FilePathType.Directory)
					{
						continue;
					}
					string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
					for (int j = 0; j < files.Length; j++)
					{
						string text2 = Util.GetRelativePath(Path.GetDirectoryName(path), files[j]).Substring(2);
						if (CurrentDirectory != "")
						{
							text2 = CurrentDirectory + text2;
						}
						if (!Directory.Exists(GlobalVar.MAINFORM.MacroDirectory))
						{
							Directory.CreateDirectory(GlobalVar.MAINFORM.MacroDirectory);
							GlobalVar.MAINFORM.FSWReload();
						}
						text2 = Nmc.GetDataPath(text2);
						Directory.CreateDirectory(Path.GetDirectoryName(text2));
						File.WriteAllBytes(text2, File.ReadAllBytes(files[j]));
					}
					continue;
				}
			}
			catch
			{
			}
		}
	}

	private void fileSystemWatcher1_Created(object sender, FileSystemEventArgs e)
	{
		macroDataChanged = true;
	}

	private void fileSystemWatcher1_Renamed(object sender, RenamedEventArgs e)
	{
		macroDataChanged = true;
	}

	private void fileSystemWatcher1_Deleted(object sender, FileSystemEventArgs e)
	{
		macroDataChanged = true;
	}

	private void fileSystemWatcher2_Renamed(object sender, RenamedEventArgs e)
	{
		string fullPath = Path.GetFullPath(Path.GetDirectoryName(MacroDirectory));
		if (e.OldFullPath == fullPath)
		{
			macroDataChanged = true;
		}
	}

	private void fileSystemWatcher2_Deleted(object sender, FileSystemEventArgs e)
	{
		Util.WriteLine("fsw2_delete");
		string fullPath = Path.GetFullPath(Path.GetDirectoryName(MacroDirectory));
		if (Path.GetFullPath(e.FullPath) == fullPath)
		{
			macroDataChanged = true;
		}
	}

	private void CaptureScreen_MouseEnter(object sender, EventArgs e)
	{
	}

	private void CaptureScreen_MouseLeave(object sender, EventArgs e)
	{
	}

	private void fileSystemWatcher3_Deleted(object sender, FileSystemEventArgs e)
	{
		string fullPath = Path.GetFullPath(Path.GetDirectoryName(MacroDirectory + CurrentDirectory));
		if (e.FullPath == fullPath)
		{
			CurrentDirectory = "";
			macroDataChanged = true;
		}
	}

	private void fileSystemWatcher3_Renamed(object sender, RenamedEventArgs e)
	{
		string fullPath = Path.GetFullPath(Path.GetDirectoryName(MacroDirectory + CurrentDirectory));
		if (e.OldFullPath == fullPath)
		{
			CurrentDirectory = "";
			macroDataChanged = true;
		}
	}

	private void NXMC_VxV_ResizeBegin(object sender, EventArgs e)
	{
		((System.Windows.Forms.Control)this).SuspendLayout();
	}

	private void NXMC_VxV_ResizeEnd(object sender, EventArgs e)
	{
		((System.Windows.Forms.Control)this).ResumeLayout();
	}

	private void CaptureScreen_Paint(object sender, PaintEventArgs e)
	{
		if (CurrentCaptureFormat == CaptureStyle.OpenCV)
		{
			Bitmap bitmap = captureScreenBuffer;
			if (bitmap != null)
			{
				int x = (CaptureScreen.Width - captureScreenBuffer.Width) / 2;
				int y = (CaptureScreen.Height - captureScreenBuffer.Height) / 2;
				e.Graphics.DrawImage(bitmap, x, y);
			}
		}
	}

	private void macroSubDirCmb_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (((System.Windows.Forms.Control)(object)macroSubDirCmb).Tag != null)
		{
			return;
		}
		macroListReload();
		fileSystemWatcher5.EnableRaisingEvents = false;
		fileSystemWatcher5.Dispose();
		fileSystemWatcher5 = new FileSystemWatcher(GlobalVar.BasePath + "Macro\\" + ((System.Windows.Forms.Control)(object)macroSubDirCmb).Text);
		fileSystemWatcher5.Filter = "*";
		fileSystemWatcher5.IncludeSubdirectories = false;
		fileSystemWatcher5.EnableRaisingEvents = true;
		fileSystemWatcher5.Created += delegate
		{
			((System.Windows.Forms.Control)this).Invoke((Delegate)(Action)delegate
			{
				macroListReload();
			});
		};
		fileSystemWatcher5.Deleted += delegate
		{
			((System.Windows.Forms.Control)this).Invoke((Delegate)(Action)delegate
			{
				macroListReload();
			});
		};
		fileSystemWatcher5.Renamed += delegate
		{
			((System.Windows.Forms.Control)this).Invoke((Delegate)(Action)delegate
			{
				macroListReload();
			});
		};
	}

	private void macroListReload()
	{
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Invalid comparison between Unknown and I4
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Invalid comparison between Unknown and I4
		((System.Windows.Forms.Control)(object)macroSubDirCmb).Tag = "update";
		string text = ((System.Windows.Forms.Control)(object)macroSelCmb).Text;
		((System.Windows.Forms.ComboBox)(object)macroSelCmb).BeginUpdate();
		((System.Windows.Forms.ComboBox)(object)macroSelCmb).Items.Clear();
		if (((ListControl)(object)macroSubDirCmb).SelectedIndex == ((System.Windows.Forms.ComboBox)(object)macroSubDirCmb).Items.Count - 1)
		{
			pokeconScriptFiles.Clear();
			string[] files = Directory.GetFiles(Path.GetFullPath(GlobalVar.BasePath + "Macro\\" + ((System.Windows.Forms.Control)(object)macroSubDirCmb).Text), "*", SearchOption.TopDirectoryOnly);
			((System.Windows.Forms.Control)(object)buttonEx2).Text = "実行";
			if ((int)((FormEx)this).FormTheme == 1)
			{
				buttonEx2.Image = Resources.B1;
			}
			else
			{
				buttonEx2.Image = Resources.B1_L;
			}
			string[] array = files;
			foreach (string text2 in array)
			{
				if (!(Path.GetExtension(text2) == ".py"))
				{
					continue;
				}
				string item = "";
				string[] array2 = File.ReadAllLines(text2);
				for (int j = 0; j < array2.Length; j++)
				{
					string text3 = array2[j].Trim();
					if (text3.Length > 6 && text3.Substring(0, 4) == "NAME")
					{
						int num = text3.IndexOf('\'');
						if (num != -1)
						{
							int num2 = text3.LastIndexOf('\'');
							item = text3.Substring(num + 1, num2 - num - 1);
							break;
						}
						int num3 = text3.IndexOf('"');
						if (num3 != -1)
						{
							int num4 = text3.LastIndexOf('"');
							item = text3.Substring(num3 + 1, num4 - num3 - 1);
							break;
						}
					}
				}
				pokeconScriptFiles.Add(text2);
				((System.Windows.Forms.ComboBox)(object)macroSelCmb).Items.Add(item);
			}
		}
		else
		{
			string[] files2 = Directory.GetFiles(Path.GetFullPath(GlobalVar.BasePath + "Macro\\" + ((System.Windows.Forms.Control)(object)macroSubDirCmb).Text), "*", SearchOption.TopDirectoryOnly);
			((System.Windows.Forms.Control)(object)buttonEx2).Text = "読込";
			if ((int)((FormEx)this).FormTheme == 1)
			{
				buttonEx2.Image = Resources.B6;
			}
			else
			{
				buttonEx2.Image = Resources.B6_L;
			}
			string[] array = files2;
			foreach (string path in array)
			{
				string extension = Path.GetExtension(path);
				if (extension == ".nxc" || extension == ".nmc")
				{
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
					((System.Windows.Forms.ComboBox)(object)macroSelCmb).Items.Add(fileNameWithoutExtension);
				}
			}
		}
		if (((System.Windows.Forms.ComboBox)(object)macroSelCmb).Items.Count == 0)
		{
			((System.Windows.Forms.ComboBox)(object)macroSelCmb).Items.Add("-");
		}
		((System.Windows.Forms.Control)(object)macroSelCmb).Text = text;
		((System.Windows.Forms.Control)(object)macroSubDirCmb).Tag = null;
		((System.Windows.Forms.ComboBox)(object)macroSelCmb).EndUpdate();
	}

	private void macroDirReload()
	{
		((System.Windows.Forms.Control)(object)macroSubDirCmb).Tag = "update";
		if (!Directory.Exists(GlobalVar.BasePath + "Macro"))
		{
			Directory.CreateDirectory(GlobalVar.BasePath + "Macro");
		}
		if (!Directory.Exists(GlobalVar.BasePath + "Macro\\Default"))
		{
			Directory.CreateDirectory(GlobalVar.BasePath + "Macro\\Default");
		}
		if (!Directory.Exists(GlobalVar.BasePath + "Macro\\Poke-Controller"))
		{
			Directory.CreateDirectory(GlobalVar.BasePath + "Macro\\Poke-Controller");
		}
		if (!Directory.Exists(GlobalVar.BasePath + "Macro\\Poke-Controller\\Template"))
		{
			Directory.CreateDirectory(GlobalVar.BasePath + "Macro\\Poke-Controller\\Template");
		}
		string[] directories = Directory.GetDirectories(GlobalVar.BasePath + "Macro", "*", SearchOption.TopDirectoryOnly);
		string text = ((System.Windows.Forms.Control)(object)macroSubDirCmb).Text;
		((System.Windows.Forms.ComboBox)(object)macroSubDirCmb).BeginUpdate();
		((System.Windows.Forms.ComboBox)(object)macroSubDirCmb).Items.Clear();
		((System.Windows.Forms.ComboBox)(object)macroSubDirCmb).Items.Add("Default");
		for (int i = 0; i < directories.Length; i++)
		{
			string fileName = Path.GetFileName(directories[i]);
			if (!(fileName == "Default") && !(fileName == "Poke-Controller"))
			{
				((System.Windows.Forms.ComboBox)(object)macroSubDirCmb).Items.Add(Path.GetFileName(directories[i]));
			}
		}
		((System.Windows.Forms.ComboBox)(object)macroSubDirCmb).Items.Add("Poke-Controller");
		((System.Windows.Forms.Control)(object)macroSubDirCmb).Text = text;
		((System.Windows.Forms.Control)(object)macroSubDirCmb).Tag = null;
		((System.Windows.Forms.ComboBox)(object)macroSubDirCmb).EndUpdate();
	}

	private void buttonEx2_Click(object sender, EventArgs e)
	{
		if (((ListControl)(object)macroSubDirCmb).SelectedIndex == ((System.Windows.Forms.ComboBox)(object)macroSubDirCmb).Items.Count - 1)
		{
			if (_pokeconRnnning)
			{
				if (_pokeconProcess != null)
				{
					try
					{
						_pokeconProcess.Kill();
						return;
					}
					catch (Exception)
					{
						return;
					}
				}
				return;
			}
			if (Nmc.SubRunningNmc != "")
			{
				Nmc.Cancel = true;
				while (Nmc.Running)
				{
					System.Windows.Forms.Application.DoEvents();
				}
			}
			string selectedMacro = ((System.Windows.Forms.Control)(object)macroSelCmb).Text;
			buttonEx2.Image = Resources.B4;
			((System.Windows.Forms.Control)(object)buttonEx2).Text = "停止";
			_pokeconRnnning = true;
			((System.Windows.Forms.Control)(object)macroSelCmb).Enabled = false;
			((System.Windows.Forms.Control)(object)macroSubDirCmb).Enabled = false;
			button6.Enabled = false;
			button4.Enabled = false;
			cH552へ書き込みToolStripMenuItem.Enabled = false;
			cH552SERIALセットアップToolStripMenuItem.Enabled = false;
			Task.Factory.StartNew(delegate
			{
				try
				{
					if (!Directory.Exists(GlobalVar.BasePath + "Macro\\Poke-Controller\\Template"))
					{
						Directory.CreateDirectory(GlobalVar.BasePath + "Macro\\Poke-Controller\\Template");
					}
					Directory.CreateDirectory(GlobalVar.BasePath + "Poke-Controller\\Commands\\PythonCommands");
					Util.CopyDirectory(GlobalVar.BasePath + "Macro\\Poke-Controller\\", GlobalVar.BasePath + "Poke-Controller\\Commands\\PythonCommands");
					string contents = "[LINE]\rtoken = " + NxCommand.LineNotifyToken;
					File.WriteAllText(GlobalVar.BasePath + "Poke-Controller\\line_token.ini", contents);
					Directory.CreateDirectory(GlobalVar.BasePath + "Poke-Controller\\Template");
					Directory.CreateDirectory(GlobalVar.BasePath + "Poke-Controller\\Captures");
					Util.CopyDirectory(GlobalVar.BasePath + "Macro\\Poke-Controller\\Template", GlobalVar.BasePath + "Poke-Controller\\Template");
					DsDevice[] devicesOfCat = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
					int num = -1;
					for (int i = 0; i < devicesOfCat.Length; i++)
					{
						if (devicesOfCat[i].Name == "NX2 Virtual Camera")
						{
							num = i;
							break;
						}
					}
					_pokeconProcess = new Process();
					_pokeconProcess.StartInfo.FileName = GlobalVar.BasePath + "Poke-Controller\\Poke-Controller.exe";
					_pokeconProcess.StartInfo.UseShellExecute = false;
					_pokeconProcess.StartInfo.RedirectStandardOutput = true;
					_pokeconProcess.StartInfo.RedirectStandardError = true;
					_pokeconProcess.StartInfo.CreateNoWindow = true;
					_pokeconProcess.EnableRaisingEvents = true;
					_pokeconProcess.StartInfo.Arguments = num + " " + selectedMacro;
					_pokeconProcess.StartInfo.WorkingDirectory = GlobalVar.BasePath + "Poke-Controller\\";
					_pokeconProcess.Exited += delegate
					{
						_pokeconProcess.WaitForExit();
						((System.Windows.Forms.Control)this).Invoke((Delegate)(Action)delegate
						{
							//IL_0011: Unknown result type (might be due to invalid IL or missing references)
							//IL_0017: Invalid comparison between Unknown and I4
							((System.Windows.Forms.Control)(object)buttonEx2).Text = "実行";
							if ((int)((FormEx)this).FormTheme == 1)
							{
								buttonEx2.Image = Resources.B1;
							}
							else
							{
								buttonEx2.Image = Resources.B1_L;
							}
							((System.Windows.Forms.Control)(object)macroSelCmb).Enabled = true;
							((System.Windows.Forms.Control)(object)macroSubDirCmb).Enabled = true;
							button6.Enabled = true;
							button4.Enabled = true;
							cH552へ書き込みToolStripMenuItem.Enabled = true;
							cH552SERIALセットアップToolStripMenuItem.Enabled = true;
							_pokeconRnnning = false;
							Environment.CurrentDirectory = GlobalVar.BasePath;
							MacroDeactive();
						});
					};
					_pokeconProcess.Start();
					MacroActive();
				}
				catch
				{
					if (_pokeconProcess != null)
					{
						try
						{
							_pokeconProcess.Kill();
						}
						catch
						{
						}
					}
					((System.Windows.Forms.Control)this).Invoke((Delegate)(Action)delegate
					{
						//IL_0011: Unknown result type (might be due to invalid IL or missing references)
						//IL_0017: Invalid comparison between Unknown and I4
						((System.Windows.Forms.Control)(object)buttonEx2).Text = "実行";
						if ((int)((FormEx)this).FormTheme == 1)
						{
							buttonEx2.Image = Resources.B1;
						}
						else
						{
							buttonEx2.Image = Resources.B1_L;
						}
						((System.Windows.Forms.Control)(object)macroSelCmb).Enabled = true;
						((System.Windows.Forms.Control)(object)macroSubDirCmb).Enabled = true;
						button6.Enabled = true;
						button4.Enabled = true;
						cH552へ書き込みToolStripMenuItem.Enabled = true;
						cH552SERIALセットアップToolStripMenuItem.Enabled = true;
						_pokeconRnnning = false;
						MacroDeactive();
					});
				}
				while (true)
				{
					try
					{
						string text2 = _pokeconProcess.StandardOutput.ReadLine();
						if (text2 == null)
						{
							break;
						}
						if (text2 != "")
						{
							Console.WriteLine(text2);
						}
					}
					catch (Exception value)
					{
						Console.WriteLine(value);
						break;
					}
				}
			});
		}
		else
		{
			string text = GlobalVar.BasePath + "Macro\\" + ((System.Windows.Forms.Control)(object)macroSubDirCmb).Text + "\\" + ((System.Windows.Forms.Control)(object)macroSelCmb).Text;
			if (File.Exists(text + ".nxc"))
			{
				LoadMacro(text + ".nxc");
			}
			else if (File.Exists(text + ".nmc"))
			{
				LoadMacro(text + ".nmc");
			}
		}
	}

	private void buttonEx4_Click(object sender, EventArgs e)
	{
		textBox1.Clear();
	}

	private void cH552SERIALセットアップToolStripMenuItem_Click(object sender, EventArgs e)
	{
		MacroActive();
		button4.Enabled = false;
		button6.Enabled = false;
		buttonEx2.Enabled = false;
		cH552へ書き込みToolStripMenuItem.Enabled = false;
		cH552SERIALセットアップToolStripMenuItem.Enabled = false;
		マクロの読み込みToolStripMenuItem.Enabled = false;
		GlobalVar.TaskName[4] = "CH552への書き込み中";
		TaskView();
		byte[] cH55xSwitchSerialControl_ino = Resources.CH55xSwitchSerialControl_ino;
		if (new CH552Flash().FlashHex(cH55xSwitchSerialControl_ino, 10000L))
		{
			System.Windows.Forms.MessageBox.Show("ファームウェアの書き込みに成功しました。", "NX Macro Controller", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}
		else
		{
			System.Windows.Forms.MessageBox.Show("ファームウェアの書き込みに失敗しました。", "NX Macro Controller", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		MacroDeactive();
		button4.Enabled = true;
		button6.Enabled = true;
		buttonEx2.Enabled = true;
		cH552へ書き込みToolStripMenuItem.Enabled = true;
		cH552SERIALセットアップToolStripMenuItem.Enabled = true;
		マクロの読み込みToolStripMenuItem.Enabled = true;
		GlobalVar.TaskName[4] = "";
		TaskView();
	}

	private void cH552へ書き込みToolStripMenuItem_Click(object sender, EventArgs e)
	{
		MacroActive();
		button4.Enabled = false;
		button6.Enabled = false;
		buttonEx2.Enabled = false;
		cH552へ書き込みToolStripMenuItem.Enabled = false;
		cH552SERIALセットアップToolStripMenuItem.Enabled = false;
		マクロの読み込みToolStripMenuItem.Enabled = false;
		GlobalVar.TaskName[4] = "CH552への書き込み中";
		TaskView();
		Nmc.Code = CodeEdit.Text;
		string cH552Program = Nmc.GetCH552Program();
		File.WriteAllText(GlobalVar.BasePath + "CH552\\NXCtoC.c", cH552Program);
		foreach (string item in Directory.EnumerateFiles(GlobalVar.BasePath + "CH552", "NXCtoC.*"))
		{
			if (!(Path.GetExtension(item) == ".c"))
			{
				File.Delete(item);
			}
		}
		Process process = Process.Start(new ProcessStartInfo("cmd.exe", "/c \"" + Path.GetFullPath(GlobalVar.BasePath + "CH552\\compile.bat") + "\"")
		{
			CreateNoWindow = true,
			UseShellExecute = false,
			WorkingDirectory = Path.GetFullPath(GlobalVar.BasePath + "CH552")
		});
		if (process.WaitForExit(30000))
		{
			try
			{
				foreach (string item2 in Directory.EnumerateFiles(GlobalVar.BasePath + "CH552", "NXCtoC.*"))
				{
					if (!(Path.GetExtension(item2) == ".hex") && !(Path.GetExtension(item2) == ".c"))
					{
						File.Delete(item2);
					}
				}
				if (new CH552Flash().FlashHex(File.ReadAllBytes(GlobalVar.BasePath + "CH552\\NXCtoC.hex"), 10000L))
				{
					System.Windows.Forms.MessageBox.Show("プログラムの書き込みに成功しました。", "NX Macro Controller", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
				else
				{
					System.Windows.Forms.MessageBox.Show("プログラムの書き込みに失敗しました。", "NX Macro Controller", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			catch
			{
				System.Windows.Forms.MessageBox.Show("プログラムの書き込みに失敗しました。", "NX Macro Controller", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
		else
		{
			System.Windows.Forms.MessageBox.Show("プログラムの書き込みに失敗しました。", "NX Macro Controller", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		process.Close();
		foreach (string item3 in Directory.EnumerateFiles(GlobalVar.BasePath + "CH552", "NXCtoC.*"))
		{
			File.Delete(item3);
		}
		MacroDeactive();
		button4.Enabled = true;
		button6.Enabled = true;
		buttonEx2.Enabled = true;
		cH552へ書き込みToolStripMenuItem.Enabled = true;
		cH552SERIALセットアップToolStripMenuItem.Enabled = true;
		マクロの読み込みToolStripMenuItem.Enabled = true;
		GlobalVar.TaskName[4] = "";
		TaskView();
	}

	private void マクロの新規作成ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.InitialDirectory = Path.GetFullPath(GlobalVar.BasePath + "Macro\\" + ((System.Windows.Forms.Control)(object)macroSubDirCmb).Text);
		saveFileDialog.Filter = "NX Macro Controller用マクロファイル(*.nxc)|*.nxc|すべてのファイル(*.*)|*.*";
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			CodeEdit.Text = "//ここにマクロを記述する";
			Nmc.Code = CodeEdit.Text;
			Nmc.ResourcesImages.Clear();
			Nmc.FilePath = Path.GetFullPath(Path.GetDirectoryName(saveFileDialog.FileName)) + "\\";
			Nmc.AllPath = saveFileDialog.FileName;
			byte[] fileData = Nmc.GetFileData();
			if (File.Exists(saveFileDialog.FileName))
			{
				File.Delete(saveFileDialog.FileName);
			}
			File.WriteAllBytes(saveFileDialog.FileName, fileData);
			((System.Windows.Forms.Control)(object)this).Text = GlobalVar.AppName + " - " + Path.GetFileName(saveFileDialog.FileName);
			マクロを上書き保存ToolStripMenuItem.Enabled = true;
			flowLayoutPanel3.Enabled = true;
			flowLayoutPanel3.Visible = true;
			SetMacroDirectory(saveFileDialog.FileName);
		}
	}

	private void discordサーバーToolStripMenuItem_Click(object sender, EventArgs e)
	{
		Process.Start("https://discord.gg/9VwVrsAAAQ");
	}

	private void macroSubDirCmb_TextUpdate(object sender, EventArgs e)
	{
	}

	private void macroSelCmb_TextUpdate(object sender, EventArgs e)
	{
		macroSelCmbText = ((System.Windows.Forms.Control)(object)macroSelCmb).Text;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		((FormEx)this).Dispose(disposing);
	}

	private void InitializeComponent()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Expected O, but got Unknown
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Expected O, but got Unknown
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Expected O, but got Unknown
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Expected O, but got Unknown
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Expected O, but got Unknown
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Expected O, but got Unknown
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Expected O, but got Unknown
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Expected O, but got Unknown
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Expected O, but got Unknown
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Expected O, but got Unknown
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Expected O, but got Unknown
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Expected O, but got Unknown
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Expected O, but got Unknown
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Expected O, but got Unknown
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Expected O, but got Unknown
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Expected O, but got Unknown
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Expected O, but got Unknown
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Expected O, but got Unknown
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Expected O, but got Unknown
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Expected O, but got Unknown
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Expected O, but got Unknown
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Expected O, but got Unknown
		components = new Container();
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(NXMC_VxV));
		CaptureScreen = new PictureBox();
		CaptureContext = new ContextMenuStrip(components);
		画面をキャプチャToolStripMenuItem = new ToolStripMenuItem();
		全画面キャプチャToolStripMenuItem = new ToolStripMenuItem();
		CaptureBGW = new BackgroundWorker();
		groupBox1 = new GroupBoxEx();
		ghostPanel7 = new GhostPanel();
		button2 = new System.Windows.Forms.Button();
		button1 = new System.Windows.Forms.Button();
		buttonEx1 = new ButtonEx();
		CapConnect = new ButtonEx();
		CapDeviceList = new ComboBoxEx();
		vScrollBar1 = new ScrollBarEx();
		menuStrip1 = new MenuStrip();
		ファイルToolStripMenuItem = new ToolStripMenuItem();
		マクロの新規作成ToolStripMenuItem = new ToolStripMenuItem();
		マクロの読み込みToolStripMenuItem = new ToolStripMenuItem();
		マクロを上書き保存ToolStripMenuItem = new ToolStripMenuItem();
		マクロの保存ToolStripMenuItem = new ToolStripMenuItem();
		toolStripMenuItem1 = new ToolStripSeparator();
		amiiboの読み込みToolStripMenuItem = new ToolStripMenuItem();
		toolStripMenuItem5 = new ToolStripSeparator();
		終了ToolStripMenuItem = new ToolStripMenuItem();
		設定ToolStripMenuItem = new ToolStripMenuItem();
		BTSetUpToolStripMenuItem = new ToolStripMenuItem();
		cH552SERIALセットアップToolStripMenuItem = new ToolStripMenuItem();
		cH552へ書き込みToolStripMenuItem = new ToolStripMenuItem();
		接続ToolStripMenuItem = new ToolStripMenuItem();
		toolStripMenuItem3 = new ToolStripSeparator();
		環境設定ToolStripMenuItem1 = new ToolStripMenuItem();
		共有ToolStripMenuItem = new ToolStripMenuItem();
		マクロ共有サーバーに接続ToolStripMenuItem = new ToolStripMenuItem();
		aboutToolStripMenuItem = new ToolStripMenuItem();
		バージョン情報ToolStripMenuItem = new ToolStripMenuItem();
		discordサーバーToolStripMenuItem = new ToolStripMenuItem();
		readmeToolStripMenuItem = new ToolStripMenuItem();
		ヘルプToolStripMenuItem = new ToolStripMenuItem();
		button6 = new ButtonEx();
		button4 = new ButtonEx();
		button3 = new ButtonEx();
		panel1 = new System.Windows.Forms.Panel();
		hScrollBar1 = new ScrollBarEx();
		label1 = new System.Windows.Forms.Label();
		elementHost1 = new ElementHost();
		toolTip1 = new System.Windows.Forms.ToolTip(components);
		tabControl1 = new TabControlEx();
		tabPage2 = new TabPage();
		tabPage4 = new TabPage();
		tabPageMHXX = new TabPage();
		mhxxCharmRNG1 = new MHXXCharmRNG();
		ghostPanel6 = new GhostPanel();
		buttonEx5 = new ButtonEx();
		buttonEx4 = new ButtonEx();
		groupBoxEx1 = new GroupBoxEx();
		scrollBarEx1 = new ScrollBarEx();
		textBox1 = new System.Windows.Forms.TextBox();
		tabPage3 = new TabPage();
		tabControlEx1 = new TabControlEx();
		tabPage5 = new TabPage();
		scrollBarEx3 = new ScrollBarEx();
		flowLayoutPanel1 = new FlowLayoutPanel();
		tabPage6 = new TabPage();
		scrollBarEx4 = new ScrollBarEx();
		flowLayoutPanel3 = new FlowLayoutPanel();
		tabPage1 = new TabPage();
		scrollBarEx2 = new ScrollBarEx();
		flowLayoutPanel2 = new FlowLayoutPanel();
		panel2 = new System.Windows.Forms.Panel();
		ghostPanel4 = new GhostPanel();
		splitter1 = new Splitter();
		panel4 = new System.Windows.Forms.Panel();
		panel3 = new System.Windows.Forms.Panel();
		label2 = new System.Windows.Forms.Label();
		ghostPanel5 = new GhostPanel();
		label4 = new System.Windows.Forms.Label();
		macroSubDirCmb = new ComboBoxEx();
		buttonEx3 = new ButtonEx();
		ComPortList = new ComboBoxEx();
		buttonEx2 = new ButtonEx();
		label3 = new System.Windows.Forms.Label();
		ComConnect = new ButtonEx();
		macroSelCmb = new ComboBoxEx();
		timer1 = new System.Windows.Forms.Timer(components);
		fileSystemWatcher1 = new FileSystemWatcher();
		fileSystemWatcher2 = new FileSystemWatcher();
		fileSystemWatcher3 = new FileSystemWatcher();
		fileSystemWatcher4 = new FileSystemWatcher();
		fileSystemWatcher5 = new FileSystemWatcher();
		mouseHook1 = new MouseHook();
		((ISupportInitialize)CaptureScreen).BeginInit();
		CaptureContext.SuspendLayout();
		((System.Windows.Forms.Control)(object)groupBox1).SuspendLayout();
		((System.Windows.Forms.Control)(object)ghostPanel7).SuspendLayout();
		menuStrip1.SuspendLayout();
		panel1.SuspendLayout();
		((System.Windows.Forms.Control)(object)tabControl1).SuspendLayout();
		tabPage2.SuspendLayout();
		tabPage4.SuspendLayout();
		tabPageMHXX.SuspendLayout();
		((System.Windows.Forms.Control)(object)ghostPanel6).SuspendLayout();
		((System.Windows.Forms.Control)(object)groupBoxEx1).SuspendLayout();
		tabPage3.SuspendLayout();
		((System.Windows.Forms.Control)(object)tabControlEx1).SuspendLayout();
		tabPage5.SuspendLayout();
		tabPage6.SuspendLayout();
		tabPage1.SuspendLayout();
		panel2.SuspendLayout();
		((System.Windows.Forms.Control)(object)ghostPanel4).SuspendLayout();
		panel4.SuspendLayout();
		panel3.SuspendLayout();
		((System.Windows.Forms.Control)(object)ghostPanel5).SuspendLayout();
		((ISupportInitialize)fileSystemWatcher1).BeginInit();
		((ISupportInitialize)fileSystemWatcher2).BeginInit();
		((ISupportInitialize)fileSystemWatcher3).BeginInit();
		((ISupportInitialize)fileSystemWatcher4).BeginInit();
		((ISupportInitialize)fileSystemWatcher5).BeginInit();
		((System.Windows.Forms.Control)this).SuspendLayout();
		CaptureScreen.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		CaptureScreen.BackColor = System.Drawing.SystemColors.ControlDarkDark;
		CaptureScreen.ContextMenuStrip = CaptureContext;
		CaptureScreen.Location = new System.Drawing.Point(1, 1);
		CaptureScreen.Margin = new Padding(0);
		CaptureScreen.Name = "CaptureScreen";
		CaptureScreen.Size = new System.Drawing.Size(624, 322);
		CaptureScreen.SizeMode = PictureBoxSizeMode.Zoom;
		CaptureScreen.TabIndex = 0;
		CaptureScreen.TabStop = false;
		CaptureScreen.SizeChanged += CaptureScreen_SizeChanged;
		CaptureScreen.Click += CaptureScreen_Click;
		CaptureScreen.Paint += CaptureScreen_Paint;
		CaptureScreen.MouseDown += CaptureScreen_MouseDown;
		CaptureScreen.MouseEnter += CaptureScreen_MouseEnter;
		CaptureScreen.MouseLeave += CaptureScreen_MouseLeave;
		CaptureScreen.MouseMove += CaptureScreen_MouseMove;
		CaptureScreen.MouseUp += CaptureScreen_MouseUp;
		CaptureContext.Items.AddRange(new ToolStripItem[2] { 画面をキャプチャToolStripMenuItem, 全画面キャプチャToolStripMenuItem });
		CaptureContext.Name = "CaptureContext";
		CaptureContext.Size = new System.Drawing.Size(154, 48);
		CaptureContext.Opening += CaptureContext_Opening;
		画面をキャプチャToolStripMenuItem.Name = "画面をキャプチャToolStripMenuItem";
		画面をキャプチャToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
		画面をキャプチャToolStripMenuItem.Text = "画面をキャプチャ";
		画面をキャプチャToolStripMenuItem.Click += 画面をキャプチャToolStripMenuItem_Click;
		全画面キャプチャToolStripMenuItem.Name = "全画面キャプチャToolStripMenuItem";
		全画面キャプチャToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
		全画面キャプチャToolStripMenuItem.Text = "全画面キャプチャ";
		全画面キャプチャToolStripMenuItem.Click += 全画面キャプチャToolStripMenuItem_Click;
		CaptureBGW.WorkerReportsProgress = true;
		CaptureBGW.WorkerSupportsCancellation = true;
		CaptureBGW.DoWork += backgroundWorker1_DoWork;
		CaptureBGW.ProgressChanged += backgroundWorker1_ProgressChanged;
		groupBox1.BorderColor = System.Drawing.Color.Black;
		((System.Windows.Forms.Control)(object)groupBox1).Controls.Add((System.Windows.Forms.Control)(object)ghostPanel7);
		((System.Windows.Forms.Control)(object)groupBox1).Controls.Add(button2);
		((System.Windows.Forms.Control)(object)groupBox1).Controls.Add(button1);
		((System.Windows.Forms.Control)(object)groupBox1).Controls.Add((System.Windows.Forms.Control)(object)buttonEx1);
		((System.Windows.Forms.Control)(object)groupBox1).Controls.Add((System.Windows.Forms.Control)(object)CapConnect);
		((System.Windows.Forms.Control)(object)groupBox1).Controls.Add((System.Windows.Forms.Control)(object)CapDeviceList);
		((System.Windows.Forms.Control)(object)groupBox1).Dock = DockStyle.Fill;
		((System.Windows.Forms.Control)(object)groupBox1).Location = new System.Drawing.Point(371, 0);
		((System.Windows.Forms.Control)(object)groupBox1).Margin = new Padding(3, 2, 3, 2);
		((System.Windows.Forms.Control)(object)groupBox1).MinimumSize = new System.Drawing.Size(0, 381);
		((System.Windows.Forms.Control)(object)groupBox1).Name = "groupBox1";
		((System.Windows.Forms.Control)(object)groupBox1).Padding = new Padding(3, 2, 3, 2);
		((System.Windows.Forms.Control)(object)groupBox1).Size = new System.Drawing.Size(641, 381);
		((System.Windows.Forms.Control)(object)groupBox1).TabIndex = 1;
		((System.Windows.Forms.GroupBox)(object)groupBox1).TabStop = false;
		((System.Windows.Forms.Control)(object)groupBox1).Text = "映像";
		((System.Windows.Forms.Control)(object)groupBox1).Enter += groupBox1_Enter;
		((System.Windows.Forms.Control)(object)ghostPanel7).Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		((System.Windows.Forms.Control)(object)ghostPanel7).Controls.Add(CaptureScreen);
		((System.Windows.Forms.Control)(object)ghostPanel7).Location = new System.Drawing.Point(9, 45);
		((System.Windows.Forms.Control)(object)ghostPanel7).Name = "ghostPanel7";
		((System.Windows.Forms.Control)(object)ghostPanel7).Size = new System.Drawing.Size(626, 324);
		((System.Windows.Forms.Control)(object)ghostPanel7).TabIndex = 5;
		button2.Location = new System.Drawing.Point(558, 16);
		button2.Name = "button2";
		button2.Size = new System.Drawing.Size(75, 23);
		button2.TabIndex = 4;
		button2.Text = "button2";
		button2.UseVisualStyleBackColor = true;
		button2.Visible = false;
		button2.Click += button2_Click;
		button1.Location = new System.Drawing.Point(476, 17);
		button1.Name = "button1";
		button1.Size = new System.Drawing.Size(75, 23);
		button1.TabIndex = 1;
		button1.Text = "button1";
		button1.UseVisualStyleBackColor = true;
		button1.Visible = false;
		button1.Click += button1_Click_4;
		((ButtonBase)(object)buttonEx1).FlatAppearance.BorderSize = 0;
		((System.Windows.Forms.Control)(object)buttonEx1).ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		buttonEx1.Image = Resources.B3_LINK;
		((System.Windows.Forms.Control)(object)buttonEx1).Location = new System.Drawing.Point(333, 17);
		((System.Windows.Forms.Control)(object)buttonEx1).Margin = new Padding(3, 2, 3, 2);
		((System.Windows.Forms.Control)(object)buttonEx1).Name = "buttonEx1";
		((System.Windows.Forms.Control)(object)buttonEx1).Size = new System.Drawing.Size(75, 23);
		((System.Windows.Forms.Control)(object)buttonEx1).TabIndex = 3;
		((System.Windows.Forms.Control)(object)buttonEx1).Text = "接続";
		((ButtonBase)(object)buttonEx1).TextImageRelation = TextImageRelation.ImageBeforeText;
		((ButtonBase)(object)buttonEx1).UseVisualStyleBackColor = true;
		((System.Windows.Forms.Control)(object)buttonEx1).Visible = false;
		((System.Windows.Forms.Control)(object)buttonEx1).Click += buttonEx1_Click;
		((ButtonBase)(object)CapConnect).FlatAppearance.BorderSize = 0;
		((System.Windows.Forms.Control)(object)CapConnect).ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		CapConnect.Image = Resources.B3;
		((System.Windows.Forms.Control)(object)CapConnect).Location = new System.Drawing.Point(252, 17);
		((System.Windows.Forms.Control)(object)CapConnect).Margin = new Padding(3, 2, 3, 2);
		((System.Windows.Forms.Control)(object)CapConnect).Name = "CapConnect";
		((System.Windows.Forms.Control)(object)CapConnect).Size = new System.Drawing.Size(75, 23);
		((System.Windows.Forms.Control)(object)CapConnect).TabIndex = 1;
		((System.Windows.Forms.Control)(object)CapConnect).Text = "接続";
		((ButtonBase)(object)CapConnect).TextImageRelation = TextImageRelation.ImageBeforeText;
		((ButtonBase)(object)CapConnect).UseVisualStyleBackColor = true;
		((System.Windows.Forms.Control)(object)CapConnect).Click += CapConnect_Click;
		((System.Windows.Forms.Control)(object)CapConnect).KeyDown += tabControl1_KeyDown;
		((System.Windows.Forms.Control)(object)CapConnect).PreviewKeyDown += button6_PreviewKeyDown;
		((System.Windows.Forms.Control)(object)CapDeviceList).BackColor = System.Drawing.Color.FromArgb(33, 33, 35);
		CapDeviceList.BorderColor = System.Drawing.Color.FromArgb(65, 65, 67);
		CapDeviceList.BorderStyle = ButtonBorderStyle.Solid;
		CapDeviceList.ContentsCheck = true;
		((System.Windows.Forms.ComboBox)(object)CapDeviceList).DrawMode = DrawMode.OwnerDrawFixed;
		CapDeviceList.DropDownStyle = ComboBoxStyle.DropDownList;
		((System.Windows.Forms.ComboBox)(object)CapDeviceList).FlatStyle = FlatStyle.Flat;
		((System.Windows.Forms.Control)(object)CapDeviceList).ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		((ListControl)(object)CapDeviceList).FormattingEnabled = true;
		((System.Windows.Forms.Control)(object)CapDeviceList).Location = new System.Drawing.Point(9, 17);
		((System.Windows.Forms.Control)(object)CapDeviceList).Margin = new Padding(3, 2, 3, 2);
		((System.Windows.Forms.Control)(object)CapDeviceList).Name = "CapDeviceList";
		((System.Windows.Forms.Control)(object)CapDeviceList).Size = new System.Drawing.Size(237, 23);
		((System.Windows.Forms.Control)(object)CapDeviceList).TabIndex = 0;
		((System.Windows.Forms.ComboBox)(object)CapDeviceList).SelectedIndexChanged += CapDeviceList_SelectedIndexChanged;
		((System.Windows.Forms.Control)(object)CapDeviceList).Enter += CapDeviceList_Enter;
		((System.Windows.Forms.Control)(object)CapDeviceList).KeyDown += CapDeviceList_KeyDown;
		((System.Windows.Forms.Control)(object)CapDeviceList).PreviewKeyDown += CapDeviceList_PreviewKeyDown;
		((System.Windows.Forms.Control)(object)vScrollBar1).ForeColor = System.Drawing.Color.FromArgb(255, 255, 128);
		vScrollBar1.LargeChange = 8000;
		((System.Windows.Forms.Control)(object)vScrollBar1).Location = new System.Drawing.Point(321, 3);
		vScrollBar1.Maximum = 40000;
		((System.Windows.Forms.Control)(object)vScrollBar1).Name = "vScrollBar1";
		((System.Windows.Forms.Control)(object)vScrollBar1).Size = new System.Drawing.Size(19, 290);
		vScrollBar1.SmallChange = 4000;
		((System.Windows.Forms.Control)(object)vScrollBar1).TabIndex = 4000;
		((System.Windows.Forms.Control)(object)vScrollBar1).Text = "scrollBarEx1";
		vScrollBar1.Scroll += vScrollBar1_Scroll;
		menuStrip1.AllowDrop = true;
		menuStrip1.Font = new Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, 0);
		menuStrip1.Items.AddRange(new ToolStripItem[4] { ファイルToolStripMenuItem, 設定ToolStripMenuItem, 共有ToolStripMenuItem, aboutToolStripMenuItem });
		menuStrip1.Location = new System.Drawing.Point(0, 0);
		menuStrip1.Name = "menuStrip1";
		menuStrip1.Padding = new Padding(5, 1, 0, 1);
		menuStrip1.RenderMode = ToolStripRenderMode.Professional;
		menuStrip1.Size = new System.Drawing.Size(1012, 24);
		menuStrip1.TabIndex = 0;
		menuStrip1.Text = "menuStrip1";
		menuStrip1.Click += menuStrip1_Click;
		menuStrip1.DragDrop += NXMC_VxV_DragDrop;
		menuStrip1.DragEnter += NXMC_VxV_DragEnter;
		ファイルToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[8] { マクロの新規作成ToolStripMenuItem, マクロの読み込みToolStripMenuItem, マクロを上書き保存ToolStripMenuItem, マクロの保存ToolStripMenuItem, toolStripMenuItem1, amiiboの読み込みToolStripMenuItem, toolStripMenuItem5, 終了ToolStripMenuItem });
		ファイルToolStripMenuItem.Name = "ファイルToolStripMenuItem";
		ファイルToolStripMenuItem.Size = new System.Drawing.Size(56, 22);
		ファイルToolStripMenuItem.Text = "ファイル";
		マクロの新規作成ToolStripMenuItem.Name = "マクロの新規作成ToolStripMenuItem";
		マクロの新規作成ToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
		マクロの新規作成ToolStripMenuItem.Text = "マクロの新規作成";
		マクロの新規作成ToolStripMenuItem.Click += マクロの新規作成ToolStripMenuItem_Click;
		マクロの読み込みToolStripMenuItem.Name = "マクロの読み込みToolStripMenuItem";
		マクロの読み込みToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
		マクロの読み込みToolStripMenuItem.Text = "マクロの読み込み";
		マクロの読み込みToolStripMenuItem.Click += マクロの読み込みToolStripMenuItem_Click;
		マクロを上書き保存ToolStripMenuItem.Enabled = false;
		マクロを上書き保存ToolStripMenuItem.Name = "マクロを上書き保存ToolStripMenuItem";
		マクロを上書き保存ToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
		マクロを上書き保存ToolStripMenuItem.Text = "マクロを上書き保存";
		マクロを上書き保存ToolStripMenuItem.Click += マクロを上書き保存ToolStripMenuItem_Click;
		マクロの保存ToolStripMenuItem.Name = "マクロの保存ToolStripMenuItem";
		マクロの保存ToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
		マクロの保存ToolStripMenuItem.Text = "マクロに名前を付けて保存";
		マクロの保存ToolStripMenuItem.Click += マクロの保存ToolStripMenuItem_Click;
		toolStripMenuItem1.Name = "toolStripMenuItem1";
		toolStripMenuItem1.Size = new System.Drawing.Size(205, 6);
		amiiboの読み込みToolStripMenuItem.Name = "amiiboの読み込みToolStripMenuItem";
		amiiboの読み込みToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
		amiiboの読み込みToolStripMenuItem.Text = "Amiiboの読み込み";
		amiiboの読み込みToolStripMenuItem.Click += amiibo読込ToolStripMenuItem_Click;
		toolStripMenuItem5.Name = "toolStripMenuItem5";
		toolStripMenuItem5.Size = new System.Drawing.Size(205, 6);
		終了ToolStripMenuItem.Name = "終了ToolStripMenuItem";
		終了ToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
		終了ToolStripMenuItem.Text = "終了";
		終了ToolStripMenuItem.Click += 終了ToolStripMenuItem_Click;
		設定ToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[6] { BTSetUpToolStripMenuItem, cH552SERIALセットアップToolStripMenuItem, cH552へ書き込みToolStripMenuItem, 接続ToolStripMenuItem, toolStripMenuItem3, 環境設定ToolStripMenuItem1 });
		設定ToolStripMenuItem.Name = "設定ToolStripMenuItem";
		設定ToolStripMenuItem.Size = new System.Drawing.Size(51, 22);
		設定ToolStripMenuItem.Text = "ツール";
		設定ToolStripMenuItem.DropDownOpened += 設定ToolStripMenuItem_DropDownOpened;
		設定ToolStripMenuItem.Click += 設定ToolStripMenuItem_Click;
		BTSetUpToolStripMenuItem.Name = "BTSetUpToolStripMenuItem";
		BTSetUpToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
		BTSetUpToolStripMenuItem.Text = "無線接続セットアップ";
		BTSetUpToolStripMenuItem.Click += BTSetUpToolStripMenuItem_Click;
		cH552SERIALセットアップToolStripMenuItem.Name = "cH552SERIALセットアップToolStripMenuItem";
		cH552SERIALセットアップToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
		cH552SERIALセットアップToolStripMenuItem.Text = "CH552-SERIALセットアップ";
		cH552SERIALセットアップToolStripMenuItem.Click += cH552SERIALセットアップToolStripMenuItem_Click;
		cH552へ書き込みToolStripMenuItem.Name = "cH552へ書き込みToolStripMenuItem";
		cH552へ書き込みToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
		cH552へ書き込みToolStripMenuItem.Text = "CH552へ書き込み";
		cH552へ書き込みToolStripMenuItem.Click += cH552へ書き込みToolStripMenuItem_Click;
		接続ToolStripMenuItem.Name = "接続ToolStripMenuItem";
		接続ToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
		接続ToolStripMenuItem.Text = "接続";
		接続ToolStripMenuItem.DropDownOpened += 接続ToolStripMenuItem_DropDownOpened;
		接続ToolStripMenuItem.Click += 接続ToolStripMenuItem_Click;
		toolStripMenuItem3.Name = "toolStripMenuItem3";
		toolStripMenuItem3.Size = new System.Drawing.Size(203, 6);
		環境設定ToolStripMenuItem1.Name = "環境設定ToolStripMenuItem1";
		環境設定ToolStripMenuItem1.Size = new System.Drawing.Size(206, 22);
		環境設定ToolStripMenuItem1.Text = "オプション";
		環境設定ToolStripMenuItem1.Click += 環境設定ToolStripMenuItem_Click;
		共有ToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[1] { マクロ共有サーバーに接続ToolStripMenuItem });
		共有ToolStripMenuItem.Name = "共有ToolStripMenuItem";
		共有ToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
		共有ToolStripMenuItem.Text = "マクロ共有サーバー";
		マクロ共有サーバーに接続ToolStripMenuItem.Name = "マクロ共有サーバーに接続ToolStripMenuItem";
		マクロ共有サーバーに接続ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
		マクロ共有サーバーに接続ToolStripMenuItem.Text = "接続";
		マクロ共有サーバーに接続ToolStripMenuItem.Click += マクロ共有サーバーに接続ToolStripMenuItem_Click;
		aboutToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[4] { バージョン情報ToolStripMenuItem, discordサーバーToolStripMenuItem, readmeToolStripMenuItem, ヘルプToolStripMenuItem });
		aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
		aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 22);
		aboutToolStripMenuItem.Text = "About";
		バージョン情報ToolStripMenuItem.Name = "バージョン情報ToolStripMenuItem";
		バージョン情報ToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
		バージョン情報ToolStripMenuItem.Text = "バージョン情報";
		バージョン情報ToolStripMenuItem.Click += バージョン情報ToolStripMenuItem_Click;
		discordサーバーToolStripMenuItem.Name = "discordサーバーToolStripMenuItem";
		discordサーバーToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
		discordサーバーToolStripMenuItem.Text = "Discordサーバー";
		discordサーバーToolStripMenuItem.Click += discordサーバーToolStripMenuItem_Click;
		readmeToolStripMenuItem.Name = "readmeToolStripMenuItem";
		readmeToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
		readmeToolStripMenuItem.Text = "Readme";
		readmeToolStripMenuItem.Click += readmeToolStripMenuItem_Click;
		ヘルプToolStripMenuItem.Name = "ヘルプToolStripMenuItem";
		ヘルプToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
		ヘルプToolStripMenuItem.Text = "ヘルプ";
		ヘルプToolStripMenuItem.Click += ヘルプToolStripMenuItem_Click;
		((System.Windows.Forms.Control)(object)button6).BackgroundImageLayout = ImageLayout.Zoom;
		((ButtonBase)(object)button6).FlatAppearance.BorderSize = 0;
		((System.Windows.Forms.Control)(object)button6).ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		button6.Image = Resources.B1;
		((System.Windows.Forms.Control)(object)button6).Location = new System.Drawing.Point(7, 5);
		((System.Windows.Forms.Control)(object)button6).Margin = new Padding(3, 2, 3, 2);
		((System.Windows.Forms.Control)(object)button6).Name = "button6";
		((System.Windows.Forms.Control)(object)button6).Size = new System.Drawing.Size(75, 23);
		((System.Windows.Forms.Control)(object)button6).TabIndex = 0;
		((System.Windows.Forms.Control)(object)button6).Text = "実行";
		((ButtonBase)(object)button6).TextImageRelation = TextImageRelation.ImageBeforeText;
		((ButtonBase)(object)button6).UseVisualStyleBackColor = true;
		((System.Windows.Forms.Control)(object)button6).Click += button6_Click;
		((System.Windows.Forms.Control)(object)button6).KeyDown += tabControl1_KeyDown;
		((System.Windows.Forms.Control)(object)button6).PreviewKeyDown += button6_PreviewKeyDown;
		((System.Windows.Forms.Control)(object)button4).Anchor = AnchorStyles.Top | AnchorStyles.Right;
		((ButtonBase)(object)button4).FlatAppearance.BorderSize = 0;
		((System.Windows.Forms.Control)(object)button4).ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		button4.Image = Resources.B2;
		((System.Windows.Forms.Control)(object)button4).Location = new System.Drawing.Point(198, 5);
		((System.Windows.Forms.Control)(object)button4).Margin = new Padding(3, 2, 3, 2);
		((System.Windows.Forms.Control)(object)button4).Name = "button4";
		((System.Windows.Forms.Control)(object)button4).Size = new System.Drawing.Size(75, 23);
		((System.Windows.Forms.Control)(object)button4).TabIndex = 1;
		((System.Windows.Forms.Control)(object)button4).Text = "記録";
		((ButtonBase)(object)button4).TextImageRelation = TextImageRelation.ImageBeforeText;
		((ButtonBase)(object)button4).UseVisualStyleBackColor = true;
		((System.Windows.Forms.Control)(object)button4).Click += button4_Click;
		((System.Windows.Forms.Control)(object)button4).KeyDown += tabControl1_KeyDown;
		((System.Windows.Forms.Control)(object)button4).PreviewKeyDown += button6_PreviewKeyDown;
		((System.Windows.Forms.Control)(object)button3).Anchor = AnchorStyles.Top | AnchorStyles.Right;
		((ButtonBase)(object)button3).FlatAppearance.BorderSize = 0;
		((System.Windows.Forms.Control)(object)button3).ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		button3.Image = (System.Drawing.Image)componentResourceManager.GetObject("button3.Image");
		((System.Windows.Forms.Control)(object)button3).Location = new System.Drawing.Point(279, 5);
		((System.Windows.Forms.Control)(object)button3).Margin = new Padding(3, 2, 3, 2);
		((System.Windows.Forms.Control)(object)button3).Name = "button3";
		((System.Windows.Forms.Control)(object)button3).Size = new System.Drawing.Size(75, 23);
		((System.Windows.Forms.Control)(object)button3).TabIndex = 2;
		((System.Windows.Forms.Control)(object)button3).Text = "入力補助";
		((ButtonBase)(object)button3).UseVisualStyleBackColor = true;
		((System.Windows.Forms.Control)(object)button3).Click += button3_Click;
		((System.Windows.Forms.Control)(object)button3).KeyDown += tabControl1_KeyDown;
		((System.Windows.Forms.Control)(object)button3).PreviewKeyDown += button6_PreviewKeyDown;
		panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		panel1.BackColor = System.Drawing.Color.Red;
		panel1.Controls.Add((System.Windows.Forms.Control)(object)hScrollBar1);
		panel1.Controls.Add((System.Windows.Forms.Control)(object)vScrollBar1);
		panel1.Controls.Add(label1);
		panel1.Controls.Add(elementHost1);
		panel1.Location = new System.Drawing.Point(6, 32);
		panel1.Margin = new Padding(3, 2, 3, 2);
		panel1.Name = "panel1";
		panel1.Size = new System.Drawing.Size(348, 316);
		panel1.TabIndex = 3;
		((System.Windows.Forms.Control)(object)hScrollBar1).ForeColor = System.Drawing.Color.FromArgb(255, 255, 128);
		hScrollBar1.LargeChange = 8000;
		((System.Windows.Forms.Control)(object)hScrollBar1).Location = new System.Drawing.Point(5, 298);
		hScrollBar1.Maximum = 40000;
		((System.Windows.Forms.Control)(object)hScrollBar1).Name = "hScrollBar1";
		hScrollBar1.Orientation = (ScrollBarOrientation)0;
		((System.Windows.Forms.Control)(object)hScrollBar1).Size = new System.Drawing.Size(290, 19);
		hScrollBar1.SmallChange = 4000;
		((System.Windows.Forms.Control)(object)hScrollBar1).TabIndex = 4001;
		((System.Windows.Forms.Control)(object)hScrollBar1).Text = "scrollBarEx1";
		label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		label1.BackColor = System.Drawing.SystemColors.ScrollBar;
		label1.Location = new System.Drawing.Point(-342, 299);
		label1.Margin = new Padding(0);
		label1.Name = "label1";
		label1.Size = new System.Drawing.Size(100, 23);
		label1.TabIndex = 3;
		elementHost1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		elementHost1.Font = new Font("Consolas", 9f, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, 0);
		elementHost1.Location = new System.Drawing.Point(1, 1);
		elementHost1.Margin = new Padding(3, 2, 3, 2);
		elementHost1.Name = "elementHost1";
		elementHost1.Size = new System.Drawing.Size(346, 313);
		elementHost1.TabIndex = 0;
		elementHost1.Text = "elementHost1";
		elementHost1.Child = null;
		tabControl1.ActiveColor = System.Drawing.Color.FromArgb(0, 122, 204);
		((System.Windows.Forms.Control)(object)tabControl1).AllowDrop = true;
		tabControl1.BackTabColor = System.Drawing.Color.FromArgb(28, 28, 28);
		tabControl1.BorderColor = System.Drawing.Color.FromArgb(30, 30, 30);
		tabControl1.ClosingButtonColor = System.Drawing.Color.WhiteSmoke;
		tabControl1.ClosingMessage = null;
		((System.Windows.Forms.Control)(object)tabControl1).Controls.Add(tabPage2);
		((System.Windows.Forms.Control)(object)tabControl1).Controls.Add(tabPage4);
		((System.Windows.Forms.Control)(object)tabControl1).Controls.Add(tabPage3);
		((System.Windows.Forms.Control)(object)tabControl1).Controls.Add(tabPage1);
		((System.Windows.Forms.Control)(object)tabControl1).Controls.Add(tabPageMHXX);
		tabControl1.DeActiveColor = System.Drawing.Color.FromArgb(63, 63, 70);
		((System.Windows.Forms.Control)(object)tabControl1).Dock = DockStyle.Fill;
		tabControl1.EnabledTabDrag = false;
		((System.Windows.Forms.Control)(object)tabControl1).Font = new Font("Segoe UI", 8.25f, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, 0);
		tabControl1.HeaderColor = System.Drawing.Color.FromArgb(45, 45, 48);
		tabControl1.HorizontalLineColor = System.Drawing.Color.FromArgb(0, 122, 204);
		((System.Windows.Forms.TabControl)(object)tabControl1).ItemSize = new System.Drawing.Size(240, 16);
		((System.Windows.Forms.Control)(object)tabControl1).Location = new System.Drawing.Point(0, 0);
		((System.Windows.Forms.Control)(object)tabControl1).MinimumSize = new System.Drawing.Size(360, 150);
		((System.Windows.Forms.Control)(object)tabControl1).Name = "tabControl1";
		((System.Windows.Forms.TabControl)(object)tabControl1).SelectedIndex = 0;
		tabControl1.SelectedTextColor = System.Drawing.Color.FromArgb(255, 255, 255);
		tabControl1.ShowClosingButton = false;
		tabControl1.ShowClosingMessage = false;
		((System.Windows.Forms.Control)(object)tabControl1).Size = new System.Drawing.Size(368, 380);
		((System.Windows.Forms.Control)(object)tabControl1).TabIndex = 0;
		tabControl1.TextColor = System.Drawing.Color.FromArgb(255, 255, 255);
		((System.Windows.Forms.Control)(object)tabControl1).SizeChanged += tabControl1_SizeChanged;
		((System.Windows.Forms.Control)(object)tabControl1).DragDrop += NXMC_VxV_DragDrop;
		((System.Windows.Forms.Control)(object)tabControl1).DragEnter += NXMC_VxV_DragEnter;
		((System.Windows.Forms.Control)(object)tabControl1).KeyDown += tabControl1_KeyDown;
		tabPage2.BackColor = System.Drawing.Color.FromArgb(33, 33, 35);
		tabPage2.Controls.Add((System.Windows.Forms.Control)(object)button6);
		tabPage2.Controls.Add((System.Windows.Forms.Control)(object)button4);
		tabPage2.Controls.Add(panel1);
		tabPage2.Controls.Add((System.Windows.Forms.Control)(object)button3);
		tabPage2.Location = new System.Drawing.Point(4, 20);
		tabPage2.Name = "tabPage2";
		tabPage2.Padding = new Padding(3);
		tabPage2.Size = new System.Drawing.Size(360, 356);
		tabPage2.TabIndex = 1;
		tabPage2.Text = "コード";
		tabPage2.Click += tabPage2_Click;
		tabPage4.BackColor = System.Drawing.Color.FromArgb(33, 33, 35);
		tabPage4.Controls.Add((System.Windows.Forms.Control)(object)ghostPanel6);
		tabPage4.Location = new System.Drawing.Point(4, 20);
		tabPage4.Name = "tabPage4";
		tabPage4.Padding = new Padding(3);
		tabPage4.Size = new System.Drawing.Size(360, 356);
		tabPage4.TabIndex = 4;
		tabPage4.Text = "ログ";
		((System.Windows.Forms.Control)(object)ghostPanel6).Controls.Add((System.Windows.Forms.Control)(object)buttonEx5);
		((System.Windows.Forms.Control)(object)ghostPanel6).Controls.Add((System.Windows.Forms.Control)(object)buttonEx4);
		((System.Windows.Forms.Control)(object)ghostPanel6).Controls.Add((System.Windows.Forms.Control)(object)groupBoxEx1);
		((System.Windows.Forms.Control)(object)ghostPanel6).Dock = DockStyle.Fill;
		((System.Windows.Forms.Control)(object)ghostPanel6).Location = new System.Drawing.Point(3, 3);
		((System.Windows.Forms.Control)(object)ghostPanel6).Name = "ghostPanel6";
		((System.Windows.Forms.Control)(object)ghostPanel6).Size = new System.Drawing.Size(354, 350);
		((System.Windows.Forms.Control)(object)ghostPanel6).TabIndex = 0;
		((System.Windows.Forms.Control)(object)ghostPanel6).Paint += ghostPanel6_Paint;
		((System.Windows.Forms.Control)(object)buttonEx5).BackgroundImageLayout = ImageLayout.Zoom;
		((ButtonBase)(object)buttonEx5).FlatAppearance.BorderSize = 0;
		((System.Windows.Forms.Control)(object)buttonEx5).ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		buttonEx5.Image = Resources.B1;
		((System.Windows.Forms.Control)(object)buttonEx5).Location = new System.Drawing.Point(4, 2);
		((System.Windows.Forms.Control)(object)buttonEx5).Margin = new Padding(3, 2, 3, 2);
		((System.Windows.Forms.Control)(object)buttonEx5).Name = "buttonEx5";
		((System.Windows.Forms.Control)(object)buttonEx5).Size = new System.Drawing.Size(75, 23);
		((System.Windows.Forms.Control)(object)buttonEx5).TabIndex = 5;
		((System.Windows.Forms.Control)(object)buttonEx5).Text = "実行";
		((ButtonBase)(object)buttonEx5).TextImageRelation = TextImageRelation.ImageBeforeText;
		((ButtonBase)(object)buttonEx5).UseVisualStyleBackColor = true;
		((System.Windows.Forms.Control)(object)buttonEx5).Click += button6_Click;
		((System.Windows.Forms.Control)(object)buttonEx5).KeyDown += tabControl1_KeyDown;
		((System.Windows.Forms.Control)(object)buttonEx4).Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		((ButtonBase)(object)buttonEx4).FlatAppearance.BorderSize = 0;
		((System.Windows.Forms.Control)(object)buttonEx4).ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		buttonEx4.Image = null;
		((System.Windows.Forms.Control)(object)buttonEx4).Location = new System.Drawing.Point(3, 322);
		((System.Windows.Forms.Control)(object)buttonEx4).Margin = new Padding(3, 2, 3, 2);
		((System.Windows.Forms.Control)(object)buttonEx4).Name = "buttonEx4";
		((System.Windows.Forms.Control)(object)buttonEx4).Size = new System.Drawing.Size(348, 23);
		((System.Windows.Forms.Control)(object)buttonEx4).TabIndex = 4;
		((System.Windows.Forms.Control)(object)buttonEx4).Text = "ログのクリア";
		((ButtonBase)(object)buttonEx4).TextImageRelation = TextImageRelation.ImageBeforeText;
		((ButtonBase)(object)buttonEx4).UseVisualStyleBackColor = true;
		((System.Windows.Forms.Control)(object)buttonEx4).Click += buttonEx4_Click;
		((System.Windows.Forms.Control)(object)groupBoxEx1).Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		groupBoxEx1.BorderColor = System.Drawing.Color.FromArgb(65, 65, 67);
		((System.Windows.Forms.Control)(object)groupBoxEx1).Controls.Add((System.Windows.Forms.Control)(object)scrollBarEx1);
		((System.Windows.Forms.Control)(object)groupBoxEx1).Controls.Add(textBox1);
		((System.Windows.Forms.Control)(object)groupBoxEx1).Location = new System.Drawing.Point(3, 30);
		((System.Windows.Forms.Control)(object)groupBoxEx1).Name = "groupBoxEx1";
		((System.Windows.Forms.Control)(object)groupBoxEx1).Padding = new Padding(0);
		((System.Windows.Forms.Control)(object)groupBoxEx1).Size = new System.Drawing.Size(348, 287);
		((System.Windows.Forms.Control)(object)groupBoxEx1).TabIndex = 2;
		((System.Windows.Forms.GroupBox)(object)groupBoxEx1).TabStop = false;
		((System.Windows.Forms.Control)(object)scrollBarEx1).Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
		scrollBarEx1.LargeChange = 8000;
		((System.Windows.Forms.Control)(object)scrollBarEx1).Location = new System.Drawing.Point(329, 1);
		scrollBarEx1.Maximum = 40000;
		((System.Windows.Forms.Control)(object)scrollBarEx1).Name = "scrollBarEx1";
		((System.Windows.Forms.Control)(object)scrollBarEx1).Size = new System.Drawing.Size(19, 286);
		scrollBarEx1.SmallChange = 4000;
		((System.Windows.Forms.Control)(object)scrollBarEx1).TabIndex = 1;
		((System.Windows.Forms.Control)(object)scrollBarEx1).Text = "scrollBarEx1";
		scrollBarEx1.Scroll += scrollBarEx1_Scroll;
		textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		textBox1.BorderStyle = BorderStyle.None;
		textBox1.Location = new System.Drawing.Point(6, 7);
		textBox1.Multiline = true;
		textBox1.Name = "textBox1";
		textBox1.ReadOnly = true;
		textBox1.ScrollBars = ScrollBars.Vertical;
		textBox1.Size = new System.Drawing.Size(342, 274);
		textBox1.TabIndex = 0;
		textBox1.TextChanged += textBox1_TextChanged;
		textBox1.Layout += textBox1_Layout;
		tabPage3.AllowDrop = true;
		tabPage3.BackColor = System.Drawing.Color.FromArgb(33, 33, 35);
		tabPage3.Controls.Add((System.Windows.Forms.Control)(object)tabControlEx1);
		tabPage3.Location = new System.Drawing.Point(4, 20);
		tabPage3.Name = "tabPage3";
		tabPage3.Size = new System.Drawing.Size(360, 356);
		tabPage3.TabIndex = 2;
		tabPage3.Text = "リソース";
		tabPage3.DragEnter += tabPage3_DragEnter;
		tabControlEx1.ActiveColor = System.Drawing.Color.FromArgb(0, 122, 204);
		((System.Windows.Forms.Control)(object)tabControlEx1).AllowDrop = true;
		tabControlEx1.BackTabColor = System.Drawing.Color.FromArgb(28, 28, 28);
		tabControlEx1.BorderColor = System.Drawing.Color.FromArgb(30, 30, 30);
		tabControlEx1.ClosingButtonColor = System.Drawing.Color.WhiteSmoke;
		tabControlEx1.ClosingMessage = null;
		((System.Windows.Forms.Control)(object)tabControlEx1).Controls.Add(tabPage5);
		((System.Windows.Forms.Control)(object)tabControlEx1).Controls.Add(tabPage6);
		tabControlEx1.DeActiveColor = System.Drawing.Color.FromArgb(63, 63, 70);
		((System.Windows.Forms.Control)(object)tabControlEx1).Dock = DockStyle.Fill;
		tabControlEx1.EnabledTabDrag = false;
		tabControlEx1.HeaderColor = System.Drawing.Color.FromArgb(45, 45, 48);
		tabControlEx1.HorizontalLineColor = System.Drawing.Color.FromArgb(0, 122, 204);
		((System.Windows.Forms.TabControl)(object)tabControlEx1).ItemSize = new System.Drawing.Size(240, 16);
		((System.Windows.Forms.Control)(object)tabControlEx1).Location = new System.Drawing.Point(0, 0);
		((System.Windows.Forms.Control)(object)tabControlEx1).Name = "tabControlEx1";
		((System.Windows.Forms.TabControl)(object)tabControlEx1).SelectedIndex = 0;
		tabControlEx1.SelectedTextColor = System.Drawing.Color.FromArgb(255, 255, 255);
		tabControlEx1.ShowClosingButton = false;
		tabControlEx1.ShowClosingMessage = false;
		((System.Windows.Forms.Control)(object)tabControlEx1).Size = new System.Drawing.Size(360, 356);
		((System.Windows.Forms.Control)(object)tabControlEx1).TabIndex = 1;
		tabControlEx1.TextColor = System.Drawing.Color.FromArgb(255, 255, 255);
		tabPage5.BackColor = System.Drawing.Color.FromArgb(33, 33, 35);
		tabPage5.Controls.Add((System.Windows.Forms.Control)(object)scrollBarEx3);
		tabPage5.Controls.Add(flowLayoutPanel1);
		tabPage5.Location = new System.Drawing.Point(4, 20);
		tabPage5.Name = "tabPage5";
		tabPage5.Size = new System.Drawing.Size(352, 332);
		tabPage5.TabIndex = 0;
		tabPage5.Text = "画像";
		((System.Windows.Forms.Control)(object)scrollBarEx3).Dock = DockStyle.Right;
		scrollBarEx3.LargeChange = 8000;
		((System.Windows.Forms.Control)(object)scrollBarEx3).Location = new System.Drawing.Point(333, 0);
		scrollBarEx3.Maximum = 40000;
		((System.Windows.Forms.Control)(object)scrollBarEx3).Name = "scrollBarEx3";
		((System.Windows.Forms.Control)(object)scrollBarEx3).Size = new System.Drawing.Size(19, 332);
		scrollBarEx3.SmallChange = 4000;
		((System.Windows.Forms.Control)(object)scrollBarEx3).TabIndex = 1;
		((System.Windows.Forms.Control)(object)scrollBarEx3).Text = "scrollBarEx3";
		scrollBarEx3.Scroll += scrollBarEx3_Scroll;
		flowLayoutPanel1.AllowDrop = true;
		flowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		flowLayoutPanel1.AutoScroll = true;
		flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
		flowLayoutPanel1.Name = "flowLayoutPanel1";
		flowLayoutPanel1.Size = new System.Drawing.Size(352, 332);
		flowLayoutPanel1.TabIndex = 0;
		flowLayoutPanel1.Click += flowLayoutPanel1_Click;
		flowLayoutPanel1.DragDrop += flowLayoutPanel1_DragDrop;
		flowLayoutPanel1.DragEnter += flowLayoutPanel1_DragEnter;
		flowLayoutPanel1.Paint += flowLayoutPanel1_Paint;
		flowLayoutPanel1.Resize += flowLayoutPanel1_Resize;
		tabPage6.BackColor = System.Drawing.Color.FromArgb(33, 33, 35);
		tabPage6.Controls.Add((System.Windows.Forms.Control)(object)scrollBarEx4);
		tabPage6.Controls.Add(flowLayoutPanel3);
		tabPage6.Location = new System.Drawing.Point(4, 20);
		tabPage6.Name = "tabPage6";
		tabPage6.Size = new System.Drawing.Size(352, 332);
		tabPage6.TabIndex = 1;
		tabPage6.Text = "ファイル";
		((System.Windows.Forms.Control)(object)scrollBarEx4).Dock = DockStyle.Right;
		scrollBarEx4.LargeChange = 8000;
		((System.Windows.Forms.Control)(object)scrollBarEx4).Location = new System.Drawing.Point(333, 0);
		scrollBarEx4.Maximum = 40000;
		((System.Windows.Forms.Control)(object)scrollBarEx4).Name = "scrollBarEx4";
		((System.Windows.Forms.Control)(object)scrollBarEx4).Size = new System.Drawing.Size(19, 332);
		scrollBarEx4.SmallChange = 4000;
		((System.Windows.Forms.Control)(object)scrollBarEx4).TabIndex = 2;
		((System.Windows.Forms.Control)(object)scrollBarEx4).Text = "scrollBarEx4";
		scrollBarEx4.Scroll += scrollBarEx4_Scroll;
		flowLayoutPanel3.AllowDrop = true;
		flowLayoutPanel3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		flowLayoutPanel3.AutoScroll = true;
		flowLayoutPanel3.Enabled = false;
		flowLayoutPanel3.Location = new System.Drawing.Point(0, 0);
		flowLayoutPanel3.Name = "flowLayoutPanel3";
		flowLayoutPanel3.Size = new System.Drawing.Size(352, 332);
		flowLayoutPanel3.TabIndex = 1;
		flowLayoutPanel3.Visible = false;
		flowLayoutPanel3.DragDrop += flowLayoutPanel3_DragDrop;
		flowLayoutPanel3.DragEnter += flowLayoutPanel1_DragEnter;
		flowLayoutPanel3.Resize += flowLayoutPanel3_Resize;
		tabPage1.BackColor = System.Drawing.Color.FromArgb(33, 33, 35);
		tabPage1.Controls.Add((System.Windows.Forms.Control)(object)scrollBarEx2);
		tabPage1.Controls.Add(flowLayoutPanel2);
		tabPage1.Location = new System.Drawing.Point(4, 20);
		tabPage1.Name = "tabPage1";
		tabPage1.Size = new System.Drawing.Size(360, 356);
		tabPage1.TabIndex = 3;
		tabPage1.Text = "ショートカット";
		// ── MHXX お守り乱数タブ ──
		tabPageMHXX.BackColor = System.Drawing.Color.FromArgb(33, 33, 35);
		tabPageMHXX.Controls.Add(mhxxCharmRNG1);
		tabPageMHXX.Location = new System.Drawing.Point(4, 20);
		tabPageMHXX.Name = "tabPageMHXX";
		tabPageMHXX.Padding = new Padding(0);
		tabPageMHXX.Size = new System.Drawing.Size(360, 356);
		tabPageMHXX.TabIndex = 4;
		tabPageMHXX.Text = "MHXX お守り";
		mhxxCharmRNG1.Dock = DockStyle.Fill;
		mhxxCharmRNG1.Location = new System.Drawing.Point(0, 0);
		mhxxCharmRNG1.Name = "mhxxCharmRNG1";
		mhxxCharmRNG1.Size = new System.Drawing.Size(360, 356);
		mhxxCharmRNG1.TabIndex = 0;
		((System.Windows.Forms.Control)(object)scrollBarEx2).Dock = DockStyle.Right;
		scrollBarEx2.LargeChange = 8000;
		((System.Windows.Forms.Control)(object)scrollBarEx2).Location = new System.Drawing.Point(341, 0);
		scrollBarEx2.Maximum = 0;
		((System.Windows.Forms.Control)(object)scrollBarEx2).Name = "scrollBarEx2";
		((System.Windows.Forms.Control)(object)scrollBarEx2).Size = new System.Drawing.Size(19, 356);
		scrollBarEx2.SmallChange = 4000;
		((System.Windows.Forms.Control)(object)scrollBarEx2).TabIndex = 0;
		((System.Windows.Forms.Control)(object)scrollBarEx2).Text = "scrollBarEx2";
		scrollBarEx2.Scroll += scrollBarEx2_Scroll;
		flowLayoutPanel2.AllowDrop = true;
		flowLayoutPanel2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		flowLayoutPanel2.AutoScroll = true;
		flowLayoutPanel2.Location = new System.Drawing.Point(0, 0);
		flowLayoutPanel2.Name = "flowLayoutPanel2";
		flowLayoutPanel2.Size = new System.Drawing.Size(360, 356);
		flowLayoutPanel2.TabIndex = 1;
		flowLayoutPanel2.Scroll += flowLayoutPanel2_Scroll;
		flowLayoutPanel2.DragDrop += flowLayoutPanel2_DragDrop;
		flowLayoutPanel2.DragEnter += flowLayoutPanel2_DragEnter;
		flowLayoutPanel2.MouseEnter += flowLayoutPanel2_MouseEnter;
		flowLayoutPanel2.Resize += flowLayoutPanel2_Resize;
		panel2.Controls.Add((System.Windows.Forms.Control)(object)ghostPanel4);
		panel2.Controls.Add(panel3);
		panel2.Controls.Add((System.Windows.Forms.Control)(object)ghostPanel5);
		panel2.Controls.Add(menuStrip1);
		panel2.Dock = DockStyle.Fill;
		panel2.Location = new System.Drawing.Point(4, 29);
		panel2.Margin = new Padding(0);
		panel2.Name = "panel2";
		panel2.Size = new System.Drawing.Size(1012, 450);
		panel2.TabIndex = 10;
		panel2.Click += panel2_Click;
		((System.Windows.Forms.Control)(object)ghostPanel4).Controls.Add((System.Windows.Forms.Control)(object)groupBox1);
		((System.Windows.Forms.Control)(object)ghostPanel4).Controls.Add(splitter1);
		((System.Windows.Forms.Control)(object)ghostPanel4).Controls.Add(panel4);
		((System.Windows.Forms.Control)(object)ghostPanel4).Dock = DockStyle.Fill;
		((System.Windows.Forms.Control)(object)ghostPanel4).Location = new System.Drawing.Point(0, 53);
		((System.Windows.Forms.Control)(object)ghostPanel4).Name = "ghostPanel4";
		((System.Windows.Forms.Control)(object)ghostPanel4).Size = new System.Drawing.Size(1012, 380);
		((System.Windows.Forms.Control)(object)ghostPanel4).TabIndex = 7;
		splitter1.Location = new System.Drawing.Point(368, 0);
		splitter1.Name = "splitter1";
		splitter1.Size = new System.Drawing.Size(3, 380);
		splitter1.TabIndex = 7;
		splitter1.TabStop = false;
		panel4.Controls.Add((System.Windows.Forms.Control)(object)tabControl1);
		panel4.Dock = DockStyle.Left;
		panel4.Location = new System.Drawing.Point(0, 0);
		panel4.MinimumSize = new System.Drawing.Size(368, 0);
		panel4.Name = "panel4";
		panel4.Size = new System.Drawing.Size(368, 380);
		panel4.TabIndex = 6;
		panel3.BackColor = System.Drawing.Color.Blue;
		panel3.Controls.Add(label2);
		panel3.Dock = DockStyle.Bottom;
		panel3.Location = new System.Drawing.Point(0, 433);
		panel3.Name = "panel3";
		panel3.Size = new System.Drawing.Size(1012, 17);
		panel3.TabIndex = 5;
		label2.Dock = DockStyle.Fill;
		label2.ForeColor = System.Drawing.Color.White;
		label2.Location = new System.Drawing.Point(0, 0);
		label2.Name = "label2";
		label2.Size = new System.Drawing.Size(1012, 17);
		label2.TabIndex = 0;
		label2.Text = "準備完了";
		label2.TextAlign = ContentAlignment.BottomLeft;
		label2.Click += label2_Click;
		((System.Windows.Forms.Control)(object)ghostPanel5).AllowDrop = true;
		((System.Windows.Forms.Control)(object)ghostPanel5).Controls.Add(label4);
		((System.Windows.Forms.Control)(object)ghostPanel5).Controls.Add((System.Windows.Forms.Control)(object)macroSubDirCmb);
		((System.Windows.Forms.Control)(object)ghostPanel5).Controls.Add((System.Windows.Forms.Control)(object)buttonEx3);
		((System.Windows.Forms.Control)(object)ghostPanel5).Controls.Add((System.Windows.Forms.Control)(object)ComPortList);
		((System.Windows.Forms.Control)(object)ghostPanel5).Controls.Add((System.Windows.Forms.Control)(object)buttonEx2);
		((System.Windows.Forms.Control)(object)ghostPanel5).Controls.Add(label3);
		((System.Windows.Forms.Control)(object)ghostPanel5).Controls.Add((System.Windows.Forms.Control)(object)ComConnect);
		((System.Windows.Forms.Control)(object)ghostPanel5).Controls.Add((System.Windows.Forms.Control)(object)macroSelCmb);
		((System.Windows.Forms.Control)(object)ghostPanel5).Dock = DockStyle.Top;
		((System.Windows.Forms.Control)(object)ghostPanel5).Location = new System.Drawing.Point(0, 24);
		((System.Windows.Forms.Control)(object)ghostPanel5).Name = "ghostPanel5";
		((System.Windows.Forms.Control)(object)ghostPanel5).Size = new System.Drawing.Size(1012, 29);
		((System.Windows.Forms.Control)(object)ghostPanel5).TabIndex = 8;
		((System.Windows.Forms.Control)(object)ghostPanel5).DragDrop += NXMC_VxV_DragDrop;
		((System.Windows.Forms.Control)(object)ghostPanel5).DragEnter += NXMC_VxV_DragEnter;
		label4.AutoSize = true;
		label4.Location = new System.Drawing.Point(154, 5);
		label4.Margin = new Padding(10);
		label4.Name = "label4";
		label4.Size = new System.Drawing.Size(11, 13);
		label4.TabIndex = 11;
		label4.Text = "/";
		((System.Windows.Forms.Control)(object)macroSubDirCmb).BackColor = System.Drawing.Color.FromArgb(33, 33, 35);
		macroSubDirCmb.BorderColor = System.Drawing.Color.FromArgb(65, 65, 67);
		macroSubDirCmb.BorderStyle = ButtonBorderStyle.Solid;
		macroSubDirCmb.ContentsCheck = true;
		((System.Windows.Forms.ComboBox)(object)macroSubDirCmb).DrawMode = DrawMode.OwnerDrawFixed;
		macroSubDirCmb.DropDownStyle = ComboBoxStyle.DropDownList;
		((System.Windows.Forms.ComboBox)(object)macroSubDirCmb).FlatStyle = FlatStyle.Flat;
		((System.Windows.Forms.Control)(object)macroSubDirCmb).ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		((ListControl)(object)macroSubDirCmb).FormattingEnabled = true;
		((System.Windows.Forms.ComboBox)(object)macroSubDirCmb).Items.AddRange(new object[1] { "Default" });
		((System.Windows.Forms.Control)(object)macroSubDirCmb).Location = new System.Drawing.Point(8, 0);
		((System.Windows.Forms.Control)(object)macroSubDirCmb).Name = "macroSubDirCmb";
		((System.Windows.Forms.Control)(object)macroSubDirCmb).Size = new System.Drawing.Size(133, 23);
		((System.Windows.Forms.Control)(object)macroSubDirCmb).TabIndex = 0;
		((System.Windows.Forms.ComboBox)(object)macroSubDirCmb).SelectedIndexChanged += macroSubDirCmb_SelectedIndexChanged;
		((System.Windows.Forms.ComboBox)(object)macroSubDirCmb).TextUpdate += macroSubDirCmb_TextUpdate;
		((ButtonBase)(object)buttonEx3).FlatAppearance.BorderSize = 0;
		((System.Windows.Forms.Control)(object)buttonEx3).ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		buttonEx3.Image = Resources.B7;
		((System.Windows.Forms.Control)(object)buttonEx3).Location = new System.Drawing.Point(396, 0);
		((System.Windows.Forms.Control)(object)buttonEx3).Margin = new Padding(3, 2, 3, 2);
		((System.Windows.Forms.Control)(object)buttonEx3).Name = "buttonEx3";
		((System.Windows.Forms.Control)(object)buttonEx3).Size = new System.Drawing.Size(31, 23);
		((System.Windows.Forms.Control)(object)buttonEx3).TabIndex = 2;
		((ButtonBase)(object)buttonEx3).TextImageRelation = TextImageRelation.ImageBeforeText;
		((ButtonBase)(object)buttonEx3).UseVisualStyleBackColor = true;
		((System.Windows.Forms.Control)(object)buttonEx3).Click += buttonEx3_Click;
		((System.Windows.Forms.Control)(object)ComPortList).BackColor = System.Drawing.Color.FromArgb(33, 33, 35);
		ComPortList.BorderColor = System.Drawing.Color.FromArgb(65, 65, 67);
		ComPortList.BorderStyle = ButtonBorderStyle.Solid;
		ComPortList.ContentsCheck = true;
		((System.Windows.Forms.ComboBox)(object)ComPortList).DrawMode = DrawMode.OwnerDrawFixed;
		ComPortList.DropDownStyle = ComboBoxStyle.DropDownList;
		((System.Windows.Forms.ComboBox)(object)ComPortList).FlatStyle = FlatStyle.Flat;
		((System.Windows.Forms.Control)(object)ComPortList).ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		((ListControl)(object)ComPortList).FormattingEnabled = true;
		((System.Windows.Forms.ComboBox)(object)ComPortList).Items.AddRange(new object[1] { "接続先を選択" });
		((System.Windows.Forms.Control)(object)ComPortList).Location = new System.Drawing.Point(545, 0);
		((System.Windows.Forms.Control)(object)ComPortList).Name = "ComPortList";
		((System.Windows.Forms.Control)(object)ComPortList).Size = new System.Drawing.Size(156, 23);
		((System.Windows.Forms.Control)(object)ComPortList).TabIndex = 4;
		((System.Windows.Forms.ComboBox)(object)ComPortList).DropDown += ComPortList_DropDown;
		((System.Windows.Forms.Control)(object)ComPortList).Click += comboBoxEx2_Click;
		((System.Windows.Forms.Control)(object)ComPortList).Enter += comboBoxEx2_Enter;
		((ButtonBase)(object)buttonEx2).FlatAppearance.BorderSize = 0;
		((System.Windows.Forms.Control)(object)buttonEx2).ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		buttonEx2.Image = Resources.B6;
		((System.Windows.Forms.Control)(object)buttonEx2).Location = new System.Drawing.Point(433, 0);
		((System.Windows.Forms.Control)(object)buttonEx2).Margin = new Padding(3, 2, 3, 2);
		((System.Windows.Forms.Control)(object)buttonEx2).Name = "buttonEx2";
		((System.Windows.Forms.Control)(object)buttonEx2).Size = new System.Drawing.Size(75, 23);
		((System.Windows.Forms.Control)(object)buttonEx2).TabIndex = 3;
		((System.Windows.Forms.Control)(object)buttonEx2).Text = "読込";
		((ButtonBase)(object)buttonEx2).TextImageRelation = TextImageRelation.ImageBeforeText;
		((ButtonBase)(object)buttonEx2).UseVisualStyleBackColor = true;
		((System.Windows.Forms.Control)(object)buttonEx2).Click += buttonEx2_Click;
		label3.AutoSize = true;
		label3.Location = new System.Drawing.Point(521, 5);
		label3.Margin = new Padding(10);
		label3.Name = "label3";
		label3.Size = new System.Drawing.Size(11, 13);
		label3.TabIndex = 6;
		label3.Text = "/";
		((ButtonBase)(object)ComConnect).FlatAppearance.BorderSize = 0;
		((System.Windows.Forms.Control)(object)ComConnect).ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		ComConnect.Image = Resources.B5;
		((System.Windows.Forms.Control)(object)ComConnect).Location = new System.Drawing.Point(707, 0);
		((System.Windows.Forms.Control)(object)ComConnect).Margin = new Padding(3, 2, 3, 2);
		((System.Windows.Forms.Control)(object)ComConnect).Name = "ComConnect";
		((System.Windows.Forms.Control)(object)ComConnect).Size = new System.Drawing.Size(75, 23);
		((System.Windows.Forms.Control)(object)ComConnect).TabIndex = 5;
		((System.Windows.Forms.Control)(object)ComConnect).Text = "接続";
		((ButtonBase)(object)ComConnect).TextImageRelation = TextImageRelation.ImageBeforeText;
		((ButtonBase)(object)ComConnect).UseVisualStyleBackColor = true;
		((System.Windows.Forms.Control)(object)ComConnect).Click += ComConnect_Click;
		((System.Windows.Forms.Control)(object)macroSelCmb).BackColor = System.Drawing.Color.FromArgb(33, 33, 35);
		macroSelCmb.BorderColor = System.Drawing.Color.FromArgb(65, 65, 67);
		macroSelCmb.BorderStyle = ButtonBorderStyle.Solid;
		macroSelCmb.ContentsCheck = true;
		((System.Windows.Forms.ComboBox)(object)macroSelCmb).DrawMode = DrawMode.OwnerDrawFixed;
		macroSelCmb.DropDownStyle = ComboBoxStyle.DropDownList;
		((System.Windows.Forms.ComboBox)(object)macroSelCmb).FlatStyle = FlatStyle.Flat;
		((System.Windows.Forms.Control)(object)macroSelCmb).ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		((ListControl)(object)macroSelCmb).FormattingEnabled = true;
		((System.Windows.Forms.ComboBox)(object)macroSelCmb).Items.AddRange(new object[1] { "マクロを選択" });
		((System.Windows.Forms.Control)(object)macroSelCmb).Location = new System.Drawing.Point(178, 0);
		((System.Windows.Forms.Control)(object)macroSelCmb).Name = "macroSelCmb";
		((System.Windows.Forms.Control)(object)macroSelCmb).Size = new System.Drawing.Size(212, 23);
		((System.Windows.Forms.Control)(object)macroSelCmb).TabIndex = 1;
		((System.Windows.Forms.ComboBox)(object)macroSelCmb).TextUpdate += macroSelCmb_TextUpdate;
		timer1.Enabled = true;
		timer1.Interval = 20;
		timer1.Tick += timer1_Tick;
		fileSystemWatcher1.EnableRaisingEvents = true;
		fileSystemWatcher1.SynchronizingObject = (ISynchronizeInvoke)this;
		fileSystemWatcher1.Created += fileSystemWatcher1_Created;
		fileSystemWatcher1.Deleted += fileSystemWatcher1_Deleted;
		fileSystemWatcher1.Renamed += fileSystemWatcher1_Renamed;
		fileSystemWatcher2.EnableRaisingEvents = true;
		fileSystemWatcher2.SynchronizingObject = (ISynchronizeInvoke)this;
		fileSystemWatcher2.Deleted += fileSystemWatcher2_Deleted;
		fileSystemWatcher2.Renamed += fileSystemWatcher2_Renamed;
		fileSystemWatcher3.EnableRaisingEvents = true;
		fileSystemWatcher3.SynchronizingObject = (ISynchronizeInvoke)this;
		fileSystemWatcher3.Deleted += fileSystemWatcher3_Deleted;
		fileSystemWatcher3.Renamed += fileSystemWatcher3_Renamed;
		fileSystemWatcher4.EnableRaisingEvents = true;
		fileSystemWatcher4.SynchronizingObject = (ISynchronizeInvoke)this;
		fileSystemWatcher5.EnableRaisingEvents = true;
		fileSystemWatcher5.SynchronizingObject = (ISynchronizeInvoke)this;
		mouseHook1.MouseHooked += mouseHook1_MouseHooked_2;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = AutoScaleMode.Font;
		((Form)this).ClientSize = new System.Drawing.Size(1020, 485);
		((System.Windows.Forms.Control)this).Controls.Add(panel2);
		((System.Windows.Forms.Control)(object)this).Font = new Font("Segoe UI", 8.25f, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, 0);
		((System.Windows.Forms.Control)(object)this).ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
		((Form)this).Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
		((Form)this).KeyPreview = true;
		((Form)this).MainMenuStrip = menuStrip1;
		((System.Windows.Forms.Control)(object)this).MinimumSize = new System.Drawing.Size(1036, 524);
		((System.Windows.Forms.Control)this).Name = "NXMC_VxV";
		((System.Windows.Forms.Control)this).Padding = new Padding(1);
		((System.Windows.Forms.Control)(object)this).Text = "NX Macro Controller ver2.00";
		((Form)this).Activated += NXMC_VxV_Activated;
		((Form)this).FormClosing += Form1_FormClosing;
		((Form)this).Load += Form1_Load;
		((Form)this).Shown += NXMC_VxV_Shown;
		((Form)this).ResizeBegin += NXMC_VxV_ResizeBegin;
		((Form)this).ResizeEnd += NXMC_VxV_ResizeEnd;
		((System.Windows.Forms.Control)this).SizeChanged += NXMC_VxV_SizeChanged;
		((System.Windows.Forms.Control)this).Click += NXMC_VxV_Click;
		((System.Windows.Forms.Control)this).DragDrop += NXMC_VxV_DragDrop;
		((System.Windows.Forms.Control)this).DragEnter += NXMC_VxV_DragEnter;
		((System.Windows.Forms.Control)this).KeyPress += NXMC_VxV_KeyPress;
		((System.Windows.Forms.Control)this).PreviewKeyDown += NXMC_VxV_PreviewKeyDown;
		((System.Windows.Forms.Control)this).Resize += NXMC_VxV_Resize;
		((System.Windows.Forms.Control)this).Controls.SetChildIndex(panel2, 0);
		((ISupportInitialize)CaptureScreen).EndInit();
		CaptureContext.ResumeLayout(performLayout: false);
		((System.Windows.Forms.Control)(object)groupBox1).ResumeLayout(false);
		((System.Windows.Forms.Control)(object)groupBox1).PerformLayout();
		((System.Windows.Forms.Control)(object)ghostPanel7).ResumeLayout(false);
		menuStrip1.ResumeLayout(performLayout: false);
		menuStrip1.PerformLayout();
		panel1.ResumeLayout(performLayout: false);
		((System.Windows.Forms.Control)(object)tabControl1).ResumeLayout(false);
		tabPage2.ResumeLayout(performLayout: false);
		tabPage4.ResumeLayout(performLayout: false);
		((System.Windows.Forms.Control)(object)ghostPanel6).ResumeLayout(false);
		((System.Windows.Forms.Control)(object)groupBoxEx1).ResumeLayout(false);
		((System.Windows.Forms.Control)(object)groupBoxEx1).PerformLayout();
		tabPage3.ResumeLayout(performLayout: false);
		((System.Windows.Forms.Control)(object)tabControlEx1).ResumeLayout(false);
		tabPage5.ResumeLayout(performLayout: false);
		tabPage6.ResumeLayout(performLayout: false);
		tabPage1.ResumeLayout(performLayout: false);
		tabPageMHXX.ResumeLayout(performLayout: false);
		panel2.ResumeLayout(performLayout: false);
		panel2.PerformLayout();
		((System.Windows.Forms.Control)(object)ghostPanel4).ResumeLayout(false);
		panel4.ResumeLayout(performLayout: false);
		panel3.ResumeLayout(performLayout: false);
		((System.Windows.Forms.Control)(object)ghostPanel5).ResumeLayout(false);
		((System.Windows.Forms.Control)(object)ghostPanel5).PerformLayout();
		((ISupportInitialize)fileSystemWatcher1).EndInit();
		((ISupportInitialize)fileSystemWatcher2).EndInit();
		((ISupportInitialize)fileSystemWatcher3).EndInit();
		((ISupportInitialize)fileSystemWatcher4).EndInit();
		((ISupportInitialize)fileSystemWatcher5).EndInit();
		((System.Windows.Forms.Control)this).ResumeLayout(false);
	}
}
