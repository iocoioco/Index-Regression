using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_Tradegy.Library
{
    public static class WeightManager
    {
        private static readonly Dictionary<string, double> _weights = 
            new Dictionary<string, double>();
        public static IReadOnlyDictionary<string, double> Weights => _weights;

        private static readonly string _path = @"C:\병신\data\Weights.txt";

        public static void Load()
        {
            _weights.Clear();
            if (!File.Exists(_path))
                return;

            foreach (var line in File.ReadLines(_path))
            {
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 && double.TryParse(parts[1], out double value))
                    _weights[parts[0]] = value;
            }
        }

        public static void Save()
        {
            File.WriteAllLines(_path, _weights.Select(kv => $"{kv.Key} {kv.Value}"));
        }

        public static double Get(string key) =>
            _weights.TryGetValue(key, out var value) ? value : 0;

        public static void Set(string key, double value) =>
            _weights[key] = value;
    }
}
