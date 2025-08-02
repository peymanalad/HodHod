using System;

namespace HodHod.Reports;

public enum ReportActionType
{
    ReportStatusChange = 0,
    AddNote = 1,
    EditNote = 2,
    DeleteNote = 3,
    AddComment = 4,
    EditComment = 5,
    DeleteComment = 6,
    AddToArchive = 7,
    RestoreReport = 8,
    BlockSenderPhoneNumber = 9,
    ChangeIsStarredByCurrentUser = 10,
    ReportReferral = 11,
    ChangeReportCategory = 12
}