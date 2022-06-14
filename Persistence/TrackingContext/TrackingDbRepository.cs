using Persistence.Common;

namespace Persistence.TrackingContext
{
    internal class TrackingDbRepository<T> : GenericRepository<T> where T : class
    {
        public TrackingDbRepository(ITrackingDbContext context) : base(context)
        {
        }
    }
}
