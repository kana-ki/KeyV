using System.Windows.Forms;

namespace KeyV.Keyboard {

    public delegate ulong LowLevelKeyboardProcCallback (int code, KeyboardMessage wParam, ref Keys key);

}