using System;
using System.IO;
using System.Linq;
using System.Text;

#if !Mini
namespace CDPlayer
{
    /// <summary>
    /// A resolvable station source: recognizes its own share links and turns them
    /// into a direct stream URL. Add new radio services by implementing this and
    /// registering in StationResolver.Sources.
    /// </summary>
    internal interface IStationSource
    {
        string Name { get; }
        bool CanResolve(string input);
        StationResolver.Result Resolve(string input, out string streamUrl);
    }

    /// <summary>
    /// Turns user input into the real stream URL.
    /// Direct http:// and https:// URLs are the first-class default and always work
    /// (https is played through the local proxy). Share links from known services
    /// (radio.garden, TuneIn) are detected case by case and resolved by their source.
    /// </summary>
    internal static class StationResolver
    {
        public enum Result { Ok, Invalid, NetworkFailed }

        internal static readonly IStationSource[] Sources =
        {
            new RadioGardenSource(),
            new TuneInSource(),
        };

        /// <summary>True when a registered source recognizes this input as its share link
        /// (i.e. resolution is needed and the result is worth caching).</summary>
        public static bool IsResolvableLink(string input)
        {
            input = (input ?? string.Empty).Trim();
            foreach (IStationSource s in Sources)
                if (s.CanResolve(input)) return true;
            return false;
        }

        public static Result Resolve(string input, out string streamUrl)
        {
            streamUrl = null;
            input = (input ?? string.Empty).Trim();

            foreach (IStationSource s in Sources)
                if (s.CanResolve(input))
                    return s.Resolve(input, out streamUrl);

            // no source claimed it - direct URLs pass through untouched
            if (input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                input.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                streamUrl = input;
                return Result.Ok;
            }
            return Result.Invalid;
        }

        /// <summary>
        /// Briefly connects to the stream (following redirects) and returns its Content-Type,
        /// or null if unknown/unreachable. ModAudioStream only decodes audio/mpeg.
        /// </summary>
        public static string ProbeContentType(string url)
        {
            try
            {
                string finalUrl;
                using (TlsWeb w = TlsWeb.OpenFollow(url, null, 4000, 3, out finalUrl))
                {
                    return w.ContentType;
                }
            }
            catch { return null; }
        }

        public static bool IsPlayableFormat(string contentType)
        {
            if (string.IsNullOrEmpty(contentType)) return true; // unknown - let playback try
            contentType = contentType.ToLowerInvariant();
            return contentType.Contains("audio/mpeg") || contentType.Contains("audio/mp3")
                || contentType.Contains("audio/x-mpeg");
        }
    }

    /// <summary>
    /// radio.garden share link (https://radio.garden/listen/{slug}/{channelId})
    /// -> listen API 302 -> Location header is the station's stream URL.
    /// </summary>
    internal class RadioGardenSource : IStationSource
    {
        public string Name => "Radio Garden";

        private const string ListenApi = "https://radio.garden/api/ara/content/listen/{0}/channel.mp3";

        public bool CanResolve(string input) => input.Contains("radio.garden");

        public StationResolver.Result Resolve(string input, out string streamUrl)
        {
            streamUrl = null;
            string channelId = ExtractChannelId(input);
            if (channelId == null) return StationResolver.Result.Invalid;

            try
            {
                // one hop is enough - the Location of the first 302 is the station's stream URL
                using (TlsWeb w = TlsWeb.Open(string.Format(ListenApi, channelId), null, 5000))
                {
                    if (w.StatusCode >= 300 && w.StatusCode < 400 && !string.IsNullOrEmpty(w.Location))
                    {
                        streamUrl = w.Location;
                        return StationResolver.Result.Ok;
                    }
                    return StationResolver.Result.Invalid;
                }
            }
            catch (Exception)
            {
                return StationResolver.Result.NetworkFailed;
            }
        }

        internal static string ExtractChannelId(string link)
        {
            try
            {
                Uri uri = new Uri(link);
                string[] segments = uri.AbsolutePath.Trim('/').Split('/');
                if (segments.Length >= 2 && segments[0] == "listen")
                    return segments[segments.Length - 1];
                return null;
            }
            catch { return null; }
        }
    }

    /// <summary>
    /// TuneIn share link (https://tunein.com/radio/Some-Station-s12345/)
    /// -> Tune.ashx API returns one stream URL per line for the station id.
    /// The API answers on plain http, so no TLS involved at resolve time.
    /// </summary>
    internal class TuneInSource : IStationSource
    {
        public string Name => "TuneIn";

        private const string TuneApi = "http://opml.radiotime.com/Tune.ashx?id={0}&formats=mp3";

        public bool CanResolve(string input) =>
            input.Contains("tunein.com");

        public StationResolver.Result Resolve(string input, out string streamUrl)
        {
            streamUrl = null;
            string stationId = ExtractStationId(input);
            if (stationId == null) return StationResolver.Result.Invalid;

            try
            {
                using (TlsWeb w = TlsWeb.Open(string.Format(TuneApi, stationId), null, 5000))
                {
                    if (w.StatusCode != 200) return StationResolver.Result.Invalid;
                    // body: plain text, one stream URL per line; take the first
                    string body = ReadSome(w.Body, 4096);
                    foreach (string line in body.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string candidate = line.Trim();
                        if (candidate.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                            candidate.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        {
                            streamUrl = candidate;
                            return StationResolver.Result.Ok;
                        }
                    }
                    return StationResolver.Result.Invalid;
                }
            }
            catch (Exception)
            {
                return StationResolver.Result.NetworkFailed;
            }
        }

        /// <summary>Station id is the trailing "-s12345" segment of the share link path.</summary>
        internal static string ExtractStationId(string link)
        {
            try
            {
                Uri uri = new Uri(link);
                string[] segments = uri.AbsolutePath.Trim('/').Split('/');
                if (segments.Length == 0) return null;
                string last = segments[segments.Length - 1];
                int dash = last.LastIndexOf("-s", StringComparison.OrdinalIgnoreCase);
                if (dash < 0 || dash + 2 >= last.Length) return null;
                string id = last.Substring(dash + 1); // "s12345"
                for (int i = 1; i < id.Length; i++)
                    if (!char.IsDigit(id[i])) return null;
                return id;
            }
            catch { return null; }
        }

        private static string ReadSome(Stream s, int maxBytes)
        {
            byte[] buf = new byte[maxBytes];
            int total = 0;
            while (total < maxBytes)
            {
                int n = s.Read(buf, total, maxBytes - total);
                if (n <= 0) break;
                total += n;
            }
            return Encoding.UTF8.GetString(buf, 0, total);
        }
    }
}
#endif