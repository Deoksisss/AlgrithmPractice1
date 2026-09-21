using System;
using System.Globalization;
using AlgorithmPractice1.Core.Models;
using Avalonia.Data.Converters;

namespace AlgorithmPractice1.GUI.ViewModels;

public static class ProgressConverters
{
    public static readonly IValueConverter StatusToString =
        new FuncValueConverter<AlgorithmExecutionStatus, string>(s => s.ToDisplayString());

    public static readonly IValueConverter StatusToIcon =
        new FuncValueConverter<AlgorithmExecutionStatus, string>(s => s.ToIcon());
}
