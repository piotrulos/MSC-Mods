using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MSCLoader;

#if !Mini
namespace CDPlayer
{
    /// <summary>
    /// Tiny HTTP proxy on 127.0.0.1. ModAudioStream (Mono, no TLS 1.2) connects to
    /// http://127.0.0.1:{port}/s{slot}; we fetch the real https stream through
    /// BouncyCastle TLS (TlsWeb) and pipe bytes back verbatim, ICY headers included.
    /// </summary>
    internal static class LocalStreamProxy
    {
        private static TcpListener listener;
        private static Thread acceptThread;
        private static volatile bool running;
        private static readonly Dictionary<int, string> slotTargets = new Dictionary<int, string>();
        private static readonly object gate = new object();
        public static int Port { get; private set; }

        public static string MapSlot(int slot, string targetUrl)
        {
            EnsureStarted();
            lock (gate) slotTargets[slot] = targetUrl;
            return "http://127.0.0.1:" + Port + "/s" + slot;
        }

        private static void EnsureStarted()
        {
            if (running) return;
            listener = new TcpListener(IPAddress.Loopback, 0);   // OS picks a free port
            listener.Start();
            Port = ((IPEndPoint)listener.LocalEndpoint).Port;
            running = true;
            acceptThread = new Thread(AcceptLoop);
            acceptThread.IsBackground = true;                    // dies with the game process
            acceptThread.Start();
            ModConsole.Print("[CDPlayer:InternetRadio] Local proxy on 127.0.0.1:" + Port);
        }

        public static void Stop()
        {
            running = false;
            try { if (listener != null) listener.Stop(); } catch { }
        }

        private static void AcceptLoop()
        {
            while (running)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Thread t = new Thread(delegate () { Handle(client); });
                    t.IsBackground = true;
                    t.Start();
                }
                catch { if (!running) return; }
            }
        }

        private static void Handle(TcpClient client)
        {
            TlsWeb upstream = null;
            try
            {
                client.ReceiveTimeout = 10000;
                client.SendTimeout = 15000;
                NetworkStream cs = client.GetStream();

                // read client request head
                string reqHead = ReadHead(cs);
                if (reqHead == null) return;
                string[] lines = reqHead.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                // "GET /s0 HTTP/1.1"
                string[] reqLine = lines[0].Split(' ');
                if (reqLine.Length < 2) return;
                int slot = ParseSlot(reqLine[1]);

                string target;
                lock (gate)
                {
                    if (slot < 0 || !slotTargets.TryGetValue(slot, out target))
                    {
                        byte[] nf = Encoding.ASCII.GetBytes("HTTP/1.1 404 Not Found\r\nConnection: close\r\n\r\n");
                        cs.Write(nf, 0, nf.Length);
                        return;
                    }
                }

                // forward Icy-MetaData request header if the player sent it
                List<string> extra = new List<string>();
                foreach (string l in lines)
                    if (l.StartsWith("Icy-MetaData", StringComparison.OrdinalIgnoreCase)) extra.Add(l);

                string finalUrl;
                upstream = TlsWeb.OpenFollow(target, extra.ToArray(), 15000, 5, out finalUrl);

                // replay upstream status+headers verbatim, then pump the body
                cs.Write(upstream.RawHead, 0, upstream.RawHead.Length);
                byte[] buf = new byte[16 * 1024];
                int n;
                while (running && (n = upstream.Body.Read(buf, 0, buf.Length)) > 0)
                    cs.Write(buf, 0, n);
            }
            catch (Exception e)
            {
                if (CDPlayer.debugInfo != null && CDPlayer.debugInfo.GetValue())
                    ModConsole.Warning("[CDPlayer:InternetRadio] Proxy connection ended: " + e.Message);
            }
            finally
            {
                if (upstream != null) upstream.Dispose();
                try { client.Close(); } catch { }
            }
        }

        private static int ParseSlot(string path)
        {
            // "/s0", "/s1"...
            if (path.Length >= 3 && path[0] == '/' && path[1] == 's')
            {
                int slot;
                if (int.TryParse(path.Substring(2), out slot)) return slot;
            }
            return -1;
        }

        private static string ReadHead(Stream s)
        {
            MemoryStream head = new MemoryStream();
            int matched = 0;
            while (matched < 4)
            {
                int b;
                try { b = s.ReadByte(); } catch { return null; }
                if (b < 0) return null;
                head.WriteByte((byte)b);
                if ((matched % 2 == 0 && b == '\r') || (matched % 2 == 1 && b == '\n')) matched++;
                else matched = (b == '\r') ? 1 : 0;
                if (head.Length > 16 * 1024) return null;
            }
            return Encoding.ASCII.GetString(head.ToArray());
        }
    }
}
#endif