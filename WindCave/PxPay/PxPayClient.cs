using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Xml;
using System.Xml.Linq;
using BryantBrothers.WindCave.WindCave.Enums;
using BryantBrothers.WindCave.WindCave.PxPay.Enums;

namespace BryantBrothers.WindCave.PxPay
{
    /// <summary>
    /// PX Pay Client
    /// </summary>
	public class PxPayClient
	{
		private static readonly HttpClient WebClient = new HttpClient();
		private const string WindCaveUrl = "https://sec.windcave.com/pxaccess/pxpay.aspx";

        private readonly string PxPayUserId;
        private readonly string PxPayKey;
        private readonly bool UseTls12;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pxPayUserId"> PX Pay UserID </param>
        /// <param name="pxPayKey"> PX Pay Key </param>
        /// <param name="useTls12"> Ensures we are using TLS 1.2  Set to false if you don't want this.</param>
		public PxPayClient(string pxPayUserId, string pxPayKey, bool useTls12 = true)
		{
            PxPayUserId = pxPayUserId;
            PxPayKey = pxPayKey;
            UseTls12 = useTls12;
        }

        /// <summary>
        /// Create a new PX Pay Purchase transaction.
        /// This will process the transaction immediately.
        /// </summary>
        /// <param name="transactionRequest"> Details of the transaction to create. </param>
        /// <returns></returns>
        public CreateTransactionResult CreatePurchase(CreateTransactionRequest transactionRequest)
        {
            return CreateTransaction(transactionRequest, TransactionType.Purchase);
        }

        /// <summary>
        /// Create a new PX Pay Auth transaction.
        /// Authorise will reserve the amount for up to 7 days after which time it will revert.  A complete transaction will be required to Complete the Auth.
        /// </summary>
        /// <param name="transactionRequest"> Details of the transaction to create. </param>
        /// <returns></returns>
        public CreateTransactionResult CreateAuth(CreateTransactionRequest transactionRequest)
        {
            return CreateTransaction(transactionRequest, TransactionType.Auth);
        }

        /// <summary>
        /// Processes the response and returns details from the transaction.
        /// </summary>
        /// <param name="responseCode"> Response code passed to the success or failure page. </param>
        public TransactionDetails ProcessResponse(string responseCode)
		{
            // Ensure we use TLS 1.2
            if (UseTls12)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }

            var xmlRequest = BuildProcessResponseXmlRequest(responseCode);

            var transactionDetails = new TransactionDetails();

            try
			{
                // Post to Paystation URL
                var httpResponse = WebClient.PostAsync(WindCaveUrl, new StringContent(xmlRequest.ToString()))
                    .GetAwaiter()
                    .GetResult();

                var rawResponse = httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                // Check if successful
                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new Exception("Could not get response from payment gateway!");
                }

                // Deserialise XML result     
                var response = new XmlDocument();
                response.LoadXml(rawResponse);

                var rootNode = response.GetElementsByTagName("Response")[0];

                // Valid attribute
                transactionDetails.ValidTransaction = rootNode.Attributes["valid"].Value == "1";

                // Map fields
                transactionDetails.AmountSettlement = Convert.ToInt32(XmlHelper.GetString(response, "AmountSettlement").Replace(".", ""));
                transactionDetails.AuthCode = XmlHelper.GetString(response, "AuthCode");
                transactionDetails.CardName = XmlHelper.GetString(response, "CardName");
                transactionDetails.CardNumber = XmlHelper.GetString(response, "CardNumber");
                transactionDetails.DateExpiry = XmlHelper.GetString(response, "DateExpiry");
                transactionDetails.DpsTxnRef = XmlHelper.GetString(response, "DpsTxnRef");
                transactionDetails.Success = XmlHelper.GetString(response, "Success") == "1";
                transactionDetails.ResponseText = XmlHelper.GetString(response, "ResponseText");
                transactionDetails.DpsBillingId = XmlHelper.GetString(response, "DpsBillingId");
                transactionDetails.CardHolderName = XmlHelper.GetString(response, "CardHolderName");
                transactionDetails.CurrencySettlement = XmlHelper.GetEnum<WindCaveCurrency>(response, "CurrencySettlement");
                transactionDetails.PaymentMethod = "card payment";
                if (XmlHelper.TagExists(response, "PaymentMethod"))
                {
                    transactionDetails.PaymentMethod = XmlHelper.GetString(response, "PaymentMethod");
                }
                transactionDetails.TxnData1 = XmlHelper.GetString(response, "TxnData1");
                transactionDetails.TxnData2 = XmlHelper.GetString(response, "TxnData2");
                transactionDetails.TxnData3 = XmlHelper.GetString(response, "TxnData3");
                transactionDetails.TxnType = XmlHelper.GetEnum<TransactionType>(response, "TxnType");
                transactionDetails.CurrencyInput = XmlHelper.GetEnum<WindCaveCurrency>(response, "CurrencyInput");
                transactionDetails.MerchantReference = XmlHelper.GetString(response, "MerchantReference");
                transactionDetails.ClientIpAddress = null;
                if (XmlHelper.TagExists(response, "ClientIpAddress"))
                {
                    transactionDetails.ClientIpAddress = XmlHelper.GetString(response, "ClientIpAddress");
                }
                transactionDetails.TxnId = XmlHelper.GetString(response, "TxnId");
                transactionDetails.EmailAddress = XmlHelper.GetString(response, "EmailAddress");
                transactionDetails.BillingId = XmlHelper.GetString(response, "BillingId");
                transactionDetails.TxnMac = XmlHelper.GetString(response, "TxnMac");
                transactionDetails.CardNumber2 = XmlHelper.GetString(response, "CardNumber2");
                transactionDetails.Cvc2ResultCode = GetCvcResultCode(response, "Cvc2ResultCode");
            }
            catch (Exception ex)
			{
                throw ex;
			}

            return transactionDetails;
        }

        #region Private methods...

        /// <summary>
        /// Create a new PX Pay transaction
        /// </summary>
        /// <param name="transactionRequest"> Details of the transaction to create. </param>
        /// <param name="transactionType"> Transaction Type (Purchase or Auth) </param>
        /// <returns></returns>
        private CreateTransactionResult CreateTransaction(CreateTransactionRequest transactionRequest, TransactionType transactionType)
        {
            // Ensure we use TLS 1.2
            if (UseTls12)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }

            // Build the request
            var xmlRequest = BuildTransactionXmlRequest(transactionRequest, transactionType);

            var result = new CreateTransactionResult
            {
                IsSuccessful = false
            };

            try
            {
                // Post to Paystation URL
                var httpResponse = WebClient.PostAsync(WindCaveUrl, new StringContent(xmlRequest.ToString()))
                    .GetAwaiter()
                    .GetResult();

                var rawResponse = httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                // Check if successful
                if (!httpResponse.IsSuccessStatusCode)
                {
                    result.Error = new ErrorDetails
                    {
                        ErrorMessage = "Could not access payment gateway!"
                    };

                    return result;
                }

                // Deserialise XML result     
                var response = new XmlDocument();
                response.LoadXml(rawResponse);

                var rootNode = response.GetElementsByTagName("Request")[0];

                var isValid = rootNode.Attributes["valid"].Value == "1";

                // Check if valid
                if (!isValid)
                {
                    result.Error = new ErrorDetails
                    {
                        ErrorMessage = "Invalid request to payment gateway!"
                    };

                    return result;
                }

                var hasResponseCode = XmlHelper.TagExists(response, "Reco");
                if (hasResponseCode)
                {
                    var reco = XmlHelper.GetString(response, "Reco");
                    var responseText = XmlHelper.GetString(response, "ResponseText");

                    result.Error = new ErrorDetails
                    {
                        ErrorMessage = $"Response code: {reco} - {responseText}"
                    };
                } 
                else
                {
                    var uri = XmlHelper.GetString(response, "URI");

                    // Success
                    result.SecurePaymentUrl = new Uri(uri);
                    result.IsSuccessful = true;
                }
            }
            catch (Exception e)
            {
                result.Error = new ErrorDetails
                {
                    ErrorMessage = $"Exception occured creating payment transaction. {e.Message.ToString()}"
                };
            }

            return result;
        }

        /// <summary>
        /// Maps the CvcResultCode field.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="tagName"></param>
        /// <returns></returns>
        private CvcResultCode GetCvcResultCode(XmlDocument xml, string tagName)
		{
            var result = XmlHelper.GetString(xml, tagName);

			switch (result)
			{
                case "M": return CvcResultCode.Matched;
                case "N": return CvcResultCode.NoMatch;
                case "P": return CvcResultCode.NotProcessed;
                case "S": return CvcResultCode.CvcRequiredButNotSent;
                case "U": return CvcResultCode.CvcNotSupportedByIssuer;
                default: return CvcResultCode.Unknown;
			}
        }

		/// <summary>
		/// Builds the XML request for a new Transaction.
		/// </summary>
		/// <param name="transactionRequest"> Transaction request details </param>
        /// <param name="transactionType"> Transaction Type </param>
		/// <returns></returns>
		private XElement BuildTransactionXmlRequest(CreateTransactionRequest transactionRequest, TransactionType transactionType)
        {
            // These fields are required
            var fields = new List<XElement> {
                new XElement("PxPayUserId", PxPayUserId),                                
                new XElement("PxPayKey", PxPayKey),                                      
                new XElement("AmountInput", (transactionRequest.Amount / 100.0).ToString("0.00")), 
            };

            // BillingId (optional)
            if (!string.IsNullOrEmpty(transactionRequest.BillingId))
            {
                fields.Add(new XElement("BillingId", transactionRequest.BillingId));
			}

            // CurrencyInput (required)
            fields.Add(new XElement("CurrencyInput", transactionRequest.CurrencyInput.ToString()));

            // EmailAddress (optional)
            if (!string.IsNullOrEmpty(transactionRequest.EmailAddress))
			{
                fields.Add(new XElement("EmailAddress", transactionRequest.EmailAddress));
			}

            // EnableAddBillCard (optional)
            if (transactionRequest.EnableAddBillCard)
			{
                fields.Add(new XElement("EnableAddBillCard", "1"));
			}

            // RecurringMode (optional)
            if (transactionRequest.RecurringMode.HasValue)
			{
                fields.Add(new XElement("RecurringMode", transactionRequest.RecurringMode.Value.ToString().ToLower()));
			}

            // MerchantReference (optional)
            if (!string.IsNullOrEmpty(transactionRequest.MerchantReference))
			{
                fields.Add(new XElement("MerchantReference", transactionRequest.MerchantReference));
			}

            // DpsBillingId (optional)
            if (!string.IsNullOrEmpty(transactionRequest.DpsBillingId))
			{
                fields.Add(new XElement("DpsBillingId", transactionRequest.DpsBillingId));
			}

            // TxnData1 (optional)
            if (!string.IsNullOrEmpty(transactionRequest.TxnData1))
			{
                fields.Add(new XElement("TxnData1", transactionRequest.TxnData1));
			}

            // TxnData2 (optional)
            if (!string.IsNullOrEmpty(transactionRequest.TxnData2))
            {
                fields.Add(new XElement("TxnData2", transactionRequest.TxnData2));
            }

            // TxnData3 (optional)
            if (!string.IsNullOrEmpty(transactionRequest.TxnData3))
            {
                fields.Add(new XElement("TxnData3", transactionRequest.TxnData3));
            }

            // TxnType (required)
            fields.Add(new XElement("TxnType", transactionType.ToString()));

            // TxnId (required)
            fields.Add(new XElement("TxnId", transactionRequest.TxnId));

            // UrlFail (required)
            fields.Add(new XElement("UrlFail", transactionRequest.UrlFail));

            // UrlSuccess (required)
            fields.Add(new XElement("UrlSuccess", transactionRequest.UrlSuccess));

            // UrlCallback (optional)
            if (!string.IsNullOrEmpty(transactionRequest.UrlCallback))
			{
                fields.Add(new XElement("UrlCallback", transactionRequest.UrlCallback));
			}

            // Opt (optional)
            if (!string.IsNullOrEmpty(transactionRequest.Opt))
			{
                fields.Add(new XElement("Opt", transactionRequest.Opt));
			}

            // ClientType (optional)
            if (transactionRequest.ClientType.HasValue)
            {
                fields.Add(new XElement("ClientType", transactionRequest.ClientType.Value.ToString()));
            }

            // ForcePaymentMethod
            if (transactionRequest.ForcePaymentMethod.HasValue)
			{
                fields.Add(new XElement("ForcePaymentMethod", transactionRequest.ForcePaymentMethod.ToString()));
			}

            // DebtRepaymentIndicator
            if (transactionRequest.DebtRepaymentIndicator)
			{
                fields.Add(new XElement("DebtRepaymentIndicator", "1"));
			}

            // InstallmentNumber
            if (transactionRequest.InstallmentNumber.HasValue)
            {
                fields.Add(new XElement("InstallmentNumber", transactionRequest.InstallmentNumber.Value.ToString()));
            }

            // InstallmentCount
            if (transactionRequest.InstallmentCount.HasValue)
            {
                fields.Add(new XElement("InstallmentCount", transactionRequest.InstallmentCount.Value.ToString()));
            }

            // Build the XML object and return it
            var xmlRequest = new XElement("GenerateRequest", fields);

            return xmlRequest;
        }

        /// <summary>
        /// Builds the XML request to retrieve a transactions details.
        /// </summary>
        /// <param name="transactionResponseCode"> Transaction Response Code </param>
        /// <returns></returns>
        private XElement BuildProcessResponseXmlRequest(string transactionResponseCode)
        {
            var xmlRequest = new XElement("ProcessResponse",
                new XElement("PxPayUserId", PxPayUserId),
                new XElement("PxPayKey", PxPayKey),
                new XElement("Response", transactionResponseCode)
            );

            return xmlRequest;
        }

        #endregion
    }
}
