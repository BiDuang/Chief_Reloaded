﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Skia;
using SkiaSharp;

namespace Chief_Reloaded;

public class CustomFontManagerImpl : IFontManagerImpl
{
    private readonly string[] _bcp47 =
        { CultureInfo.CurrentCulture.ThreeLetterISOLanguageName, CultureInfo.CurrentCulture.TwoLetterISOLanguageName };

    private readonly Typeface[] _customTypefaces;
    private readonly string _defaultFamilyName;

    //Load font resources in the project, you can load multiple font resources
    private readonly Typeface _defaultTypeface = new("resm:Chief_Reloaded.Assets.Fonts.#Microsoft YaHei");

    public CustomFontManagerImpl()
    {
        _customTypefaces = new[] { _defaultTypeface };
        _defaultFamilyName = _defaultTypeface.FontFamily.FamilyNames.PrimaryFamilyName;
    }

    public string GetDefaultFontFamilyName()
    {
        return _defaultFamilyName;
    }

    public IEnumerable<string> GetInstalledFontFamilyNames(bool checkForUpdates = false)
    {
        return _customTypefaces.Select(x => x.FontFamily.Name);
    }

    public bool TryMatchCharacter(int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontFamily fontFamily,
        CultureInfo culture, out Typeface typeface)
    {
        foreach (var customTypeface in _customTypefaces)
        {
            if (customTypeface.GlyphTypeface.GetGlyph((uint)codepoint) == 0) continue;

            typeface = new Typeface(customTypeface.FontFamily.Name, fontStyle, fontWeight);

            return true;
        }

        var fallback = SKFontManager.Default.MatchCharacter(fontFamily?.Name, (SKFontStyleWeight)fontWeight,
            SKFontStyleWidth.Normal, (SKFontStyleSlant)fontStyle, _bcp47, codepoint);

        typeface = new Typeface(fallback?.FamilyName ?? _defaultFamilyName, fontStyle, fontWeight);

        return true;
    }

    public IGlyphTypefaceImpl CreateGlyphTypeface(Typeface typeface)
    {
        SKTypeface skTypeface;

        switch (typeface.FontFamily.Name)
        {
            case FontFamily.DefaultFontFamilyName:
            case "微软雅黑": //font family name
                skTypeface = SKTypeface.FromFamilyName(_defaultTypeface.FontFamily.Name);
                break;
            default:
                skTypeface = SKTypeface.FromFamilyName(typeface.FontFamily.Name,
                    (SKFontStyleWeight)typeface.Weight, SKFontStyleWidth.Normal, (SKFontStyleSlant)typeface.Style);
                break;
        }

        //解决linux系统下skTypeface是null
        if (skTypeface == null) skTypeface = SKTypeface.FromFamilyName(_defaultTypeface.FontFamily.Name);
        //如果是centos7之类的使用linux里面的字体
        //if (skTypeface == null)
        //{
        //    skTypeface = SKTypeface.FromFamilyName("WenQuanYi Micro Hei");
        //}

        return new GlyphTypefaceImpl(skTypeface);
    }
}