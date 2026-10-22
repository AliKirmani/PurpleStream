using System.Globalization;
using System.Text.RegularExpressions;

namespace PurpleStream;

record Country(string Code, string Name, string Urdu = "")
{
    public override string ToString() => Urdu.Length > 0 ? $"{Name} / {Urdu}" : Name;
}
record ChannelOverride(string Country = "", string Genre = "");
record ChannelLocation(Country Country, string Genre);
record DirectorySelection(string Country = "", string Genre = "");

static class ChannelDirectory
{
    public const string Unknown = "??";
    public const string International = "INT";
    public static readonly string[] Genres = { "News / نیوز", "Sports / کھیل", "Music / موسیقی", "Drama / ڈرامہ", "Movies / فلمیں", "Kids / بچے", "Religion / مذہبی", "Documentary / دستاویزی", "Food / کھانا", "Entertainment / تفریح", "Other / غیرمتعین" };
    public static readonly Dictionary<string, Country> Countries = BuildCountries();
    static readonly Dictionary<string, string[]> Aliases = new()
    {
        ["PK"] = new[] { "Pakistan", "Pakistani", "پاکستان", "PAK" },
        ["IN"] = new[] { "India", "Indian", "بھارت", "IND" },
        ["GB"] = new[] { "United Kingdom", "Britain", "British", "UK", "U.K.", "England", "Scotland", "Wales", "برطانیہ" },
        ["US"] = new[] { "United States", "USA", "U.S.A.", "امریکہ" },
        ["AE"] = new[] { "United Arab Emirates", "UAE", "Emirates", "امارات" },
        ["SA"] = new[] { "Saudi Arabia", "Saudi", "KSA", "سعودی" },
        ["BD"] = new[] { "Bangladesh", "Bangladeshi", "بنگلہ دیش", "BAN" },
        ["AF"] = new[] { "Afghanistan", "Afghan", "افغانستان", "AFG" },
        ["LK"] = new[] { "Sri Lanka", "Sri Lankan", "Srilanka", "سری لنکا" },
        ["NP"] = new[] { "Nepal", "Nepali", "نیپال" },
        ["TR"] = new[] { "Turkey", "Turkiye", "Türkiye", "Turkish", "ترکی" },
        ["IR"] = new[] { "Iran", "Iranian", "ایران" },
        ["CN"] = new[] { "China", "Chinese", "چین" },
        ["KR"] = new[] { "South Korea", "Korea Republic", "Korean" },
        ["KP"] = new[] { "North Korea" },
        ["RU"] = new[] { "Russia", "Russian", "روس" },
        ["DE"] = new[] { "Germany", "German", "Deutschland", "جرمنی" },
        ["FR"] = new[] { "France", "French", "فرانس" },
        ["IT"] = new[] { "Italy", "Italian", "Italia", "اٹلی" },
        ["ES"] = new[] { "Spain", "Spanish", "Espana", "España", "اسپین" },
        ["PT"] = new[] { "Portugal", "Portuguese" },
        ["CA"] = new[] { "Canada", "Canadian", "کینیڈا" },
        ["AU"] = new[] { "Australia", "Australian", "آسٹریلیا" },
        ["NZ"] = new[] { "New Zealand", "نیوزی لینڈ" },
        ["ZA"] = new[] { "South Africa", "South African", "جنوبی افریقہ" },
        ["EG"] = new[] { "Egypt", "Egyptian", "مصر" },
        ["MA"] = new[] { "Morocco", "Moroccan", "Maroc", "مراکش" },
        ["DZ"] = new[] { "Algeria", "Algerian", "الجزائر" },
        ["NL"] = new[] { "Netherlands", "Dutch", "Holland" },
        ["IE"] = new[] { "Ireland", "Irish" },
        ["BE"] = new[] { "Belgium", "Belgian" },
        ["CH"] = new[] { "Switzerland", "Swiss" },
        ["SE"] = new[] { "Sweden", "Swedish" },
        ["NO"] = new[] { "Norway", "Norwegian" },
        ["DK"] = new[] { "Denmark", "Danish" },
        ["FI"] = new[] { "Finland", "Finnish" },
        ["PL"] = new[] { "Poland", "Polish" },
        ["RO"] = new[] { "Romania", "Romanian" },
        ["GR"] = new[] { "Greece", "Greek" },
        ["BR"] = new[] { "Brazil", "Brazilian", "Brasil" },
        ["MX"] = new[] { "Mexico", "Mexican", "México" },
        ["AR"] = new[] { "Argentina", "Argentinian" },
        ["PH"] = new[] { "Philippines", "Filipino" },
        ["MY"] = new[] { "Malaysia", "Malaysian" },
        ["ID"] = new[] { "Indonesia", "Indonesian" },
        ["TH"] = new[] { "Thailand", "Thai" },
        ["VN"] = new[] { "Vietnam", "Vietnamese", "Viet Nam" },
        ["CZ"] = new[] { "Czech Republic", "Czechia", "Czech" },
        ["PS"] = new[] { "Palestine", "Palestinian", "فلسطین" },
        ["XK"] = new[] { "Kosovo" }
    };

    static Dictionary<string, Country> BuildCountries()
    {
        var result = new Dictionary<string, Country>(StringComparer.OrdinalIgnoreCase);
        // Enumerating regions also supports countries with no dedicated UI culture.
        for (char a = 'A'; a <= 'Z'; a++) for (char b = 'A'; b <= 'Z'; b++)
        {
            try { var region = new RegionInfo($"{a}{b}"); var code = region.TwoLetterISORegionName; if (code.Length == 2) result[code] = new(code, region.EnglishName); }
            catch (ArgumentException) { }
        }
        foreach (var (code, english, urdu) in new[] {
            ("PK","Pakistan","پاکستان"),("IN","India","بھارت"),("GB","United Kingdom","برطانیہ"),("US","United States","امریکہ"),
            ("AE","United Arab Emirates","امارات"),("SA","Saudi Arabia","سعودی عرب"),("BD","Bangladesh","بنگلہ دیش"),
            ("AF","Afghanistan","افغانستان"),("LK","Sri Lanka","سری لنکا"),("NP","Nepal","نیپال"),("TR","Türkiye","ترکی"),
            ("IR","Iran","ایران"),("IQ","Iraq","عراق"),("CN","China","چین"),("JP","Japan","جاپان"),("CA","Canada","کینیڈا"),
            ("AU","Australia","آسٹریلیا"),("NZ","New Zealand","نیوزی لینڈ"),("ZA","South Africa","جنوبی افریقہ"),
            ("EG","Egypt","مصر"),("QA","Qatar","قطر"),("KW","Kuwait","کویت"),("OM","Oman","عمان"),("BH","Bahrain","بحرین"),
            ("DE","Germany","جرمنی"),("FR","France","فرانس"),("IT","Italy","اٹلی"),("ES","Spain","اسپین"),("RU","Russia","روس"),
            ("PS","Palestine","فلسطین"),("XK","Kosovo",""),(International,"International / Mixed","بین الاقوامی"),(Unknown,"Unidentified country","نامعلوم ملک")
        }) result[code] = new(code, english, urdu);
        return result;
    }

    static readonly Dictionary<string, string> NameLookup = BuildNameLookup();
    static readonly Regex CountryNames = new(@"(?<![\p{L}\p{N}])(?:" + string.Join("|", NameLookup.Keys.OrderByDescending(x => x.Length).Select(Regex.Escape)) + @")(?![\p{L}\p{N}])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    static Dictionary<string, string> BuildNameLookup()
    {
        var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in Countries.Values.Where(c => c.Code.Length == 2)) { lookup[c.Name] = c.Code; if (c.Urdu.Length > 0) lookup[c.Urdu] = c.Code; }
        foreach (var pair in Aliases) foreach (var alias in pair.Value) lookup[alias] = pair.Key;
        return lookup;
    }
    static string? FullCountry(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var found = new HashSet<string>();
        foreach (Match match in CountryNames.Matches(value)) found.Add(NameLookup[match.Value]);
        return found.Count == 1 ? found.First() : found.Count > 1 ? International : null;
    }

    static string? PrefixCountry(string value)
    {
        var match = Regex.Match(value.Trim(), @"^(?:\[|\(|\{)?([A-Za-z]{2})(?:\]|\)|\})?(?=[\s:|_\-]|$)");
        if (match.Success && Countries.ContainsKey(match.Groups[1].Value)) return match.Groups[1].Value.ToUpperInvariant();
        return null;
    }
    public static string FindCountry(Channel channel)
    {
        if (!string.IsNullOrWhiteSpace(channel.CountryHint))
        {
            var hints = channel.CountryHint.Split(new[] { ';', ',', '/', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (hints.Length == 1 && Countries.ContainsKey(hints[0])) return hints[0].ToUpperInvariant();
            var explicitCountry = FullCountry(channel.CountryHint);
            if (explicitCountry != null) return explicitCountry;
            if (hints.Count(h => Countries.ContainsKey(h)) > 1) return International;
        }
        var group = FullCountry(channel.Group) ?? PrefixCountry(channel.Group);
        if (group != null) return group;
        var name = FullCountry(channel.Name) ?? PrefixCountry(channel.Name);
        if (name != null) return name;
        if (Regex.IsMatch(channel.Name, @"\b(PTV|ARY|Dunya|Samaa|Hum\s+(TV|News|Masala|Sitaray)|Geo\s+(News|Entertainment|Super|Kahani)|Har\s*Pal\s*Geo|Express\s*Entertainment|92\s*News|Dawn\s*News|Madani\s*(TV|Channel))\b", RegexOptions.IgnoreCase)) return "PK";
        if (Regex.IsMatch(channel.Group, @"\b(international|worldwide|global|multi[ -]?country)\b", RegexOptions.IgnoreCase)) return International;
        return Unknown;
    }

    static string? GenreIn(string text)
    {
        var rules = new (int Genre, string Pattern)[] {
            (1,@"\b(sports?|cricket|football|soccer|tennis|formula\s*1|f1|racing|wwe|espn|bein|supersport|willow|geo\s*super)\b|کھیل|اسپورٹس"),
            (0,@"\b(news|cnn|al\s*jazeera|dunya|samaa|dawn\s*news)\b|نیوز|خبریں"),
            (2,@"\b(music|musical|musik|muzik|mtv|vh1|9xm|9x\s*m|ary\s*muzik)\b|موسیقی|میوزک|گانے"),
            (5,@"\b(kids?|cartoons?|nick(elodeon)?|disney\s*junior|baby\s*tv|pogo|cbeebies)\b|بچوں|کارٹون"),
            (6,@"\b(religio(n|us)|islam(ic)?|quran|qur'an|madani|makkah|madina|peace\s*tv|qtv|christian|devotional)\b|مذہبی|اسلامی|قرآن"),
            (8,@"\b(food|cook(ing)?|masala|chef|kitchen)\b|کھانا|پکوان"),
            (3,@"\b(dramas?|serials?|ary\s*(digital|zindagi)|hum\s*(tv|sitaray)|geo\s*(kahani|entertainment)|har\s*pal\s*geo|express\s*entertainment)\b|ڈرامہ|ڈرامے"),
            (4,@"\b(movies?|cinema|films?|hollywood|bollywood)\b|فلم"),
            (7,@"\b(documentar(y|ies)|discovery|national\s*geographic|nat\s*geo|animal\s*planet|history|science|nature|wildlife)\b|دستاویزی"),
            (9,@"\b(entertainment|general|variety|comedy)\b|تفریح|مزاح")
        };
        foreach (var rule in rules) if (Regex.IsMatch(text, rule.Pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) return Genres[rule.Genre];
        return null;
    }
    public static ChannelLocation Locate(Channel channel, IReadOnlyDictionary<string, ChannelOverride>? overrides = null)
    {
        var country = FindCountry(channel);
        var genre = GenreIn(channel.Name) ?? GenreIn(channel.Group) ?? Genres[^1];
        if (overrides != null && overrides.TryGetValue(channel.Url, out var custom))
        {
            if (Countries.ContainsKey(custom.Country)) country = custom.Country;
            if (Genres.Contains(custom.Genre)) genre = custom.Genre;
        }
        return new(Countries[country], genre);
    }
}

static class DirectoryTests
{
    public static void Run()
    {
        void Check(string name, string group, string code, int genre, string hint = "")
        {
            var found = ChannelDirectory.Locate(new(name, "https://fixture.invalid/" + name, group, CountryHint: hint));
            if (found.Country.Code != code || found.Genre != ChannelDirectory.Genres[genre]) throw new Exception($"Classification failed: {name}: {found}");
        }
        Check("Geo News HD", "123", "PK", 0);
        Check("ARY Digital", "Pakistan Entertainment", "PK", 3);
        Check("Geo Entertainment", "123", "PK", 3);
        Check("Geo Super", "123", "PK", 1);
        Check("PTV Sports", "PK: General", "PK", 1);
        Check("ARY Musik", "Pakistan", "PK", 2);
        Check("HUM Masala", "Pakistan", "PK", 8);
        Check("Example Music", "India Music", "IN", 2);
        Check("BBC News", "UK News", "GB", 0);
        Check("ESPN", "US: Sports", "US", 1);
        Check("Unknown Feed", "123", ChannelDirectory.Unknown, 10);
        Check("International Music", "Global Music", ChannelDirectory.International, 2);
        Check("Music Box", "Other", "PK", 2, "PK");
        Check("World News", "Other", ChannelDirectory.International, 0, "PK;IN");
        Check("Music Box", "Pakistan", "JP", 2, "JP");
        Check("Documentary", "France", "FR", 7);
        Check("News", "ZA | News", "ZA", 0);
        var channel = new Channel("Unknown feed", "https://fixture.invalid/manual", "123");
        var custom = new Dictionary<string, ChannelOverride> { [channel.Url] = new("PK", ChannelDirectory.Genres[2]) };
        var located = ChannelDirectory.Locate(channel, custom);
        if (located.Country.Code != "PK" || located.Genre != ChannelDirectory.Genres[2]) throw new Exception("Manual correction failed");
        var saved = System.Text.Json.JsonSerializer.Deserialize<Saved>(System.Text.Json.JsonSerializer.Serialize(new Saved { Organization = custom }))!;
        if (ChannelDirectory.Locate(channel, saved.Organization) != located) throw new Exception("Correction persistence failed");
        if (ChannelDirectory.Countries.Count < 190) throw new Exception("Global country support incomplete");
        var m3u = Playlist.Parse("#EXTM3U\n#EXTINF:-1 tvg-country=\"PK\" group-title=\"Music\",Music Box\nhttps://fixture.invalid/test");
        if (m3u.Count != 1 || ChannelDirectory.Locate(m3u[0]).Country.Code != "PK") throw new Exception("M3U country metadata failed");
        var comma = Playlist.Parse("#EXTINF:-1 group-title=\"Pakistan, Music\",FM Music\nhttps://fixture.invalid/comma");
        if (comma.Count != 1 || comma[0].Name != "FM Music" || ChannelDirectory.Locate(comma[0]).Genre != ChannelDirectory.Genres[2]) throw new Exception("Quoted group comma corrupted channel name");
        using var flag = FlagIcons.Draw("PK");
        if (flag.Width != 32 || flag.GetPixel(2, 8).ToArgb() != Color.White.ToArgb()) throw new Exception("Pakistan flag failed");
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "directory-test.txt"), "PASS: country/genre classification, provider ID fallback, explicit M3U countries, multi-country feeds, no fabricated country for unknown feeds, manual corrections/settings persistence, worldwide country support, offline Pakistan flag.");
    }
}
