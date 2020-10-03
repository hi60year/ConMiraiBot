using System;
using System.Collections.Generic;
using System.Text;
using Mirai_CSharp.Models;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

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
        public MiraiHttpSession session;
        public override int MemberNumRequired { get; } = 3;
        public TimeSpan BanSpan { get; private set; }


        public MemberBan(long objectId, long gpid, MiraiHttpSession session, TimeSpan banSpan):base(objectId, gpid)
        {
            BanSpan = banSpan;
            this.session = session;
            BanSpan = banSpan;
            session.SendGroupMessageAsync(gpid, new IMessageBase[]{new PlainMessage("即将进行对"),
                                                                   new AtMessage(ObjectId),
                                                                   new PlainMessage($"禁言{BanSpan}的禁言操作。该操作需要3" +
                                                                   "名群友的同步确认，2分钟内发送accept进行确认")}).Wait();
        }
        public override async Task Act()
        {
            await session.MuteAsync(ObjectId, GroupId, BanSpan);
        }
    }
}
