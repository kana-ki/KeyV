using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace KeyV {

    public class Keyboard {

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        static extern byte GetKeyState(byte bVk);

        private const byte VK_SHIFT = 0x10;
        private const byte VK_CONTROL = 0x11;
        private const byte VK_ALT = 0x12;

        private const byte KEY_PRESS = 0x0;
        private const byte KEY_RELEASE = 0x2;

        private bool _restoreShift = false;
        private bool _restoreControl = false;
        private bool _restoreAlt = false;

        public void PressKeys (string str) {
            this.ClearModifiers();
            foreach (var ch in str) {
                SendKeys.Send(ch.ToString());
                Thread.Sleep(10);
            }
            this.RestoreModifiers();
        }

        private void ClearModifiers () {
            this._restoreShift = (GetKeyState(VK_SHIFT) & 0x80) == 0x80;
            this._restoreControl = (GetKeyState(VK_CONTROL) & 0x80) == 0x80;
            this._restoreAlt = (GetKeyState(VK_ALT) & 0x8) == 0x80;
            keybd_event(VK_SHIFT, 0, KEY_RELEASE, 0);
            keybd_event(VK_ALT, 0, KEY_RELEASE, 0);
            keybd_event(VK_CONTROL, 0, KEY_RELEASE, 0);
        }

        private void RestoreModifiers () {
            if (this._restoreShift) keybd_event(VK_SHIFT, 0, KEY_PRESS, 0);
            if (this._restoreControl) keybd_event(VK_CONTROL, 0, KEY_PRESS, 0);
            if (this._restoreAlt) keybd_event(VK_ALT, 0, KEY_PRESS, 0);
            this._restoreShift = this._restoreControl = this._restoreAlt = false;
        }

    }

}
