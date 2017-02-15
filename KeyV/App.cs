using System;
using System.Windows.Forms;

namespace KeyV {

    public class App {

        readonly NotifyIcon _notifyIcon;
        private readonly GlobalHotkey _globalHotkey;

        public App() {
            this._notifyIcon = this.CreateIcon();
            this._globalHotkey = this.RegisterHotkey();
            Application.Run();
        }

        private NotifyIcon CreateIcon() {
            var notifyIcon = new NotifyIcon();
            notifyIcon.Icon = Properties.Resources.Icon;
            notifyIcon.Visible = true;
            notifyIcon.ContextMenu = new ContextMenu(new[] {
                new MenuItem("Exit KeyPaste", this.Exit_Click)
            });
            return this._notifyIcon;
        }

        private void Exit_Click(object obj, EventArgs args) {
            Application.Exit();
        }

        private GlobalHotkey RegisterHotkey() {
            var hotkey = new GlobalHotkey();
            hotkey.RegisterGlobalHotKey((int)Keys.V, GlobalHotkey.MOD_CONTROL | GlobalHotkey.MOD_ALT);
            hotkey.HotKeyPressed += this.HotKeyPressed;
            return hotkey;
        }

        private void HotKeyPressed() {
            if (Clipboard.ContainsText()) {
                new Keyboard().PressKeys(Clipboard.GetText());
            }
        }

        ~App() {
            this._globalHotkey.Dispose();
        }

        [STAThread]
        public static void Main() {
            new App();
        }

    }
}
