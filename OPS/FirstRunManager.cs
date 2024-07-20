// FirstRunManager.cs
using Newtonsoft.Json;
using System.IO;

public class FirstRunManager
{
    private const string FilePath = "firstRun.json";
    public static FirstRunData _data;

    public static bool IsFirstRun()
    {
        if (_data == null)
        {
            LoadData();
        }
        if (_data.FirstRun)
        {
            _data.Width = 1280;
            _data.Height = 1024;
        }
        return _data.FirstRun;
    }

    public static void SetFirstRun(bool firstRun, int width, int height)
    {
        if (_data == null)
        {
            LoadData();
        }

        _data.FirstRun = firstRun;
        _data.Height = height;
        _data.Width = width;
        SaveData();
    }

    private static void LoadData()
    {
        if (File.Exists(FilePath))
        {
            var json = File.ReadAllText(FilePath);
            _data = JsonConvert.DeserializeObject<FirstRunData>(json);
        }
        else
        {
            _data = new FirstRunData { FirstRun = true };
        }
    }

    private static void SaveData()
    {
        var json = JsonConvert.SerializeObject(_data);
        File.WriteAllText(FilePath, json);
    }

    public class FirstRunData
    {
        public bool FirstRun { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }

    }
}