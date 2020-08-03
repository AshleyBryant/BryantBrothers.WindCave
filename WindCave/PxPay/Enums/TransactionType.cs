using System;
using System.Collections.Generic;
using System.Text;

namespace BryantBrothers.WindCave.WindCave.PxPay.Enums
{
	/// <summary>
	/// Valid transaction types.
	/// </summary>
	public enum TransactionType
	{
		/// <summary>
		/// Purchase will process the transaction immediately.
		/// </summary>
		Purchase,

		/// <summary>
		/// Authorise will reserve the amount for up to 7 days after which time it will revert.  A complete transaction will be required to Complete the Auth.
		/// </summary>
		Auth
	}
}
