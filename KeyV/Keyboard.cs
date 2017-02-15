using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace KeyV {

    public class Keyboard {

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        private const byte VK_SHIFT = 0x10;
        private const byte VK_CONTROL = 0x11;
        private const byte VK_ALT = 0x12;

        private const byte KEY_RELEASE = 0x2; // Press is 0x0

        private static readonly Dictionary <char, string> _sanitization = new Dictionary <char, string> {
            { '{', "{{}"}, { '}', "{}}"}, { '+', "{+}"}, { '^', "{^}"}, { '%', "{%}"},
            { '~', "{~}"}, { '[', "{[}"}, { ']', "{]}"}, { '(', "{(}"}, { ')', "{)}"}
        };

        public void AltTab () {
            this.ClearModifiers();
            SendKeys.SendWait("%{TAB}");
            Thread.Sleep(100);
        }

        public void PressKeys (string str) {
            this.ClearModifiers();
            foreach (var ch in str) {
                var input = this.Sanitize(ch);
                SendKeys.SendWait(input);
                Thread.Sleep(15);
            }
        }

        private string Sanitize (char ch) {
            if (_sanitization.ContainsKey(ch)) return _sanitization[ch];
            return ch.ToString();
        }

        private void ClearModifiers () {
            keybd_event(VK_SHIFT, 0, KEY_RELEASE, 0);
            keybd_event(VK_ALT, 0, KEY_RELEASE, 0);
            keybd_event(VK_CONTROL, 0, KEY_RELEASE, 0);
        }

    }

}
