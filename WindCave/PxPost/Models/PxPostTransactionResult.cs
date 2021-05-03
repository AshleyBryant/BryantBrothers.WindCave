using System;
using System.Collections.Generic;
using System.Text;

namespace BryantBrothers.WindCave.PxPost
{
	/// <summary>
	/// Px Post Transaction Result
	/// </summary>
	public class PxPostTransactionResult
	{
		public bool IsSuccessful { get; set; }

		public ErrorDetails Error { get; set; }

		public string ExpiryDate { get; set; }

		public string CardName { get; set; }

		public string ResponseText { get; set; }

		public string HelpText { get; set; }
	}
}
