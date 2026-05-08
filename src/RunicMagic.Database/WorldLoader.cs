using Dapper;
using Microsoft.Data.SqlClient;

namespace RunicMagic.Database;

public class WorldLoader
{
    private readonly string connectionString;

    public WorldLoader(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public async Task<IEnumerable<EntityData>> LoadAsync()
    {
        await using var conn = new SqlConnection(connectionString);

        var entityRows = (await conn.QueryAsync<EntityRow>(
            "select Id, EntityTypeId, Label, X, Y, Width, Height, HasAgency, Weight, Strength, IsTranslucent, Angle, MaxStructuralIntegrity, CurrentStructuralIntegrity, DragCoefficient, AngularDragCoefficient, GroundFrictionCoefficient from Entities")).AsList();

        var lifeRows = (await conn.QueryAsync<LifeRow>(
            "select EntityId, MaxHitPoints, CurrentHitPoints from EntityLife"))
            .ToDictionary(r => r.EntityId);

        var chargeRows = (await conn.QueryAsync<ChargeRow>(
            "select EntityId, MaxCharge, CurrentCharge from EntityCharge"))
            .ToDictionary(r => r.EntityId);

        var locomotionRows = (await conn.QueryAsync<LocomotionRow>(
            "select EntityId, LocomotionEfficiency from EntityLocomotion"))
            .ToDictionary(r => r.EntityId);

        var inscriptionGroups = (await conn.QueryAsync<InscriptionRow>(
            "select EntityId, SpellText from Inscription"))
            .GroupBy(r => r.EntityId)
            .ToDictionary(g => g.Key, g => g.Select(r => r.SpellText).ToArray());

        var patrolBehaviorRows = (await conn.QueryAsync<PatrolBehaviorRow>(
            "select Id, EntityId, Speed from PatrolBehaviors"))
            .AsList();

        var patrolWaypointsByBehavior = (await conn.QueryAsync<PatrolWaypointRow>(
            "select PatrolBehaviorId, Sequence, X, Y, WaitTicks from PatrolWaypoints"))
            .GroupBy(r => r.PatrolBehaviorId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(r => r.Sequence)
                      .Select(r => new PatrolWaypointData(r.Sequence, r.X, r.Y, r.WaitTicks))
                      .ToArray());

        var patrolBehaviorsByEntity = patrolBehaviorRows
            .GroupBy(r => r.EntityId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(r =>
                {
                    patrolWaypointsByBehavior.TryGetValue(r.Id, out var waypoints);
                    return new PatrolBehaviorData(r.Id, r.Speed, waypoints ?? []);
                }).ToArray());

        return entityRows.Select(row =>
        {
            lifeRows.TryGetValue(row.Id, out var life);
            chargeRows.TryGetValue(row.Id, out var charge);
            locomotionRows.TryGetValue(row.Id, out var locomotion);
            inscriptionGroups.TryGetValue(row.Id, out var inscriptions);
            patrolBehaviorsByEntity.TryGetValue(row.Id, out var patrolBehaviors);

            var aiData = patrolBehaviors != null ? new AIData(row.Id, PatrolBehaviors: patrolBehaviors) : null;

            return new EntityData(
                Id: row.Id,
                TypeId: row.EntityTypeId,
                Label: row.Label,
                X: row.X,
                Y: row.Y,
                Width: row.Width,
                Height: row.Height,
                HasAgency: row.HasAgency,
                Weight: row.Weight,
                Strength: row.Strength,
                IsTranslucent: row.IsTranslucent,
                Angle: row.Angle,
                MaxHitPoints: life?.MaxHitPoints,
                CurrentHitPoints: life?.CurrentHitPoints,
                MaxCharge: charge?.MaxCharge,
                CurrentCharge: charge?.CurrentCharge,
                InscriptionTexts: inscriptions,
                MaxStructuralIntegrity: row.MaxStructuralIntegrity,
                CurrentStructuralIntegrity: row.CurrentStructuralIntegrity,
                DragCoefficient: row.DragCoefficient,
                AngularDragCoefficient: row.AngularDragCoefficient,
                GroundFrictionCoefficient: row.GroundFrictionCoefficient,
                LocomotionEfficiency: locomotion?.LocomotionEfficiency,
                AIData: aiData);
        });
    }

    private record EntityRow(Guid Id, long EntityTypeId, string Label, long X, long Y, long Width, long Height, bool HasAgency, long Weight, long Strength, bool IsTranslucent, double Angle, long MaxStructuralIntegrity, long CurrentStructuralIntegrity, double DragCoefficient, double AngularDragCoefficient, double GroundFrictionCoefficient);
    private record LifeRow(Guid EntityId, long MaxHitPoints, long CurrentHitPoints);
    private record ChargeRow(Guid EntityId, long MaxCharge, long CurrentCharge);
    private record LocomotionRow(Guid EntityId, double LocomotionEfficiency);
    private record InscriptionRow(Guid EntityId, string SpellText);
    private record PatrolBehaviorRow(long Id, Guid EntityId, double Speed);
    private record PatrolWaypointRow(long PatrolBehaviorId, int Sequence, long X, long Y, long WaitTicks);
}
