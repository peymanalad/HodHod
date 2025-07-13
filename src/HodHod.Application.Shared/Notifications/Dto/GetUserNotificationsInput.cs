using System;
using Abp.Notifications;
using HodHod.Dto;

namespace HodHod.Notifications.Dto;

public class GetUserNotificationsInput : PagedInputDto
{
    public UserNotificationState? State { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}

