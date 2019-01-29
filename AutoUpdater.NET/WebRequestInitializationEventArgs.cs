using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AutoUpdaterDotNET {
	/// <summary>
	/// 
	/// </summary>
	public class WebRequestInitializationEventArgs : EventArgs {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="webRequest"></param>
		public WebRequestInitializationEventArgs( WebRequest webRequest ) {
			WebRequest = webRequest;
		}

		/// <summary>
		/// 
		/// </summary>
		public WebRequest WebRequest { get; set; }

	}
}
