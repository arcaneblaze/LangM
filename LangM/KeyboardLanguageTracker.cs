using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace LangM;

public partial class KeyboardLanguageTracker : Form
{
    private Label debugLabel;
    private System.Windows.Forms.Timer timer;
    
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint processId);

    [DllImport("user32.dll")]
    private static extern IntPtr GetKeyboardLayout(uint threadId);

    [DllImport("user32.dll")]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(POINT point);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
    
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetCursorInfo(out CURSORINFO pci);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [StructLayout(LayoutKind.Sequential)]
    public struct CURSORINFO
    {
        public uint cbSize;
        public uint flags;
        public IntPtr hCursor;
        public POINT ptScreenPos;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    private const int SW_SHOWNOACTIVATE = 4;
    private const int WS_EX_TOOLWINDOW = 0x80;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    
    private const int CURSORINFO_SIZE = 64;
    private const uint OCR_IBEAM = 0x7F00;

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            cp.ExStyle |= WS_EX_TOOLWINDOW;
            cp.ExStyle |= WS_EX_NOACTIVATE;
            return cp;
        }
    }

    public KeyboardLanguageTracker()
    {
        InitializeComponents();
        SetupTimer();
    }

    private void InitializeComponents()
    {
        try
        {
            this.Icon = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"));
            var _notifyIcon = new NotifyIcon();
            var _trayContextMenu = new ContextMenuStrip();
            _notifyIcon.Icon = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"));
            _notifyIcon.Text = "LangM";
            _notifyIcon.ContextMenuStrip = _trayContextMenu;
            var addToStartup = new ToolStripMenuItem("Add To Startup");
            addToStartup.CheckOnClick = true;
            addToStartup.Checked = AppHelper.AddedToStartup();
            addToStartup.Click += (sender, args) =>
            {
                AppHelper.Logic(addToStartup.Checked);
            };
            var exitMenuItem = new ToolStripMenuItem("Exit");
            exitMenuItem.Click += (sender, args) =>
            {
                Application.Exit();
            };
            _trayContextMenu.Items.Add(addToStartup);
            _trayContextMenu.Items.Add(exitMenuItem);
            _notifyIcon.Visible = true;
            
            
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.BackColor = Color.Black;
            this.TransparencyKey = Color.Black;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(300, 100);
            
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Normal;
            
            debugLabel = new Label
            {
                AutoSize = true,
                BackColor = ColorTranslator.FromHtml("#181818"),
                ForeColor = ColorTranslator.FromHtml("#e0e0e0"),
                Font = new Font("Consolas", 9),
                Padding = new Padding(5),
                BorderStyle = BorderStyle.None,
                MaximumSize = new Size(300, 0)
            };

            this.Controls.Add(debugLabel);

            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                    Application.Exit();
            };
            
            ShowWindow(this.Handle, SW_SHOWNOACTIVATE);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"{ex.Message}");
        }
    }

    private void SetupTimer()
    {
        try
        {
            timer = new System.Windows.Forms.Timer
            {
                Interval = 5
            };
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Timer error{ex.Message}");
        }
    }

    protected override bool ShowWithoutActivation
    {
        get { return true; }
    }
    static LanguageMapper languageMapper = new LanguageMapper("languages.json");
    private void Timer_Tick(object sender, EventArgs e)
    {
        try
        {
            StringBuilder debugInfo = new StringBuilder();

            if (GetCursorPos(out POINT cursorPos))
            {
                IntPtr windowUnderCursor = WindowFromPoint(cursorPos);
                
                if (windowUnderCursor != IntPtr.Zero)
                {
                    uint processId;
                    uint threadId = GetWindowThreadProcessId(windowUnderCursor, out processId);
                    IntPtr keyboardLayout = GetKeyboardLayout(threadId);
                    string language = languageMapper.GetLanguageFromHKL(keyboardLayout);
                    debugInfo.AppendLine($"{language.Split("=>")[1].Trim()}");

                    this.Location = new Point(cursorPos.X + 20, cursorPos.Y + 20);
                    ShowWindow(this.Handle, SW_SHOWNOACTIVATE);
                }
            }

            debugLabel.Text = debugInfo.ToString();
            this.Size = new Size(300, debugLabel.PreferredHeight + 10);
        }
        catch (Exception ex)
        {
            debugLabel.Text = $"Error: {ex.Message}";
        }
    }
}