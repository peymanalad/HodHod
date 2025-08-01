﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Auditing;

namespace HodHod.Auditing;

public interface IExpiredAndDeletedAuditLogBackupService
{
    bool CanBackup();

    Task Backup(List<AuditLog> auditLogs);
}