using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Xml;
using System.Xml.Linq;

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
    }
}
