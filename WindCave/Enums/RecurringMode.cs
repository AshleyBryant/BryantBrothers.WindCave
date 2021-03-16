using System;
using System.Collections.Generic;
using System.Text;

namespace BryantBrothers.WindCave.WindCave.Enums
{
	/// <summary>
	/// Valid recurring mode options.
	/// </summary>
	public enum RecurringMode
	{
		RecurringInitial,
		InstallmentInitial,
		CredentialOnFileInitial,
		UnscheduledCredentialOnFileInitial,
		CredentialOnFile,
		UnscheduledCredentialOnFile,
		Incremental,
		Installment,
		Recurring,
		RecurringNoExpiry,
		Resubmission,
		Reauthorisation,
		DelayedCharges,
		Noshow
	}
}
