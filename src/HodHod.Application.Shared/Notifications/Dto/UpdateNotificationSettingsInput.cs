using System.Collections.Generic;

namespace HodHod.Notifications.Dto;

public class UpdateNotificationSettingsInput
{
    public bool ReceiveNotifications { get; set; }

    public List<NotificationSubscriptionDto> Notifications { get; set; }
}

