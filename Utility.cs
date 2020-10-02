#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Mirai_CSharp.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Reflection.Metadata.Ecma335;

namespace Mirai_CSharp.Robot
{
    static class Utility
    {
        public static bool SiglePlainMessageChecker(IGroupMessageEventArgs args)
            => args.Chain.Length == 1 && args.Chain[0] is PlainMessage;

        /// <summary>
        /// 处理一个命令。判断一个字符串是否是命令并加以处理
        /// </summary>
        /// <param name="msg">要处理的字符串</param>
        /// <param name="processedMsg">
        /// 丢弃命令头，如果不是命令，则该参数和msg保持一致
        /// </param>
        /// <returns>返回这个字符串是否是命令，如果是，返回true，否则返回false</returns>
        public static bool ProcessCommandString(string msg, out string processedMsg)
        {
            processedMsg = msg;
            var indexOfFirstSpace = msg.IndexOf(' ');
            if (indexOfFirstSpace == -1 || Config.CommandStart.Contains(msg.Substring(0, indexOfFirstSpace)))
                return false;
            else
            {
                processedMsg = msg.Substring(indexOfFirstSpace + 1);
                return true;
            }
        }

        /// <summary>
        /// 处理一个命令会话，判断一个会话是否是命令并加以处理
        /// </summary>
        /// <param name="args"></param>
        /// <param name="processedMsg">丢弃命令头，如果不是命令，则为null</param>
        /// <returns>返回这个会话是否是命令，如果是，返回true，否则返回false</returns>
        public static bool ProcessCommandSession(IGroupMessageEventArgs args, out string? processedMsg)
        {
            processedMsg = null;
            if (!SiglePlainMessageChecker(args))
                return false;
            var ret = ProcessCommandString((args.Chain[0] as PlainMessage).Message, out processedMsg);
            return ret;
        }

        /// <summary>
        /// 带长度检查的群消息发送函数，当消息超过500字符或三个图片时，将进行省略
        /// </summary>
        /// <param name="session">会话</param>
        /// <param name="chain">消息链</param>
        /// <returns></returns>
        public static async Task<int> LengthSensitiveSengGroupMessageAsync(this MiraiHttpSession session, long groupId, IMessageBase[] chain)
        {
            const int MaxLength = 500;
            const int MaxImage = 3;
            int imageNum = 0;
            int textLength = 0;
            var newChain = new List<IMessageBase>();

            int allTextLength = chain.Select(x => x as PlainMessage)
                                     .Where(x => x != null)
                                     .Select(x => x.Message.Length)
                                     .Sum();

            int allImageNum = chain.Where(x => x is ImageMessage).Count();

            bool textOverflow = false;
            bool imageOverflow = false;

            foreach (var msg in chain)
            {
                switch (msg)
                {
                    case PlainMessage plmsg:
                        int remain = MaxLength - textLength;
                        if (plmsg.Message.Length <= remain)
                        {
                            newChain.Add(plmsg);
                        }
                        else
                        {
                            newChain.Add(new PlainMessage(plmsg.Message.Substring(0, remain)));
                            textOverflow = true;
                            goto p;
                        }
                        break;
                    case ImageMessage immsg:
                        if (imageNum++ <= MaxImage)
                        {
                            newChain.Add(immsg);
                        }
                        else
                        {
                            imageOverflow = true;
                            goto p;
                        }
                        break;
                }
            }
p:
            if (textOverflow)
                newChain.Add(new PlainMessage($"\r\n默认情况下只输出500个字符，而本" + 
                    $"消息共{allTextLength}个字符，多余的部分被截断，用--showall或-sa参数展开所有的内容。警告：恶意刷屏可能导致您被管理员禁言"));
            if (imageOverflow)
                newChain.Add(new PlainMessage($"\r\n默认情况下只能输出三个图片，而本消息共{allImageNum}张图片，多余" +
                    $"的部分被截断，用--showall或-sa参数展开所有的内容。警告：恶意刷屏可能导致您被管理员禁言"));

            return await session.SendGroupMessageAsync(groupId, chain);
        }

        /// <summary>
        /// 解析shell-like命令的命令参数
        /// </summary>
        /// <param name="str">要解析的命令参数</param>
        /// <returns></returns>
        public static Dictionary<string, string?> ShellLikeArgumentParser(string str)
        {
            // TODO : 选用合适的正则表达式来解决这项工作
            throw new NotImplementedException();
        }
    }
}
