using System;

namespace BuoniPastoGui.Models;

public enum TipoBuono { Pranzo, Cena }

public sealed record BuonoResult(
    DateTime Data,
    TimeSpan Uscita,
    TipoBuono Tipo
);
