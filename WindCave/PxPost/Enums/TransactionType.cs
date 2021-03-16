using System;
using System.Collections.Generic;
using System.Text;

namespace BryantBrothers.WindCave.WindCave.PxPost.Enums
{
	/// <summary>
	/// Valid transaction types.
	/// </summary>
	public enum TransactionType
	{
		/// <summary>
		/// Authorise will reserve the amount for up to 7 days after which time it will revert.  A complete transaction will be required to Complete the Auth.
		/// </summary>
		Auth,

		/// <summary>
		/// Completes (settles) a pre-approved Auth Transaction. The DpsTxnRef value returned by the original approved Auth transaction must be supplied.
		/// </summary>
		Complete,

		/// <summary>
		/// Purchase will process the transaction immediately.
		/// </summary>
		Purchase,

		/// <summary>
		/// Refund - Funds transferred immediately. Must be enabled as a special option.
		/// </summary>
		Refund,

		/// <summary>
		/// Validation transaction. On the amount 0.00 or 1.00 validates card details including expiry date. 
		/// Often utilised with the EnableAddBillCard property set to 1 to automatically generate a card's token for rebilling. 
		/// Note that the Validate transaction type may not be enabled by default on live accounts. 
		/// </summary>
		Validate
	}
}
