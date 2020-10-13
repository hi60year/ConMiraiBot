using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mirai_CSharp.Models;

namespace Mirai_CSharp.Robot
{
    static class Global
    {
        public static Dictionary<long, LinkedList<int>> messageMemory;
        public static Dictionary<long, AbstractSuspendManagementTask> currentTask;
        public static Dictionary<long, HashSet<long>> acceptList;
        public static string[] aphorisms; 
        static Global()
        {
            messageMemory = new Dictionary<long, LinkedList<int>>();
            currentTask = new Dictionary<long, AbstractSuspendManagementTask>();
            acceptList = new Dictionary<long, HashSet<long>>();
            foreach(var gpid in Config.ApplyedGroups)
            {
                messageMemory.Add(gpid, new LinkedList<int>());
                currentTask.Add(gpid, null);
                acceptList.Add(gpid, new HashSet<long>());
            }
            using var file = new StreamReader(@"..\..\Files\Aphorisms.txt");
            aphorisms = file.ReadToEnd().Split("-----", StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
