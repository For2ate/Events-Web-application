namespace EventApp.Api.Core.Interfaces {

    public interface IFileStorageService {

        Task<string> SaveFileAsync(IFormFile file);

    }

}
