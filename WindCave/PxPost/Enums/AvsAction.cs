using System;
using System.Collections.Generic;
using System.Text;

namespace BryantBrothers.WindCave.WindCave.PxPost.Enums
{
	/// <summary>
	/// Valid AVS Actions.
	/// </summary>
	public enum AvsAction
	{
		/// <summary>
		/// Do not check AVS details with acquirer, but pass them through to Windcave only for the record.
		/// </summary>
		DoNotCheck = 0,

		/// <summary>
		/// Attempt AVS check. If the acquirer doesn't support AVS or AVS is unavailable, then the transaction will proceed as normal. If AVS is supported and the AVS check fails, then the transaction will be declined.
		/// </summary>
		AttemptCheck = 1,

		/// <summary>
		/// The transactions needs to be checked by AVS, even if isn't available, otherwise the transaction will be blocked.
		/// </summary>
		CheckRequired = 2,

		/// <summary>
		/// AVS check will be attempted and any outcome will be recorded, but ignored i.e. transaction will not be declined if AVS fails or unavailable.
		/// </summary>
		CheckAttempedButIgnored = 3,

		/// <summary>
		/// Attempt AVS check. If the acquirer doesn't support AVS or AVS is unavailable, then the transaction will proceed as normal. If AVS is supported and the AVS check fails with a response of “N” (address and postcode both do NOT match what issuer has on file), then the transaction will be declined. Partial AVS matches such as postal code only matches or address only matches will be accepted.
		/// </summary>
		AttemptCheckWithPartialMatches = 4
	}
}
