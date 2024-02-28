﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Experimental.BuildCop;

/// <summary>
/// Representation of a single report of a single finding from a BuildAnalyzer
/// Each rule has upfront known message format - so only the concrete arguments are added
/// Optionally a location is attached - in the near future we might need to support multiple locations
///  (for 2 cases - a) grouped result for multiple occurrences; b) a single report for a finding resulting from combination of multiple locations)
/// </summary>
public sealed class BuildCopResult : IBuildCopResult
{
    public static BuildCopResult Create(BuildAnalyzerRule rule, ElementLocation location, params string[] messageArgs)
    {
        return new BuildCopResult(rule, location, messageArgs);
    }

    public BuildCopResult(BuildAnalyzerRule buildAnalyzerRule, ElementLocation location, string[] messageArgs)
    {
        BuildAnalyzerRule = buildAnalyzerRule;
        Location = location;
        MessageArgs = messageArgs;
    }

    internal BuildEventArgs ToEventArgs(BuildAnalyzerResultSeverity severity)
        => severity switch
        {
            BuildAnalyzerResultSeverity.Info => new BuildCopResultMessage(this),
            BuildAnalyzerResultSeverity.Warning => new BuildCopResultWarning(this),
            BuildAnalyzerResultSeverity.Error => new BuildCopResultError(this),
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null),
        };

    public BuildAnalyzerRule BuildAnalyzerRule { get; }
    public ElementLocation Location { get; }

    public string LocationString => Location.LocationString;

    public string[] MessageArgs { get; }
    public string MessageFormat => BuildAnalyzerRule.MessageFormat;

    public string FormatMessage() =>
        _message ??= $"{(Equals(Location ?? ElementLocation.EmptyLocation, ElementLocation.EmptyLocation) ? string.Empty : (Location!.LocationString + ": "))}{BuildAnalyzerRule.Id}: {string.Format(BuildAnalyzerRule.MessageFormat, MessageArgs)}";

    private string? _message;
}
