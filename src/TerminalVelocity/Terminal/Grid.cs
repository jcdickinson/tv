using System;
using System.Collections;
using System.Collections.Generic;
using System.Composition;

namespace TerminalVelocity.Terminal
{
    [Export]
    public class Grid : IEnumerable<Row>
    {
        public struct RowEnumerator : IEnumerator<Row>
        {
            public Row Current => _rows[(_index) % _rows.Length];
            object IEnumerator.Current => Current;

            private readonly Row[] _rows;
            private readonly int _end;
            private int _index;

            public RowEnumerator(Row[] rows, int index, int end)
            {
                _rows = rows;
                _end = end;
                _index = index;
            }

            public void Dispose() {}
            public void Reset() => throw new NotSupportedException();

            public bool MoveNext()
            {
                _index = (_index + 1) % _rows.Length;
                if (_index == _end)
                {
                    --_index;
                    return false;
                } 
                return true;
            }

        }

        private Row[] _rows;
        private int _startIndex;
        private int _currentIndex;
        private int _endIndex;

        public Grid()
        {
            _rows = new Row[30];
            _startIndex = _rows.Length - 1;
            _endIndex = 0;
        }

        public void Append(Row row)
        {
            _endIndex = (_endIndex + 1) % _rows.Length;
            if (_endIndex == _startIndex)
                _startIndex = (_startIndex + 1) % _rows.Length;
            _rows[_currentIndex] = row;
            _currentIndex = (_currentIndex + 1) % _rows.Length;
        }

        public RowEnumerator GetEnumerator()
        {
            return new RowEnumerator(_rows, _startIndex, _endIndex);
        }

        IEnumerator<Row> IEnumerable<Row>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
