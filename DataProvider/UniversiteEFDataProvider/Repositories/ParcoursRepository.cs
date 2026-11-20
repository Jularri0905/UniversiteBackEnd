using Microsoft.EntityFrameworkCore;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Data;

namespace UniversiteEFDataProvider.Repositories;

public class ParcoursRepository(UniversiteDbContext context) : Repository<Parcours>(context), IParcoursRepository
{
    public async Task<Parcours> AddEtudiantAsync(long idParcours, long idEtudiant)
    {
        ArgumentNullException.ThrowIfNull(Context.Parcours);
        ArgumentNullException.ThrowIfNull(Context.Etudiants);

        Parcours p = (await Context.Parcours
            .Include(parcours => parcours.Inscrits)
            .FirstOrDefaultAsync(parcours => parcours.Id == idParcours))!;
            
        Etudiant e = (await Context.Etudiants.FindAsync(idEtudiant))!;

        p.Inscrits ??= new List<Etudiant>();
        if (!p.Inscrits.Contains(e))
        {
            p.Inscrits.Add(e);
        }

        await Context.SaveChangesAsync();
        return p;
    }

    public async Task<Parcours> AddEtudiantAsync(Parcours parcours, Etudiant etudiant)
    {
        return await AddEtudiantAsync(parcours.Id, etudiant.Id);
    }

    public async Task<Parcours> AddEtudiantAsync(long idParcours, long[] idEtudiants)
    {
        ArgumentNullException.ThrowIfNull(Context.Parcours);
        ArgumentNullException.ThrowIfNull(Context.Etudiants);

        Parcours p = (await Context.Parcours
            .Include(parcours => parcours.Inscrits)
            .FirstOrDefaultAsync(parcours => parcours.Id == idParcours))!;

        p.Inscrits ??= new List<Etudiant>();

        foreach (long idEtudiant in idEtudiants)
        {
            Etudiant e = (await Context.Etudiants.FindAsync(idEtudiant))!;
            if (!p.Inscrits.Contains(e))
            {
                p.Inscrits.Add(e);
            }
        }

        await Context.SaveChangesAsync();
        return p;
    }

    public async Task<Parcours> AddEtudiantAsync(Parcours? parcours, List<Etudiant> etudiants)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        long[] ids = etudiants.Select(e => e.Id).ToArray();
        return await AddEtudiantAsync(parcours.Id, ids);
    }

    public async Task<Parcours> AddUeAsync(long idParcours, long idUe)
    {
        ArgumentNullException.ThrowIfNull(Context.Parcours);
        ArgumentNullException.ThrowIfNull(Context.Ues);

        Parcours p = (await Context.Parcours
            .Include(parcours => parcours.UesEnseignees)
            .FirstOrDefaultAsync(parcours => parcours.Id == idParcours))!;
            
        Ue u = (await Context.Ues.FindAsync(idUe))!;

        p.UesEnseignees ??= new List<Ue>();
        if (!p.UesEnseignees.Contains(u))
        {
            p.UesEnseignees.Add(u);
        }

        await Context.SaveChangesAsync();
        return p;
    }

    public async Task<Parcours> AddUeAsync(Parcours parcours, Ue ue)
    {
        return await AddUeAsync(parcours.Id, ue.Id);
    }

    public async Task<Parcours> AddUeAsync(long idParcours, long[] idUes)
    {
        ArgumentNullException.ThrowIfNull(Context.Parcours);
        ArgumentNullException.ThrowIfNull(Context.Ues);

        Parcours p = (await Context.Parcours
            .Include(parcours => parcours.UesEnseignees)
            .FirstOrDefaultAsync(parcours => parcours.Id == idParcours))!;

        p.UesEnseignees ??= new List<Ue>();

        foreach (long idUe in idUes)
        {
            Ue u = (await Context.Ues.FindAsync(idUe))!;
            if (!p.UesEnseignees.Contains(u))
            {
                p.UesEnseignees.Add(u);
            }
        }

        await Context.SaveChangesAsync();
        return p;
    }

    public async Task<Parcours> AddUeAsync(Parcours? parcours, List<Ue> ues)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        long[] ids = ues.Select(u => u.Id).ToArray();
        return await AddUeAsync(parcours.Id, ids);
    }
}