using VehicleDeclarations.Entity;

namespace VehicleDeclarations.Service;

public interface IPdfReportService
{
    byte[] Generate(VehicleSaleDeclaration declaration);
}
