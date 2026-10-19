using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace LteRefSensTester.Models
{
    /// <summary>
    /// Single row in loss table (frequency → loss mapping)
    /// </summary>
    public class LossRow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        void N(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));

        double _f, _i, _o;
        public double Freq
        {
            get => _f;
            set
            {
                _f = value;
                N(nameof(Freq));
            }
        }
        public double Input
        {
            get => _i;
            set
            {
                _i = value;
                N(nameof(Input));
            }
        }
        public double Output
        {
            get => _o;
            set
            {
                _o = value;
                N(nameof(Output));
            }
        }
    }

    /// <summary>
    /// Loss table with interpolation support
    /// </summary>
    public class LossTable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        void N(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));

        string _name = "Default";
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                N(nameof(Name));
            }
        }

        public ObservableCollection<LossRow> Rows { get; set; } = new();

        /// <summary>
        /// Get interpolated loss value for given frequency (MHz)
        /// </summary>
        public double GetLoss(double freqMHz)
        {
            if (Rows == null || Rows.Count == 0) return 0;
            if (Rows.Count == 1) return Rows[0].Output;

            var s = Rows.OrderBy(e => e.Freq).ToList();
            if (freqMHz <= s[0].Freq) return s[0].Output;
            if (freqMHz >= s[^1].Freq) return s[^1].Output;

            for (int i = 0; i < s.Count - 1; i++)
            {
                if (freqMHz >= s[i].Freq && freqMHz <= s[i + 1].Freq)
                {
                    double t = (freqMHz - s[i].Freq) / (s[i + 1].Freq - s[i].Freq);
                    return s[i].Output + t * (s[i + 1].Output - s[i].Output);
                }
            }
            return 0;
        }

        public static string TablesFolder =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         "LteRefSens", "LossTables");

        /// <summary>
        /// Load all loss tables from folder
        /// </summary>
        public static List<LossTable> LoadAll(string folder)
        {
            var list = new List<LossTable>();
            try
            {
                Directory.CreateDirectory(folder);
                foreach (var f in Directory.GetFiles(folder, "*.loss.json"))
                {
                    try
                    {
                        var raw = JsonSerializer.Deserialize<LossTableDto>(File.ReadAllText(f));
                        if (raw != null)
                        {
                            var t = new LossTable { Name = raw.Name ?? "Default" };
                            foreach (var r in raw.Rows ?? new())
                                t.Rows.Add(new LossRow { Freq = r.Freq, Input = r.Input, Output = r.Output });
                            list.Add(t);
                        }
                    }
                    catch { }
                }
            }
            catch { }
            return list;
        }

        /// <summary>
        /// Save table to JSON file
        /// </summary>
        public void SaveToFile(string folder)
        {
            Directory.CreateDirectory(folder);
            var safe = string.Concat(Name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
            var dto = new LossTableDto
            {
                Name = Name,
                Rows = Rows.Select(r => new LossRowDto { Freq = r.Freq, Input = r.Input, Output = r.Output }).ToList()
            };
            File.WriteAllText(Path.Combine(folder, safe + ".loss.json"),
                JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true }));
        }

        /// <summary>
        /// Delete table file
        /// </summary>
        public void DeleteFile(string folder)
        {
            var safe = string.Concat(Name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
            var path = Path.Combine(folder, safe + ".loss.json");
            if (File.Exists(path)) File.Delete(path);
        }

        /// <summary>
        /// Clone table with new name
        /// </summary>
        public LossTable Clone(string newName)
        {
            var copy = new LossTable { Name = newName };
            foreach (var r in Rows)
                copy.Rows.Add(new LossRow { Freq = r.Freq, Input = r.Input, Output = r.Output });
            return copy;
        }

        // DTO classes for JSON serialization
        private class LossTableDto
        {
            public string? Name { get; set; }
            public List<LossRowDto>? Rows { get; set; }
        }

        private class LossRowDto
        {
            public double Freq { get; set; }
            public double Input { get; set; }
            public double Output { get; set; }
        }
    }
}
