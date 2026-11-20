using Microsoft.EntityFrameworkCore;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Data;

namespace UniversiteEFDataProvider.Repositories;

public class NoteRepository(UniversiteDbContext context) : Repository<Note>(context), INoteRepository
{
    public async Task AffecterNoteAsync(long idEtudiant, long idUe, float valeur)
    {
        ArgumentNullException.ThrowIfNull(Context.Notes);
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        ArgumentNullException.ThrowIfNull(Context.Ues);

        // Vérification que l'étudiant et l'UE existent
        Etudiant e = (await Context.Etudiants.FindAsync(idEtudiant))!;
        Ue u = (await Context.Ues.FindAsync(idUe))!;

        // Recherche d'une note existante via la clé composite
        Note? noteExistante = await Context.Notes.FindAsync(idEtudiant, idUe);

        if (noteExistante != null)
        {
            // Mise à jour si la note existe déjà
            noteExistante.Valeur = valeur;
        }
        else
        {
            // Création d'une nouvelle note sinon
            Note nouvelleNote = new Note
            {
                EtudiantId = e.Id,
                UeId = u.Id,
                Valeur = valeur
            };
            await Context.Notes.AddAsync(nouvelleNote);
        }

        await Context.SaveChangesAsync();
    }

    public async Task AffecterNoteAsync(Etudiant etudiant, Ue ue, float valeur)
    {
        await AffecterNoteAsync(etudiant.Id, ue.Id, valeur);
    }

    public async Task<Note?> FindNoteAsync(long idEtudiant, long idUe)
    {
        ArgumentNullException.ThrowIfNull(Context.Notes);
        return await Context.Notes
            .Include(n => n.Etudiant)
            .Include(n => n.Ue)
            .FirstOrDefaultAsync(n => n.EtudiantId == idEtudiant && n.UeId == idUe);
    }
}