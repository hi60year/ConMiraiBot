using System;
using System.Collections.Generic;
using System.Text;
using Mirai_CSharp.Models;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Net.Mail;

namespace Mirai_CSharp.Robot
{
    abstract class AbstractSuspendManagementTask
    {
        public abstract int MemberNumRequired{ get; }
        public virtual async Task Act(){ throw new NotSupportedException(); }

        protected int currentMemberNum = 0;
        
        public long ObjectId { get; protected set; }
        public long GroupId { get; protected set; }

        public int CurrentMemberNum
        {
            get => currentMemberNum;
            [Obsolete("请使用异步的IncreaseCurrentNum", true)]
            set => throw new NotSupportedException();
        }

        public async Task IncreaseCurrentNum()
        {
            if (++currentMemberNum == MemberNumRequired)
                await Act();
        }

        public AbstractSuspendManagementTask(long objectId, long gpid)
        {
            ObjectId = objectId;
            GroupId = gpid;
        }
    }

    sealed class MemberBan : AbstractSuspendManagementTask
    {
        private MiraiHttpSession session;
        public override int MemberNumRequired => 3;
        public TimeSpan BanSpan { get; private set; }


        public MemberBan(long objectId, long gpid, MiraiHttpSession session, TimeSpan banSpan):base(objectId, gpid)
        {
            BanSpan = banSpan;
            this.session = session;
            session.SendGroupMessageAsync(gpid, new IMessageBase[]{new PlainMessage("即将进行对"),
                                                                   new AtMessage(ObjectId),
                                                                   new PlainMessage($"禁言{BanSpan}的禁言操作。该操作需要3" +
                                                                   "名群友的同步确认，2分钟内发送accept进行确认")}).Wait();
        }
        public override async Task Act()
        {
            Global.currentTask[GroupId] = null;
            await session.SendGroupMessageAsync(GroupId, new PlainMessage("正在向服务器发送禁言请求，请注意，若禁言的对象是机器人本身或权" +
                "限高于等于机器人的对象，则不会生效"));
            await session.MuteAsync(ObjectId, GroupId, BanSpan);
        }
    }

    sealed class MemberKick : AbstractSuspendManagementTask
    {
        public override int MemberNumRequired => 5;
        private MiraiHttpSession session;
        public MemberKick(long objectId, long gpid, MiraiHttpSession session):base(objectId, gpid)
        {
            this.session = session;
            session.SendGroupMessageAsync(GroupId, new IMessageBase[]{ new PlainMessage("即将进行对"),
                                                                       new AtMessage(ObjectId),
                                                                       new PlainMessage("的移除操作，该操作需要5名群友进行同步，输入accept来进行确认") }).Wait();
        }
        public override async Task Act()
        {
            Global.currentTask[GroupId] = null;
            await session.SendGroupMessageAsync(GroupId, new PlainMessage("正在向服务器发送移除请求，请注意，若禁言的对象是机器人本身或权" +
                "限高于等于机器人的对象，则不会生效"));
            await session.KickMemberAsync(ObjectId, GroupId);
        }
    }

    sealed class MessageRevoke : AbstractSuspendManagementTask
    {
        private MiraiHttpSession session;
        public override int MemberNumRequired => 3;

        private int messageId;
        
        public MessageRevoke(long objectId, long gpid, MiraiHttpSession session, int messageId) : base(objectId, gpid)
        {
            this.session = session;
            this.messageId = messageId;

            session.SendGroupMessageAsync(GroupId, new IMessageBase[] {new PlainMessage("正在发起对"),
                                                                       new AtMessage(objectId),
                                                                       new PlainMessage("消息的撤回。该操作需要3名群友进行同步操作，输入accept以进行同步。") });
        }

        public override async Task Act()
        {
            Global.currentTask[GroupId] = null;
            await session.SendGroupMessageAsync(GroupId, new PlainMessage("正在向服务器发送撤回请求，请注意，如果消息" +
                "来源的发送方权限高于等于机器人，则该命令将无效"));
            await session.RevokeMessageAsync(messageId);
        }
    }
}
