using ChromaFx.Core;
using ChromaFx.Processing.Filters.Interfaces;
using ChromaFx.Processing.Numerics;

namespace ChromaFx.Processing;

public static class ImageFilterExtensions
{
    public static Image ApplyFilter(this Image image, IFilter filter, Rectangle targetLocation = default)
    {
        return filter.Apply(image, targetLocation);
    }

    public static Image ApplyFilter<TFilter>(this Image image, Rectangle targetLocation = default)
        where TFilter : IFilter, new()
    {
        return image.ApplyFilter(new TFilter(), targetLocation);
    }
}
