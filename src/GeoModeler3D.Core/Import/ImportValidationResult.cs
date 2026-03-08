namespace GeoModeler3D.Core.Import;

/// <summary>Result of validating an import file.</summary>
public record ImportValidationResult(
    bool IsValid,
    string? ErrorMessage = null,
    int? EstimatedEntityCount = null,
    long? FileSizeBytes = null);
