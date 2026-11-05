namespace AIS.Models
{
    // The Oracle procedure's supported operations and the two existing page authorities.
    // Neither operation nor page ID is accepted from an untrusted request body.
    public enum LegacyParaOperation
    {
        Submit,
        Lookup,
        List,
        Detail,
        Decide
    }

    public enum LegacyParaPage
    {
        Maker = 192,
        Checker = 193
    }
}
