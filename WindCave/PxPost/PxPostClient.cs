using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Xml;
using System.Xml.Linq;
using BryantBrothers.WindCave.WindCave.Enums;
using BryantBrothers.WindCave.WindCave.PxPost.Enums;
using BryantBrothers.WindCave.WindCave.PxPost.Models;

namespace BryantBrothers.WindCave.PxPost
{
    /// <summary>
    /// PX Post Client
    /// </summary>
	public class PxPostClient
	{
		private static readonly HttpClient WebClient = new HttpClient();
		private const string WindCaveUrl = "https://sec.windcave.com/pxpost.aspx";

        private readonly string PxPostUsername;
        private readonly string PxPostPassword;
        private readonly bool UseTls12;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pxPostUsername"> PX Post Username </param>
        /// <param name="pxPostPassword"> PX Post Password </param>
        /// <param name="useTls12"> Ensures we are using TLS 1.2  Set to false if you don't want this.</param>
		public PxPostClient(string pxPostUsername, string pxPostPassword, bool useTls12 = true)
		{
            PxPostUsername = pxPostUsername;
            PxPostPassword = pxPostPassword;
            UseTls12 = useTls12;
        }

        /// <summary>
        /// Process a rebill for an existing DPS billing ID.
        /// </summary>
        /// <param name="dpsBillingId"> Existing DPS billing ID for client. </param>
        /// <param name="amount"> Amount in cents to bill card. </param>
        /// <param name="transactionId"> Unique order id provided by us (not WindCave) </param>
        /// <param name="merchantReference"> Optional reference </param>
        /// <param name="currency"> Curreny (default NZD) </param>
        public PxPostTransactionResult ProcessRebill(string dpsBillingId, int amount, string transactionId, string merchantReference, WindCaveCurrency currency = WindCaveCurrency.NZD)
		{
            var transactionRequest = new PxPostTransaction
            {
                DpsBillingId = dpsBillingId,
                Amount = amount,
                InputCurrency = currency,
                RecurringMode = RecurringMode.Recurring,
                TxnType = TransactionType.Purchase,
                TxnId = transactionId,
                MerchantReference = merchantReference,
            };

            return ProcessTransaction(transactionRequest);
        }

        /// <summary>
        /// Process a new PX Post transaction (generic method)
        /// </summary>
        /// <param name="transactionRequest"> Details of the transaction to create. </param>
        /// <returns></returns>
        public PxPostTransactionResult ProcessTransaction(PxPostTransaction transactionRequest)
        {
            // Ensure we use TLS 1.2
            if (UseTls12)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }

            // Build the request
            var xmlRequest = BuildTransactionXmlRequest(transactionRequest);

            var result = new PxPostTransactionResult
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

                var rootNode = response.GetElementsByTagName("Txn")[0];
                var transactionNode = rootNode.SelectSingleNode("Transaction");

                // "Success"
                var isValid = transactionNode.Attributes["success"].Value == "1";
                var reco = transactionNode.Attributes["reco"].Value;

                // Check if valid
                if (!isValid)
                {
                    result.Error = new ErrorDetails
                    {
                        ErrorMessage = "Invalid request to payment gateway!"
                    };

                    return result;
                }

                // Get extra details
                result.ExpiryDate = XmlHelper.GetString(transactionNode, "DateExpiry");
                result.CardName = XmlHelper.GetString(transactionNode, "CardName");
                
                result.ResponseText = XmlHelper.GetString(rootNode, "ResponseText");
                result.HelpText = XmlHelper.GetString(rootNode, "HelpText");
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
		/// Builds the XML request for a new Transaction.
		/// </summary>
		/// <param name="transactionRequest"> Transaction request details </param>
		/// <returns></returns>
		private XElement BuildTransactionXmlRequest(PxPostTransaction transactionRequest)
        {
            // These fields are required
            var fields = new List<XElement> {
                new XElement("PostUsername", PxPostUsername),
                new XElement("PostPassword", PxPostPassword),
                new XElement("Amount", (transactionRequest.Amount / 100.0).ToString("0.00")),
            };

            // InputCurrency (required)
            fields.Add(new XElement("InputCurrency", transactionRequest.InputCurrency.ToString()));

            // CardHolderName (optional)
            if (!string.IsNullOrEmpty(transactionRequest.CardHolderName))
            {
                fields.Add(new XElement("CardHolderName", transactionRequest.CardHolderName));
            }

            // ClientType (optional)
            if (transactionRequest.ClientType.HasValue)
            {
                fields.Add(new XElement("ClientType", transactionRequest.ClientType.Value.ToString().ToLower()));
            }

            // BillingId (optional)
            if (!string.IsNullOrEmpty(transactionRequest.BillingId))
            {
                fields.Add(new XElement("BillingId", transactionRequest.BillingId));
            }

            // Cvc2 (optional)
            if (!string.IsNullOrEmpty(transactionRequest.Cvc2))
            {
                fields.Add(new XElement("Cvc2", transactionRequest.Cvc2));
            }

            // Cvc2Presence (optional)
            if (transactionRequest.Cvc2Presence.HasValue)
            {
                fields.Add(new XElement("Cvc2Presence", transactionRequest.Cvc2Presence.Value));
            }

            // DateExpiry (optional)
            if (!string.IsNullOrEmpty(transactionRequest.DateExpiry))
            {
                fields.Add(new XElement("DateExpiry", transactionRequest.DateExpiry));
            }

            // DpsBillingId (optional)
            if (!string.IsNullOrEmpty(transactionRequest.DpsBillingId))
            {
                fields.Add(new XElement("DpsBillingId", transactionRequest.DpsBillingId));
            }

            // DpsTxnRef (optional)
            if (!string.IsNullOrEmpty(transactionRequest.DpsTxnRef))
            {
                fields.Add(new XElement("DpsTxnRef", transactionRequest.DpsTxnRef));
            }

            // EnableAddBillCard (optional)
            if (transactionRequest.EnableAddBillCard.HasValue)
            {
                fields.Add(new XElement("EnableAddBillCard", transactionRequest.EnableAddBillCard.Value ? "1" : "0"));
            }

            // MerchantReference (required)
            if (!string.IsNullOrEmpty(transactionRequest.MerchantReference))
            {
                fields.Add(new XElement("MerchantReference", transactionRequest.MerchantReference));
            }

            // ReceiptEmail (required)
            if (!string.IsNullOrEmpty(transactionRequest.ReceiptEmail))
            {
                fields.Add(new XElement("ReceiptEmail", transactionRequest.ReceiptEmail));
            }

            // RecurringMode (optional)
            if (transactionRequest.RecurringMode.HasValue)
            {
                fields.Add(new XElement("RecurringMode", transactionRequest.RecurringMode.Value.ToString()));
            }

            // TxnType (required)
            fields.Add(new XElement("TxnType", transactionRequest.TxnType.ToString()));

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

            // TxnId (optional)
            if (!string.IsNullOrEmpty(transactionRequest.TxnId))
            {
                fields.Add(new XElement("TxnId", transactionRequest.TxnId));
            }

            // EnableAvsData (optional)
            if (transactionRequest.EnableAvsData.HasValue)
			{
                fields.Add(new XElement("EnableAvsData", transactionRequest.EnableAvsData.Value ? "1" : "0"));
			}

            // AvsAction (optinoal)
            if (transactionRequest.AvsAction.HasValue)
			{
                fields.Add(new XElement("AvsAction", (int)transactionRequest.AvsAction.Value));
            }

            // AvsPostCode (optional)
            if (!string.IsNullOrEmpty(transactionRequest.AvsPostCode))
			{
                fields.Add(new XElement("AvsPostCode", transactionRequest.AvsPostCode));
            }

            // AvsStreetAddress (optional)
            if (!string.IsNullOrEmpty(transactionRequest.AvsStreetAddress))
            {
                fields.Add(new XElement("AvsStreetAddress", transactionRequest.AvsStreetAddress));
            }

            // DateStart (optional)
            if (!string.IsNullOrEmpty(transactionRequest.DateStart))
            {
                fields.Add(new XElement("DateStart", transactionRequest.DateStart));
            }

            // IssueNumber (optional)
            if (transactionRequest.IssueNumber.HasValue)
			{
                fields.Add(new XElement("IssueNumber", transactionRequest.IssueNumber.Value));
            }

            // Track2 (optional)
            if (!string.IsNullOrEmpty(transactionRequest.Track2))
            {
                fields.Add(new XElement("Track2", transactionRequest.Track2));
            }

            // Build the XML object and return it
            var xmlRequest = new XElement("Txn", fields);

            return xmlRequest;
        }
    }
}
