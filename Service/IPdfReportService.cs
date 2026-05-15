using Orders.Entity;

namespace Orders.Service;

public interface IPdfReportService
{
    byte[] Generate(Order order);
}
