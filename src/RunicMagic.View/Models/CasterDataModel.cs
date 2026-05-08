namespace RunicMagic.View.Models;

public record CasterDataModel(
    long? CurrentHitPoints,
    long? MaxHitPoints,
    long CurrentIntegrity,
    long MaxIntegrity,
    long? CurrentPower,
    long? MaxPower);
