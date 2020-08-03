using System;
using System.Collections.Generic;
using System.Text;

namespace BryantBrothers.WindCave.WindCave.PxPay.Enums
{
	/// <summary>
	/// Valid CVC result codes.
	/// </summary>
	public enum CvcResultCode
	{
		/// <summary>
		/// CVC matched that on card.
		/// </summary>
		Matched,

		/// <summary>
		/// CVC did not match that on card.
		/// </summary>
		NoMatch,

		/// <summary>
		/// CVC request not processed
		/// </summary>
		NotProcessed,

		/// <summary>
		/// CVC should be on the card, but merchant has sent code indicating there was no CVC.
		/// </summary>
		CvcRequiredButNotSent,

		/// <summary>
		/// Issuer does not support CVC
		/// </summary>
		CvcNotSupportedByIssuer,

		/// <summary>
		/// Could not map value from WindCave.
		/// </summary>
		Unknown
	}
}
