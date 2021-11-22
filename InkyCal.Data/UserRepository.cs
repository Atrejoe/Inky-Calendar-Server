using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InkyCal.Data
{

	public static class UserRepository
	{

		public static async Task<User> GetUser(this IdentityUser identityUser)
		{
			if (identityUser is null)
				return null;

			return await GetUser(identityUser.Id);
		}

		public static async Task<User> GetUser(this string identityUserId)
		{
			using var c = new ApplicationDbContext();
			var result = await c.Set<User>()
								.SingleOrDefaultAsync(x => x.IdentityUserId == identityUserId);

			if (result is null)
			{
				result = new User() { IdentityUserId = identityUserId };
				c.Add(result);
				await c.SaveChangesAsync();
			}

			return result;
		}

		public static async Task<IEnumerable<User>> GetAll()
		{
			using var c = new ApplicationDbContext();
			return await c.Set<User>().Include(x => x.Panels).ToArrayAsync();
		}

		public static async Task StoreToken(GoogleOAuthAccess token)
		{
			using var c = new ApplicationDbContext();
			token.User = new User() { Id = token.User.Id };
			c.Entry(token.User).State = EntityState.Unchanged;
			c.Add(token);
			await c.SaveChangesAsync();
		}
		public static async Task DeleteToken(int idToken)
		{
			using var c = new ApplicationDbContext();
			var token = new GoogleOAuthAccess() { Id = idToken };
			c.Entry(token).State = EntityState.Deleted;
			await c.SaveChangesAsync();
		}

		public static async Task<GoogleOAuthAccess[]> GetTokens(int idUser)
		{
			using var c = new ApplicationDbContext();
			return await c.Set<GoogleOAuthAccess>().Where(x => x.User.Id == idUser).ToArrayAsync();
		}

		/// <summary>
		/// Updates the tokens access token
		/// </summary>
		/// <param name="refreshedToken"></param>
		/// <returns></returns>
		public static async Task UpdateAccessToken(GoogleOAuthAccess refreshedToken)
		{
			using var c = new ApplicationDbContext();
			var token = await c.Set<GoogleOAuthAccess>().SingleAsync(x => x.Id == refreshedToken.Id);
			token.AccessToken = refreshedToken.AccessToken;
			token.AccessTokenExpiry = refreshedToken.AccessTokenExpiry;
			await c.SaveChangesAsync();
		}
	}
}
