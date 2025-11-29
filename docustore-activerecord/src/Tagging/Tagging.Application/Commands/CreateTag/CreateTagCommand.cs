using MediatR;
using Tagging.Application.DTOs;

namespace Tagging.Application.Commands.CreateTag;

public record CreateTagCommand(
    string Name,
    string? Description,
    string UserId
) : IRequest<TagDto>;
