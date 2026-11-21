using Etl.Domain;

namespace Etl.Application;

public interface IStagingWriter
{
    Task WriteAsync(ExtractionResult result, CancellationToken cancellationToken);
}
