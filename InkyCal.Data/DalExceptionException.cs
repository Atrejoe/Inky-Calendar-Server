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
		public DalException() { }
		public DalException(string message) : base(message) { }
		public DalException(string message, Exception inner) : base(message, inner) { }
		protected DalException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
