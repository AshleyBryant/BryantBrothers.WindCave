using System;
using System.Collections.Generic;
using System.Text;

namespace BryantBrothers.WindCave.WindCave.Enums
{
	/// <summary>
	/// Valid client types.
	/// </summary>
	public enum Cvc2Presence
	{
		/// <summary>
		/// You (MERCHANT) have chosen not to submit CVC.
		/// </summary>
		MerchantNotSubmitted = 0,

		/// <summary>
		/// You (MERCHANT) have included CVC in the Auth / Purchase.
		/// </summary>
		MerchantIncluded = 1,

		/// <summary>
		/// Card holder has stated CVC is illegible.
		/// </summary>
		CardHolderStatedIllegible = 2,

		/// <summary>
		/// Card holder has stated CVC is not on the card.
		/// </summary>
		CardHolderStatedMissing = 9
	}
}
