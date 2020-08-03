using System;
using System.Collections.Generic;
using System.Text;

namespace BryantBrothers.WindCave.PxPay
{
	/// <summary>
	/// Transaction Result
	/// </summary>
	public class CreateTransactionResult
	{
		public bool IsSuccessful { get; set; }

		public Uri SecurePaymentUrl { get; set; }

		public ErrorDetails Error { get; set; }
	}
}
