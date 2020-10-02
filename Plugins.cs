using System;
using System.Collections.Generic;
using System.Text;
using Mirai_CSharp.Models;
using System.Threading.Tasks;

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
            if(! Utility.CommandChecker("help"))
            return false;
        }
    }
}
