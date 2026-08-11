using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Security;

#if !Mini
namespace CDPlayer
{
    /// <summary>
    /// Minimal HTTP/1.1 client that works around Unity 5 Mono's lack of TLS 1.2:
    /// http:// -> plain TcpClient, https:// -> TcpClient wrapped in BouncyCastle TLS 1.2.
    /// Understands normal "HTTP/1.x" and Shoutcast "ICY" status lines.
    /// Requires BouncyCastle.Crypto.dll (net20 build, e.g. v1.8.9).
    /// </summary>
    internal class TlsWeb : IDisposable
    {
        public int StatusCode;
        public string Location;                 // set on 3xx
        public string ContentType;              // e.g. audio/mpeg, audio/aac
        public byte[] RawHead;                  // status line + headers + blank line, verbatim (for proxy replay)
        public Stream Body;                     // positioned right after headers

        private TcpClient tcp;
        private TlsClientProtocol tlsProtocol;

        public static TlsWeb Open(string url, string[] extraHeaders, int timeoutMs)
        {
            Uri uri = new Uri(url);
            bool https = uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
            int port = uri.Port > 0 ? uri.Port : (https ? 443 : 80);

            TlsWeb w = new TlsWeb();
            w.tcp = new TcpClient();
            w.tcp.SendTimeout = timeoutMs;
            w.tcp.ReceiveTimeout = timeoutMs;
            w.tcp.Connect(uri.Host, port);

            Stream net = w.tcp.GetStream();
            if (https)
            {
                w.tlsProtocol = new TlsClientProtocol(net, new SecureRandom());
                w.tlsProtocol.Connect(new PermissiveTlsClient(uri.Host));
                net = w.tlsProtocol.Stream;
            }

            // request
            StringBuilder req = new StringBuilder();
            req.Append("GET ").Append(uri.PathAndQuery).Append(" HTTP/1.1\r\n");
            req.Append("Host: ").Append(uri.Host).Append("\r\n");
            req.Append("User-Agent: CDPlayerEnhanced-InternetRadio/1.0\r\n");
            req.Append("Connection: close\r\n");
            if (extraHeaders != null)
                foreach (string h in extraHeaders) req.Append(h).Append("\r\n");
            req.Append("\r\n");
            byte[] reqBytes = Encoding.ASCII.GetBytes(req.ToString());
            net.Write(reqBytes, 0, reqBytes.Length);
            net.Flush();

            // read status line + headers byte-by-byte so Body starts exactly after them
            MemoryStream head = new MemoryStream();
            int matched = 0; // looking for \r\n\r\n
            while (matched < 4)
            {
                int b = net.ReadByte();
                if (b < 0) throw new IOException("Connection closed before headers ended");
                head.WriteByte((byte)b);
                if ((matched % 2 == 0 && b == '\r') || (matched % 2 == 1 && b == '\n')) matched++;
                else matched = (b == '\r') ? 1 : 0;
                if (head.Length > 32 * 1024) throw new IOException("Header section too large");
            }
            w.RawHead = head.ToArray();
            string headText = Encoding.ASCII.GetString(w.RawHead);
            ParseHead(headText, out w.StatusCode, out w.Location, out w.ContentType);
            w.Body = net;
            return w;
        }

        /// <summary>Follow up to maxRedirects; returns the final connection and (out) final url.</summary>
        public static TlsWeb OpenFollow(string url, string[] extraHeaders, int timeoutMs, int maxRedirects, out string finalUrl)
        {
            finalUrl = url;
            for (int i = 0; i <= maxRedirects; i++)
            {
                TlsWeb w = Open(finalUrl, extraHeaders, timeoutMs);
                if (w.StatusCode >= 300 && w.StatusCode < 400 && !string.IsNullOrEmpty(w.Location))
                {
                    string loc = w.Location;
                    if (loc.StartsWith("/")) loc = new Uri(new Uri(finalUrl), loc).AbsoluteUri;
                    w.Dispose();
                    finalUrl = loc;
                    continue;
                }
                return w;
            }
            throw new IOException("Too many redirects");
        }

        private static void ParseHead(string headText, out int status, out string location, out string contentType)
        {
            status = 0; location = null; contentType = null;
            string[] lines = headText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) return;
            // "HTTP/1.1 302 Found" or "ICY 200 OK"
            string[] parts = lines[0].Split(' ');
            if (parts.Length >= 2) int.TryParse(parts[1], out status);
            for (int i = 1; i < lines.Length; i++)
            {
                int c = lines[i].IndexOf(':');
                if (c <= 0) continue;
                string name = lines[i].Substring(0, c).Trim();
                string value = lines[i].Substring(c + 1).Trim();
                if (name.Equals("Location", StringComparison.OrdinalIgnoreCase))
                    location = value;
                else if (name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    contentType = value;
            }
        }

        public void Dispose()
        {
            try { if (tlsProtocol != null) tlsProtocol.Close(); } catch { }
            try { if (tcp != null) tcp.Close(); } catch { }
        }

        // ---- BouncyCastle TLS 1.2 client accepting any certificate (game env has no trust store) ----
        private class PermissiveTlsClient : DefaultTlsClient
        {
            private readonly string host;
            public PermissiveTlsClient(string host) { this.host = host; }

            public override IDictionary GetClientExtensions()
            {
                IDictionary ext = base.GetClientExtensions();
                if (ext == null) ext = new Hashtable();
                ArrayList names = new ArrayList();
                names.Add(new ServerName(NameType.host_name, host));       // SNI - required by CDNs (radio.garden)
                TlsExtensionsUtilities.AddServerNameExtension(ext, new ServerNameList(names));
                return ext;
            }

            public override TlsAuthentication GetAuthentication() { return new AcceptAll(); }

            private class AcceptAll : TlsAuthentication
            {
                public void NotifyServerCertificate(Certificate serverCertificate) { /* accept */ }
                public TlsCredentials GetClientCredentials(CertificateRequest certificateRequest) { return null; }
            }
        }
    }
}
#endif