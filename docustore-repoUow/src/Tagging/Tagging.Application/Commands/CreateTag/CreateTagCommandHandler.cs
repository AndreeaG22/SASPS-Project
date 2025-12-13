using MediatR;
using Tagging.Application.DTOs;
using Tagging.Application.Interfaces;
using Tagging.Domain.Entities;

namespace Tagging.Application.Commands.CreateTag;

public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, TagDto>
{
    private readonly ITaggingUnitOfWork _unitOfWork;

    public CreateTagCommandHandler(ITaggingUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TagDto> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        // Check if tag with same name already exists (case-insensitive)
        var existingTag = await _unitOfWork.Tags.GetByNameAsync(request.Name, cancellationToken);
        if (existingTag != null)
        {
            throw new InvalidOperationException($"Tag with name '{request.Name}' already exists");
        }

        var tag = Tag.Create(request.Name, request.Description, request.UserId);
        
        await _unitOfWork.Tags.AddAsync(tag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TagDto(
            Id: tag.Id,
            Name: tag.Name,
            Description: tag.Description,
            CreatedAt: tag.CreatedAt,
            CreatedBy: tag.CreatedBy
        );
    }
}
