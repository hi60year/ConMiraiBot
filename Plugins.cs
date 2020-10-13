#nullable enable
#pragma warning disable CS0618 
using System;
using System.Collections.Generic;
using System.Text;
using Mirai_CSharp.Models;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Linq;
using static Mirai_CSharp.Robot.Utility;
using Microsoft.VisualBasic;

namespace Mirai_CSharp.Robot
{
    public partial class Plugins
    {
        public async Task<bool> ManagementUsingAt(MiraiHttpSession session, IGroupMessageEventArgs e)
        {
            for (int i = 0; i < e.Chain.Length; i++)
            {
                if (e.Chain[i] is AtMessage atmsg)
                    e.Chain[i] = new PlainMessage(atmsg.Target.ToString());
                else if( !(e.Chain[i] is SourceMessage) && !(e.Chain[i] is PlainMessage) ) return false;
            }

            var newChain = new IMessageBase[] {e.Chain[0], new PlainMessage(e.Chain.Skip(1).Select(msg => ((PlainMessage) msg).Message)
                                                                            .Aggregate((x, y) => x + y)) };
            var newArgs = new GroupMessageEventArgs(newChain, e.Sender);
            await MemberBan(session, newArgs);
            await MemberKick(session, newArgs);
            return false;
        }
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

        static readonly string[] memberBanCommandList = new string[] { "禁言", "mute", "ban" };
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
            if(Global.currentTask[e.Sender.Group.Id] != null)
                await session.SendGroupMessageAsync(e.Sender.Group.Id, new PlainMessage("挂起的任务已超时"));
            Global.currentTask[e.Sender.Group.Id] = null;
            return false;
        }

        static readonly string[] memberKickCommandList = new string[] { "kick", "移除", "移出", "踢"};
        public async Task<bool> MemberKick(MiraiHttpSession session, IGroupMessageEventArgs e)
        {
            if (!Config.ApplyedGroups.Contains(e.Sender.Group.Id)) return false;
            string? command;
            bool iscommand = Utility.ProcessCommandSession(e, out command);
            if (!iscommand)
                return false;
            var commandArgs = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (commandArgs.Length != 2 || !memberKickCommandList.Contains(commandArgs[0]))
                return false;
            
            long target;
            if (!long.TryParse(commandArgs[1], out target))
            {
                await session.SendGroupMessageAsync(e.Sender.Group.Id,
                                                    new IMessageBase[] { new PlainMessage("输入的参数有误") },
                                                    ((SourceMessage) e.Chain[0]).Id);
                return false;
            }
            if (Global.currentTask[e.Sender.Group.Id] != null)
            {
                await session.SendGroupMessageAsync(e.Sender.Group.Id, new IMessageBase[] { new AtMessage(e.Sender.Id),
                                                                                            new PlainMessage("已有命令注册，请等待被注册命令结束") });
                return false;
            }

            Global.currentTask[e.Sender.Group.Id] = new MemberKick(target, e.Sender.Group.Id, session);
            await Task.Delay(TimeSpan.FromMinutes(2));
            if (Global.currentTask[e.Sender.Group.Id] != null)
                await session.SendGroupMessageAsync(e.Sender.Group.Id, new PlainMessage("挂起的任务已超时"));
            Global.currentTask[e.Sender.Group.Id] = null;
            return false;
        }

        public async Task<bool> Accept(MiraiHttpSession session, IGroupMessageEventArgs e)
        {
            if (!Config.ApplyedGroups.Contains(e.Sender.Group.Id) || !Utility.SiglePlainMessageChecker(e)) return false;
            if (((PlainMessage) e.Chain[1]).Message.ToLower() != "accept") return false;
            var task = Global.currentTask[e.Sender.Group.Id];
            if (task == null)
            {
                await session.SendGroupMessageAsync(e.Sender.Group.Id, new PlainMessage("当前无挂起的task！"));
                return false;
            }
            else
            {
                if (!Global.acceptList[e.Sender.Group.Id].Add(e.Sender.Id))
                {
                    await session.SendGroupMessageAsync(e.Sender.Group.Id, new IMessageBase[] { new PlainMessage("请勿重复确认") },
                        ((SourceMessage)e.Chain[0]).Id);
                    return false;
                }
                //这个顺序是为了保证即使禁言操作抛出了异常，send group message也能正常输出最后一个同步状态而不用写一个丑陋的try-finally块
                await session.SendGroupMessageAsync(e.Sender.Group.Id, new PlainMessage($"同步状态{task.CurrentMemberNum + 1}/{task.MemberNumRequired}"));
                await task.IncreaseCurrentNum();
            }
            return false;
        }

        public async Task<bool> OnAt(MiraiHttpSession session, IGroupMessageEventArgs e)
        {
            bool selfAted = e.Chain.Where(msg => msg is AtMessage)
                                   .Cast<AtMessage>()
                                   .Select(atmsg => atmsg.Target)
                                   .Contains(session.QQNumber.Value);
            if (selfAted)
            {
                var imgmsg = await session.UploadPictureAsync(UploadTarget.Group, @"..\..\..\images\kokoaCry.jpg");
                await session.SendGroupMessageAsync(e.Sender.Group.Id,
                                                    new IMessageBase[] { new PlainMessage("检测到您正在at我！主人很懒" +
                                                        "，不想实现接受at信息调用我的模块，请直接使用文字命令来调用！"),imgmsg},
                                                    ((SourceMessage)e.Chain[0]).Id);
            }
            return false;
        }

        public async Task<bool> MessageIdsRecorder(MiraiHttpSession session, IGroupMessageEventArgs e)
        {
            if (!Config.ApplyedGroups.Contains(e.Sender.Group.Id)) return false;
            Global.messageMemory[e.Sender.Group.Id].Keep100MessageIds(((SourceMessage)e.Chain[0]).Id);
            return false;
        }

        static readonly string[] messageRevokerCommandList = new string[] {"撤回", "revoke"};
        public async Task<bool> MessageRevoker(MiraiHttpSession session, IGroupMessageEventArgs e)
        {
            if (!Config.ApplyedGroups.Contains(e.Sender.Group.Id)) return false;
            var qtmsg = e.Chain.FirstOrDefault(msg => msg is QuoteMessage);
            if (qtmsg == null)
                return false;
            
            if ( !Utility.PlainMessageLinker(e.Chain).Split().Intersect(messageRevokerCommandList).Any() ||
                 !Config.CommandStart.Contains( Utility.PlainMessageLinker(e.Chain).Split().First() ) )
                return false;

            QuoteMessage quoteMessage = (QuoteMessage)qtmsg;
            if (Global.currentTask[e.Sender.Group.Id] != null)
            {
                await session.SendGroupMessageAsync(e.Sender.Group.Id, new IMessageBase[] { new AtMessage(e.Sender.Id),
                                                                                                new PlainMessage("已有命令注册，请等待被注册命令结束") });
                return false;
            }

            Global.currentTask[e.Sender.Group.Id] = new MessageRevoke(quoteMessage.SenderId, e.Sender.Group.Id, session, quoteMessage.Id);

            await Task.Delay(TimeSpan.FromMinutes(2));
            if (Global.currentTask[e.Sender.Group.Id] != null)
                await session.SendGroupMessageAsync(e.Sender.Group.Id, new PlainMessage("挂起的任务已超时"));
            Global.currentTask[e.Sender.Group.Id] = null;
            return false;
        }

        static readonly string[] aphorismCommandList = new string[] {"每日一句", "心灵鸡汤"};
        public async Task<bool> Aphorism(MiraiHttpSession session, IGroupMessageEventArgs e)
        {
            string str;
            if (!ProcessCommandSession(e, out str))
            {
                if(!aphorismCommandList.Contains(str))
                    return false;
                var rd = new Random();
                await session.SendGroupMessageAsync(e.Sender.Group.Id,
                                                    new PlainMessage(Global.aphorisms[rd.Next(Global.aphorisms.Length)]));
            }
            return false;
        }

        public async Task<bool> Disconnected(MiraiHttpSession session, Exception e)
        {
            // e.Exception: 引发掉线的响应异常, 按需处理
            MiraiHttpSessionOptions options = new MiraiHttpSessionOptions("127.0.0.1", 8080, "1c783ceb3f5344d097781ab771021efc");
            while (true)
            {
                try
                {
                    await session.ConnectAsync(options, 3197173556); // 连到成功为止, QQ号自填, 你也可以另行处理重连的 behaviour
                    return true;
                }
                catch (Exception)
                {
                    await Task.Delay(1000);
                }
            }
        }
    }
}
