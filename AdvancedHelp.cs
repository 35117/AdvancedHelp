using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;
using On.OTAPI;
using Terraria;
using Terraria.Chat;
using Terraria.GameContent.NetModules;
using Terraria.Net;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace AdvancedHelp
{
    [ApiVersion(2, 1)] // 确保插件与TShock的版本兼容
    public class AdvancedHelp : TerrariaPlugin
    {
        public override string Author => "35117";
        public override string Description => "提供自定义的帮助命令";
        public override string Name => "AdvancedHelp";
        public override Version Version => new Version(1, 0, 0);
        internal static Configuration Config = new();

        public AdvancedHelp(Main game) : base(game)
        {
            base.Order = 1;
        }

        public override void Initialize()
        {
            GeneralHooks.ReloadEvent += ReloadEvent;
            try
            {
                Config = Configuration.Read();
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"[AdvancedHelp] 加载配置文件失败: {ex}");
            }

            if (Config.EnabledTshockHelpCover)
            {
                Commands.ChatCommands.RemoveAll((Command x) => x.Name == "help");
                Commands.ChatCommands.Add(new Command(Help, "help"));
                Hooks.MessageBuffer.InvokeGetData += MessageBuffer_InvokeGetData;
            }
            else
            {
                Commands.ChatCommands.Add(new Command(Help, Config.HelpCommand));
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeneralHooks.ReloadEvent -= ReloadEvent;
                if (Config.EnabledTshockHelpCover)
                {
                    Hooks.MessageBuffer.InvokeGetData -= MessageBuffer_InvokeGetData;
                    Commands.ChatCommands.RemoveAll(c => c.Name == "help");
                }
                else
                {
                    var asm = Assembly.GetExecutingAssembly();
                    Commands.ChatCommands.RemoveAll(c => c.CommandDelegate.Method?.DeclaringType?.Assembly == asm);
                }
            }
            base.Dispose(disposing);
        }

        #region 重读
        private static void ReloadEvent(ReloadEventArgs e)
        {
            Config = Configuration.Read();
            e.Player?.SendSuccessMessage("[AdvancedHelp] 重新加载配置完毕。");
            Config.Write();
        }
        #endregion

        #region 加载配置
        private static void LoadConfig()
        {
            Config = Configuration.Read();
            Config.Write();
        }
        #endregion
        private bool MessageBuffer_InvokeGetData(Hooks.MessageBuffer.orig_InvokeGetData orig, MessageBuffer instance, ref byte packetId, ref int readOffset, ref int start, ref int length, ref int messageType, int maxPackets)
        {
            try
            {
                // 如果消息类型是82，即玩家发送的聊天消息
                if (messageType == 82)
                {
                    instance.ResetReader();
                    instance.reader.BaseStream.Position = (long)(start + 1);
                    if (instance.reader.ReadUInt16() == NetManager.Instance.GetId<NetTextModule>())
                    {
                        ChatMessage chatMessage = ChatMessage.Deserialize(instance.reader);
                        // 如果消息内容是"/help"
                        if (chatMessage.CommandId._name == "Help")
                        {
                            TSPlayer player = TShock.Players[instance.whoAmI];
                            // 调用自定义的帮助方法
                            Help(new CommandArgs(null, player, new List<string> { chatMessage.Text.Substring(5) })); // 5是"/help"的长度
                            // 返回false，表示我们已处理该消息，不需要TShock再处理
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"[AdvancedHelp] 消息处理失败: {ex}");
            }
            // 调用原始的处理方法
            return orig(instance, ref packetId, ref readOffset, ref start, ref length, ref messageType, maxPackets);
        }
        // 自定义的帮助方法
        private static void Help(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                // 如果没有参数，发送默认帮助信息
                foreach (var line in Config.DefaultHelpContent)
                {
                    args.Player.SendMessage(line, 255, 255, 255);
                }
            }
            else
            {
                // 如果有参数，检查参数帮助内容
                string combinedParams = string.Join(" ", args.Parameters);
                bool sentHelp = false;
                if (Config.ArgumentHelpContent.TryGetValue(combinedParams, out var helpContentList))
                {
                    foreach (var helpContent in helpContentList)
                    {
                        args.Player.SendMessage(helpContent, 255, 255, 255);
                    }
                    sentHelp = true;
                }
                if (!sentHelp)
                {
                    foreach (var line in Config.DefaultHelpContent)
                    {
                        args.Player.SendMessage(line, 255, 255, 255);
                    }
                }
            }
        }
    }
}