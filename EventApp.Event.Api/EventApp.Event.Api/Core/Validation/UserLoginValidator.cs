using EventApp.Models.UserDTO.Requests;
using FluentValidation;

namespace EventApp.Api.Core.Validation {

    public class UserLoginValidator : AbstractValidator<UserLoginRequestModel>{

        public UserLoginValidator() {

            RuleFor(x => x.Email)
               .NotEmpty().WithMessage("Email обязателен для заполнения.")
               .MaximumLength(256).WithMessage("Email не может превышать 256 символов.")
               .EmailAddress().WithMessage("Требуется указать действительный адрес электронной почты.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Пароль обязателен для заполнения.");

        }

    }

}
