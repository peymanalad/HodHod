﻿using System;
using System.Linq;
using GraphQL;
using GraphQL.Execution;
using GraphQLParser.AST;

namespace HodHod.Core.Extensions;

public static class ContextExtensions
{
    public static IResolveFieldContext ContainsArgument<TArgType>(this IResolveFieldContext context,
        string argumentName,
        Action<TArgType> argumentContainsAction)
    {
        if (context.Arguments.ContainsKey(argumentName))
        {
            var argument = context.Arguments[argumentName];
            if (argument.Source == ArgumentSource.FieldDefault)
            {
                return context;
            }

            argumentContainsAction(context.GetArgument<TArgType>(argumentName));
        }

        return context;
    }

    /// <summary>
    /// Returns true if the given fieldSelector exists in the selection of the query.
    /// </summary>
    /// <param name="context">The working context</param>
    /// <param name="fieldSelector">The query of the field selector. For example items:organizationUnits:displayName</param>
    /// <param name="namespaceSeperator">The seperator character of the fieldSelector. Default is :</param>
    /// <returns></returns>
    public static bool HasSelectionField(this IResolveFieldContext context, string fieldSelector, char namespaceSeperator = ':')
    {
        if (string.IsNullOrWhiteSpace(fieldSelector))
        {
            return false;
        }

        if (context.SubFields == null)
        {
            return false;
        }

        var fragments = fieldSelector.Split(new[] { namespaceSeperator }, StringSplitOptions.RemoveEmptyEntries);

        if (fragments.Length == 1)
        {
            return context.SubFields.ContainsKey(fragments[0]);
        }

        if (context.SubFields[fragments[0]].Field == null)
        {
            return false;
        }

        if (context.SubFields[fragments[0]].Field?.SelectionSet == null)
        {
            return false;
        }

        if (context.SubFields[fragments[0]].Field?.SelectionSet.Selections == null)
        {
            return false;
        }


        var selections = context.SubFields[fragments[0]].Field?.SelectionSet.Selections;

        for (var i = 1; i < fragments.Length; i++)
        {
            if (selections == null)
            {
                return false;
            }

            var field = selections.Select(selection => (GraphQLField)selection).FirstOrDefault(f => f.Name == fragments[i]);
            if (field == null)
            {
                return false;
            }

            if (i == fragments.Length - 1)
            {
                return true;
            }

            selections = field.SelectionSet?.Selections;
        }

        return true;
    }
}

