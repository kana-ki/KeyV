namespace KeyV.Keyboard {

    public enum HookType {

        CallWndProc = 0x0004,
        CallWndProcRet = 0x000C,
        Cbt = 0x0005,
        Debug = 0x0009,
        ForegroundIdle = 0x000B,
        GetMessage = 0x0003,
        JournalPlayback = 0x0001,
        JournalRecord = 0x0000,
        Keyboard = 0x0002,
        KeyboardGlobalOnly = 0x000D,
        Mouse = 0x0007,
        MouseGlobalOnly = 0x000E,
        MsgFilter = 0x00FF,
        Shell = 0x00A,
        SysMsgFilter = 0x0006

    }

}