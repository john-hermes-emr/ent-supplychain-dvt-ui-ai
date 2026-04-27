using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVT.Core.Helper
{
    public ref struct EfficientCsvParser
    {
        private ReadOnlySpan<char> _data;
        private int _position;

        public EfficientCsvParser(ReadOnlySpan<char> data)
        {
            _data = data;
            _position = 0;
        }

        public bool TryReadRow(out CsvRow row)
        {
            if (_position >= _data.Length)
            {
                row = default;
                return false;
            }

            var remaining = _data.Slice(_position);
            var newlineIndex = remaining.IndexOf('\n');

            ReadOnlySpan<char> line;
            if (newlineIndex == -1)
            {
                line = remaining;
                _position = _data.Length;
            }
            else
            {
                line = remaining.Slice(0, newlineIndex);
                _position += newlineIndex + 1;
            }

            // Handle Windows line endings
            if (line.Length > 0 && line[^1] == '\r')
            {
                line = line.Slice(0, line.Length - 1);
            }

            row = new CsvRow(line);
            return true;
        }
    }

    public ref struct CsvRow
    {
        private ReadOnlySpan<char> _line;
        private int _fieldCount;

        public CsvRow(ReadOnlySpan<char> line)
        {
            _line = line;
            _fieldCount = -1;  // Lazy count
        }

        public int FieldCount
        {
            get
            {
                if (_fieldCount == -1)
                {
                    _fieldCount = 1;
                    foreach (var c in _line)
                    {
                        if (c == '|') _fieldCount++;
                    }

                    // Handle trailing pipe
                    if (_line.Length > 0 && _line[_line.Length - 1] == '|')
                        _fieldCount--;
                }               

                return _fieldCount;
            }
        }

        public readonly bool IsEmptyLine => _line.IsEmpty || _line.Trim().IsEmpty;

        public ReadOnlySpan<char> GetField(int index)
        {
            var remaining = _line;
            var currentIndex = 0;

            while (!remaining.IsEmpty)
            {
                var pipeIndex = remaining.IndexOf('|');

                ReadOnlySpan<char> field;
                if (pipeIndex == -1)
                {
                    field = remaining;
                    remaining = ReadOnlySpan<char>.Empty;
                }
                else
                {
                    field = remaining.Slice(0, pipeIndex);
                    remaining = remaining.Slice(pipeIndex + 1);
                }

                if (currentIndex == index)
                {
                    return field;
                }

                currentIndex++;
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }

        public ReadOnlySpan<char> GetFieldOrDefault(int index)
        {
            if(index >= FieldCount)
            {
                return new ReadOnlySpan<char>();
            }

            return GetField(index);
        }

        public List<string> GetAsSplitStringList()
        {
            return _line.Slice(0, _line.Length)
                .ToString()
                .Split('|')
                .Select(field => field.Trim())
                .ToList();
        }
    }
}
