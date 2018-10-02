using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TerminalVelocity.VT
{
    // https://github.com/jwilm/vte/blob/master/src/table.rs.in

    internal static class VTStateActionExtensions
    {
        private struct StateChangeDescriptor
        {
            public readonly byte Min;
            public readonly byte Max;
            public readonly VTStateAction Result;

            public StateChangeDescriptor(byte min, byte max, VTStateAction result)
                => (Min, Max, Result) = (min, max, result);

            public IEnumerable<ushort> GetRange(ushort key)
            {
                for(var i = (ushort)Min; i <= Max; i++)
                    yield return (ushort)(key | i);
            }

            public static implicit operator StateChangeDescriptor(
                (byte Min, byte Max, VTParserState State, VTParserAction Action) desc)
                => new StateChangeDescriptor(desc.Min, desc.Max, new VTStateAction(desc.State, desc.Action));
                
            public static implicit operator StateChangeDescriptor(
                (byte Single, VTParserState State, VTParserAction Action) desc)
                => new StateChangeDescriptor(desc.Single, desc.Single, new VTStateAction(desc.State, desc.Action));
                
            public static implicit operator StateChangeDescriptor(
                (byte Min, byte Max, VTParserAction Action) desc)
                => new StateChangeDescriptor(desc.Min, desc.Max, new VTStateAction(desc.Action));
                
            public static implicit operator StateChangeDescriptor(
                (byte Single, VTParserAction Action) desc)
                => new StateChangeDescriptor(desc.Single, desc.Single, new VTStateAction(desc.Action));
                
            public static implicit operator StateChangeDescriptor(
                (byte Min, byte Max, VTParserState State) desc)
                => new StateChangeDescriptor(desc.Min, desc.Max, new VTStateAction(desc.State));
                
            public static implicit operator StateChangeDescriptor(
                (byte Single, VTParserState State) desc)
                => new StateChangeDescriptor(desc.Single, desc.Single, new VTStateAction(desc.State));
        }

        private static readonly Func<ushort, VTStateAction> StateChange = CreateStateChange();

        private static readonly VTParserAction[] EntryActions = new[]
        {
            VTParserAction.None, // VTState.Anywhere
            VTParserAction.Clear, // VTState.CsiEntry
            VTParserAction.None, // VTState.CsiIgnore
            VTParserAction.None, // VTState.CsiIntermediate
            VTParserAction.None, // VTState.CsiParam
            VTParserAction.Clear, // VTState.DcsEntry
            VTParserAction.None, // VTState.DcsIgnore
            VTParserAction.None, // VTState.DcsIntermediate
            VTParserAction.None, // VTState.DcsParam
            VTParserAction.Hook, // VTState.DcsPassthrough
            VTParserAction.Clear, // VTState.Escape
            VTParserAction.None, // VTState.EscapeIntermediate
            VTParserAction.None, // VTState.Ground
            VTParserAction.OscStart, // VTState.OscString
            VTParserAction.None, // VTState.SosPmApcString
            VTParserAction.None  // VTState.Utf8
        };

        private static readonly VTParserAction[] ExitActions = new[]
        {
            VTParserAction.None, // VTState.Anywhere
            VTParserAction.None, // VTState.CsiEntry
            VTParserAction.None, // VTState.CsiIgnore
            VTParserAction.None, // VTState.CsiIntermediate
            VTParserAction.None, // VTState.CsiParam
            VTParserAction.None, // VTState.DcsEntry
            VTParserAction.None, // VTState.DcsIgnore
            VTParserAction.None, // VTState.DcsIntermediate
            VTParserAction.None, // VTState.DcsParam
            VTParserAction.Unhook, // VTState.DcsPassthrough
            VTParserAction.None, // VTState.Escape
            VTParserAction.None, // VTState.EscapeIntermediate
            VTParserAction.None, // VTState.Ground
            VTParserAction.OscEnd, // VTState.OscString
            VTParserAction.None, // VTState.SosPmApcString
            VTParserAction.None // VTState.Utf8
        };

        private static Func<ushort, VTStateAction> CreateStateChange()
        {
            var typeofShort = typeof(ushort);
            var typeofVTStateAction = typeof(VTStateAction);

            var paramKey = Expression.Parameter(typeofShort, "key");
            var defaultResult = Expression.Default(typeofVTStateAction);

            var cases = new List<SwitchCase>();
            
            #region State Change Table

            cases.AddRange(CreateStateCase(VTParserState.Anywhere, 
                (0x18, VTParserState.Ground, VTParserAction.Execute),
                (0x1a, VTParserState.Ground, VTParserAction.Execute),
                (0x1b, VTParserState.Escape)
            ));

            cases.AddRange(CreateStateCase(VTParserState.Ground,
                (0x00, 0x17, VTParserAction.Execute),
                (0x19, VTParserAction.Execute),
                (0x1C, 0x1F, VTParserAction.Execute),
                (0x20, 0x7F, VTParserAction.Print),
                (0x80, 0x8F, VTParserAction.Execute),
                (0x91, 0x9A, VTParserAction.Execute),
                (0x9C, VTParserAction.Execute),
                (0xC2, 0xDF, VTParserState.Utf8, VTParserAction.BeginUtf8),
                (0xE0, 0xEF, VTParserState.Utf8, VTParserAction.BeginUtf8),
                (0xF0, 0xF4, VTParserState.Utf8, VTParserAction.BeginUtf8)
            ));

            cases.AddRange(CreateStateCase(VTParserState.Escape, 
                (0x00, 0x17, VTParserAction.Execute),
                (0x19, VTParserAction.Execute),
                (0x1C, 0x1F, VTParserAction.Execute),
                (0x7F, VTParserAction.Ignore),
                (0x20, 0x2F, VTParserState.EscapeIntermediate, VTParserAction.Collect),
                (0x30, 0x4F, VTParserState.Ground, VTParserAction.EscDispatch),
                (0x51, 0x57, VTParserState.Ground, VTParserAction.EscDispatch),
                (0x59, VTParserState.Ground, VTParserAction.EscDispatch),
                (0x5A, VTParserState.Ground, VTParserAction.EscDispatch),
                (0x5C, VTParserState.Ground, VTParserAction.EscDispatch),
                (0x60, 0x7E, VTParserState.Ground, VTParserAction.EscDispatch),
                (0x5B, VTParserState.CsiEntry),
                (0x5D, VTParserState.OscString),
                (0x50, VTParserState.DcsEntry),
                (0x58, VTParserState.SosPmApcString),
                (0x5E, VTParserState.SosPmApcString),
                (0x5F, VTParserState.SosPmApcString)
            ));

            cases.AddRange(CreateStateCase(VTParserState.EscapeIntermediate, 
                (0x00, 0x17, VTParserAction.Execute),
                (0x19, VTParserAction.Execute),
                (0x1C, 0x1F, VTParserAction.Execute),
                (0x7F, VTParserAction.Ignore),
                (0x20, 0x2F, VTParserAction.Collect),
                (0x30, 0x7E, VTParserState.Ground, VTParserAction.EscDispatch)
            ));

            cases.AddRange(CreateStateCase(VTParserState.CsiEntry,
                (0x00, 0x17, VTParserAction.Execute),
                (0x19, VTParserAction.Execute),
                (0x1C, 0x1F, VTParserAction.Execute),
                (0x7F, VTParserAction.Ignore),
                (0x20, 0x2F, VTParserState.CsiIntermediate, VTParserAction.Collect),
                (0x3A, VTParserState.CsiIgnore),
                (0x30, 0x39, VTParserState.CsiParam, VTParserAction.Param),
                (0x3B, VTParserState.CsiParam, VTParserAction.Param),
                (0x3C, 0x3F, VTParserState.CsiParam, VTParserAction.Collect),
                (0x40, 0x7E, VTParserState.Ground, VTParserAction.CsiDispatch)
            ));

            cases.AddRange(CreateStateCase(VTParserState.CsiIgnore,
                (0x00, 0x17, VTParserAction.Execute),
                (0x19, VTParserAction.Execute),
                (0x1C, 0x1F, VTParserAction.Execute),
                (0x7F, VTParserAction.Ignore),
                (0x20, 0x3F, VTParserAction.Ignore),
                (0x40, 0x7E, VTParserState.Ground)
            ));

            cases.AddRange(CreateStateCase(VTParserState.CsiParam,
                (0x00, 0x17, VTParserAction.Execute),
                (0x19, VTParserAction.Execute),
                (0x1C, 0x1F, VTParserAction.Execute),
                (0x7F, VTParserAction.Ignore),
                (0x3B, VTParserAction.Param),
                (0x3A, VTParserState.CsiIgnore),
                (0x30, 0x39, VTParserAction.Param),
                (0x3C, 0x3F, VTParserState.CsiIgnore),
                (0x20, 0x2F, VTParserState.CsiIntermediate, VTParserAction.Collect),
                (0x40, 0x7E, VTParserState.Ground, VTParserAction.CsiDispatch)
            ));

            cases.AddRange(CreateStateCase(VTParserState.CsiIntermediate,
                (0x00, 0x17, VTParserAction.Execute),
                (0x19, VTParserAction.Execute),
                (0x1C, 0x1F, VTParserAction.Execute),
                (0x7F, VTParserAction.Ignore),
                (0x20, 0x2F, VTParserAction.Collect),
                (0x30, 0x3F, VTParserState.CsiIgnore),
                (0x40, 0x7E, VTParserState.Ground, VTParserAction.CsiDispatch)
            ));

            cases.AddRange(CreateStateCase(VTParserState.DcsEntry,
                (0x00, 0x17, VTParserAction.Ignore),
                (0x19, VTParserAction.Ignore),
                (0x1C, 0x1F, VTParserAction.Ignore),
                (0x7F, VTParserAction.Ignore),
                (0x3A, VTParserState.DcsIgnore),
                (0x20, 0x2F, VTParserState.DcsIntermediate, VTParserAction.Collect),
                (0x30, 0x39, VTParserState.DcsParam, VTParserAction.Param),
                (0x3B, VTParserState.DcsParam, VTParserAction.Param),
                (0x3B, 0x3F, VTParserState.DcsParam, VTParserAction.Collect),
                (0x40, 0x7E, VTParserState.DcsPassthrough)
            ));

            cases.AddRange(CreateStateCase(VTParserState.DcsIntermediate,
                (0x00, 0x17, VTParserAction.Ignore),
                (0x19, VTParserAction.Ignore),
                (0x1C, 0x1F, VTParserAction.Ignore),
                (0x7F, VTParserAction.Ignore),
                (0x20, 0x2F, VTParserAction.Collect),
                (0x30, 0x3F, VTParserState.DcsIgnore),
                (0x40, 0x7E, VTParserState.DcsPassthrough)
            ));

            cases.AddRange(CreateStateCase(VTParserState.DcsIgnore,
                (0x00, 0x17, VTParserAction.Ignore),
                (0x19, VTParserAction.Ignore),
                (0x1C, 0x1F, VTParserAction.Ignore),
                (0x20, 0x7F, VTParserAction.Ignore),
                (0x9C, VTParserState.Ground)
            ));

            cases.AddRange(CreateStateCase(VTParserState.DcsParam,
                (0x00, 0x17, VTParserAction.Ignore),
                (0x19, VTParserAction.Ignore),
                (0x1C, 0x1F, VTParserAction.Ignore),
                (0x7F, VTParserAction.Ignore),
                (0x30, 0x39, VTParserAction.Param),
                (0x3B, VTParserAction.Param),
                (0x3A, VTParserState.DcsIgnore),
                (0x3C, 0x3F, VTParserState.DcsIgnore),
                (0x20, 0x2F, VTParserState.DcsIntermediate, VTParserAction.Collect),
                (0x40, 0x7E, VTParserState.DcsPassthrough)
            ));

            cases.AddRange(CreateStateCase(VTParserState.DcsPassthrough,
                (0x00, 0x17, VTParserAction.Put),
                (0x19, VTParserAction.Put),
                (0x1C, 0x1F, VTParserAction.Put),
                (0x20, 0x7E, VTParserAction.Put),
                (0x7F, VTParserAction.Ignore),
                (0x9C, VTParserState.Ground)
            ));

            cases.AddRange(CreateStateCase(VTParserState.SosPmApcString,
                (0x00, 0x17, VTParserAction.Ignore),
                (0x19, VTParserAction.Ignore),
                (0x1C, 0x1F, VTParserAction.Ignore),
                (0x20, 0x7F, VTParserAction.Ignore),
                (0x9C, VTParserState.Ground)
            ));

            cases.AddRange(CreateStateCase(VTParserState.OscString,
                (0x00, 0x06, VTParserAction.Ignore),
                (0x07, VTParserState.Ground),
                (0x08, 0x17, VTParserAction.Ignore),
                (0x19, VTParserAction.Ignore),
                (0x1C, 0x1F, VTParserAction.Ignore),
                (0x20, 0xFF, VTParserAction.OscPut)
            ));

            #endregion State Change Table

            var switchCase = Expression.Switch(paramKey, defaultResult, cases.ToArray());
            var lambda = Expression.Lambda<Func<ushort, VTStateAction>>(switchCase, paramKey);
            return lambda.Compile();
        }

        private static IEnumerable<SwitchCase> CreateStateCase(VTParserState state, params StateChangeDescriptor[] descs)
        {
            var key = (ushort)((ushort)state << 8);
            for (var i = 0; i < descs.Length; i++)
            {
                var desc = descs[i];
                var testValues = desc.GetRange(key).Select(x => Expression.Constant(x));
                yield return Expression.SwitchCase(
                    Expression.Constant(desc.Result),
                    testValues
                );
            }
        }

        public static VTStateAction GetStateChange(this VTParserState state, byte next)
            =>  StateChange((ushort)((ushort)state << 8 | next));

        public static VTStateAction WithEntryAction(this VTStateAction stateAction)
            => stateAction.WithAction(EntryActions[(byte)stateAction.State]);

        public static VTStateAction WithExitAction(this VTStateAction stateAction)
            => stateAction.WithAction(ExitActions[(byte)stateAction.State]);
    }
}