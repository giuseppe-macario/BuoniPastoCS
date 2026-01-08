using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using BuoniPastoGui.Models;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace BuoniPastoGui.Services;

public sealed class PdfBuoniService
{
    private static readonly TimeSpan PranzoDa = new(15, 30, 0);
    private static readonly TimeSpan CenaDa   = new(20, 30, 0);

    private static readonly Regex RowRegex =
        new(@"(?<data>\d{2}/\d{2}/\d{4})\s+(?<ing>\d{2}[.,]\d{2})\s+(?<usc>\d{2}[.,]\d{2})",
            RegexOptions.Compiled);

    public List<BuonoResult> AnalizzaFile(string pdfPath)
    {
        var risultati = new List<BuonoResult>();
        using var reader = new PdfReader(pdfPath);
        using var pdf = new PdfDocument(reader);

        for (int p = 1; p <= pdf.GetNumberOfPages(); p++)
        {
            var text = PdfTextExtractor.GetTextFromPage(pdf.GetPage(p));
            risultati.AddRange(AnalizzaTesto(pdfPath, text));
        }

        return risultati;
    }

    private static IEnumerable<BuonoResult> AnalizzaTesto(string file, string text)
    {
        foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var m = RowRegex.Match(line);
            if (!m.Success) continue;

            var dataStr = m.Groups["data"].Value;
            var uscitaStr = m.Groups["usc"].Value.Replace(',', ':').Replace('.', ':');

            if (!DateTime.TryParseExact(dataStr, "dd/MM/yyyy", null,
                    System.Globalization.DateTimeStyles.None, out var data))
                continue;

            if (!TimeSpan.TryParse(uscitaStr, out var uscita))
                continue;

            if (uscita == TimeSpan.Zero) continue;

            bool pranzo = (data.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday or DayOfWeek.Sunday)
                          && uscita >= PranzoDa;

            bool cena = uscita >= CenaDa;

            if (pranzo)
                yield return new BuonoResult(data, uscita, TipoBuono.Pranzo);

            if (cena)
                yield return new BuonoResult(data, uscita, TipoBuono.Cena);
        }
    }
}

