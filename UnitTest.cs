using System;
using System.Collections.Generic;
using System.Text;
using Mirai_CSharp.Models;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Mirai_CSharp.Robot
{
    public static class UnitTest
    {
        private static List<int> ids = new List<int>();

        public static async Task Run(MiraiHttpSession session, long groupNum)
        {
            ids.Add(await session.SendGroupMessageAsync(groupNum, new PlainMessage("Unit Test successfully launched.")));
            ids.Add(await session.SendGroupMessageAsync(groupNum, new PlainMessage("1. PlainTextTest")));
            var sw = new Stopwatch();
            for (int i = 0; i < 3; i++)
            {
                //start timing here
                sw.Start();
                ids.Add(await session.SendGroupMessageAsync(groupNum, new PlainMessage("hello world")));
                //end timing here
                sw.Stop();
                ids.Add(await session.SendGroupMessageAsync(groupNum, new PlainMessage($"Success. time consumed : {sw.ElapsedMilliseconds}ms")));
                //reset timing here
                sw.Reset();
            }

            ids.Add(await session.SendGroupMessageAsync(groupNum, new PlainMessage("2. ImageTest")));
            sw.Start();
            //send a image
            ids.Add(await session.SendGroupMessageAsync(groupNum, new ImageMessage(null,
                url: "https://ss0.bdstatic.com/70cFvHSh_Q1YnxGkpoWK1HF6hhy/it/u=3581134888,2217954696&fm=26&gp=0.jpg", null)));
            sw.Stop();
            ids.Add(await session.SendGroupMessageAsync(groupNum, new PlainMessage($"Success. time consumed : {sw.ElapsedMilliseconds}ms")));
            sw.Reset();

            ids.Add(await session.SendGroupMessageAsync(groupNum, new PlainMessage("3. MessageChainTest")));
            sw.Start();
            ids.Add(
              await session.SendGroupMessageAsync(groupNum, new IMessageBase[]{ 
                    new PlainMessage("hello world"),
                    new ImageMessage(null,
                                     url: "https://ss0.bdstatic.com/70cFvHSh_Q1YnxGkpoWK1HF6hhy/it/u=3581134888,2217954696&fm=26&gp=0.jpg",
                                     null)
                  })//SendGpMsgAsync, array
            );//ids.Add
            sw.Stop();
            ids.Add(await session.SendGroupMessageAsync(groupNum, new PlainMessage($"Success. time consumed : {sw.ElapsedMilliseconds}ms")));
            sw.Reset();

            ids.Add(await session.SendGroupMessageAsync(groupNum, new PlainMessage("4. RevokeTest")));
            ids.Add(await session.SendGroupMessageAsync(groupNum, new PlainMessage("all test messages will be revoked in 1 minutes.")));
            Thread.Sleep(TimeSpan.FromMinutes(1));
            sw.Start();
            foreach (var id in ids)
            {
                await session.RevokeMessageAsync(id);
            }
            sw.Stop();
            ids.Add(await session.SendGroupMessageAsync(groupNum, new PlainMessage($"Success. time consumed : {sw.ElapsedMilliseconds}ms")));
            sw.Reset();

            ids.Add(await session.SendGroupMessageAsync(groupNum, new PlainMessage("Unit test done.")));
        }
    }
}
