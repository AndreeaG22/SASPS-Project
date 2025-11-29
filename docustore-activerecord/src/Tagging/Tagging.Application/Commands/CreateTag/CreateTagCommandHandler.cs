using MediatR;
using Tagging.Application.DTOs;
using Tagging.Domain.Entities;

namespace Tagging.Application.Commands.CreateTag;

public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, TagDto>
{
    public async Task<TagDto> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        // Check if tag with same name already exists (case-insensitive)
        var existingTag = await Tag.FindByName(request.Name, cancellationToken);
        if (existingTag != null)
        {
            throw new InvalidOperationException($"Tag with name '{request.Name}' already exists");
        }

        var tag = Tag.Create(request.Name, request.Description, request.UserId);
        await tag.Save(cancellationToken);

        return new TagDto(
            Id: tag.Id,
            Name: tag.Name,
            Description: tag.Description,
            CreatedAt: tag.CreatedAt,
            CreatedBy: tag.CreatedBy
        );
    }
}
