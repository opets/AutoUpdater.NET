using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AutoUpdaterDotNET {
	/// <summary>
	/// 
	/// </summary>
	public class WebClientInitializationEventArgs : EventArgs {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="webClient"></param>
		public WebClientInitializationEventArgs( WebClient webClient ) {
			WebClient = webClient;
		}

		/// <summary>
		/// 
		/// </summary>
		public WebClient WebClient { get; set; }

	}
}
