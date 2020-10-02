#nullable enable
using Mirai_CSharp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.Console;

namespace Mirai_CSharp.Robot
{
    public static class Program
    {
        public static async Task Main()
        {
            
            MiraiHttpSessionOptions options = new MiraiHttpSessionOptions("127.0.0.1", 8080, "1c783ceb3f5344d097781ab771021efc");
            await using MiraiHttpSession session = new MiraiHttpSession();
            var plugin = new Plugins();
            session.GroupMessageEvt += plugin.ImageMonitor;
            await session.ConnectAsync(options, 3197173556);
            // await UnitTest.Run(session, 947904856);
            while (true)
            {
                if (await Console.In.ReadLineAsync() == "exit")
                {
                    return;
                }
            }
        }
    }
}