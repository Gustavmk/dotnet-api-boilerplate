using Ardalis.Result;
using Boilerplate.Application.Common;
using Boilerplate.Application.Extensions;
using MediatR;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Boilerplate.Application.Features.Heroes.CreateHero;

public class CreateHeroHandler : IRequestHandler<CreateHeroRequest, Result<GetHeroResponse>>
{
    private readonly IContext _context;


    public CreateHeroHandler(IContext context)
    {
        _context = context;
    }

    public async Task<Result<GetHeroResponse>> Handle(CreateHeroRequest request, CancellationToken cancellationToken)
    {
        using var activity = OpenTelemetryExtensions.ActivitySource.StartActivity("CreateHero", ActivityKind.Internal);
        activity?.SetTag("hero.name", request.Name);
        activity?.SetTag("hero.type", request.HeroType.ToString());
        activity?.SetTag("hero.team", request.Team);

        var created = Mapper.ToHeroEntity(request);
        _context.Heroes.Add(created);
        await _context.SaveChangesAsync(cancellationToken);

        activity?.SetTag("hero.id", created.Id.ToString());
        OpenTelemetryExtensions.HeroesCreatedCounter.Add(
            1,
            new KeyValuePair<string, object?>("hero.type", request.HeroType.ToString()));

        return Mapper.ToHeroDto(created);
    }
}
