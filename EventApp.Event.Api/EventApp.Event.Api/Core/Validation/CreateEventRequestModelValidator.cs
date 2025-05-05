using EventApp.Models.EventDTO.Request;
using FluentValidation;

namespace EventApp.Api.Core.Validation {
    public class CreateEventRequestModelValidator : AbstractValidator<CreateEventRequestModel> {
        public CreateEventRequestModelValidator() {
           
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Название события обязательно для заполнения.")
                .Length(3, 200).WithMessage("Длина названия события должна быть от 3 до 200 символов.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Описание события обязательно для заполнения.")
                .MaximumLength(2000).WithMessage("Описание события не должно превышать 2000 символов.");

            RuleFor(x => x.DateOfEvent)
                .NotEmpty().WithMessage("Дата проведения события обязательна.")
                .GreaterThan(DateTime.UtcNow).WithMessage("Дата события должна быть в будущем.");
         

            RuleFor(x => x.MaxNumberOfParticipants)
                .GreaterThan(0).WithMessage("Максимальное количество участников должно быть больше нуля.");
           
            RuleFor(x => x.ImageUrl)
                .Must(uri => string.IsNullOrWhiteSpace(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl))
                .WithMessage("URL изображения должен быть действительным абсолютным URL-адресом.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Идентификатор категории обязателен.") 
                .NotEqual(Guid.Empty).WithMessage("Идентификатор категории не должен быть пустым GUID."); 
        
        }

    }

}
