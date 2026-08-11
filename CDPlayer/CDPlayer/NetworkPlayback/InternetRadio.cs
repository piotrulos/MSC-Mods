using MSCLoader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CDPlayer;

#if !Mini
/// <summary>
/// Radio Garden support for the internet radio channels (3 and 4).
/// Channel textboxes accept: direct http:// or https:// stream URLs (first-class default)
/// and share links from registered sources (radio.garden, TuneIn - see StationResolver.Sources).
///
/// Two resolution modes:
///   ResolveAuto()  - menu load: cache-first, network only for uncached share links.
///   ResolveForce() - "Resolve stations" button: always re-resolves and overwrites the cache.
/// </summary>
internal static class InternetRadio
{
    internal static SettingsText ch3Status, ch4Status;

    private static Mod mod;

    private class CacheEntry
    {
        public string Url;      // resolved stream url
        public string Format;   // probed Content-Type (null = unknown)
    }
    // key: radio.garden share link exactly as entered
    private static Dictionary<string, CacheEntry> cache = new Dictionary<string, CacheEntry>();
    // session-only probed formats for direct URLs (not persisted)
    private static readonly Dictionary<string, string> sessionFormats = new Dictionary<string, string>();

    private static string CacheFile => Path.Combine(ModLoader.GetModSettingsFolder(mod), "internetradio_cache.json");

    public static void Init(Mod owner)
    {
        mod = owner;
        LoadCache();
    }

    /// <summary>Menu-load resolution: uses cache, touches the network only for uncached share links.</summary>
    public static void ResolveAuto()
    {
        ResolveChannel(CDPlayer.channel3url, ch3Status, force: false);
        ResolveChannel(CDPlayer.channel4url, ch4Status, force: false);
        SaveCache();
    }

    /// <summary>Button resolution: ignores cache, re-resolves everything, replaces cache entries.</summary>
    public static void ResolveForce()
    {
        ResolveChannel(CDPlayer.channel3url, ch3Status, force: true);
        ResolveChannel(CDPlayer.channel4url, ch4Status, force: true);
        SaveCache();
    }

    private static void ResolveChannel(SettingsTextBox box, SettingsText status, bool force)
    {
        if (box == null) return;
        string input = (box.GetValue() ?? string.Empty).Trim();

        if (input.Length == 0)
        {
            SetStatus(status, "<color=grey>empty - paste a stream URL or a radio.garden share link</color>");
            return;
        }

        bool isShareLink = StationResolver.IsResolvableLink(input);

        // cache hit on auto resolution - no network at all
        if (isShareLink && !force && cache.TryGetValue(input, out CacheEntry hit))
        {
            ShowReady(status, hit.Url, hit.Format, cached: true);
            return;
        }

        // direct URLs: nothing to resolve; probe format only when the user explicitly asks
        if (!isShareLink)
        {
            if (!input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !input.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                SetStatus(status, "<color=red>invalid link - expected http(s):// stream URL, a radio.garden share link or a tunein.com station link</color>");
                return;
            }
            if (force)
                sessionFormats[input] = StationResolver.ProbeContentType(input);
            sessionFormats.TryGetValue(input, out string knownFmt);
            if (!StationResolver.IsPlayableFormat(knownFmt)) { ShowUnsupported(status, knownFmt); return; }
            ShowReady(status, input, knownFmt, cached: false);
            return;
        }

        // share link, network resolution (auto cache-miss or forced)
        StationResolver.Result r = StationResolver.Resolve(input, out string url);
        switch (r)
        {
            case StationResolver.Result.Ok:
                string fmt = StationResolver.ProbeContentType(url);
                cache[input] = new CacheEntry { Url = url, Format = fmt };
                if (!StationResolver.IsPlayableFormat(fmt)) { ShowUnsupported(status, fmt); return; }
                ShowReady(status, url, fmt, cached: false);
                break;

            case StationResolver.Result.NetworkFailed:
                if (cache.TryGetValue(input, out CacheEntry old))
                    SetStatus(status, "<color=yellow>station service unreachable - using previously resolved:</color> " + old.Url);
                else
                    SetStatus(status, "<color=red>could not reach the station service - check your internet connection, then press 'Resolve stations'.</color>");
                break;

            default:
                SetStatus(status, "<color=red>invalid link - expected http(s):// stream URL, a radio.garden share link or a tunein.com station link</color>");
                break;
        }
    }

    /// <summary>
    /// Playable URL for a streaming channel (1 = channel 3 box, 2 = channel 4 box).
    /// Never blocks on network: direct URLs and cached resolutions only.
    /// Returns null when nothing playable is configured (caller falls back to static).
    /// </summary>
    public static string GetChannelUrl(int channel)
    {
        SettingsTextBox box = channel == 1 ? CDPlayer.channel3url : CDPlayer.channel4url;
        if (box == null) return null;
        string input = (box.GetValue() ?? string.Empty).Trim();
        if (input.Length == 0) return null;

        string url, fmt;
        if (StationResolver.IsResolvableLink(input))
        {
            if (!cache.TryGetValue(input, out CacheEntry entry))
            {
                ModConsole.Warning("[CDPlayer:InternetRadio] Channel " + (channel + 2) + " share link is not resolved yet - open Settings > CD Player Enhanced and press 'Resolve stations'.");
                return null;
            }
            url = entry.Url;
            fmt = entry.Format;
        }
        else
        {
            url = input;
            sessionFormats.TryGetValue(input, out fmt);
        }

        if (!StationResolver.IsPlayableFormat(fmt))
        {
            ModConsole.Warning("[CDPlayer:InternetRadio] Channel " + (channel + 2) + " streams " + fmt + " - only MP3 (audio/mpeg) is supported.");
            return null;
        }

        // Mono has no TLS 1.2 - https goes through the loopback proxy
        if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return LocalStreamProxy.MapSlot(channel, url);
        return url;
    }

    // ---- status helpers ----
    private static void ShowReady(SettingsText status, string url, string fmt, bool cached)
    {
        bool proxied = url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        SetStatus(status, "<color=lime>ready</color> <color=grey>[" + (fmt ?? "unknown format") + "]</color>"
                          + (cached ? " <color=grey>(cached)</color>" : "")
                          + (proxied ? " <color=aqua>(via local proxy)</color> " : " ") + url);
    }

    private static void ShowUnsupported(SettingsText status, string fmt)
    {
        SetStatus(status, "<color=orange>unsupported format [" + fmt + "] - only MP3 (audio/mpeg) streams can be played. " +
                          "Pick another station or paste the station's MP3 stream URL.</color>");
    }

    private static void SetStatus(SettingsText status, string text)
    {
        if (status != null) status.SetValue(text);
        if (CDPlayer.debugInfo != null && CDPlayer.debugInfo.GetValue())
            ModConsole.Print("[CDPlayer:InternetRadio] " + text);
    }

    // ---- cache persistence (JSON, no delimiter pitfalls) ----
    private static void LoadCache()
    {
        try
        {
            if (!File.Exists(CacheFile)) return;
            cache = JsonConvert.DeserializeObject<Dictionary<string, CacheEntry>>(File.ReadAllText(CacheFile))
                    ?? new Dictionary<string, CacheEntry>();
        }
        catch (Exception e)
        {
            cache = new Dictionary<string, CacheEntry>();
            ModConsole.Warning("[CDPlayer:InternetRadio] Cache load failed (will be rebuilt): " + e.Message);
        }
    }

    private static void SaveCache()
    {
        try
        {
            // keep only entries for links currently entered in the channel boxes
            List<string> current = new List<string>();
            if (CDPlayer.channel3url != null) current.Add((CDPlayer.channel3url.GetValue() ?? string.Empty).Trim());
            if (CDPlayer.channel4url != null) current.Add((CDPlayer.channel4url.GetValue() ?? string.Empty).Trim());

            Dictionary<string, CacheEntry> pruned = new Dictionary<string, CacheEntry>();
            foreach (KeyValuePair<string, CacheEntry> kv in cache)
                if (current.Contains(kv.Key)) pruned[kv.Key] = kv.Value;
            cache = pruned;

            File.WriteAllText(CacheFile, JsonConvert.SerializeObject(cache, Formatting.Indented));
        }
        catch (Exception e) { ModConsole.Warning("[CDPlayer:InternetRadio] Cache save failed: " + e.Message); }
    }
}
#endif