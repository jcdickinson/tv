using System;
using System.Drawing;

namespace TerminalVelocity.Terminal
{
    internal struct AnsiParserEvents
    {
        public delegate void InputEvent(ReadOnlySpan<char> characters);
        public delegate void PutTabEvent(int tabCount);
        public delegate void BackspaceEvent();
        public delegate void CarriageReturnEvent();
        public delegate void LineFeedEvent();
        public delegate void BellEvent();
        public delegate void NewLineEvent();
        public delegate void SetHorizontalTabStopEvent();
        public delegate void IdentifyTerminalEvent();
        public delegate void SetTitleEvent(ReadOnlySpan<char> characters);
        public delegate void SetColorEvent(NamedColor index, in Color color);
        public delegate void SetCursorEvent(CursorStyle cursorStyle);
        public delegate void SetClipboardEvent(ReadOnlySpan<char> characters);
        public delegate void ResetColorEvent(NamedColor index);
        public delegate void ReverseIndexEvent();
        public delegate void ResetStateEvent();
        public delegate void SaveCursorPositionEvent();
        public delegate void RestoreCursorPositionEvent();
        public delegate void DecTestEvent();
        public delegate void SetKeypadApplicationModeEvent();
        public delegate void UnsetKeypadApplicationModeEvent();
        public delegate void InsertBlankEvent(long count);
        public delegate void MoveUpEvent(long count, bool cr);
        public delegate void MoveDownEvent(long count, bool cr);
        public delegate void MoveForwardEvent(long count);
        public delegate void MoveBackwardEvent(long count);

        public InputEvent Input;
        public PutTabEvent PutTab;
        public BackspaceEvent Backspace;
        public CarriageReturnEvent CarriageReturn;
        public LineFeedEvent LineFeed;
        public BellEvent Bell;
        public NewLineEvent NewLine;
        public SetHorizontalTabStopEvent SetHorizontalTabStop;
        public IdentifyTerminalEvent IdentifyTerminal;
        public SetTitleEvent SetTitle;
        public SetColorEvent SetColor;
        public SetCursorEvent SetCursor;
        public SetClipboardEvent SetClipboard;
        public ResetColorEvent ResetColor;
        public ReverseIndexEvent ReverseIndex;
        public ResetStateEvent ResetState;
        public SaveCursorPositionEvent SaveCursorPosition;
        public RestoreCursorPositionEvent RestoreCursorPosition;
        public DecTestEvent DecTest;
        public SetKeypadApplicationModeEvent SetKeypadApplicationMode;
        public UnsetKeypadApplicationModeEvent UnsetKeypadApplicationMode;
        public InsertBlankEvent InsertBlank;
        public MoveUpEvent MoveUp;
        public MoveDownEvent MoveDown;
        public MoveForwardEvent MoveForward;
        public MoveBackwardEvent MoveBackward;
    }
}
