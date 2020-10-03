#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using Mirai_CSharp.Models;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Linq;

namespace Mirai_CSharp.Robot
{
    public partial class Plugins
    {
        public async Task<bool> ImageMonitor(MiraiHttpSession session, IGroupMessageEventArgs e)
        {
            var chain = e.Chain;
            foreach (var msg in chain)
            {
                if (msg is ImageMessage image)
                {
                    await using var file = new System.IO.StreamWriter("ImageMonitor.txt", true);
                    await file.WriteLineAsync($"{DateTime.Now.ToLongTimeString()} {e.Sender.Group.Id} {e.Sender.Id} {e.Sender.Name} {image.Url}");
                }
            }
            return false;
        }

        public async Task<bool> Helper(MiraiHttpSession session, IGroupMessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        static string[] memberBanCommandList = new string[] { "禁言", "mute", "ban" };
        public async Task<bool> MemberBan(MiraiHttpSession session, IGroupMessageEventArgs e)
        {
            if(!Config.ApplyedGroups.Contains(e.Sender.Group.Id)) return false;
            string? command;
            bool isCommand = Utility.ProcessCommandSession(e, out command);
            if (!isCommand)
                return false;
            var commandArgs = command.Split(' ', count : 3, StringSplitOptions.RemoveEmptyEntries);
            if(commandArgs.Length != 3 || ! memberBanCommandList.Contains(commandArgs[0].ToLower()))
                return false;

            long target;
            string[] time;
            int h, m, s;
            
            bool ok = long.TryParse(commandArgs[1], out target);
            time = commandArgs[2].Split(':', StringSplitOptions.RemoveEmptyEntries);
            ok &= (time.Length == 3);
            if (!ok)
            {
                await session.SendGroupMessageAsync(e.Sender.Group.Id, new IMessageBase[] { new AtMessage(e.Sender.Id),
                                                                                            new PlainMessage("参数错误") });
                return false;
            }
            if ( !int.TryParse(time[0], out h) || !int.TryParse(time[1], out m) || !int.TryParse(time[2], out s))
            {
                await session.SendGroupMessageAsync(e.Sender.Group.Id, new IMessageBase[] { new AtMessage(e.Sender.Id),
                                                                                            new PlainMessage("参数错误") });
                return false;
            }
            if (Global.currentTask[e.Sender.Group.Id] != null)
            {
                await session.SendGroupMessageAsync(e.Sender.Group.Id, new IMessageBase[] { new AtMessage(e.Sender.Id),
                                                                                            new PlainMessage("已有命令注册，请等待被注册命令结束") });
                return false;
            }

            Global.currentTask[e.Sender.Group.Id] = new MemberBan(target, e.Sender.Group.Id, session, new TimeSpan(h, m, s));
            await Task.Delay(TimeSpan.FromMinutes(2));
            Global.currentTask[e.Sender.Group.Id] = null;

            return false;
        }

        public async Task<bool> Accept(MiraiHttpSession session, IGroupMessageEventArgs e)
        {
            if (!Config.ApplyedGroups.Contains(e.Sender.Group.Id) || !Utility.SiglePlainMessageChecker(e)) return false;
            if (((PlainMessage) e.Chain[0]).Message.ToLower() != "accept") return false;
            var task = Global.currentTask[e.Sender.Group.Id];
            if (task == null)
            {
                await session.SendGroupMessageAsync(e.Sender.Group.Id, new PlainMessage("当前无挂起的task！"));
                return false;
            }
            else
                await task.IncreaseCurrentNum();
            return false;
        }
    }
}
