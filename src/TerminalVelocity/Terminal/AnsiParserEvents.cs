/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

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
        public delegate void MoveForwardEvent(long count, bool tabs);
        public delegate void MoveBackwardEvent(long count, bool tabs);
        public delegate void ClearTabulationEvent(TabulationClearMode clearMode);
        public delegate void GotoEvent(long? column = default, long? line = default);
        public delegate void ClearScreenEvent(ClearMode clearMode);
        public delegate void ClearLineEvent(LineClearMode clearMode);
        public delegate void ScrollUpEvent(long count);
        public delegate void ScrollDownEvent(long count);
        public delegate void InsertBlankLinesEvent(long count);
        public delegate void UnsetModeEvent(Mode mode);
        public delegate void DeleteLinesEvent(long count);
        public delegate void EraseCharactersEvent(long count);
        public delegate void DeleteCharactersEvent(long count);
        public delegate void SetModeEvent(Mode mode);
        public delegate void TerminalAttributeEvent(TerminalAttribute attribute, NamedColor? index = default, in Color? color = default);
        public delegate void DeviceStatusEvent(long param);
        public delegate void SetScrollingRegionEvent(long? top, long? bottom);
        public delegate void SetCursorStyleEvent(CursorStyle? style);

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
        public ClearTabulationEvent ClearTabulation;
        public GotoEvent Goto;
        public ClearScreenEvent ClearScreen;
        public ClearLineEvent ClearLine;
        public ScrollUpEvent ScrollUp;
        public ScrollDownEvent ScrollDown;
        public InsertBlankLinesEvent InsertBlankLines;
        public UnsetModeEvent UnsetMode;
        public DeleteLinesEvent DeleteLines;
        public EraseCharactersEvent EraseCharacters;
        public DeleteCharactersEvent DeleteCharacters;
        public SetModeEvent SetMode;
        public TerminalAttributeEvent TerminalAttribute;
        public DeviceStatusEvent DeviceStatus;
        public SetScrollingRegionEvent SetScrollingRegion;
        public SetCursorStyleEvent SetCursorStyle;
    }
}
