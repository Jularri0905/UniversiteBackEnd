using Microsoft.EntityFrameworkCore;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Data;

namespace UniversiteEFDataProvider.Repositories;

public class UeRepository(UniversiteDbContext context) : Repository<Ue>(context), IUeRepository
{
    public async Task<Ue> AddParcoursAsync(long idUe, long idParcours)
    {
        ArgumentNullException.ThrowIfNull(Context.Ues);
        ArgumentNullException.ThrowIfNull(Context.Parcours);

        Ue u = (await Context.Ues
            .Include(ue => ue.EnseigneeDans)
            .FirstOrDefaultAsync(ue => ue.Id == idUe))!;

        Parcours p = (await Context.Parcours.FindAsync(idParcours))!;

        u.EnseigneeDans ??= new List<Parcours>();
        if (!u.EnseigneeDans.Contains(p))
        {
            u.EnseigneeDans.Add(p);
        }

        await Context.SaveChangesAsync();
        return u;
    }

    public async Task<Ue> AddParcoursAsync(Ue ue, Parcours parcours)
    {
        return await AddParcoursAsync(ue.Id, parcours.Id);
    }

    public async Task<Ue?> FindUeCompleteAsync(long idUe)
    {
        ArgumentNullException.ThrowIfNull(Context.Ues);
        return await Context.Ues
            .Include(u => u.EnseigneeDans)
            .Include(u => u.Notes)
            .ThenInclude(n => n.Etudiant)
            .FirstOrDefaultAsync(u => u.Id == idUe);
    }
}