using System;
using BryantBrothers.WindCave.WindCave.PxPay.Enums;

namespace BryantBrothers.WindCave.PxPay
{
	/// <summary>
	/// Transaction Request
	/// </summary>
	public class CreateTransactionRequest
	{
		/// <summary>
		/// Required (Max "999999.99")
		/// The Amount in cents for the transaction.
		/// </summary>
		public int Amount { get; set; }

		/// <summary>
		/// Optional (Max 32 chars)
		/// If a token based billing transaction is to be created, a BillingId may be supplied. This is an identifier
		/// supplied by the merchant application that is used to identify a customer or billing entry and can be used
		/// as input instead of card number and date expiry for subsequent billing transactions.
		/// </summary>
		public string BillingId { get; set; }

		/// <summary>
		/// Required.
		/// Specifies the currency to be used e.g. “NZD” or “AUD”. 
		/// </summary>
		public PxPayCurrency CurrencyInput { get; set; }

		/// <summary>
		/// Optional (Max 255 bytes).
		/// The EmailAddress field can be used to store a customer's email address and will be returned in the
		/// transaction response.The response data along with the email address can then be used by the merchant
		/// to generate a notification/receipt email for the customer
		/// </summary>
		public string EmailAddress { get; set; }

		/// <summary>
		/// Optional.
		/// The EnableAddbillCard field is used if subsequent billing is required. Setting this field to true will cause
		/// Windcave to store the credit card details for future billing purposes.A DpsBillingId(Windcave generated)
		/// or BillingId(merchant generated) is used to reference the card.
		/// </summary>
		public bool EnableAddBillCard { get; set; }

		/// <summary>
		/// Optional (Max 255 bytes).
		/// Use to set one of the card storage reasons.
		/// </summary>
		public RecurringMode? RecurringMode { get; set; }

		/// <summary>
		/// Optional (Max 64 bytes).
		/// The Merchant Reference field is a free text field used to store a reference against a transaction. The
		/// merchant reference allows users to easily find and identify the transaction in Payline transaction query
		/// and Windcave reports.The merchant reference is returned in the transaction response, which can be
		/// used interpreted by the merchant website.Common uses for the merchant reference field are invoice
		/// and order numbers
		/// </summary>
		public string MerchantReference { get; set; }

		/// <summary>
		/// Optional (Max 16 chars).
		/// When output, contains the Windcave generated BillingId. Only returned for transactions that are
		/// requested by the application with the EnableAddBillCard value is set to true indicating a token billing
		/// entry should be created.
		/// </summary>
		public string DpsBillingId { get; set; }

		/// <summary>
		/// Optional (Max 255 bytes).
		/// The TxnData fields are free text fields that can be used to store information against a transaction. This
		/// can be used to store information such as customer name, address, phone number etc.This data is then
		/// returned in the transaction response and can also be retrieved from Windcave reports
		/// </summary>
		public string TxnData1 { get; set; }

		/// <summary>
		/// Optional (Max 255 bytes).
		/// The TxnData fields are free text fields that can be used to store information against a transaction. This
		/// can be used to store information such as customer name, address, phone number etc.This data is then
		/// returned in the transaction response and can also be retrieved from Windcave reports
		/// </summary>
		/// </summary>
		public string TxnData2 { get; set; }

		/// <summary>
		/// Optional (Max 255 bytes).
		/// The TxnData fields are free text fields that can be used to store information against a transaction. This
		/// can be used to store information such as customer name, address, phone number etc.This data is then
		/// returned in the transaction response and can also be retrieved from Windcave reports
		/// </summary>
		/// </summary>
		public string TxnData3 { get; set; }

		/// <summary>
		/// Required (Max 16 bytes).
		/// Should always contain a unique, merchant application generated value that uniquely identifies the transaction.
		/// Used by Windcave® to check for a duplicate transaction generated from Merchant web site. 
		/// </summary>
		public string TxnId { get; set; }

		/// <summary>
		/// Required (Max 255 bytes).
		/// Url of the page to redirect to if the transaction failed. 
		/// Should start with the protocol, e.g. "https://"
		/// </summary>
		public string UrlFail { get; set; }

		/// <summary>
		/// Required (Max 255 bytes).
		/// Url of the page to redirect to if the transaction succeeded.
		/// Should start with the protocol, e.g. "https://"
		/// </summary>
		public string UrlSuccess { get; set; }

		/// <summary>
		/// Optional (Max 255 bytes).
		/// Externally accessible URL of page or resource to receive the notification on the conclusion of the HPP
		/// session when the transaction outcome is determined.
		/// </summary>
		public string UrlCallback { get; set; }

		/// <summary>
		/// Optional (Max 64 bytes).
		/// This optional parameter can be used to set a timeout value for the hosted payments page or block/allow specified card BIN ranges.
		/// The value must be in the format "TO=yymmddHHmm" e.g. "TO=1010142221" for 2010 October 14th 10:21pm.
		/// Time should be in UTC.
		/// </summary>
		public string Opt { get; set; }

		/// <summary>
		/// Optional.
		/// Allows the Client type to be passed through at transaction time.
		/// </summary>
		public ClientType? ClientType { get; set; }

		/// <summary>
		/// Optional.
		/// Allows to force the specific payment method on the hosted payment page to use if the PxPayUserId has multiple payment methods configured.
		/// </summary>
		public ForcePaymentMethod? ForcePaymentMethod { get; set; }

		/// <summary>
		/// Optional.
		/// Only set this field to true to indicate that a debt repayment transaction is to be processed. 
		/// </summary>
		public bool DebtRepaymentIndicator { get; set; }

		/// <summary>
		/// Optional.
		/// Number based value that is used to indicate the current payment number for an installment
		/// transaction.For example, if the consumer is making payment 1 of 12, then this value should be set to 1.
		/// Only used for installment based payments.
		/// </summary>
		public int? InstallmentNumber { get; set; }

		/// <summary>
		/// Optional.
		/// The number value is used to indicate the total number of payments for an installment transaction. For
		/// example, if the consumer is making 12 installment payments for total payment, then this value should be
		/// set to 12. Only used for installment based payments.
		/// </summary>
		public int? InstallmentCount { get; set; }
	}
}
