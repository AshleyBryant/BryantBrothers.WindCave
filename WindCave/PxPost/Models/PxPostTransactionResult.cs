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
	}
}
