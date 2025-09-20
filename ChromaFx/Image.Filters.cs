using ChromaFx.Filters.Interfaces;
using ChromaFx.Numerics;

namespace ChromaFx;

public partial class Image
{
    /// <summary>
    /// Applies the filter to the specified location.
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The resulting image (this)</returns>
    public Image Apply(IFilter filter, Rectangle targetLocation = default)
    {
        return filter.Apply(this, targetLocation);
    }

    /// <summary>
    /// Applies the filter to the specified location.
    /// </summary>
    /// <typeparam name="TFilter">The type of the filter.</typeparam>
    /// <param name="targetLocation">The target location.</param>
    /// <returns>The resulting image (this)</returns>
    public Image Apply<TFilter>(Rectangle targetLocation = default)
        where TFilter : IFilter, new()
    {
        return Apply(new TFilter(), targetLocation);
    }
}
