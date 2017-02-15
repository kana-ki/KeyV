using System;
using System.Windows.Forms;

namespace KeyV {

    public class App {

        private readonly Keyboard _keyboard;
        private readonly NotifyIcon _notifyIcon;
        private readonly GlobalHotkey _globalHotkey;

        public App() {
            this._keyboard = new Keyboard();
            this._notifyIcon = this.CreateIcon();
            this._globalHotkey = this.RegisterHotkey();
            Application.Run();
        }

        private NotifyIcon CreateIcon() {
            var notifyIcon = new NotifyIcon {
                Icon = Properties.Resources.Icon,
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
            this._notifyIcon.Visible = false;
            this._globalHotkey.UnregisterGlobalHotKey();
            Application.Exit();
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
                this._notifyIcon.Icon = Properties.Resources.Icon_lit;
                this._keyboard.PressKeys(Clipboard.GetText());
                this._notifyIcon.Icon = Properties.Resources.Icon;
            }
        }

        ~App() {
            this._notifyIcon.Dispose();
            this._globalHotkey.Dispose();
        }

        [STAThread]
        public static void Main() {
            new App();
        }

    }
}
