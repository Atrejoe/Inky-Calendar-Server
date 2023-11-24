using System;

namespace InkyCal.Data
{
	/// <summary>
	/// General DAL exception
	/// </summary>
	/// <seealso cref="Exception" />
	public class DalException : Exception
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="DalException"/> class.
		/// </summary>
		public DalException() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="DalException"/> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public DalException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="DalException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner.</param>
		public DalException(string message, Exception inner) : base(message, inner) { }
	}
}
