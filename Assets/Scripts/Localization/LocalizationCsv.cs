using System;
using System.Collections.Generic;
using System.Text;

namespace VampireSurvivorLike
{
    public static class LocalizationCsv
    {
        public static Dictionary<string, string> ParseKeyValueTable(string csvContent)
        {
            var result = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(csvContent)) return result;

            var lines = csvContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var startIndex = 0;
            if (lines.Length > 0)
            {
                var header = ParseLine(lines[0]);
                if (header.Length >= 2 && string.Equals(header[0], "key", StringComparison.OrdinalIgnoreCase))
                {
                    startIndex = 1;
                }
            }

            for (var i = startIndex; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                var values = ParseLine(line);
                if (values.Length < 2) continue;

                var key = values[0];
                if (string.IsNullOrWhiteSpace(key)) continue;

                var value = values[1];
                result[key.Trim()] = value ?? string.Empty;
            }

            return result;
        }

        private static string[] ParseLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                        continue;
                    }

                    inQuotes = !inQuotes;
                    continue;
                }

                if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                    continue;
                }

                current.Append(c);
            }

            result.Add(current.ToString());
            return result.ToArray();
        }
    }
}
