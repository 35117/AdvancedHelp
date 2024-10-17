using Newtonsoft.Json;
using TShockAPI;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace AdvancedHelp
{
    internal class Configuration
    {
        #region 读取与创建配置文件方法
        public static readonly string FilePath = Path.Combine(TShock.SavePath, "AdvancedHelp.json");

        public void Write()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }

        public static Configuration Read()
        {
            if (!File.Exists(FilePath))
            {
                var newConfig = new Configuration();
                newConfig.Initialize();
                newConfig.Write();
                TShock.Log.ConsoleError("[AdvancedHelp] 未找配置文件，已新建预设");
                return newConfig;
            }
            else
            {
                string jsonContent = File.ReadAllText(FilePath);
                return JsonConvert.DeserializeObject<Configuration>(jsonContent)!;
            }
        }
        #endregion
        [JsonProperty("Tshock原生Help覆盖", Order = -10)]
        public bool EnabledTshockHelpCover { get; set; } = true;
        [JsonProperty("不启用覆盖时帮助指令", Order = -9)]
        public string HelpCommand { get; set; } = "help2";
        [JsonProperty("默认帮助内容", Order = 0)]
        public List<string> DefaultHelpContent { get; set; } = new List<string>();

        [JsonProperty("参数帮助内容", Order = 1)]
        public Dictionary<string, List<string>> ArgumentHelpContent { get; set; } = new Dictionary<string, List<string>>();

        public void Initialize()
        {
            DefaultHelpContent = new List<string>
            {
                "这是默认的帮助信息。",
                "如果输入 'help' 而没有参数，将显示此信息。"
            };
            ArgumentHelpContent = new Dictionary<string, List<string>>()
            {
                { "cmd1", new List<string> { "这是 'cmd1' 的帮助信息。", "这是 'cmd1' 的另一条帮助信息。" } },
                { "cmd 2", new List<string> { "这是 'cmd 2' 的帮助信息。", "参数帮助允许携带空格", "只要输入help时输入一样的信息即可" } }
            };
        }
    }
}
