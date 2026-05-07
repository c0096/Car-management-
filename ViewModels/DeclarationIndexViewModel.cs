using VehicleDeclarations.Entity;

namespace VehicleDeclarations.ViewModels;

public sealed class DeclarationIndexViewModel
{
    public PagedResult<VehicleSaleDeclaration> Results { get; init; } = new();

    public SearchOptions Options { get; init; } = new();

    public string NextDirectionFor(string sort)
    {
        if (!string.Equals(Options.Sort, sort, StringComparison.OrdinalIgnoreCase))
        {
            return "asc";
        }

        return string.Equals(Options.Direction, "asc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";
    }
}
