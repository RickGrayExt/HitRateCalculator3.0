namespace Contracts;
public record StartRunCommand(Guid RunId, string DatasetPath, RunParams Params);
public record SalesPatternsIdentified(Guid RunId, List<SkuDemand> Demand, RunParams Params);
public record SkuGroupsCreated(Guid RunId, List<SkuGroup> Groups, RunParams Params);
public record ShelfLocationsAssigned(Guid RunId, List<ShelfLocation> Locations, RunParams Params);
public record RackLayoutCalculated(Guid RunId, List<Rack> Racks, RunParams Params);
public record BatchesCreated(Guid RunId, List<Batch> Batches, string Mode, RunParams Params);
public record StationsAllocated(Guid RunId, List<StationAssignment> Assignments, RunParams Params);
public record HitRateCalculated(Guid RunId, HitRateResult Result);
