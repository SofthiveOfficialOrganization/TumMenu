using Application.Abstractions;
using Application.Images.DTOs;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Images.Commands;

public sealed record CreateImageCommand(
	string ImageUrl,
	string? AltText,
	int? SortOrder,
	string? Slot,
	int? Width,
	int? Height,
	Guid ReferenceId,
	ImageRefType ImageRefType
) : IRequest<ImageDTO>, ITransactionalRequest;

public class CreateImageCommandHandler(
	)
{
}