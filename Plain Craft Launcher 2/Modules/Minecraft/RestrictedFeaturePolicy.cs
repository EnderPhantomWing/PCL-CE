using PCL.Core.App.Region;
using PCL.Core.App.Localization;
using PCL.Core.UI;

namespace PCL;

internal static class RestrictedFeaturePolicy
{
    internal static RegionAssessment Assess() => RegionPolicy.Evaluate(RegionPolicy.ReadWindowsSignals());

    internal static bool IsRestricted => Assess().IsRestricted;

    internal static bool EnableRegionRestriction => ReadChinaFlag();

    private static bool ReadChinaFlag()
    {
        var raw = Environment.GetEnvironmentVariable("CHINA")?.Trim();
        // 只有显式 false / 0 才放行，其余一律启用限制
        return !(raw == "0" || (bool.TryParse(raw, out var b) && !b));
    }
    
    internal static bool Check(string feature, bool showHint = true)
    {
        if (!EnableRegionRestriction) return true;
        
        var assessment = Assess();
        if (!assessment.IsRestricted) return true;

        ModBase.Log($"[RegionPolicy] 已阻止功能 {feature}，判定原因：{assessment.Reason}");
        if (showHint)
        {
            HintService.Hint(Lang.Text(assessment.Classification == RegionClassification.Unknown
                ? "Region.Feature.Unknown"
                : "Region.Feature.Restricted"), HintType.Error);
        }
        return false;
    }

    internal static void ThrowIfRestricted(string feature)
    {
        if (Check(feature, false)) return;
        throw new ModBase.CancelledException();
    }
}
