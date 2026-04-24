using Dapper;
using Microsoft.Data.SqlClient;

namespace RunicMagic.Database;

public class WorldLoader(string connectionString)
{
    public async Task<IEnumerable<EntityData>> LoadAsync()
    {
        await using var conn = new SqlConnection(connectionString);

        var entityRows = (await conn.QueryAsync<EntityRow>(
            "select Id, EntityTypeId, Label, X, Y, Width, Height, HasAgency, Weight, IsTranslucent, Angle, MaxStructuralIntegrity, CurrentStructuralIntegrity from Entities")).AsList();

        var lifeRows = (await conn.QueryAsync<LifeRow>(
            "select EntityId, MaxHitPoints, CurrentHitPoints from EntityLife"))
            .ToDictionary(r => r.EntityId);

        var chargeRows = (await conn.QueryAsync<ChargeRow>(
            "select EntityId, MaxCharge, CurrentCharge from EntityCharge"))
            .ToDictionary(r => r.EntityId);

        var inscriptionGroups = (await conn.QueryAsync<InscriptionRow>(
            "select EntityId, SpellText from Inscription"))
            .GroupBy(r => r.EntityId)
            .ToDictionary(g => g.Key, g => g.Select(r => r.SpellText).ToArray());

        return entityRows.Select(row =>
        {
            lifeRows.TryGetValue(row.Id, out var life);
            chargeRows.TryGetValue(row.Id, out var charge);
            inscriptionGroups.TryGetValue(row.Id, out var inscriptions);

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
                IsTranslucent: row.IsTranslucent,
                Angle: row.Angle,
                MaxHitPoints: life?.MaxHitPoints,
                CurrentHitPoints: life?.CurrentHitPoints,
                MaxCharge: charge?.MaxCharge,
                CurrentCharge: charge?.CurrentCharge,
                InscriptionTexts: inscriptions,
                MaxStructuralIntegrity: row.MaxStructuralIntegrity,
                CurrentStructuralIntegrity: row.CurrentStructuralIntegrity);
        });
    }

    private record EntityRow(Guid Id, long EntityTypeId, string Label, long X, long Y, long Width, long Height, bool HasAgency, long Weight, bool IsTranslucent, double Angle, long MaxStructuralIntegrity, long CurrentStructuralIntegrity);
    private record LifeRow(Guid EntityId, long MaxHitPoints, long CurrentHitPoints);
    private record ChargeRow(Guid EntityId, long MaxCharge, long CurrentCharge);
    private record InscriptionRow(Guid EntityId, string SpellText);
}
