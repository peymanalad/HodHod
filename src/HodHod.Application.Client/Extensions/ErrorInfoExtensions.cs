﻿using System;
using System.Linq;
using Abp.Collections.Extensions;
using Abp.Runtime.Validation;
using Abp.Web.Models;

namespace HodHod.Extensions;

public static class ErrorInfoExtensions
{
    public static string GetConsolidatedMessage(this ErrorInfo errorInfo)
    {
        if (errorInfo == null)
        {
            return null;
        }

        string consolidatedMessage = errorInfo.Message;
        if (errorInfo.ValidationErrors != null && errorInfo.ValidationErrors.Any())
        {
            consolidatedMessage += errorInfo.ValidationErrors.Select(e => e.Message).JoinAsString(Environment.NewLine + " * ");
        }

        return consolidatedMessage;
    }
}

