using System;
using System.Text;
using System.Collections.Generic;

namespace pryLogger.src.ErrorNotifier
{
    /// <summary>
    /// Represents an interface for notifying and handling errors.
    /// </summary>
    public interface IErrorNotifier
    {
        /// <summary>
        /// Gets or sets the timestamp of the last notification sent.
        /// </summary>
        DateTimeOffset LastNotificationSent { get; set; }

        /// <summary>
        /// Notifies the error using the provided ErrorNotification.
        /// </summary>
        /// <param name="error">The ErrorNotification to send.</param>
        void Notify(ErrorNotification error);

        /// <summary>
        /// Sets an attachment file for the error notification.
        /// </summary>
        /// <param name="fileName">The file name to attach.</param>
        /// <returns>The current instance of IErrorNotifier.</returns>
        IErrorNotifier SetAttachMent(string fileName);
    }
}
