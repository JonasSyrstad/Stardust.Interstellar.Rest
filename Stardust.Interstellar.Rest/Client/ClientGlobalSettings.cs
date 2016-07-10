namespace Stardust.Interstellar.Rest.Client
{
    public static class ClientGlobalSettings
    {
        /// <summary>
        /// Gets or sets the time-out value in milliseconds for the System.Net.HttpWebRequest.GetResponse
        /// and System.Net.HttpWebRequest.GetRequestStream methods.
        /// </summary>
        public static int? Timeout { get; set; }

        /// <summary>
        /// Gets or sets a time-out in milliseconds when writing to or reading from a stream.
        /// </summary>
        public static int? ReadWriteTimeout { get; set; }

        /// <summary>
        /// Gets or sets a timeout, in milliseconds, to wait until the 100-Continue is received
        /// from the server.
        /// </summary>
        public static int? ContinueTimeout { get; set; }

        public static void SetExtendedTimeouts()
        {
            Timeout = 300000;
            ReadWriteTimeout = 3000000;
            ContinueTimeout= 300000;
        }
    }
}