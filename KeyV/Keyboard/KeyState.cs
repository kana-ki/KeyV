using System;

namespace KeyV.Keyboard {

    [Flags]
    public enum KeyState {

        Toggled = 0x0001,
        Down = 0x0080,
        Up = 0x0000

    }

}