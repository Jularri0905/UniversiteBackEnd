using UniversiteDomain.Entities;

namespace UniversiteDomain.DataAdapters;

public interface IUeRepository : IRepository<Ue>
{
    Task<Ue> AddParcoursAsync(long idUe, long idParcours);
    
    Task<Ue> AddParcoursAsync(Ue ue, Parcours parcours);
    
    Task<Ue?> FindUeCompleteAsync(long idUe);
}