﻿using System;

namespace TerminalVelocity.Terminal
{
    internal enum ParserAction : byte
    {
        None = 0,
        Clear = 1,
        Collect = 2,
        CsiDispatch = 3,
        EscDispatch = 4,
        Execute = 5,
        Hook = 6,
        Ignore = 7,
        OscEnd = 8,
        OscPut = 9,
        OscStart = 10,
        Param = 11,
        Print = 12,
        Put = 13,
        Unhook = 14,
        BeginUtf8 = 15,
    }

    internal enum ParserState
    {
        Anywhere = 0,
        CsiEntry = 1,
        CsiIgnore = 2,
        CsiIntermediate = 3,
        CsiParam = 4,
        DcsEntry = 5,
        DcsIgnore = 6,
        DcsIntermediate = 7,
        DcsParam = 8,
        DcsPassthrough = 9,
        Escape = 10,
        EscapeIntermediate = 11,
        Ground = 12,
        OscString = 13,
        SosPmApcString = 14,
        Utf8 = 15,
    }

    // https://en.wikipedia.org/wiki/C0_and_C1_control_codes
    internal enum ControlCode : byte
    {
        // C0
        Null = 0x00,
        StartOfHeading = 0x01,
        StartOfText = 0x02,
        EndOfText = 0x03,
        EndOfTransmission = 0x04,
        Enquiry = 0x05,
        Acknowledge = 0x06,
        Bell = 0x07,
        Alert = Bell,
        Backspace = 0x08,
        CharacterTabulation = 0x09,
        HorizontalTabulation = CharacterTabulation,
        LineFeed = 0x0A,
        LineTabulation = 0x0B,
        VerticalTabulation = LineTabulation,
        FormFeed = 0x0C,
        CarriageReturn = 0x0D,
        ShiftOut = 0x0E,
        ShiftIn = 0x0F,
        DataLinkEscape = 0x10,
        DeviceControlOne = 0x11,
        DeviceControlTwo = 0x12,
        DeviceControlThree = 0x13,
        DeviceControlFour = 0x14,
        NegativeAcknowledge = 0x15,
        SynchronousIdle = 0x16,
        EndOfTranmissionBlock = 0x17,
        Cancel = 0x18,
        EndOfMedium = 0x19,
        Substitute = 0x1A,
        Escape = 0x1B,
        FileSeparator = 0x1C,
        GroupSeparator = 0x1D,
        RecordSeparator = 0x1E,
        UnitSeparator = 0x1F,
        Space = 0x20,
        Del = 0x7F,

        // C1
        PaddingCharacter = 0x80,
        HighOctetPreset = 0x81,
        BreakPermittedHere = 0x82,
        NoBreakHere = 0x83,
        Index = 0x84,
        NewLine = 0x85,
        StartOfSelectedArea = 0x86,
        EndOfSelectedArea = 0x87,
        CharacterTabulationSet = 0x88,
        HorizontalTabulationSet = CharacterTabulationSet,
        CharacterTabulationWithJustification = 0x89,
        HorizontalTabulationWithJustification = CharacterTabulationWithJustification,
        LineTabulationSet = 0x8A,
        VerticalTabulationSet = LineTabulationSet,
        PartialLineForward = 0x8B,
        PartialLineDown = PartialLineForward,
        PartialLineBackward = 0x8C,
        PartialLineUp = PartialLineBackward,
        ReverseLineFeed = 0x8D,
        ReverseIndex = ReverseLineFeed,
        SingleShift2 = 0x8E,
        SingleShift3 = 0x8F,
        DeviceControlString = 0x90,
        PrivateUse1 = 0x91,
        PrivateUse2 = 0x92,
        SetTranmissionState = 0x93,
        CancelCharacter = 0x94,
        MessageWaiting = 0x95,
        StartOfProtectedArea = 0x96,
        EndOfProtectedArea = 0x97,
        StartOfString = 0x98,
        SingleGraphicCharacterIntroducer = 0x99,
        SingleCharacterIntroducer = 0x9A,
        ControlSequenceInitiator = 0x9B,
        StringTerminator = 0x9C,
        OperatingSystemCommand = 0x9D,
        PrivacyMessage = 0x9E,
        ApplicationProgramCommand = 0x9F
    }

    internal enum ControlSequenceCommand : byte
    {
        InsertBlank = (byte)'@',
        MoveUp = (byte)'A',
        RepeatPrecedingCharacter = (byte)'b',
        TerminalAttribute = (byte)'m',
        MoveDown1 = (byte)'B',
        MoveDown2 = (byte)'e',
        IdentifyTerminal = (byte)'c',
        MoveForward1 = (byte)'C',
        MoveForward2 = (byte)'a',
        MoveBackward = (byte)'D',
        MoveDownAndCr = (byte)'E',
        MoveUpAndCr = (byte)'F',
        SetTabulationMode = (byte)'g',
        GotoColumn1 = (byte)'G',
        GotoColumn2 = (byte)'`',
        Goto1 = (byte)'H',
        Goto2 = (byte)'f',
        MoveForwardTabs = (byte)'I',
        ClearScreen = (byte)'J',
        ClearLine = (byte)'K',
        ScrollUp = (byte)'S',
        ScrollDown = (byte)'T',
        InsertBlankLines = (byte)'L',
        UnsetMode = (byte)'l',
        DeleteLines = (byte)'M',
        EraseChars = (byte)'X',
        DeleteChars = (byte)'P',
        MoveBackwardTabs = (byte)'Z',
        GotoLine = (byte)'d',
        SetMode = (byte)'h',
        DeviceStatus = (byte)'n',
        SetScrollingRegion = (byte)'r',
        SaveCursorPosition = (byte)'s',
        RestoreCursorPosition = (byte)'u',
        SetCursorStyle = (byte)'q'
    }

    internal enum EscapeCommand : byte
    {
        ConfigureAsciiCharSet = (byte)'B',
        ConfigureSpecialCharSet = (byte)'0',
        LineFeed = (byte)'D',
        NextLine = (byte)'E',
        SetHorizontalTabStop = (byte)'H',
        ReverseIndex = (byte)'M',
        IdentifyTerminal = (byte)'Z',
        SaveCursorPosition = (byte)'7',
        DecTest = (byte)'8',
        RestoreCursorPosition = DecTest,
        SetKeypadApplicationMode = (byte)'=',
        UnsetKeypadApplicationMode = (byte)'>',
        StringTerminator = (byte)'\\',
        ResetState = (byte)'c'
    }

    internal enum OsCommand : short
    {
        SetWindowIconAndTitle = 0,
        SetWindowIcon = 1,
        SetWindowTitle = 2,
        SetColor = 4,
        SetForegroundColor = 10,
        SetBackgroundColor = 11,
        SetCursorColor = 12,
        SetCursorStyle = 50,
        SetClipboard = 52,
        ResetColor = 104,
        ResetForegroundColor = 110,
        ResetBackgroundColor = 111,
        ResetCursorColor = 112,
        Unknown = -1
    }

    internal enum ClearMode : byte
    {
        Below,
        Above,
        All,
        Saved
    }

    internal enum CursorStyle : byte
    {
        Block = 0,
        Underline = 2,
        Beam = 1,
        HollowBlock = 3,
    }

    [Flags]
    internal enum IgnoredData : byte
    {
        None = 0,
        Intermediates = 0b0000_00001,
        Parameters = 0b0000_00010,
        All = Intermediates | Parameters
    }

    internal enum LineClearMode : byte
    {
        Right,
        Left,
        All,
    }

    internal enum Mode
    {
        CursorKeys = 1,
        DECCOLM = 3,
        Insert = 4,
        Origin = 6,
        LineWrap = 7,
        BlinkingCursor = 12,
        LineFeedNewLine = 20,
        ShowCursor = 25,
        ReportMouseClicks = 1000,
        ReportCellMouseMotion = 1002,
        ReportAllMouseMotion = 1003,
        ReportFocusInOut = 1004,
        SgrMouse = 1006,
        SwapScreenAndSetRestoreCursor = 1049,
        BracketedPaste = 2004,
    }

    internal enum NamedColor : short
    {
        Black = 0,
        Red,
        Green,
        Yellow,
        Blue,
        Magenta,
        Cyan,
        White,
        BrightBlack,
        BrightRed,
        BrightGreen,
        BrightYellow,
        BrightBlue,
        BrightMagenta,
        BrightCyan,
        BrightWhite,
        Foreground = 256,
        Background,
        CursorText,
        Cursor,
        DimBlack,
        DimRed,
        DimGreen,
        DimYellow,
        DimBlue,
        DimMagenta,
        DimCyan,
        DimWhite,
        BrightForeground,
        DimForeground,
    }

    internal enum TabulationClearMode : byte
    {
        Current = 0,
        All = 3,
    }
}
