using Supplier.Application.DTOs;
using Supplier.Application.Interfaces;

namespace Supplier.Service.Endpoints;

public static class SupplierEndpoints
{
    public static WebApplication MapSupplierEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/supplier")
            .WithTags("Supplier");

        group.MapPost("/dispatch", async (DispatchTransportRequest request, ISupplierService svc, CancellationToken ct) =>
        {
            await svc.DispatchTransportAsync(request, ct);
            return Results.Accepted();
        })
        .WithName("DispatchTransport");

        return app;
    }
}
