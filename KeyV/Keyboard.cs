using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace KeyV {

    public class Keyboard {

        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYUP = 0x105;
        private const byte WH_KEYBOARD_LL = 0x0D;
        private const byte KEY_PRESS = 0x0;
        private const byte KEY_RELEASE = 0x2;

        private static readonly Dictionary <char, string> _sanitization = new Dictionary <char, string> {
            {'{', "{{}"},
            {'}', "{}}"},
            {'+', "{+}"},
            {'^', "{^}"},
            {'%', "{%}"},
            {'~', "{~}"},
            {'[', "{[}"},
            {']', "{]}"},
            {'(', "{(}"},
            {')', "{)}"}
        };

        private readonly Dictionary <Keys, bool> _userKeyMap = new Dictionary <Keys, bool> {
            // Keys.Control, Keys.Alt, and Keys.Shift are modifiers - not keys
            // Keys.ControlKeys, Keys.menu, Keys.ShiftKey are more like a property - true if either Left or Right counter part is true
            // The following are the 'real' modifier *keys*
            {Keys.LShiftKey, false},
            {Keys.RShiftKey, false},
            {Keys.LControlKey, false},
            {Keys.RControlKey, false},
            {Keys.LMenu, false},
            {Keys.RMenu, false}
        };

        private IntPtr _userKeyHook;

        [DllImport ("user32.dll", SetLastError = true)]
        private static extern void keybd_event (byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport ("user32.dll")]
        [return: MarshalAs (UnmanagedType.Bool)]
        private static extern bool GetAsyncKeyState (byte bVk);

        [DllImport ("user32.dll")]
        private static extern IntPtr SetWindowsHookEx
            (int idHook, KeyboardHookCallback callback, IntPtr hInstance, uint dwThreadId);

        [DllImport ("user32.dll")]
        private static extern bool UnhookWindowsHookEx (IntPtr hInstance);

        [DllImport ("user32.dll", SetLastError = true)]
        private static extern byte CallNextHookEx (IntPtr idHook, int nCode, int wParam, ref Keys key);

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
            this.RestoreModifiers();
        }

        private string Sanitize (char ch) {
            if (_sanitization.ContainsKey(ch)) return _sanitization[ch];
            return ch.ToString();
        }

        private void ClearModifiers () {
            foreach (var modifier in this._userKeyMap.Keys.ToList()) {
                this._userKeyMap[modifier] = GetAsyncKeyState((byte) modifier);
                keybd_event((byte) modifier, 0, KEY_RELEASE, 0);
            }
            this._userKeyHook = SetWindowsHookEx(WH_KEYBOARD_LL, this.OnModifierRelease, IntPtr.Zero, 0);
        }

        private byte OnModifierRelease (int code, int wParam, ref Keys key) {
            // Keys used by the sent keys will not be restored
            if ((wParam == WM_KEYUP || wParam == WM_SYSKEYUP)
                && this._userKeyMap.ContainsKey(key)) this._userKeyMap[key] = false;
            return CallNextHookEx(this._userKeyHook, code, wParam, ref key);
        }

        private void RestoreModifiers () {
            foreach (var modifier in this._userKeyMap.Keys.ToList()) {
                if (this._userKeyMap[modifier]) keybd_event((byte) modifier, 0, KEY_PRESS, 0);
                this._userKeyMap[modifier] = false;
            }
            UnhookWindowsHookEx(this._userKeyHook);
        }

        private delegate byte KeyboardHookCallback (int code, int wParam, ref Keys key);

    }

}