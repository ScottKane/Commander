using CliFx.Extensibility;

namespace Commander.Converters;

public class DirectoryConverter : BindingConverter<string?>
{
    public override string? Convert(string? rawValue)
    {
        if (rawValue is not null && (rawValue.EndsWith('\\') || rawValue.EndsWith('/')))
            rawValue = rawValue[..^1];

        return rawValue;
    }
}