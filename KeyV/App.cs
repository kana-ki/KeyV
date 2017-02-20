using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;
using System.Windows.Forms;
using KeyV.Properties;

namespace KeyV {

    public class App {

        private readonly Keyboard _keyboard;
        private readonly NotifyIcon _notifyIcon;
        private readonly GlobalHotkey _globalHotkey;
        private readonly Dictionary <byte, Action> _ipcCommands;
        private bool _drawingAttention = false;

        public App() {
            this.PollForIpc();
            this._notifyIcon = this.CreateIcon();
            this._keyboard = new Keyboard();
            this._globalHotkey = this.RegisterHotkey();
            this._ipcCommands = new Dictionary<byte, Action> {
                { 0x10, this.DrawAttention }
            };
            this.ListenForIpc();
        }

        private void ListenForIpc() {
            var pipeServer = new NamedPipeServerStream(Process.GetCurrentProcess().ProcessName, PipeDirection.InOut, -1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            pipeServer.BeginWaitForConnection(this.HandleIpc, pipeServer);
        }

        private void HandleIpc (IAsyncResult result) {
            this.ListenForIpc();
            var pipeServer = result.AsyncState as NamedPipeServerStream;
            if (!pipeServer.IsConnected) pipeServer.WaitForConnection();
            var commandByte = (byte) pipeServer.ReadByte();
            if (this._ipcCommands.ContainsKey(commandByte)) {
                pipeServer.WriteByte(0x0);
                pipeServer.Dispose();
                this._ipcCommands[commandByte]();
            }
            else {
                pipeServer.WriteByte(0x1);
                pipeServer.Dispose();
            }
        }

        private void PollForIpc () {
            var pipeClient = new NamedPipeClientStream
                (".", Process.GetCurrentProcess().ProcessName, PipeDirection.InOut);
            try {
                pipeClient.Connect(1000);
            }
            catch (TimeoutException) {
                return;
            }
            pipeClient.WriteByte(0x10);
            try {
                pipeClient.Connect(1);
            } catch { }
            pipeClient.ReadByte();
            pipeClient.Close();
            Environment.Exit(1);
        }

        private void DrawAttention () {
            if (this._drawingAttention) return;
            this._drawingAttention = true;
            this._notifyIcon.ShowBalloonTip(3000, "KeyV", "KeyV has loaded and is available in the notification area. Press Ctrl + Alt + V to KeyV your clipboard!", ToolTipIcon.Info);
            for (var i = 0; i < 10; i++) {
                this._notifyIcon.Icon = i % 2 == 0 ? Resources.Icon_lit : Resources.Icon;
                Thread.Sleep(300);
            }
            Thread.Sleep(4000);
            this._drawingAttention = false;
        }

        private NotifyIcon CreateIcon() {
            var notifyIcon = new NotifyIcon {
                Icon = Resources.Icon,
                Text = "KeyV (Ctrl+Alt+V)",
                Visible = true
            };
            notifyIcon.DoubleClick += this.KeyV_Click;
            notifyIcon.ContextMenu = new ContextMenu(new [] {
                new MenuItem("KeyV " + Application.ProductVersion) { Enabled = false },
                new MenuItem("-"),
                new MenuItem("Exit KeyV", this.Exit_Click)
            }); 
            return notifyIcon;
        }

        private void Exit_Click(object obj, EventArgs args) {
            this.Exit();
        }

        private void KeyV_Click(object obj, EventArgs args) {
            this._keyboard.AltTab();
            this.ExecuteKeyV();
        }

        private GlobalHotkey RegisterHotkey() {
            var hotkey = new GlobalHotkey();
            hotkey.RegisterGlobalHotKey((int)Keys.V, GlobalHotkey.MOD_CONTROL | GlobalHotkey.MOD_ALT);
            hotkey.HotKeyPressed += this.ExecuteKeyV;
            return hotkey;
        }

        private void ExecuteKeyV() {
            if (Clipboard.ContainsText()) {
                this._notifyIcon.Icon = Resources.Icon_lit;
                this._keyboard.PressKeys(Clipboard.GetText());
                this._notifyIcon.Icon = Resources.Icon;
            }
        }

        private void Exit () {
            this._notifyIcon.Visible = false;
            this._globalHotkey.UnregisterGlobalHotKey();
            Application.Exit();
        }

        ~App() {
            this._notifyIcon?.Dispose();
            this._globalHotkey?.Dispose();
        }

        [STAThread]
        public static void Main() {
            new App();
            Application.Run();
        }

    }
}
