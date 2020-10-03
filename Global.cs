using System;
using System.Collections.Generic;
using System.Text;
using Mirai_CSharp.Models;

namespace Mirai_CSharp.Robot
{
    static class Global
    {
        public static Dictionary<long, Queue<long>> messageMemory;
        public static Dictionary<long, AbstractSuspendManagementTask> currentTask;
        static Global()
        {
            messageMemory = new Dictionary<long, Queue<long>>();
            foreach(var gpid in Config.ApplyedGroups)
            {
                messageMemory.Add(gpid, new Queue<long>());
                currentTask.Add(gpid, null);
            }
        }
    }
}
