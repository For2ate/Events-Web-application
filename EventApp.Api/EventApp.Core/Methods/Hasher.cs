using System.Security.Cryptography;
using System.Text;

namespace EventApp.Core.Methods {

    public static class Hasher {

        public static string HashPassword(string password) {

            try {

                if (password == null) throw new ArgumentNullException("password is null");

                using (SHA256 sha256Hash = SHA256.Create()) {

                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                    StringBuilder builder = new StringBuilder();

                    foreach (byte b in bytes) {
                        builder.Append(b.ToString("x2"));
                    }

                    return builder.ToString();

                }

            } catch (Exception ex) {

                throw;

            }

        }


    }

}
