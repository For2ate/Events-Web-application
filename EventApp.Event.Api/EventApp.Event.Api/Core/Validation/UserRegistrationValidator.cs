using EventApp.Models.UserDTO.Requests;
using FluentValidation;

namespace EventApp.Api.Core.Validation {

    public class UserRegistrationValidator : AbstractValidator<UserRegisterRequestModel> {

        public UserRegistrationValidator() {

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Имя обязательно для заполнения.")
                .MaximumLength(100).WithMessage("Имя не может превышать 100 символов.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Фамилия обязательна для заполнения.")
                .MaximumLength(100).WithMessage("Фамилия не может превышать 100 символов.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email обязателен для заполнения.")
                .MaximumLength(256).WithMessage("Email не может превышать 256 символов.")
                .EmailAddress().WithMessage("Требуется указать действительный адрес электронной почты.");

            RuleFor(x => x.BirthdayDate)
                .NotEmpty().WithMessage("Дата рождения обязательна для заполнения.")
                .LessThan(DateTime.UtcNow).WithMessage("Дата рождения должна быть в прошлом.")
                .Must(BeAtLeast18).WithMessage("Пользователь должен быть старше 18 лет.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Пароль обязателен для заполнения.")
                .MinimumLength(8).WithMessage("Пароль должен содержать не менее 8 символов.")
                .MaximumLength(100).WithMessage("Пароль не может превышать 100 символов.")
                .Matches("[A-Z]").WithMessage("Пароль должен содержать хотя бы одну заглавную букву.")
                .Matches("[a-z]").WithMessage("Пароль должен содержать хотя бы одну строчную букву.")
                .Matches("[0-9]").WithMessage("Пароль должен содержать хотя бы одну цифру.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Пароль должен содержать хотя бы один специальный символ.");

        }

        private bool BeAtLeast18(DateTime birthdayDate) {
            var yearsAgo18 = DateTime.UtcNow.AddYears(-18);
            return birthdayDate <= yearsAgo18;
        }

    }

}
