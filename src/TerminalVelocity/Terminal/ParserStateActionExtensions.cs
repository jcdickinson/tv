using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TerminalVelocity.Terminal
{
    // https://github.com/jwilm/vte/blob/master/src/table.rs.in

    internal static class ParserStateActionExtensions
    {
        private struct StateChangeDescriptor
        {
            public readonly byte Min;
            public readonly byte Max;
            public readonly ParserStateAction Result;

            public StateChangeDescriptor(byte min, byte max, ParserStateAction result)
                => (Min, Max, Result) = (min, max, result);

            public IEnumerable<ushort> GetRange(ushort key)
            {
                for (var i = (ushort)Min; i <= Max; i++)
                    yield return (ushort)(key | i);
            }

            public static implicit operator StateChangeDescriptor(
                (byte Min, byte Max, ParserState State, ParserAction Action) desc)
                => new StateChangeDescriptor(desc.Min, desc.Max, new ParserStateAction(desc.State, desc.Action));

            public static implicit operator StateChangeDescriptor(
                (byte Single, ParserState State, ParserAction Action) desc)
                => new StateChangeDescriptor(desc.Single, desc.Single, new ParserStateAction(desc.State, desc.Action));

            public static implicit operator StateChangeDescriptor(
                (byte Min, byte Max, ParserAction Action) desc)
                => new StateChangeDescriptor(desc.Min, desc.Max, new ParserStateAction(desc.Action));

            public static implicit operator StateChangeDescriptor(
                (byte Single, ParserAction Action) desc)
                => new StateChangeDescriptor(desc.Single, desc.Single, new ParserStateAction(desc.Action));

            public static implicit operator StateChangeDescriptor(
                (byte Min, byte Max, ParserState State) desc)
                => new StateChangeDescriptor(desc.Min, desc.Max, new ParserStateAction(desc.State));

            public static implicit operator StateChangeDescriptor(
                (byte Single, ParserState State) desc)
                => new StateChangeDescriptor(desc.Single, desc.Single, new ParserStateAction(desc.State));
        }

        private static readonly Func<ushort, ParserStateAction> StateChange = CreateStateChange();

        private static readonly ParserAction[] EntryActions = new[]
        {
            ParserAction.None, // VTState.Anywhere
            ParserAction.Clear, // VTState.CsiEntry
            ParserAction.None, // VTState.CsiIgnore
            ParserAction.None, // VTState.CsiIntermediate
            ParserAction.None, // VTState.CsiParam
            ParserAction.Clear, // VTState.DcsEntry
            ParserAction.None, // VTState.DcsIgnore
            ParserAction.None, // VTState.DcsIntermediate
            ParserAction.None, // VTState.DcsParam
            ParserAction.Hook, // VTState.DcsPassthrough
            ParserAction.Clear, // VTState.Escape
            ParserAction.None, // VTState.EscapeIntermediate
            ParserAction.None, // VTState.Ground
            ParserAction.OscStart, // VTState.OscString
            ParserAction.None, // VTState.SosPmApcString
            ParserAction.None  // VTState.Utf8
        };

        private static readonly ParserAction[] ExitActions = new[]
        {
            ParserAction.None, // VTState.Anywhere
            ParserAction.None, // VTState.CsiEntry
            ParserAction.None, // VTState.CsiIgnore
            ParserAction.None, // VTState.CsiIntermediate
            ParserAction.None, // VTState.CsiParam
            ParserAction.None, // VTState.DcsEntry
            ParserAction.None, // VTState.DcsIgnore
            ParserAction.None, // VTState.DcsIntermediate
            ParserAction.None, // VTState.DcsParam
            ParserAction.Unhook, // VTState.DcsPassthrough
            ParserAction.None, // VTState.Escape
            ParserAction.None, // VTState.EscapeIntermediate
            ParserAction.None, // VTState.Ground
            ParserAction.OscEnd, // VTState.OscString
            ParserAction.None, // VTState.SosPmApcString
            ParserAction.None // VTState.Utf8
        };

        private static Func<ushort, ParserStateAction> CreateStateChange()
        {
            Type typeofShort = typeof(ushort);
            Type typeofVTStateAction = typeof(ParserStateAction);

            ParameterExpression paramKey = Expression.Parameter(typeofShort, "key");
            DefaultExpression defaultResult = Expression.Default(typeofVTStateAction);

            var cases = new List<SwitchCase>();

            #region State Change Table

            cases.AddRange(CreateStateCase(ParserState.Anywhere,
                (0x18, ParserState.Ground, ParserAction.Execute),
                (0x1a, ParserState.Ground, ParserAction.Execute),
                (0x1b, ParserState.Escape)
            ));

            cases.AddRange(CreateStateCase(ParserState.Ground,
                (0x00, 0x17, ParserAction.Execute),
                (0x19, ParserAction.Execute),
                (0x1C, 0x1F, ParserAction.Execute),
                (0x20, 0x7F, ParserAction.Print),
                (0x80, 0x8F, ParserAction.Execute),
                (0x91, 0x9A, ParserAction.Execute),
                (0x9C, ParserAction.Execute),
                (0xC2, 0xDF, ParserState.Utf8, ParserAction.BeginUtf8),
                (0xE0, 0xEF, ParserState.Utf8, ParserAction.BeginUtf8),
                (0xF0, 0xF4, ParserState.Utf8, ParserAction.BeginUtf8)
            ));

            cases.AddRange(CreateStateCase(ParserState.Escape,
                (0x00, 0x17, ParserAction.Execute),
                (0x19, ParserAction.Execute),
                (0x1C, 0x1F, ParserAction.Execute),
                (0x7F, ParserAction.Ignore),
                (0x20, 0x2F, ParserState.EscapeIntermediate, ParserAction.Collect),
                (0x30, 0x4F, ParserState.Ground, ParserAction.EscDispatch),
                (0x51, 0x57, ParserState.Ground, ParserAction.EscDispatch),
                (0x59, ParserState.Ground, ParserAction.EscDispatch),
                (0x5A, ParserState.Ground, ParserAction.EscDispatch),
                (0x5C, ParserState.Ground, ParserAction.EscDispatch),
                (0x60, 0x7E, ParserState.Ground, ParserAction.EscDispatch),
                (0x5B, ParserState.CsiEntry),
                (0x5D, ParserState.OscString),
                (0x50, ParserState.DcsEntry),
                (0x58, ParserState.SosPmApcString),
                (0x5E, ParserState.SosPmApcString),
                (0x5F, ParserState.SosPmApcString)
            ));

            cases.AddRange(CreateStateCase(ParserState.EscapeIntermediate,
                (0x00, 0x17, ParserAction.Execute),
                (0x19, ParserAction.Execute),
                (0x1C, 0x1F, ParserAction.Execute),
                (0x7F, ParserAction.Ignore),
                (0x20, 0x2F, ParserAction.Collect),
                (0x30, 0x7E, ParserState.Ground, ParserAction.EscDispatch)
            ));

            cases.AddRange(CreateStateCase(ParserState.CsiEntry,
                (0x00, 0x17, ParserAction.Execute),
                (0x19, ParserAction.Execute),
                (0x1C, 0x1F, ParserAction.Execute),
                (0x7F, ParserAction.Ignore),
                (0x20, 0x2F, ParserState.CsiIntermediate, ParserAction.Collect),
                (0x3A, ParserState.CsiIgnore),
                (0x30, 0x39, ParserState.CsiParam, ParserAction.Param),
                (0x3B, ParserState.CsiParam, ParserAction.Param),
                (0x3C, 0x3F, ParserState.CsiParam, ParserAction.Collect),
                (0x40, 0x7E, ParserState.Ground, ParserAction.CsiDispatch)
            ));

            cases.AddRange(CreateStateCase(ParserState.CsiIgnore,
                (0x00, 0x17, ParserAction.Execute),
                (0x19, ParserAction.Execute),
                (0x1C, 0x1F, ParserAction.Execute),
                (0x7F, ParserAction.Ignore),
                (0x20, 0x3F, ParserAction.Ignore),
                (0x40, 0x7E, ParserState.Ground)
            ));

            cases.AddRange(CreateStateCase(ParserState.CsiParam,
                (0x00, 0x17, ParserAction.Execute),
                (0x19, ParserAction.Execute),
                (0x1C, 0x1F, ParserAction.Execute),
                (0x7F, ParserAction.Ignore),
                (0x3B, ParserAction.Param),
                (0x3A, ParserState.CsiIgnore),
                (0x30, 0x39, ParserAction.Param),
                (0x3C, 0x3F, ParserState.CsiIgnore),
                (0x20, 0x2F, ParserState.CsiIntermediate, ParserAction.Collect),
                (0x40, 0x7E, ParserState.Ground, ParserAction.CsiDispatch)
            ));

            cases.AddRange(CreateStateCase(ParserState.CsiIntermediate,
                (0x00, 0x17, ParserAction.Execute),
                (0x19, ParserAction.Execute),
                (0x1C, 0x1F, ParserAction.Execute),
                (0x7F, ParserAction.Ignore),
                (0x20, 0x2F, ParserAction.Collect),
                (0x30, 0x3F, ParserState.CsiIgnore),
                (0x40, 0x7E, ParserState.Ground, ParserAction.CsiDispatch)
            ));

            cases.AddRange(CreateStateCase(ParserState.DcsEntry,
                (0x00, 0x17, ParserAction.Ignore),
                (0x19, ParserAction.Ignore),
                (0x1C, 0x1F, ParserAction.Ignore),
                (0x7F, ParserAction.Ignore),
                (0x3A, ParserState.DcsIgnore),
                (0x20, 0x2F, ParserState.DcsIntermediate, ParserAction.Collect),
                (0x30, 0x39, ParserState.DcsParam, ParserAction.Param),
                (0x3B, ParserState.DcsParam, ParserAction.Param),
                (0x3B, 0x3F, ParserState.DcsParam, ParserAction.Collect),
                (0x40, 0x7E, ParserState.DcsPassthrough)
            ));

            cases.AddRange(CreateStateCase(ParserState.DcsIntermediate,
                (0x00, 0x17, ParserAction.Ignore),
                (0x19, ParserAction.Ignore),
                (0x1C, 0x1F, ParserAction.Ignore),
                (0x7F, ParserAction.Ignore),
                (0x20, 0x2F, ParserAction.Collect),
                (0x30, 0x3F, ParserState.DcsIgnore),
                (0x40, 0x7E, ParserState.DcsPassthrough)
            ));

            cases.AddRange(CreateStateCase(ParserState.DcsIgnore,
                (0x00, 0x17, ParserAction.Ignore),
                (0x19, ParserAction.Ignore),
                (0x1C, 0x1F, ParserAction.Ignore),
                (0x20, 0x7F, ParserAction.Ignore),
                (0x9C, ParserState.Ground)
            ));

            cases.AddRange(CreateStateCase(ParserState.DcsParam,
                (0x00, 0x17, ParserAction.Ignore),
                (0x19, ParserAction.Ignore),
                (0x1C, 0x1F, ParserAction.Ignore),
                (0x7F, ParserAction.Ignore),
                (0x30, 0x39, ParserAction.Param),
                (0x3B, ParserAction.Param),
                (0x3A, ParserState.DcsIgnore),
                (0x3C, 0x3F, ParserState.DcsIgnore),
                (0x20, 0x2F, ParserState.DcsIntermediate, ParserAction.Collect),
                (0x40, 0x7E, ParserState.DcsPassthrough)
            ));

            cases.AddRange(CreateStateCase(ParserState.DcsPassthrough,
                (0x00, 0x17, ParserAction.Put),
                (0x19, ParserAction.Put),
                (0x1C, 0x1F, ParserAction.Put),
                (0x20, 0x7E, ParserAction.Put),
                (0x7F, ParserAction.Ignore),
                (0x9C, ParserState.Ground)
            ));

            cases.AddRange(CreateStateCase(ParserState.SosPmApcString,
                (0x00, 0x17, ParserAction.Ignore),
                (0x19, ParserAction.Ignore),
                (0x1C, 0x1F, ParserAction.Ignore),
                (0x20, 0x7F, ParserAction.Ignore),
                (0x9C, ParserState.Ground)
            ));

            cases.AddRange(CreateStateCase(ParserState.OscString,
                (0x00, 0x06, ParserAction.Ignore),
                (0x07, ParserState.Ground),
                (0x08, 0x17, ParserAction.Ignore),
                (0x19, ParserAction.Ignore),
                (0x1C, 0x1F, ParserAction.Ignore),
                (0x20, 0xFF, ParserAction.OscPut)
            ));

            #endregion State Change Table

            SwitchExpression switchCase = Expression.Switch(paramKey, defaultResult, cases.ToArray());
            var lambda = Expression.Lambda<Func<ushort, ParserStateAction>>(switchCase, paramKey);
            return lambda.Compile();
        }

        private static IEnumerable<SwitchCase> CreateStateCase(ParserState state, params StateChangeDescriptor[] descs)
        {
            var key = (ushort)((ushort)state << 8);
            for (var i = 0; i < descs.Length; i++)
            {
                StateChangeDescriptor desc = descs[i];
                IEnumerable<ConstantExpression> testValues = desc.GetRange(key).Select(x => Expression.Constant(x));
                yield return Expression.SwitchCase(
                    Expression.Constant(desc.Result),
                    testValues
                );
            }
        }

        public static ParserStateAction GetStateChange(this ParserState state, byte next)
            => StateChange((ushort)((ushort)state << 8 | next));

        public static ParserStateAction WithEntryAction(this ParserStateAction stateAction)
            => stateAction.WithAction(EntryActions[(byte)stateAction.State]);

        public static ParserStateAction WithExitAction(this ParserStateAction stateAction)
            => stateAction.WithAction(ExitActions[(byte)stateAction.State]);
    }
}
