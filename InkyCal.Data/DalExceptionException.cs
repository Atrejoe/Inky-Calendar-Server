using System;

namespace InkyCal.Data
{
	/// <summary>
	/// General DAL exception
	/// </summary>
	/// <seealso cref="Exception" />
	[Serializable]
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

		/// <summary>
		/// Initializes a new instance of the <see cref="DalException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
		protected DalException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
