using Dhole.Reports.Domain.Analytics.Entities;

namespace Dhole.Reports.UnitTests;

[TestClass]
public sealed class Phase32ContentAnalyticsTests
{
    [TestMethod]
    public void AnalyticsEvent_CapturesReportingDimensions()
    {
        var id = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var item = ContentAnalyticsEvent.Create(id, "content.analytics.page-viewed", "main", DateTime.UtcNow,
            contentId: Guid.NewGuid(), campaignId: campaignId, path: "/servicios", locale: "es-CR", utmCampaign: "navidad");
        Assert.AreEqual(id, item.Id);
        Assert.AreEqual(campaignId, item.CampaignId);
        Assert.AreEqual("/servicios", item.Path);
        Assert.AreEqual("navidad", item.UtmCampaign);
    }

    [TestMethod]
    public void AnalyticsEvent_RejectsEmptyIdentity()
    {
        Assert.ThrowsExactly<ArgumentException>(() => ContentAnalyticsEvent.Create(Guid.Empty, "content.analytics.page-viewed", "main", DateTime.UtcNow));
    }

    [TestMethod]
    public void AnalyticsEvent_TruncatesUntrustedPublicDimensions()
    {
        var item = ContentAnalyticsEvent.Create(Guid.NewGuid(), "content.analytics.interaction-clicked", "main", DateTime.UtcNow,
            targetKey: new string('x', 500));
        Assert.AreEqual(200, item.TargetKey!.Length);
    }
}
