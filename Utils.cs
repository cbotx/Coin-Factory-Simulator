using System.Text.Json;

namespace CoinFactorySim {
    public static class Utils {

        public static string FormatNumber(double num) {
            if (num >= 1000000000000) {
                return $"{num / 1000000000000:0.##}t";
            } else if (num >= 1000000000) {
                return $"{num / 1000000000:0.##}b";
            } else if (num >= 1000000) {
                return $"{num / 1000000:0.##}m";
            } else if (num >= 1000) {
                return $"{num / 1000:0.##}k";
            } else {
                return $"{num:0.##}";
            }
        }
        public static void SaveToFile<T>(string path, T obj) {
            if (path == null) throw new ArgumentNullException(nameof(path));
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            string json = JsonSerializer.Serialize(obj);
            File.WriteAllText(path, json);
        }

        public static T LoadFromFile<T>(string path) {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("File not found", path);
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json)!;
        }

        public static string GetTimeString(int idx) {
            int minute = idx / 60;
            int second = idx % 60;
            return $"{minute:00}:{second:00}";
        }
    }
}
