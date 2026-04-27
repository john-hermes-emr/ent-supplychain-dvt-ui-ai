using DVT.Core.Models;
using Riok.Mapperly.Abstractions;

namespace DVT.Core
{
    [Mapper]
    public partial class Mapping
    {
        public partial JobDto JobToJobDto(Job job);

    }
}
