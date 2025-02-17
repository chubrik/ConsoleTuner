using Chubrik.XConsole.StringExtensions;
using System.Drawing;

var _random = new Random();
var selectedIndex = 0;
var defaultStep = 16;

Dictionary<string, int[]> palettes = new()
{   //                W         w         d         Y         G         C         R         M         B         y         g         c         r         m         b         n
    { "Modern",   [0xFFFFFF, 0xB4B4B4, 0x707070, 0xF8EC00, 0x00EC00, 0x00FFFF, 0xFF2000, 0xFF40FF, 0x4058FF, 0x9C9400, 0x009400, 0x00A0A0, 0xA81800, 0xA830A8, 0x3038C0, 0x000000] },
    { "Previous", [0xFFFFFF, 0xB8B8B8, 0x707070, 0xFFFF00, 0x00FF00, 0x00FFFF, 0xFF0000, 0xFF00FF, 0x4050FF, 0x989800, 0x009800, 0x009898, 0x980000, 0x980098, 0x2830B0, 0x000000] },
    { "Trivial",  [0xFFFFFF, 0xC0C0C0, 0x808080, 0xFFFF00, 0x00FF00, 0x00FFFF, 0xFF0000, 0xFF00FF, 0x0000FF, 0xA0A000, 0x00A000, 0x00A0A0, 0xA00000, 0xA000A0, 0x0000A0, 0x000000] },
    { "VGA",      [0xFFFFFF, 0xC0C0C0, 0x808080, 0xFFFF00, 0x00FF00, 0x00FFFF, 0xFF0000, 0xFF00FF, 0x0000FF, 0x808000, 0x008000, 0x008080, 0x800000, 0x800080, 0x000080, 0x000000] },
    { "Win10",    [0xF2F2F2, 0xCCCCCC, 0x767676, 0xF9F1A5, 0x16C60C, 0x61D6D6, 0xE74856, 0xB4009E, 0x3B78FF, 0xC19C00, 0x13A10E, 0x3A96DD, 0xC50F1F, 0x881798, 0x0037DA, 0x0C0C0C] },
};

string? demoTitle = palettes.First().Key;
var currentPalette = palettes.First().Value.Select(GetColor).ToList();
var tuningPalette = currentPalette;
var isFlippedColors = false;
var isTuningStarted = false;
IConsoleAnimation? tuningEllipsis = null;

var (W, w, d, Y, G, C, R, M, B, y, g, c, r, m, b, n) = (0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);

(int, int)[][] colorMap = [
    [ (W,W), (w,w), (d,d), (Y,Y), (G,G), (C,C), (R,R), (M,M), (B,B), (y,y), (g,g), (c,c), (r,r), (m,m), (b,b) ],
    [ (w,W), (d,w), (d,W), (Y,W), (G,W), (C,W), (R,W), (M,W), (B,W), (y,W), (g,W), (c,W), (r,W), (m,W), (b,W) ],
    [ (y,Y), (g,G), (c,C), (Y,w), (G,w), (C,w), (R,w), (M,w), (B,w), (y,w), (g,w), (c,w), (r,w), (m,w), (b,w) ],
    [ (r,R), (m,M), (b,B), (d,Y), (d,G), (d,C), (d,R), (d,M), (d,B), (d,y), (d,g), (d,c), (d,r), (d,m), (d,b) ],
    [ (R,Y), (M,G), (B,C), (C,Y), (G,Y), (G,C), (R,C), (M,Y), (B,G), (B,Y), (R,G), (M,C), (B,R), (R,M), (B,M) ],
    [ (r,y), (m,g), (b,c), (c,y), (g,y), (g,c), (r,c), (m,y), (b,g), (b,y), (r,g), (m,c), (b,r), (r,m), (b,m) ],
    [ (R,y), (M,g), (B,c), (C,y), (G,y), (G,c), (R,c), (M,y), (B,g), (B,y), (R,g), (M,c), (B,r), (R,m), (B,m) ],
    [ (r,Y), (m,G), (b,C), (c,Y), (g,Y), (g,C), (r,C), (m,Y), (b,G), (b,Y), (r,G), (m,C), (b,R), (r,M), (b,M) ],
];

List<List<string>> textMap;

void RandomizeText()
{
    var rowCount = colorMap.Length * 6;
    var colCount = colorMap[0].Length * 6;
    var map = new List<List<string>>();

    for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
    {
        var row = new List<string>();

        for (var colIndex = 0; colIndex < colCount; colIndex++)
        {
            var letter1 = (char)_random.Next(97, 122);
            var letter2 = (char)_random.Next(97, 122);
            row.Add($"{letter1}{letter2}");
        }

        map.Add(row);
    }

    textMap = map;
}

void RenderMenu()
{
    var bgHex = "";
    var str = "  ";

    for (int i = 0; i < currentPalette.Count; i++)
    {
        var color = currentPalette[i];
        var red = color.R.ToString("x2").ToUpper();
        var green = color.G.ToString("x2").ToUpper();
        var blue = color.B.ToString("x2").ToUpper();
        var hex = $"{red} {green} {blue}";

        if (i == selectedIndex && demoTitle == null)
            hex = hex.Underline();

        if (i != n)
            str += $"  {hex.Color(color)}    ";
        else
            bgHex = hex;
    }

    new ConsolePosition(left: (currentPalette.Count - 2) * 14 + 4, top: 0).Write(bgHex.Color(0x505050));
    new ConsolePosition(left: 0, top: 2).Write(str);
}

void Render()
{
    if (demoTitle != null && tuningEllipsis != null)
    {
        tuningEllipsis.Stop();
        tuningEllipsis = null;
    }

    RenderMenu();
    Console.SetCursorPosition(0, 0);
    var titleEnd = Console.WriteLine($"W`  {demoTitle ?? "Tuning"}").End;
    titleEnd.Write("    ");

    if (demoTitle == null && tuningEllipsis == null)
        tuningEllipsis = titleEnd.AnimateEllipsis();

    Console.WriteLine();
    Console.WriteLine();

    var bgColor = currentPalette[^1];
    var textRowIndex = 0;

    foreach (var colorRow in colorMap)
    {
        Console.WriteLine(new string(' ', (currentPalette.Count - 1) * 14 + 2).BgColor(bgColor));

        for (var i = 0; i < 6; i++)
        {
            var textRow = textMap[textRowIndex++];
            var textColIndex = 0;
            var str = "  ";

            for (var columnIndex = 0; columnIndex < colorRow.Length; columnIndex++)
            {
                var c1 = currentPalette[colorRow[columnIndex].Item1];
                var c2 = currentPalette[colorRow[columnIndex].Item2];
                var isFlipped = (isFlippedColors ? i + 1 : i) % 2 == 0;

                if (isFlipped)
                    str += textRow[textColIndex++].Color(c1);

                str += textRow[textColIndex++].Color(c2);
                str += textRow[textColIndex++].Color(c1);
                str += textRow[textColIndex++].Color(c2);
                str += textRow[textColIndex++].Color(c1);
                str += textRow[textColIndex++].Color(c2);

                if (!isFlipped)
                    str += textRow[textColIndex++].Color(c1);

                str += "  ";
            }

            Console.WriteLine(str.BgColor(bgColor));
        }
    }

    Console.WriteLine(new string(' ', (currentPalette.Count - 1) * 14 + 2).BgColor(bgColor));
}

Color GetColor(int rgb)
{
    return Color.FromArgb(red: rgb >> 16, green: (rgb >> 8) & 0xFF, blue: rgb & 0xFF);
}

//

Console.CursorVisible = false;
Console.Clear();
Console.Extras.WindowMaximize();
RandomizeText();
Render();

while (true)
{
    var keyInfo = Console.ReadKey(intercept: true);
    var colorStep = defaultStep;
    if (keyInfo.Modifiers == ConsoleModifiers.Shift) colorStep /= 4;
    var key = keyInfo.Key;

    switch (key)
    {
        case ConsoleKey.D1:
            demoTitle = palettes.ElementAt(0).Key;
            currentPalette = [.. palettes.ElementAt(0).Value.Select(GetColor)];
            Render();
            continue;

        case ConsoleKey.D2:
            demoTitle = palettes.ElementAt(1).Key;
            currentPalette = [.. palettes.ElementAt(1).Value.Select(GetColor)];
            Render();
            continue;

        case ConsoleKey.D3:
            demoTitle = palettes.ElementAt(2).Key;
            currentPalette = [.. palettes.ElementAt(2).Value.Select(GetColor)];
            Render();
            continue;

        case ConsoleKey.D4:
            demoTitle = palettes.ElementAt(3).Key;
            currentPalette = [.. palettes.ElementAt(3).Value.Select(GetColor)];
            Render();
            continue;

        case ConsoleKey.D5:
            demoTitle = palettes.ElementAt(4).Key;
            currentPalette = [.. palettes.ElementAt(4).Value.Select(GetColor)];
            Render();
            continue;

        case ConsoleKey.T:
            if (!isTuningStarted)
                tuningPalette = [.. currentPalette];

            isTuningStarted = true;
            demoTitle = null;
            currentPalette = tuningPalette;
            Render();
            continue;

        case ConsoleKey.R:
            RandomizeText();
            Render();
            continue;

        case ConsoleKey.F:
            isFlippedColors = !isFlippedColors;
            Render();
            continue;

        case ConsoleKey.Enter:
            GenerateRegFile();
            continue;
    }

    if (demoTitle != null)
        continue;

    switch (key)
    {
        case ConsoleKey.LeftArrow:
            selectedIndex--;

            if (selectedIndex == -1)
                selectedIndex = tuningPalette.Count - 1;

            RenderMenu();
            break;

        case ConsoleKey.RightArrow:
            selectedIndex++;

            if (selectedIndex == tuningPalette.Count)
                selectedIndex = 0;

            RenderMenu();
            break;

        case ConsoleKey.Q:
        {
            var color = tuningPalette[selectedIndex];
            var red = Math.Min(color.R / colorStep * colorStep + colorStep, 255);
            tuningPalette[selectedIndex] = Color.FromArgb(red, color.G, color.B);
            Render();
            break;
        }
        case ConsoleKey.W:
        {
            var color = tuningPalette[selectedIndex];
            var green = Math.Min(color.G / colorStep * colorStep + colorStep, 255);
            tuningPalette[selectedIndex] = Color.FromArgb(color.R, green, color.B);
            Render();
            break;
        }
        case ConsoleKey.E:
        {
            var color = tuningPalette[selectedIndex];
            var blue = Math.Min(color.B / colorStep * colorStep + colorStep, 255);
            tuningPalette[selectedIndex] = Color.FromArgb(color.R, color.G, blue);
            Render();
            break;
        }
        case ConsoleKey.A:
        {
            var color = tuningPalette[selectedIndex];
            var red = Math.Max(0, (color.R - 1) / colorStep * colorStep);
            tuningPalette[selectedIndex] = Color.FromArgb(red, color.G, color.B);
            Render();
            break;
        }
        case ConsoleKey.S:
        {
            var color = tuningPalette[selectedIndex];
            var green = Math.Max(0, (color.G - 1) / colorStep * colorStep);
            tuningPalette[selectedIndex] = Color.FromArgb(color.R, green, color.B);
            Render();
            break;
        }
        case ConsoleKey.D:
        {
            var color = tuningPalette[selectedIndex];
            var blue = Math.Max(0, (color.B - 1) / colorStep * colorStep);
            tuningPalette[selectedIndex] = Color.FromArgb(color.R, color.G, blue);
            Render();
            break;
        }
    }
}

void GenerateRegFile()
{
    string getHex(int colorIndex)
    {
        var color = currentPalette[colorIndex];
        var hex = $"00{color.B:x2}{color.G:x2}{color.R:x2}";
        return hex;
    }

    var text =
@$"Windows Registry Editor Version 5.00
[HKEY_CURRENT_USER\Console]

; Black, DarkGray
""ColorTable00""=dword:{getHex(n)}
""ColorTable08""=dword:{getHex(d)}

; Blue
""ColorTable01""=dword:{getHex(b)}
""ColorTable09""=dword:{getHex(B)}

; Green
""ColorTable02""=dword:{getHex(g)}
""ColorTable10""=dword:{getHex(G)}

; Cyan
""ColorTable03""=dword:{getHex(c)}
""ColorTable11""=dword:{getHex(C)}

; Red
""ColorTable04""=dword:{getHex(r)}
""ColorTable12""=dword:{getHex(R)}

; Magenta
""ColorTable05""=dword:{getHex(m)}
""ColorTable13""=dword:{getHex(M)}

; Yellow
""ColorTable06""=dword:{getHex(y)}
""ColorTable14""=dword:{getHex(Y)}

; Gray, White
""ColorTable07""=dword:{getHex(w)}
""ColorTable15""=dword:{getHex(W)}
";

    File.WriteAllText("console-colors.reg", text);
}
