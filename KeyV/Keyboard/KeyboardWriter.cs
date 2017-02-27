using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace KeyV.Keyboard {

    public class KeyboardWriter {

        private static readonly Dictionary <char, string> _sanitization = new Dictionary <char, string> {
            {'\r', null},
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

        private static readonly Dictionary <Keys, PreventReleaseDelegate> _preventReleaseDelegates =
            new Dictionary <Keys, PreventReleaseDelegate> {
                {Keys.LControlKey, input => input.Contains("\n")},
                {Keys.LShiftKey, input => input.Any(char.IsUpper)},
            };

        private readonly Dictionary <Keys, bool> _preventRelease = new Dictionary <Keys, bool> {
            {Keys.LControlKey, false},
            {Keys.LShiftKey, false},
        };

        private readonly Dictionary <Keys, bool> _restoreKeys = new Dictionary <Keys, bool> {
            // Keys.Control, Keys.Alt, and Keys.Shift are modifiers - not keys
            // Keys.ControlKeys, Keys.menu, Keys.ShiftKey are more like a property - true if either Left or Right counter part is true
            // The following are the 'real' modifier *keys*
            {Keys.LShiftKey, false},
            {Keys.RShiftKey, false},
            {Keys.LControlKey, false},
            {Keys.RControlKey, false},
            {Keys.LMenu, false},
            {Keys.RMenu, false},
        };

        private bool _restoreCaps;

        private IntPtr _userKeyHook;

        [DllImport ("user32.dll", SetLastError = true)]
        private static extern void keybd_event (byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport ("user32.dll")]
        private static extern KeyState GetKeyState (byte bVk);

        [DllImport ("user32.dll")]
        private static extern IntPtr SetWindowsHookEx
            (int idHook, LowLevelKeyboardProcCallback callback, IntPtr hInstance, uint dwThreadId);

        [DllImport ("user32.dll")]
        private static extern bool UnhookWindowsHookEx (IntPtr hInstance);

        [DllImport ("user32.dll", SetLastError = true)]
        private static extern ulong CallNextHookEx (IntPtr idHook, int nCode, KeyboardMessage wParam, ref Keys key);

        public void AltTab () {
            this.ClearModifiers();
            SendKeys.SendWait("%{TAB}");
            Thread.Sleep(100);
        }

        public void PressKeys (string str) {
            this.ClearModifiers();
            foreach (var ch in str) {
                var input = this.Sanitize(ch);
                if (input == null) continue;
                this.SetReleasePrevention(input);
                SendKeys.SendWait(input);
                Thread.Sleep(15);
            }
            this.RestoreModifiers();
        }

        private string Sanitize (char ch) {
            if (_sanitization.ContainsKey(ch)) return _sanitization[ch];
            return ch.ToString();
        }

        private void SetReleasePrevention (string input) {
            foreach (var key in _preventReleaseDelegates) if (key.Value(input)) this._preventRelease[key.Key] = true;
        }

        private void ClearModifiers () {
            if (GetKeyState((byte) Keys.Capital).HasFlag(KeyState.Toggled)) {
                this._restoreCaps = true;
                keybd_event((byte) Keys.Capital, 0, (byte) KeyAction.Press, 0);
                keybd_event((byte) Keys.Capital, 0, (byte) KeyAction.Release, 0);
            }
            foreach (var modifier in this._restoreKeys.Keys.ToList()) {
                this._restoreKeys[modifier] = GetKeyState((byte) modifier).HasFlag(KeyState.Down);
                keybd_event((byte) modifier, 0, (byte) KeyAction.Release, 0);
            }
            this._userKeyHook = SetWindowsHookEx((byte) HookType.KeyboardGlobalOnly, this.OnModifierRelease, IntPtr.Zero, 0);
        }

        private ulong OnModifierRelease (int code, KeyboardMessage wParam, ref Keys key) {
            if (code >= 0
                && (wParam == KeyboardMessage.KeyUp || wParam == KeyboardMessage.SysKeyUp)
                && this._restoreKeys.ContainsKey(key)) {
                if (this._preventRelease.ContainsKey(key)
                    && this._preventRelease[key]) this._preventRelease[key] = false;
                else this._restoreKeys[key] = false;
            }
            return CallNextHookEx(this._userKeyHook, code, wParam, ref key);
        }

        private void RestoreModifiers () {
            if (this._restoreCaps) {
                keybd_event((byte) Keys.Capital, 0, (byte) KeyAction.Press, 0);
                keybd_event((byte) Keys.Capital, 0, (byte) KeyAction.Release, 0);
                this._restoreCaps = false;
            }
            foreach (var modifier in this._restoreKeys.Keys.ToList()) {
                if (this._restoreKeys[modifier]) keybd_event((byte) modifier, 0, (byte) KeyAction.Press, 0);
                this._restoreKeys[modifier] = false;
            }
            UnhookWindowsHookEx(this._userKeyHook);
        }

    }

}