using System.Drawing.Drawing2D;

namespace PurpleStream;

static class FlagIcons
{
    static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(8) };
    static readonly SemaphoreSlim Downloads = new(6);
    static Color C(string hex) => ColorTranslator.FromHtml(hex);
    public static Bitmap Draw(string code)
    {
        var bitmap = new Bitmap(32, 22);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        void Fill(Color color, float x, float y, float w, float h) { using var b = new SolidBrush(color); g.FillRectangle(b, x, y, w, h); }
        void Disk(Color color, float x, float y, float size) { using var b = new SolidBrush(color); g.FillEllipse(b, x, y, size, size); }
        void Star(Color color, float x, float y, float radius, double rotation = -Math.PI / 2)
        {
            var points = Enumerable.Range(0, 10).Select(i => { var angle = rotation + i * Math.PI / 5; var r = i % 2 == 0 ? radius : radius * .4; return new PointF(x + (float)(r * Math.Cos(angle)), y + (float)(r * Math.Sin(angle))); }).ToArray();
            using var brush = new SolidBrush(color); g.FillPolygon(brush, points);
        }
        void Horizontal(params string[] colors) { for (int i = 0; i < colors.Length; i++) Fill(C(colors[i]), 0, 22f * i / colors.Length, 32, 22f / colors.Length + .5f); }
        void Vertical(params string[] colors) { for (int i = 0; i < colors.Length; i++) Fill(C(colors[i]), 32f * i / colors.Length, 0, 32f / colors.Length + .5f, 22); }
        void Cross(string background, string cross, string? border = null)
        {
            g.Clear(C(background));
            if (border != null) { Fill(C(border), 8, 0, 7, 22); Fill(C(border), 0, 8, 32, 7); }
            Fill(C(cross), 10, 0, 3, 22); Fill(C(cross), 0, 10, 32, 3);
        }
        switch (code.ToUpperInvariant())
        {
            case "GENRE": g.Clear(Color.Transparent); Fill(C("#E1B04C"), 3, 4, 12, 6); Fill(C("#F2C96A"), 3, 8, 26, 12); break;
            case "PK": g.Clear(C("#01411C")); Fill(Color.White, 0, 0, 8, 22); Disk(Color.White, 14, 4, 14); Disk(C("#01411C"), 17, 2, 14); Star(Color.White, 25, 6, 3); break;
            case "IN": Horizontal("#FF9933", "#FFFFFF", "#138808"); using (var pen = new Pen(C("#000080"), .7f)) { g.DrawEllipse(pen, 13, 8, 6, 6); for (int i = 0; i < 24; i++) { var a = Math.PI * i / 12; g.DrawLine(pen, 16, 11, 16 + (float)Math.Cos(a) * 3, 11 + (float)Math.Sin(a) * 3); } } break;
            case "BD": g.Clear(C("#006A4E")); Disk(C("#F42A41"), 10, 4, 14); break;
            case "JP": g.Clear(Color.White); Disk(C("#BC002D"), 10, 5, 12); break;
            case "CN": g.Clear(C("#DE2910")); Star(C("#FFDE00"), 7, 6, 4); foreach (var p in new[] { new Point(13, 2), new Point(15, 5), new Point(15, 9), new Point(13, 12) }) Star(C("#FFDE00"), p.X, p.Y, 1.5f); break;
            case "TR": g.Clear(C("#E30A17")); Disk(Color.White, 8, 4, 14); Disk(C("#E30A17"), 11, 5, 12); Star(Color.White, 23, 11, 4); break;
            case "AE": Horizontal("#00732F", "#FFFFFF", "#000000"); Fill(C("#FF0000"), 0, 0, 8, 22); break;
            case "QA": g.Clear(C("#8A1538")); Fill(Color.White, 0, 0, 8, 22); for (int i = 0; i < 9; i++) { using var b = new SolidBrush(Color.White); g.FillPolygon(b, new[] { new PointF(8, i * 22f / 9), new PointF(12, (i + .5f) * 22 / 9), new PointF(8, (i + 1) * 22f / 9) }); } break;
            case "BH": g.Clear(C("#CE1126")); Fill(Color.White, 0, 0, 8, 22); for (int i = 0; i < 5; i++) { using var b = new SolidBrush(Color.White); g.FillPolygon(b, new[] { new PointF(8, i * 22f / 5), new PointF(13, (i + .5f) * 22 / 5), new PointF(8, (i + 1) * 22f / 5) }); } break;
            case "FR": Vertical("#002395", "#FFFFFF", "#ED2939"); break;
            case "IT": Vertical("#009246", "#FFFFFF", "#CE2B37"); break;
            case "IE": Vertical("#169B62", "#FFFFFF", "#FF883E"); break;
            case "BE": Vertical("#000000", "#FDDA24", "#EF3340"); break;
            case "RO": Vertical("#002B7F", "#FCD116", "#CE1126"); break;
            case "NG": Vertical("#008751", "#FFFFFF", "#008751"); break;
            case "DE": Horizontal("#000000", "#DD0000", "#FFCC00"); break;
            case "NL": Horizontal("#AE1C28", "#FFFFFF", "#21468B"); break;
            case "RU": Horizontal("#FFFFFF", "#0039A6", "#D52B1E"); break;
            case "UA": Horizontal("#0057B7", "#FFD700"); break;
            case "PL": Horizontal("#FFFFFF", "#DC143C"); break;
            case "ID": case "MC": Horizontal("#FF0000", "#FFFFFF"); break;
            case "AT": Horizontal("#ED2939", "#FFFFFF", "#ED2939"); break;
            case "HU": Horizontal("#CE2939", "#FFFFFF", "#477050"); break;
            case "TH": Horizontal("#A51931", "#F4F5F8", "#2D2A4A", "#2D2A4A", "#F4F5F8", "#A51931"); break;
            case "VN": g.Clear(C("#DA251D")); Star(C("#FFFF00"), 16, 11, 7); break;
            case "SE": Cross("#006AA7", "#FECC00"); break;
            case "FI": Cross("#FFFFFF", "#003580"); break;
            case "DK": Cross("#C8102E", "#FFFFFF"); break;
            case "NO": Cross("#BA0C2F", "#00205B", "#FFFFFF"); break;
            case "IS": Cross("#02529C", "#DC1E35", "#FFFFFF"); break;
            case "CH": g.Clear(C("#FF0000")); Fill(Color.White, 13, 4, 6, 14); Fill(Color.White, 9, 8, 14, 6); break;
            case "IL": g.Clear(Color.White); Fill(C("#0038B8"), 0, 3, 32, 3); Fill(C("#0038B8"), 0, 16, 32, 3); using (var p = new Pen(C("#0038B8"), 1)) { g.DrawPolygon(p, new[] { new Point(16, 6), new Point(11, 14), new Point(21, 14) }); g.DrawPolygon(p, new[] { new Point(16, 16), new Point(11, 8), new Point(21, 8) }); } break;
            case "PS": Horizontal("#000000", "#FFFFFF", "#007A3D"); using (var b = new SolidBrush(C("#CE1126"))) g.FillPolygon(b, new[] { new Point(0, 0), new Point(14, 11), new Point(0, 22) }); break;
            case "US": for (int i = 0; i < 13; i++) Fill(i % 2 == 0 ? C("#B22234") : Color.White, 0, i * 22f / 13, 32, 22f / 13 + .5f); Fill(C("#3C3B6E"), 0, 0, 14, 12); for (int row = 0; row < 9; row++) for (int col = 0; col < (row % 2 == 0 ? 6 : 5); col++) Disk(Color.White, 1 + col * 2 + (row % 2), 1 + row * 1.2f, .8f); break;
            case "GB": g.Clear(C("#012169")); using (var white = new Pen(Color.White, 5)) { g.DrawLine(white, 0, 0, 32, 22); g.DrawLine(white, 0, 22, 32, 0); } using (var red = new Pen(C("#C8102E"), 2)) { g.DrawLine(red, 0, 0, 32, 22); g.DrawLine(red, 0, 22, 32, 0); } Fill(Color.White, 12, 0, 8, 22); Fill(Color.White, 0, 7, 32, 8); Fill(C("#C8102E"), 14, 0, 4, 22); Fill(C("#C8102E"), 0, 9, 32, 4); break;
            default: g.Clear(C("#493761")); using (var font = new Font("Segoe UI", 9, FontStyle.Bold)) using (var brush = new SolidBrush(Color.White)) using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }) g.DrawString(code == "??" ? "?" : code == "INT" || code.Length == 0 ? "◎" : code, font, brush, new RectangleF(0, 0, 32, 22), format); break;
        }
        return bitmap;
    }

    public static async Task<Bitmap?> Fetch(string code, string folder, CancellationToken token)
    {
        if (code.Length != 2 || code.Any(c => !char.IsAsciiLetter(c))) return null;
        var file = Path.Combine(folder, code.ToLowerInvariant() + ".png");
        try { if (File.Exists(file) && new FileInfo(file).Length < 65536) { using var cached = Image.FromFile(file); return new Bitmap(cached); } } catch { }
        bool entered = false;
        try
        {
            await Downloads.WaitAsync(token); entered = true;
            var bytes = await Client.GetByteArrayAsync("https://flagcdn.com/w40/" + code.ToLowerInvariant() + ".png", token);
            if (bytes.Length > 65536) return null;
            using var stream = new MemoryStream(bytes); using var image = Image.FromStream(stream); var bitmap = new Bitmap(image);
            try { Directory.CreateDirectory(folder); await File.WriteAllBytesAsync(file, bytes, token); } catch { }
            return bitmap;
        }
        catch { return null; }
        finally { if (entered) Downloads.Release(); }
    }
}
