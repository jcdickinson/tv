using System;
using System.Diagnostics;
using TerminalVelocity.Eventing;
using TerminalVelocity.Plugins;
using TerminalVelocity.Terminal.Events;

namespace TerminalVelocity.Terminal
{
    public class Terminal : IPlugin
    {
        private AnsiParser _ansiParser;
        private VTParser _vtParser;

        public Terminal(
            TerminalOpenEvent terminalOpen = null,
            ConsoleOutEvent onConsoleOutEvent = null
            )
        {
            var chr = CharacterParser.Create();

            var ansi = new AnsiParserEvents()
            {
                Input = (characters) => Debug.Write(new string(characters))
            };
            _ansiParser = new AnsiParser(events: ansi, utf8: chr.TryParseUtf8);

            var vt = new VTParserEvents()
            {
                EscapeSequence = _ansiParser.OnEscapeSequence,
                ControlSequence = _ansiParser.OnControlSequenceCommand,
                Execute = _ansiParser.OnExecute,
                OsCommand = _ansiParser.OnOsCommand,
                Print = _ansiParser.OnPrint
            };
            _vtParser = new VTParser(events: vt, utf8: chr.TryParseUtf8, ascii: chr.TryParseAscii);

            onConsoleOutEvent?.Subscribe(OnConsoleOut);
            terminalOpen?.Publish(new TerminalOpenEventData(
                @"C:\windows\system32\cmd.exe", "",
                null, null
            ));
        }

        private EventStatus OnConsoleOut(in ConsoleOutEventData e)
        {
            _vtParser.Process(e.Buffer.Span);
            return EventStatus.Continue;
        }

        public void Dispose()
        {

        }
    }
}
