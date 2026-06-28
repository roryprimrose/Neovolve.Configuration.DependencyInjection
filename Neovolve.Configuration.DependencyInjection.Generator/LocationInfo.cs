namespace Neovolve.Configuration.DependencyInjection.Generator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

/// <summary>
///     The <see cref="LocationInfo" /> struct is a value-equatable representation of a source <see cref="Location" /> so a
///     diagnostic location can flow through the incremental generator pipeline without breaking caching.
/// </summary>
internal readonly struct LocationInfo : IEquatable<LocationInfo>
{
    public LocationInfo(string filePath, TextSpan textSpan, LinePositionSpan lineSpan)
    {
        FilePath = filePath;
        TextSpan = textSpan;
        LineSpan = lineSpan;
    }

    public static LocationInfo? CreateFrom(Location location)
    {
        if (location.SourceTree == null)
        {
            return null;
        }

        return new LocationInfo(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
    }

    public static LocationInfo? CreateFrom(ISymbol symbol)
    {
        foreach (var location in symbol.Locations)
        {
            if (location.IsInSource)
            {
                return CreateFrom(location);
            }
        }

        return null;
    }

    public bool Equals(LocationInfo other)
    {
        return FilePath == other.FilePath && TextSpan.Equals(other.TextSpan) && LineSpan.Equals(other.LineSpan);
    }

    public override bool Equals(object? obj)
    {
        return obj is LocationInfo other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = 17;

        hash = (hash * 31) + FilePath.GetHashCode();
        hash = (hash * 31) + TextSpan.GetHashCode();
        hash = (hash * 31) + LineSpan.GetHashCode();

        return hash;
    }

    public Location ToLocation()
    {
        return Location.Create(FilePath, TextSpan, LineSpan);
    }

    public string FilePath { get; }

    public LinePositionSpan LineSpan { get; }

    public TextSpan TextSpan { get; }
}
