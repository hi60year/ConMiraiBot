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
        public static Dictionary<long, HashSet<long>> acceptList; 
        static Global()
        {
            messageMemory = new Dictionary<long, Queue<long>>();
            currentTask = new Dictionary<long, AbstractSuspendManagementTask>();
            acceptList = new Dictionary<long, HashSet<long>>();
            foreach(var gpid in Config.ApplyedGroups)
            {
                messageMemory.Add(gpid, new Queue<long>());
                currentTask.Add(gpid, null);
                acceptList.Add(gpid, new HashSet<long>());
            }
        }
    }
}
