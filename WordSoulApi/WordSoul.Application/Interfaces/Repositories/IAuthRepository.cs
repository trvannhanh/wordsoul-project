using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IAuthRepository
    {
        // ----------------------------- CREATE -----------------------------
        Task<User> RegisterUserAsync(
            User user,
            CancellationToken cancellationToken = default);

        // ----------------------------- READ -------------------------------
        Task<User?> LoginUserAsync(
            string username,
            CancellationToken cancellationToken = default);

        Task<User?> GetUserByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        // ----------------------------- UPDATE -----------------------------
        Task<User> UpdateUserAsync(
            User user,
            CancellationToken cancellationToken = default);

        // ----------------------------- OTHER -------------------------------
        Task<bool> UserExistsAsync(
            string username,
            CancellationToken cancellationToken = default);

        Task<bool> EmailExistsAsync(
            string email,
            CancellationToken cancellationToken = default);
    }
}