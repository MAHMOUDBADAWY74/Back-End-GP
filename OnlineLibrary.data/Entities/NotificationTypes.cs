using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Data.Entities
{
    public static class NotificationTypes
    {
        public const string PostLike = "PostLike";
        public const string PostUnlike = "PostUnlike";
        public const string PostComment = "PostComment";
        public const string PostShare = "PostShare";
        public const string BookAdded = "BookAdded";
        public const string MessageReceived = "MessageReceived";
        public const string PostAccepted = "PostAccepted";
        public const string PostRejected = "PostRejected";
    }
}
