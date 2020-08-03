using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Xml;
using System.Xml.Linq;
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
        /// Create a new PX Pay transaction
        /// </summary>
        /// <param name="transactionRequest"> Details of the transaction to create. </param>
        /// <returns></returns>
        public CreateTransactionResult CreateTransaction(CreateTransactionRequest transactionRequest)
        {
            // Ensure we use TLS 1.2
            if (UseTls12)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }

            var result = new CreateTransactionResult
            {
                IsSuccessful = false
            };

            var xmlRequest = BuildTransactionXmlRequest(transactionRequest);

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

                var uri = GetString(response, "URI");

                // Success
                result.SecurePaymentUrl = new Uri(uri);
                result.IsSuccessful = true;
            }
            catch (Exception e)
            {
                result.Error = new ErrorDetails { 
                    ErrorMessage = $"Exception occured creating payment transaction. {e.Message.ToString()}" 
                };
            }

            return result;
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

                var rootNode = response.GetElementsByTagName("Request")[0];

                // Valid attribute
                transactionDetails.ValidTransaction = rootNode.Attributes["valid"].Value == "1";

                // Map fields
                transactionDetails.AmountSettlement = Convert.ToInt32(GetString(response, "AmountSettlement").Replace(".", ""));
                transactionDetails.AuthCode = GetString(response, "AuthCode");
                transactionDetails.CardName = GetString(response, "CardName");
                transactionDetails.CardNumber = GetString(response, "CardNumber");
                transactionDetails.DateExpiry = GetString(response, "DateExpiry");
                transactionDetails.DpsTxnRef = GetString(response, "DpsTxnRef");
                transactionDetails.Success = GetString(response, "Success") == "1";
                transactionDetails.ResponseText = GetString(response, "ResponseText");
                transactionDetails.DpsBillingId = GetString(response, "DpsBillingId");
                transactionDetails.CardHolderName = GetString(response, "CardHolderName");
                transactionDetails.CurrencySettlement = GetEnum<PxPayCurrency>(response, "CurrencySettlement");
                transactionDetails.PaymentMethod = GetString(response, "PaymentMethod");
                transactionDetails.TxnData1 = GetString(response, "TxnData1");
                transactionDetails.TxnData2 = GetString(response, "TxnData2");
                transactionDetails.TxnData3 = GetString(response, "TxnData3");
                transactionDetails.TxnType = GetEnum<TransactionType>(response, "TxnType");
                transactionDetails.CurrencyInput = GetEnum<PxPayCurrency>(response, "CurrencyInput");
                transactionDetails.MerchantReference = GetString(response, "MerchantReference");
                transactionDetails.ClientIpAddress = GetString(response, "ClientIpAddress");
                transactionDetails.TxnId = GetString(response, "TxnId");
                transactionDetails.EmailAddress = GetString(response, "EmailAddress");
                transactionDetails.BillingId = GetString(response, "BillingId");
                transactionDetails.TxnMac = GetString(response, "TxnMac");
                transactionDetails.CardNumber2 = GetString(response, "CardNumber2");
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
        /// Gets the string value of a tag within an XMLDoc.
        /// </summary>
        /// <param name="xml"> XmlDocument </param>
        /// <param name="tagName"> Name of tag </param>
        /// <returns></returns>
        private string GetString(XmlDocument xml, string tagName)
		{
            return xml.GetElementsByTagName(tagName)[0].InnerText;
        }

        /// <summary>
        /// Gets the string value from the XMLDoc and converts it to the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <param name="tagName"></param>
        /// <returns></returns>
        private T GetEnum<T>(XmlDocument xml, string tagName)
		{
            var val = GetString(xml, tagName);

            return (T)Enum.Parse(typeof(T), val);
        }

        /// <summary>
        /// Maps the CvcResultCode field.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="tagName"></param>
        /// <returns></returns>
        private CvcResultCode GetCvcResultCode(XmlDocument xml, string tagName)
		{
            var result = GetString(xml, tagName);

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
		/// <returns></returns>
		private XElement BuildTransactionXmlRequest(CreateTransactionRequest transactionRequest)
        {
            // These fields are required
            var fields = new List<XElement> {
                new XElement("PxPayUserId", PxPayUserId),                                
                new XElement("PxPayKey", PxPayKey),                                      
                new XElement("AmountInput", transactionRequest.Amount.ToString("0.00")), 
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
            fields.Add(new XElement("TxnType", transactionRequest.TxnType.ToString()));

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
