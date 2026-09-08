using Microsoft.Extensions.Time.Testing;
using Notificaciones.Api.Telemetry;
using Notificaciones.Domain.Events;

namespace Notificaciones.UnitTests;

/// <summary>
/// The telemetry store is the read model behind the live console and the metrics endpoint.
/// It must stay bounded under load and report honest aggregates, so both properties are pinned:
/// the arithmetic of the counters and the hard ceilings of every buffer.
/// </summary>
public class InMemoryNotificationTelemetryStoreTests
{
    private static readonly DateTimeOffset Origin = new(2026, 3, 1, 10, 0, 0, TimeSpan.Zero);

    private static (InMemoryNotificationTelemetryStore Store, FakeTimeProvider Clock) CreateStore()
    {
        var clock = new FakeTimeProvider(Origin);
        return (new InMemoryNotificationTelemetryStore(clock), clock);
    }

    private static NotificationLifecycleEvent Stage(
        NotificationStage stage,
        Guid? id = null,
        double elapsed = 10,
        int payloadBytes = 0,
        string channel = "",
        NotificationType type = NotificationType.WelcomeMessage) => new()
        {
            NotificationId = id ?? Guid.NewGuid(),
            Stage = stage,
            Type = type,
            Priority = "High",
            Recipient = "de******@naindev.com",
            Subject = "Prueba",
            Source = "worker",
            ElapsedMilliseconds = elapsed,
            PayloadBytes = payloadBytes,
            Attempt = 1,
            Channel = channel,
            Timestamp = Origin
        };

    [Fact]
    public void GetSnapshot_ReportsFullSuccessWhenNothingHasBeenProcessed()
    {
        var (store, _) = CreateStore();

        var snapshot = store.GetSnapshot();

        Assert.Equal(0, snapshot.Accepted);
        Assert.Equal(0, snapshot.Dispatched);
        Assert.Equal(100d, snapshot.SuccessRate);
        Assert.Equal(0d, snapshot.P95LatencyMs);
        Assert.Null(snapshot.LastEventAt);
    }

    [Fact]
    public void Record_CountsAcceptedAndDispatchedIndependently()
    {
        var (store, _) = CreateStore();
        var id = Guid.NewGuid();

        store.Record(Stage(NotificationStage.Accepted, id));
        store.Record(Stage(NotificationStage.Dispatched, id, channel: "simulation"));

        var snapshot = store.GetSnapshot();

        Assert.Equal(1, snapshot.Accepted);
        Assert.Equal(1, snapshot.Dispatched);
        Assert.Equal(0, snapshot.InFlight);
        Assert.Equal(1, snapshot.ByChannel["simulation"]);
    }

    [Fact]
    public void Record_TracksInFlightUntilATerminalStageArrives()
    {
        var (store, _) = CreateStore();
        var id = Guid.NewGuid();

        store.Record(Stage(NotificationStage.Accepted, id));
        Assert.Equal(1, store.GetSnapshot().InFlight);

        store.Record(Stage(NotificationStage.Received, id));
        Assert.Equal(1, store.GetSnapshot().InFlight);

        store.Record(Stage(NotificationStage.Failed, id));
        Assert.Equal(0, store.GetSnapshot().InFlight);
    }

    [Fact]
    public void GetSnapshot_ComputesSuccessRateOverProcessedNotificationsOnly()
    {
        var (store, _) = CreateStore();

        for (var i = 0; i < 3; i++)
        {
            store.Record(Stage(NotificationStage.Dispatched, elapsed: 100));
        }
        store.Record(Stage(NotificationStage.Failed));

        var snapshot = store.GetSnapshot();

        Assert.Equal(3, snapshot.Dispatched);
        Assert.Equal(1, snapshot.Failed);
        Assert.Equal(75d, snapshot.SuccessRate);
    }

    [Fact]
    public void GetSnapshot_ComputesLatencyAggregatesFromDispatchedStagesOnly()
    {
        var (store, _) = CreateStore();

        foreach (var latency in new double[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 })
        {
            store.Record(Stage(NotificationStage.Dispatched, elapsed: latency));
        }

        // A Received stage must not pollute the latency window.
        store.Record(Stage(NotificationStage.Received, elapsed: 9999));

        var snapshot = store.GetSnapshot();

        Assert.Equal(55d, snapshot.AverageLatencyMs);
        Assert.Equal(100d, snapshot.P95LatencyMs);
        Assert.Equal(10d, snapshot.FastestLatencyMs);
    }

    [Fact]
    public void Record_AccumulatesRenderedBytesFromTheRenderStage()
    {
        var (store, _) = CreateStore();

        store.Record(Stage(NotificationStage.Rendered, payloadBytes: 4000));
        store.Record(Stage(NotificationStage.Rendered, payloadBytes: 6000));

        Assert.Equal(10_000, store.GetSnapshot().RenderedBytes);
    }

    [Fact]
    public void Record_GroupsAcceptedNotificationsByType()
    {
        var (store, _) = CreateStore();

        store.Record(Stage(NotificationStage.Accepted, type: NotificationType.ContactFormSubmission));
        store.Record(Stage(NotificationStage.Accepted, type: NotificationType.ContactFormSubmission));
        store.Record(Stage(NotificationStage.Accepted, type: NotificationType.SystemAlert));

        var byType = store.GetSnapshot().ByType;

        Assert.Equal(2, byType["ContactFormSubmission"]);
        Assert.Equal(1, byType["SystemAlert"]);
    }

    [Fact]
    public void GetRecentEvents_ReturnsNewestFirstAndHonoursTheLimit()
    {
        var (store, _) = CreateStore();

        for (var i = 0; i < 10; i++)
        {
            store.Record(Stage(NotificationStage.Accepted) with { Subject = $"evento-{i}" });
        }

        var recent = store.GetRecentEvents(3);

        Assert.Equal(3, recent.Count);
        Assert.Equal("evento-9", recent[0].Subject);
        Assert.Equal("evento-7", recent[2].Subject);
    }

    [Fact]
    public void GetRecentEvents_ReturnsEmptyForNonPositiveLimits()
    {
        var (store, _) = CreateStore();
        store.Record(Stage(NotificationStage.Accepted));

        Assert.Empty(store.GetRecentEvents(0));
        Assert.Empty(store.GetRecentEvents(-5));
    }

    [Fact]
    public void Record_KeepsTheRecentBufferBoundedUnderSustainedLoad()
    {
        var (store, _) = CreateStore();

        for (var i = 0; i < 2_000; i++)
        {
            store.Record(Stage(NotificationStage.Accepted));
        }

        // The ceiling is 240; asking for more must not return more.
        Assert.Equal(240, store.GetRecentEvents(5_000).Count);
    }

    [Fact]
    public void ThroughputSeries_AlwaysExposesASixtySecondWindow()
    {
        var (store, _) = CreateStore();

        Assert.Equal(60, store.GetSnapshot().ThroughputSeries.Count);
    }

    [Fact]
    public void ThroughputSeries_PlacesDispatchesInTheMostRecentBucket()
    {
        var (store, _) = CreateStore();

        store.Record(Stage(NotificationStage.Dispatched));
        store.Record(Stage(NotificationStage.Dispatched));

        var series = store.GetSnapshot().ThroughputSeries;

        Assert.Equal(2, series[^1]);
        Assert.Equal(0, series[0]);
    }

    [Fact]
    public void ThroughputSeries_DropsSamplesOnceTheyLeaveTheWindow()
    {
        var (store, clock) = CreateStore();

        store.Record(Stage(NotificationStage.Dispatched));
        Assert.Equal(1, store.GetSnapshot().ThroughputSeries[^1]);

        clock.Advance(TimeSpan.FromSeconds(61));

        Assert.All(store.GetSnapshot().ThroughputSeries, sample => Assert.Equal(0, sample));
    }

    [Fact]
    public void GetSnapshot_ReportsUptimeFromTheInjectedClock()
    {
        var (store, clock) = CreateStore();

        clock.Advance(TimeSpan.FromMinutes(5));

        Assert.Equal(300, store.GetSnapshot().UptimeSeconds);
    }

    [Fact]
    public void Record_RejectsNull()
    {
        var (store, _) = CreateStore();

        Assert.Throws<ArgumentNullException>(() => store.Record(null!));
    }

    [Fact]
    public void Record_IsSafeUnderConcurrentWriters()
    {
        var (store, _) = CreateStore();

        Parallel.For(0, 500, _ =>
        {
            store.Record(Stage(NotificationStage.Accepted));
            store.Record(Stage(NotificationStage.Dispatched, elapsed: 25));
        });

        var snapshot = store.GetSnapshot();

        Assert.Equal(500, snapshot.Accepted);
        Assert.Equal(500, snapshot.Dispatched);
    }
}
