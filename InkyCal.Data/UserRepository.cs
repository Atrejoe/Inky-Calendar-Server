using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using InkyCal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InkyCal.Data
{


	/// <summary>
	/// A repo for <see cref="User"/>s
	/// </summary>
	public static class UserRepository
	{

		/// <summary>
		/// Gets the user by <paramref name="identityUser"/>.
		/// </summary>
		/// <param name="identityUser">The identity user.</param>
		/// <returns></returns>
		public static async Task<User> GetUser(this IdentityUser identityUser)
		{
			if (identityUser is null)
				return null;

			return await GetUser(identityUser.Id);
		}


		/// <summary>
		/// Gets the user by <paramref name="identityUserId"/>
		/// </summary>
		/// <param name="identityUserId">The identity user identifier.</param>
		/// <returns></returns>
		public static async Task<User> GetUser(this string identityUserId)
		{
			using var c = new ApplicationDbContext();
			var result = await c.Set<User>()
								.SingleOrDefaultAsync(x => x.IdentityUserId == identityUserId);

			if (result is null)
			{
				result = new User() { IdentityUserId = identityUserId };
				await c.AddAsync(result);
				await c.SaveChangesAsync();
			}

			return result;
		}


		/// <summary>
		/// Gets all <see cref="User"/>s.
		/// </summary>
		/// <returns></returns>
		public static async Task<IEnumerable<User>> GetAll()
		{
			using var c = new ApplicationDbContext();
			return await c.Set<User>().Include(x => x.Panels).ToArrayAsync();
		}
	}
}
