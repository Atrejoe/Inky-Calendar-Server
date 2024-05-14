using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;
using Microsoft.EntityFrameworkCore;

namespace InkyCal.Data
{
	/// <summary>
	/// 
	/// </summary>
	[SuppressMessage("Naming", "VSSpell001:Spell Check", Justification = "'Auth' is part of OAuth")]
	public class GoogleOAuthRepository
	{

		/// <summary>
		/// Stores the token.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <exception cref="ArgumentNullException">token</exception>
		public async Task StoreToken(GoogleOAuthAccess token)
		{
			ArgumentNullException.ThrowIfNull(token);

			using var c = new ApplicationDbContext();
			token.User = new User() { Id = token.User.Id };
			c.Entry(token.User).State = EntityState.Unchanged;
			await c.AddAsync(token);
			await c.SaveChangesAsync();
		}

		/// <summary>
		/// Deletes the token.
		/// </summary>
		/// <param name="idToken">The identifier token.</param>
		public async Task DeleteToken(int idToken)
		{
			using var c = new ApplicationDbContext();
			var token = new GoogleOAuthAccess() { Id = idToken };
			c.Entry(token).State = EntityState.Deleted;
			await c.SaveChangesAsync();
		}



		/// <summary>
		/// Gets the tokens for the specified <paramref name="idUser"/>.
		/// </summary>
		/// <param name="idUser">The identifier user.</param>
		/// <returns></returns>
		public async Task<GoogleOAuthAccess[]> GetTokens(int idUser)
		{
			using var c = new ApplicationDbContext();
			return await c.Set<GoogleOAuthAccess>().Where(x => x.User.Id == idUser)
							.AsNoTracking()
							.ToArrayAsync();
		}

		/// <summary>
		/// Updates the tokens access token
		/// </summary>
		/// <param name="refreshedToken"></param>
		/// <returns></returns>
		public async Task UpdateAccessToken(GoogleOAuthAccess refreshedToken)
		{
			ArgumentNullException.ThrowIfNull(refreshedToken);

			using var c = new ApplicationDbContext();
			var token = await c.Set<GoogleOAuthAccess>().SingleAsync(x => x.Id == refreshedToken.Id);
			token.AccessToken = refreshedToken.AccessToken;
			token.AccessTokenExpiry = refreshedToken.AccessTokenExpiry;
			await c.SaveChangesAsync();
		}
	}
}
