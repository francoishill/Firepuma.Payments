

// ReSharper disable ReplaceSubstringWithRangeIndexer

namespace Firepuma.Payments.Infrastructure.PipelineBehaviors.Helpers;

public static class BehaviorPipelineHelpers
{
    public static string GetShortTypeName(Type type)
    {
        var fullName = type.FullName;
        var lastDotIndex = fullName?.LastIndexOf(".");

        return lastDotIndex >= 0
            ? fullName.Substring(lastDotIndex.Value + 1)
            : fullName;
    }
}