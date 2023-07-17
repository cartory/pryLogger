using System;
using System.Text;
using System.Collections.Generic;

namespace pryLogger.src.ErrorNotifier
{
    public interface IErrorNotifier
    {
        DateTimeOffset LastNotificationSent { get; set; }

        void Notify(ErrorNotification error);
        IErrorNotifier SetAttachMent(string fileName);
    }
}
