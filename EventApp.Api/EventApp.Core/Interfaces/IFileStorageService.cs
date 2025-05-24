using Microsoft.AspNetCore.Http;

namespace EventApp.Core.Interfaces {

    public interface IFileStorageService {

        Task<string> SaveFileAsync(IFormFile file);

    }

}
